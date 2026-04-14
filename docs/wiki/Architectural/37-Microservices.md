# Microservices Architecture Pattern

## Overview

**Category:** Architectural Pattern  
**Purpose:** Decompose application into independently deployable, loosely coupled services.  
**Complexity:** Very Hard  
**Use Case:** Large distributed systems, rapid scaling, independent team deployment

## Problem

Monolithic architectures have inherent limitations:
- Single deployment for entire application
- Scaling entire app when only one component needs scaling
- Technology lock-in
- Teams cannot work independently
- Failure in one component brings down entire app

```csharp
// BAD: Monolithic Architecture
// Single ASP.NET Core app with all functionality

public class UserController
{
    // User management
    public ActionResult Register(User user) { }
    public ActionResult Login(string email, string password) { }
}

public class OrderController
{
    // Order processing
    public ActionResult CreateOrder(Order order) { }
    public ActionResult CancelOrder(int orderId) { }
}

public class PaymentController
{
    // Payment processing
    public ActionResult ProcessPayment(Payment payment) { }
    public ActionResult RefundPayment(int paymentId) { }
}

public class NotificationController
{
    // Notifications
    public ActionResult SendEmail(string to, string subject) { }
    public ActionResult SendSMS(string phone, string message) { }
}

// Single database shared by all services
// Single deployment for entire app
// Tight coupling between services

// Problems:
// - Cannot scale payment service independently
// - User service bug can crash order service
// - Teams cannot deploy independently
// - Technology lock-in to C# and SQL Server
// - Testing requires entire app running
// - Large codebase hard to understand
```

## Solution

Decompose into independent microservices:

```csharp
// GOOD: Microservices Architecture

// ===== USER SERVICE =====
// Separate ASP.NET Core project, separate database

public class UserService
{
    private readonly IUserRepository _repository;

    public ActionResult Register(RegisterRequest request)
    {
        var user = new User { Email = request.Email, Name = request.Name };
        _repository.Save(user);
        
        // Publish domain event
        var @event = new UserRegisteredEvent { UserId = user.Id, Email = user.Email };
        _eventPublisher.Publish(@event);
        
        return Ok(user);
    }

    public ActionResult Login(LoginRequest request)
    {
        var user = _repository.GetByEmail(request.Email);
        if (user == null || !BCrypt.Verify(request.Password, user.PasswordHash))
            return Unauthorized();

        var token = _tokenGenerator.Generate(user);
        return Ok(new { token });
    }
}

// User service database (separate from others)
// UserDb:
// - Users table
// - Roles table
// - Permissions table

// ===== ORDER SERVICE =====
// Separate Node.js/Python project, separate database

public class OrderService
{
    private readonly IOrderRepository _repository;
    private readonly IHttpClient _httpClient;  // Calls User Service
    private readonly IMessageBus _messageBus;

    public async Task<ActionResult> CreateOrder(CreateOrderRequest request)
    {
        // Verify user exists (call User Service)
        var userResponse = await _httpClient.GetAsync($"/user-service/users/{request.UserId}");
        if (!userResponse.IsSuccessStatusCode)
            return BadRequest("Invalid user");

        var order = new Order 
        { 
            UserId = request.UserId, 
            Items = request.Items,
            Status = OrderStatus.Pending 
        };
        
        _repository.Save(order);

        // Publish event - triggers payment and shipping services
        var @event = new OrderCreatedEvent { OrderId = order.Id, UserId = request.UserId };
        await _messageBus.PublishAsync(@event);

        return Ok(order);
    }

    public ActionResult CancelOrder(int orderId)
    {
        var order = _repository.GetById(orderId);
        order.Status = OrderStatus.Cancelled;
        _repository.Save(order);

        // Publish cancellation event
        var @event = new OrderCancelledEvent { OrderId = orderId };
        _messageBus.Publish(@event);

        return Ok();
    }
}

// Order service database (separate)
// OrderDb:
// - Orders table
// - OrderItems table

// ===== PAYMENT SERVICE =====
// Separate project, separate database

public class PaymentService
{
    private readonly IPaymentRepository _repository;
    private readonly IPaymentGateway _gateway;

    [EventHandler]
    public async Task HandleOrderCreated(OrderCreatedEvent @event)
    {
        // Process payment when order created
        var payment = new Payment 
        { 
            OrderId = @event.OrderId, 
            Status = PaymentStatus.Processing 
        };

        _repository.Save(payment);

        try
        {
            await _gateway.ChargeAsync(payment);
            payment.Status = PaymentStatus.Completed;
        }
        catch (Exception)
        {
            payment.Status = PaymentStatus.Failed;
        }

        _repository.Save(payment);

        // Publish result
        var resultEvent = new PaymentProcessedEvent 
        { 
            OrderId = @event.OrderId, 
            Success = payment.Status == PaymentStatus.Completed 
        };
        _messageBus.Publish(resultEvent);
    }
}

// ===== NOTIFICATION SERVICE =====
// Separate project (could be different language)

public class NotificationService
{
    private readonly IEmailSender _emailSender;
    private readonly ISmsSender _smsSender;

    [EventHandler]
    public async Task HandleUserRegistered(UserRegisteredEvent @event)
    {
        await _emailSender.SendAsync(
            @event.Email,
            "Welcome!",
            "Thank you for registering"
        );
    }

    [EventHandler]
    public async Task HandleOrderCreated(OrderCreatedEvent @event)
    {
        // Get user email from User Service
        var user = await _userServiceClient.GetUserAsync(@event.UserId);
        
        await _emailSender.SendAsync(
            user.Email,
            "Order Confirmed",
            $"Your order {​@event.OrderId} has been created"
        );
    }

    [EventHandler]
    public async Task HandlePaymentProcessed(PaymentProcessedEvent @event)
    {
        // Send SMS notification
        var user = await _userServiceClient.GetUserAsync(@event.UserId);
        
        var message = @event.Success 
            ? "Payment successful" 
            : "Payment failed";
            
        await _smsSender.SendAsync(user.Phone, message);
    }
}

// ===== API GATEWAY =====
// Single entry point for all microservices

public class ApiGateway
{
    private readonly IServiceRouter _router;

    [HttpPost("api/register")]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        return await _router.Route("user-service", HttpMethod.Post, "/register", request);
    }

    [HttpPost("api/orders")]
    public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
    {
        return await _router.Route("order-service", HttpMethod.Post, "/orders", request);
    }

    [HttpPost("api/payments")]
    public async Task<IActionResult> ProcessPayment(PaymentRequest request)
    {
        return await _router.Route("payment-service", HttpMethod.Post, "/payments", request);
    }
}

// ===== MESSAGE BUS =====
// Async communication between services (RabbitMQ, Kafka)

public interface IMessageBus
{
    Task PublishAsync<T>(T message) where T : class;
    Task SubscribeAsync<T>(Func<T, Task> handler) where T : class;
}

public class RabbitMqMessageBus : IMessageBus
{
    private readonly IConnection _connection;

    public async Task PublishAsync<T>(T message) where T : class
    {
        // Serialize and publish to RabbitMQ
    }

    public async Task SubscribeAsync<T>(Func<T, Task> handler) where T : class
    {
        // Subscribe to messages and call handler
    }
}
```

## Microservices Communication Patterns

### 1. Synchronous: REST/HTTP

```csharp
// Direct service-to-service calls
public class OrderService
{
    private readonly HttpClient _httpClient;

    public async Task<User> GetUser(int userId)
    {
        var response = await _httpClient.GetAsync($"http://user-service/users/{userId}");
        var json = await response.Content.ReadAsStringAsync();
        return JsonConvert.DeserializeObject<User>(json);
    }
}

// Pros: Simple, immediate response
// Cons: Tight coupling, cascading failures, requires service availability
```

### 2. Asynchronous: Message Queue

```csharp
// Async event-driven communication
public class OrderService
{
    private readonly IMessageBus _messageBus;

    public async Task CreateOrder(Order order)
    {
        _repository.Save(order);
        
        // Publish event - other services react
        await _messageBus.PublishAsync(new OrderCreatedEvent 
        { 
            OrderId = order.Id, 
            UserId = order.UserId 
        });
    }
}

public class PaymentService
{
    [EventHandler]
    public async Task HandleOrderCreated(OrderCreatedEvent @event)
    {
        // This runs asynchronously when event is published
        await ProcessPaymentAsync(@event.OrderId);
    }
}

// Pros: Loose coupling, resilient, scales well
// Cons: Eventual consistency, harder to debug
```

### 3. API Gateway Pattern

```csharp
// Single entry point for all services
public class ApiGateway
{
    private readonly Dictionary<string, string> _serviceUrls = new()
    {
        { "user-service", "http://user-service:5000" },
        { "order-service", "http://order-service:5001" },
        { "payment-service", "http://payment-service:5002" },
        { "notification-service", "http://notification-service:5003" }
    };

    [HttpGet("users/{id}")]
    public async Task<IActionResult> GetUser(int id)
    {
        var url = $"{_serviceUrls["user-service"]}/users/{id}";
        return await ProxyRequestAsync(url);
    }

    [HttpPost("orders")]
    public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
    {
        var url = $"{_serviceUrls["order-service"]}/orders";
        return await ProxyRequestAsync(url, request);
    }

    private async Task<IActionResult> ProxyRequestAsync(string url, object body = null)
    {
        using (var client = new HttpClient())
        {
            var response = body == null
                ? await client.GetAsync(url)
                : await client.PostAsJsonAsync(url, body);

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

## Microservices Infrastructure

### Service Discovery

```csharp
// Services register themselves and discover each other
public class ConsulServiceRegistry : IServiceRegistry
{
    private readonly IConsulClient _consul;

    public async Task RegisterService(ServiceRegistration registration)
    {
        await _consul.Agent.ServiceRegister(new AgentServiceRegistration
        {
            ID = registration.ServiceId,
            Name = registration.ServiceName,
            Address = registration.Address,
            Port = registration.Port,
            Check = new AgentServiceCheck
            {
                HTTP = $"http://{registration.Address}:{registration.Port}/health",
                Interval = TimeSpan.FromSeconds(10)
            }
        });
    }

    public async Task<List<ServiceInstance>> DiscoverService(string serviceName)
    {
        var services = await _consul.Health.Service(serviceName);
        return services.Response.Select(s => new ServiceInstance
        {
            ServiceId = s.Service.ID,
            Address = s.Service.Address,
            Port = s.Service.Port
        }).ToList();
    }
}
```

### Health Checks

```csharp
// Each service exposes health endpoint
public class HealthCheckController
{
    private readonly IDatabase _database;
    private readonly IMessageBus _messageBus;

    [HttpGet("health")]
    public ActionResult HealthCheck()
    {
        var checks = new
        {
            Database = _database.IsHealthy() ? "Healthy" : "Unhealthy",
            MessageBus = _messageBus.IsConnected() ? "Healthy" : "Unhealthy",
            Status = "UP"
        };

        return Ok(checks);
    }
}
```

## Benefits of Microservices

✅ **Independent Scaling** - Scale only what needs it  
✅ **Independent Deployment** - Deploy services separately  
✅ **Technology Flexibility** - Each service can use different tech  
✅ **Team Independence** - Teams work on separate services  
✅ **Fault Isolation** - One service failure doesn't crash all  
✅ **Rapid Development** - Faster iteration on individual services  

## Drawbacks

❌ **Complexity** - Distributed systems are inherently complex  
❌ **Network Latency** - Inter-service calls are slower than in-process  
❌ **Consistency** - Eventual consistency instead of strong consistency  
❌ **Debugging** - Hard to debug across services  
❌ **Operational Overhead** - Requires sophisticated infrastructure  
❌ **Data Management** - Each service has separate database  

## Interview Questions

**Q: What is Microservices Architecture?**
A: Approach to decompose application into independently deployable, loosely coupled services. Each service owns data, has own database, and communicates via APIs/messages.

**Q: How do microservices communicate?**
A: Either synchronously via REST/RPC calls or asynchronously via message queues/event buses. Async is preferred for loose coupling.

**Q: What's the relationship between Microservices and DevOps?**
A: Microservices require sophisticated DevOps - containerization, orchestration, service discovery, monitoring. Deployment pipeline must support rapid, independent service deployments.

**Q: When should you NOT use Microservices?**
A: For small applications, simple CRUD operations, tightly coupled domains, teams lacking DevOps expertise, or when consistency is critical.

**Q: How do you handle transactions across microservices?**
A: SAGA pattern - long-running transactions coordinated either via orchestration (central coordinator) or choreography (event-driven).

**Q: What's the difference between Monolith and Microservices?**
A: Monolith is single deployable unit. Microservices are independently deployable services. Monolith simpler initially; microservices better for large, complex systems.

## When to Use Microservices

### ✅ Use When:
- Large, complex applications
- Multiple independent teams
- Need rapid scaling of specific components
- Technology flexibility required
- Fault isolation critical
- High-volume systems

### ❌ Don't Use When:
- Small, simple applications
- Single team
- Tight consistency required
- Limited DevOps expertise
- Network latency critical
- Early-stage startup

## Summary

Microservices Architecture decomposes applications into independently deployable services. While adding significant complexity, it enables rapid scaling, team independence, and fault isolation essential for large distributed systems. Requires sophisticated infrastructure, DevOps practices, and careful handling of distributed system challenges.

**Key Takeaway:** Microservices trade deployment and operational complexity for scalability, team independence, and fault isolation.

---

**Related Patterns:**
- API Gateway - Single entry point for services
- Service Discovery - Services find each other
- Circuit Breaker - Resilience between services
- SAGA Pattern - Distributed transactions
- Event Sourcing - Track changes across services
