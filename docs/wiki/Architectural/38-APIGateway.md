# API Gateway Pattern

## Overview

**Category:** Architectural Pattern  
**Purpose:** Single entry point for client requests, routing to appropriate microservices.  
**Complexity:** Medium-Hard  
**Use Case:** Microservices, API versioning, cross-cutting concerns, client abstraction

## Problem

Direct client-to-service calls create issues:
- Clients must know all service locations
- Each client must handle authentication separately
- Clients must handle different response formats
- Cross-cutting concerns scattered across services
- No rate limiting or load balancing
- Client breaks when service moved

```csharp
// BAD: Direct client-to-microservice calls
public class MobileClient
{
    private readonly HttpClient _httpClient;

    public async Task<User> GetUser(int id)
    {
        // Must know User Service location
        var response = await _httpClient.GetAsync("http://user-service:5000/users/{id}");
        return ParseResponse<User>(response);
    }

    public async Task<List<Order>> GetOrders(int userId)
    {
        // Must know Order Service location (different server!)
        var response = await _httpClient.GetAsync("http://order-service:5001/users/{userId}/orders");
        return ParseResponse<List<Order>>(response);
    }

    public async Task<Payment> ProcessPayment(PaymentRequest request)
    {
        // Must handle authentication for each service
        var token = GetAuthToken();
        var headers = new Dictionary<string, string> { { "Authorization", $"Bearer {token}" } };
        
        var response = await _httpClient.PostAsync(
            "http://payment-service:5002/payments",
            new StringContent(JsonConvert.SerializeObject(request)),
            headers
        );
        
        return ParseResponse<Payment>(response);
    }

    // Problems:
    // - Client knows all service locations
    // - Duplicate authentication logic across clients
    // - Different response formats from different services
    // - Rate limiting not centralized
    // - If service moves, client breaks
}

// Web Client has same problems:
public class WebClient { }

// Mobile Client has same problems:
public class MobileAppClient { }

// Desktop Client has same problems:
public class DesktopClient { }
```

## Solution

Single API Gateway routes all requests to services:

```csharp
// GOOD: API Gateway Pattern

public class ApiGateway
{
    private readonly IServiceRegistry _serviceRegistry;
    private readonly IAuthenticationService _authService;
    private readonly ILoadBalancer _loadBalancer;
    private readonly IRateLimiter _rateLimiter;
    private readonly ILogger _logger;

    // Single entry point for all clients
    [HttpGet("api/v1/users/{id}")]
    public async Task<IActionResult> GetUser(int id, [FromHeader] string authorization)
    {
        try
        {
            // Centralized authentication
            var user = _authService.ValidateToken(authorization);
            if (user == null)
                return Unauthorized("Invalid token");

            // Centralized rate limiting
            if (!_rateLimiter.AllowRequest(user.Id))
                return StatusCode(429, "Too many requests");

            // Service discovery
            var userServiceUrl = await _serviceRegistry.GetService("user-service");

            // Load balancing
            var instance = _loadBalancer.SelectInstance(userServiceUrl);

            // Route request
            var response = await ProxyRequest(instance, $"/users/{id}");

            _logger.Info($"User {id} retrieved by {user.Email}");
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.Error($"Error getting user {id}: {ex.Message}");
            return StatusCode(500, "Internal server error");
        }
    }

    [HttpGet("api/v1/users/{userId}/orders")]
    public async Task<IActionResult> GetUserOrders(int userId, [FromHeader] string authorization)
    {
        // Centralized authentication
        var user = _authService.ValidateToken(authorization);
        if (user == null)
            return Unauthorized();

        // Centralized rate limiting
        if (!_rateLimiter.AllowRequest(user.Id))
            return StatusCode(429, "Too many requests");

        // Route to order service
        var orderServiceUrl = await _serviceRegistry.GetService("order-service");
        var instance = _loadBalancer.SelectInstance(orderServiceUrl);
        
        var response = await ProxyRequest(instance, $"/users/{userId}/orders");
        return Ok(response);
    }

    [HttpPost("api/v1/payments")]
    public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequest request, 
                                                     [FromHeader] string authorization)
    {
        var user = _authService.ValidateToken(authorization);
        if (user == null)
            return Unauthorized();

        if (!_rateLimiter.AllowRequest(user.Id))
            return StatusCode(429);

        // Route to payment service
        var paymentServiceUrl = await _serviceRegistry.GetService("payment-service");
        var instance = _loadBalancer.SelectInstance(paymentServiceUrl);
        
        var response = await ProxyRequest(instance, "/payments", request);
        return Ok(response);
    }

    private async Task<T> ProxyRequest<T>(ServiceInstance instance, string path, object body = null)
    {
        using (var client = new HttpClient())
        {
            client.BaseAddress = new Uri($"http://{instance.Address}:{instance.Port}");
            
            HttpResponseMessage response;
            if (body == null)
                response = await client.GetAsync(path);
            else
                response = await client.PostAsJsonAsync(path, body);

            if (!response.IsSuccessStatusCode)
                throw new ServiceException($"Service returned {response.StatusCode}");

            var content = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content);
        }
    }
}

// Client now only knows API Gateway
public class Client
{
    private readonly HttpClient _httpClient;
    private readonly string _apiGatewayUrl = "http://api-gateway:8000";

    public async Task<User> GetUser(int id, string token)
    {
        // Single, simple endpoint
        var response = await _httpClient.GetAsync(
            $"{_apiGatewayUrl}/api/v1/users/{id}",
            headers: new Dictionary<string, string> { { "Authorization", $"Bearer {token}" } }
        );

        return JsonConvert.DeserializeObject<User>(await response.Content.ReadAsStringAsync());
    }

    public async Task<List<Order>> GetOrders(int userId, string token)
    {
        var response = await _httpClient.GetAsync(
            $"{_apiGatewayUrl}/api/v1/users/{userId}/orders",
            headers: new Dictionary<string, string> { { "Authorization", $"Bearer {token}" } }
        );

        return JsonConvert.DeserializeObject<List<Order>>(await response.Content.ReadAsStringAsync());
    }

    public async Task<Payment> ProcessPayment(PaymentRequest request, string token)
    {
        var response = await _httpClient.PostAsJsonAsync(
            $"{_apiGatewayUrl}/api/v1/payments",
            request,
            headers: new Dictionary<string, string> { { "Authorization", $"Bearer {token}" } }
        );

        return JsonConvert.DeserializeObject<Payment>(await response.Content.ReadAsStringAsync());
    }
}
```

## Implementation Approaches

### 1. Simple HTTP Routing Gateway

```csharp
// Basic gateway with routing rules
public class SimpleApiGateway
{
    private readonly Dictionary<string, string> _routes = new()
    {
        { "/api/users", "http://user-service:5000" },
        { "/api/orders", "http://order-service:5001" },
        { "/api/payments", "http://payment-service:5002" },
        { "/api/notifications", "http://notification-service:5003" }
    };

    [HttpRoute("{*path}")]
    public async Task<IActionResult> Route(string path)
    {
        // Find matching service
        var matchingRoute = _routes.Keys.FirstOrDefault(route => path.StartsWith(route));
        if (matchingRoute == null)
            return NotFound();

        var serviceUrl = _routes[matchingRoute];
        var remainingPath = path.Substring(matchingRoute.Length);

        // Proxy request
        using (var client = new HttpClient())
        {
            var response = await client.GetAsync($"{serviceUrl}{remainingPath}");
            var content = await response.Content.ReadAsStringAsync();
            return new ContentResult
            {
                Content = content,
                StatusCode = (int)response.StatusCode,
                ContentType = "application/json"
            };
        }
    }
}
```

### 2. Advanced Gateway with Middleware

```csharp
// Gateway with cross-cutting concerns
public class AdvancedApiGateway
{
    private readonly IAuthenticationMiddleware _auth;
    private readonly IRateLimitingMiddleware _rateLimiting;
    private readonly ILoggingMiddleware _logging;
    private readonly ICircuitBreakerMiddleware _circuitBreaker;

    public async Task<IActionResult> ProcessRequest(HttpContext context)
    {
        // Authentication middleware
        if (!_auth.Authenticate(context))
            return Unauthorized();

        // Rate limiting middleware
        if (!_rateLimiting.CheckLimit(context.User.Id))
            return StatusCode(429);

        // Logging middleware
        _logging.LogRequest(context);

        try
        {
            // Route request with circuit breaker
            var response = await _circuitBreaker.ExecuteAsync(
                () => RouteRequest(context)
            );

            _logging.LogResponse(context, response);
            return response;
        }
        catch (ServiceUnavailableException)
        {
            return StatusCode(503, "Service temporarily unavailable");
        }
    }

    private async Task<IActionResult> RouteRequest(HttpContext context)
    {
        var service = DetermineTargetService(context.Request.Path);
        return await ProxyToService(service, context);
    }
}
```

### 3. Compositional Gateway (BFF Pattern)

```csharp
// Backend for Frontend - tailored data for each client
public class MobileGateway
{
    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUserForMobile(int id)
    {
        // Get user data
        var user = await _userService.GetUser(id);
        
        // Get orders
        var orders = await _orderService.GetUserOrders(id);
        
        // Compose response tailored for mobile
        return Ok(new MobileUserResponse
        {
            Id = user.Id,
            Name = user.Name,
            Avatar = user.Avatar,  // Mobile needs avatar
            RecentOrders = orders.Take(5).ToList(),  // Only recent orders
            IsPremiumMember = user.IsPremium
        });
    }
}

public class WebGateway
{
    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUserForWeb(int id)
    {
        // Get complete user data for web
        var user = await _userService.GetUser(id);
        var orders = await _orderService.GetUserOrders(id);
        var payments = await _paymentService.GetUserPayments(id);

        // Compose response tailored for web
        return Ok(new WebUserResponse
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            Phone = user.Phone,
            AllOrders = orders,
            PaymentHistory = payments,
            Preferences = user.Preferences
        });
    }
}
```

## API Gateway Responsibilities

### 1. Request Routing
- Route requests to appropriate services
- Handle versioning (v1, v2)
- Pattern matching and path rewriting

### 2. Authentication & Authorization
- Validate tokens
- Extract user information
- Check permissions

### 3. Rate Limiting
- Limit requests per user/IP
- Throttle excessive traffic
- Queue management

### 4. Load Balancing
- Distribute requests across instances
- Health-based routing
- Sticky sessions if needed

### 5. Response Transformation
- Data mapping between clients and services
- Format conversion (XML to JSON)
- Composition (aggregating multiple services)

### 6. Monitoring & Logging
- Log all requests/responses
- Performance metrics
- Error tracking

## Benefits of API Gateway

✅ **Single Entry Point** - Clients see one URL  
✅ **Centralized Concerns** - Auth, rate limiting, logging  
✅ **Service Abstraction** - Clients don't know service locations  
✅ **API Versioning** - Different versions side-by-side  
✅ **Request Transformation** - Adapt responses for clients  
✅ **Cross-Service Features** - Circuit breaking, retries  

## Drawbacks

❌ **Single Point of Failure** - Gateway down = system down  
❌ **Complexity** - Gateway must handle all concerns  
❌ **Performance** - Adds latency (hop through gateway)  
❌ **Bottleneck** - All traffic through gateway  
❌ **Operational Overhead** - Gateway needs monitoring  

## Interview Questions

**Q: What is API Gateway?**
A: Single entry point for all client requests to microservices. Routes requests to appropriate services and handles cross-cutting concerns like authentication, rate limiting, and logging.

**Q: Why use API Gateway instead of direct client calls?**
A: API Gateway centralizes authentication, rate limiting, request logging, error handling, and service discovery. Clients don't need to know service locations.

**Q: What's the difference between API Gateway and Load Balancer?**
A: Load Balancer distributes traffic across instances of same service. API Gateway routes different requests to different services and handles application-level concerns.

**Q: How do you handle API Gateway failure?**
A: Use multiple gateway instances behind load balancer, implement health checks, circuit breakers for backend services, and caching for reads.

**Q: What's BFF pattern?**
A: Backend For Frontend - separate gateways for different clients (mobile, web, desktop) tailored to client needs rather than generic gateway.

**Q: How does API Gateway handle versioning?**
A: Route based on version prefix (/v1, /v2), accept header, or query parameter. Gateway can route different versions to different services.

## When to Use API Gateway

### ✅ Use When:
- Microservices architecture
- Multiple client types
- Need centralized authentication/rate limiting
- API versioning needed
- Cross-cutting concerns exist

### ❌ Don't Use When:
- Monolithic architecture
- Simple application
- Single client type
- No cross-cutting concerns

## Popular API Gateway Solutions

- **AWS API Gateway** - Managed service, integrated with AWS
- **Kong** - Open-source, Lua extensible
- **NGINX** - High-performance proxy
- **Ocelot** - C# library for .NET microservices
- **Tyk** - Open-source API management

## Summary

API Gateway provides single entry point for microservices, centralizing cross-cutting concerns and service abstraction. Essential for microservices architectures with multiple clients. However, requires careful design to avoid becoming bottleneck and critical failure point.

**Key Takeaway:** API Gateway centralizes cross-cutting concerns and provides single entry point to microservices.

---

**Related Patterns:**
- Microservices - Services behind gateway
- Service Discovery - Gateway discovers service locations
- Load Balancer - Distributes gateway traffic
- Circuit Breaker - Resilience for backend calls
- Rate Limiter - Throttles requests
