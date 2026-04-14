# Clean Architecture

## Overview

**Category:** Architectural Pattern  
**Purpose:** Organize code into independent, testable layers with clear dependencies flowing inward.  
**Also Called:** Onion Architecture, Hexagonal Architecture  
**Complexity:** High  
**Core Principle:** Dependency Inversion - dependencies point inward toward core business logic

## Problem

Spaghetti code with tangled dependencies:

```csharp
// BAD: Tangled dependencies
public class OrderService
{
    private SqlConnection _conn;
    private HttpClient _client;
    private FileStream _log;

    public void PlaceOrder(OrderDto dto)  // UI layer type
    {
        // Database access mixed in
        _conn.Open();
        _conn.ExecuteNonQuery("INSERT INTO Orders...");

        // HTTP call mixed in
        _client.PostAsync("https://payment.api/...");

        // File logging mixed in
        _log.WriteLine("Order placed");
    }
}

// Dependencies point outward - hard to test!
```

## Solution

Organize into concentric layers with inward dependencies:

```
┌─────────────────────────────────────────┐
│         Presentation Layer              │  (UI, Controllers)
│    (Web, API, Console, Desktop)         │
└─────────────────────────────────────────┘
           ↓ depends on
┌─────────────────────────────────────────┐
│      Application Services Layer         │  (Use cases, DTOs)
└─────────────────────────────────────────┘
           ↓ depends on
┌─────────────────────────────────────────┐
│        Domain/Business Logic Layer      │  (Entities, Rules)
└─────────────────────────────────────────┘
           ↓ depends on (inverted)
┌─────────────────────────────────────────┐
│     Infrastructure & Frameworks         │  (Database, HTTP, Logging)
└─────────────────────────────────────────┘
```

## Architecture Layers

### 1. Core Domain Layer (No dependencies)
```csharp
// Domain entities - core business logic
public class Order
{
    public int Id { get; set; }
    public string OrderNumber { get; set; }
    public List<OrderItem> Items { get; set; }
    public decimal TotalPrice { get; private set; }

    // Business rules - pure logic, no side effects
    public void AddItem(Product product, int quantity)
    {
        if (quantity <= 0)
            throw new InvalidOperationException("Invalid quantity");

        Items.Add(new OrderItem { Product = product, Quantity = quantity });
        RecalculateTotal();
    }

    private void RecalculateTotal()
    {
        TotalPrice = Items.Sum(i => i.Product.Price * i.Quantity);
    }
}
```

### 2. Application Layer (Depends on Domain)
```csharp
// Application services - use cases
public interface ICreateOrderService
{
    OrderDto CreateOrder(CreateOrderRequest request);
}

public class CreateOrderService : ICreateOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;

    public CreateOrderService(IOrderRepository orderRepo, IProductRepository productRepo)
    {
        _orderRepository = orderRepo;
        _productRepository = productRepo;
    }

    public OrderDto CreateOrder(CreateOrderRequest request)
    {
        // Application logic - orchestrates domain logic
        var order = new Order();

        foreach (var item in request.Items)
        {
            var product = _productRepository.GetById(item.ProductId);
            order.AddItem(product, item.Quantity);  // Domain logic
        }

        _orderRepository.Save(order);

        return new OrderDto
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            TotalPrice = order.TotalPrice
        };
    }
}
```

### 3. Infrastructure Layer (Implements interfaces)
```csharp
// Database access
public class EfOrderRepository : IOrderRepository
{
    private readonly ApplicationDbContext _context;

    public void Save(Order order)
    {
        _context.Orders.Add(order);
        _context.SaveChanges();
    }

    public Order GetById(int id) => _context.Orders.Find(id);
}

// HTTP calls
public class PaymentGatewayAdapter : IPaymentService
{
    private readonly HttpClient _client;

    public async Task<PaymentResult> ProcessPayment(Order order)
    {
        var response = await _client.PostAsync("/api/payment", ...);
        return new PaymentResult { Success = response.IsSuccessStatusCode };
    }
}

// Logging
public class FileLogger : ILogger
{
    public void Log(string message)
    {
        File.AppendAllText("log.txt", message);
    }
}
```

### 4. Presentation Layer (Depends on interfaces)
```csharp
// ASP.NET Controller
[ApiController]
[Route("api/[controller]")]
public class OrderController
{
    private readonly ICreateOrderService _createOrderService;

    public OrderController(ICreateOrderService createOrderService)
    {
        _createOrderService = createOrderService;
    }

    [HttpPost]
    public ActionResult CreateOrder(CreateOrderRequest request)
    {
        var orderDto = _createOrderService.CreateOrder(request);
        return Ok(orderDto);
    }
}
```

## Dependency Inversion

Core principle: **Dependencies point inward**

```csharp
// GOOD: Domain doesn't know about infrastructure
public class Order  // Core domain
{
    // No dependencies on database, HTTP, logging, etc.
    public void AddItem(Product product, int quantity) { }
}

// Infrastructure depends on domain interfaces
public interface IOrderRepository
{
    void Save(Order order);  // Domain type
}

public class EfOrderRepository : IOrderRepository
{
    public void Save(Order order)  // Implementation details
    {
        _context.Orders.Add(order);
    }
}

// Application layer bridges
public class CreateOrderService
{
    private readonly IOrderRepository _repo;  // Depends on abstraction

    public void CreateOrder(CreateOrderRequest request)
    {
        var order = new Order();  // Create domain entity
        _repo.Save(order);  // Save through interface
    }
}

// Presentation depends on application layer
public class OrderController
{
    private readonly ICreateOrderService _service;  // Depends on abstraction

    public ActionResult CreateOrder(CreateOrderRequest request)
    {
        return Ok(_service.CreateOrder(request));  // Use through interface
    }
}
```

## Project Structure

```
Solution/
├── Domain/                              (Core business logic)
│   ├── Entities/
│   │   ├── Order.cs
│   │   ├── Product.cs
│   ├── Interfaces/                      (Abstractions)
│   │   ├── IOrderRepository.cs
│   │   ├── IPaymentService.cs
│   └── ValueObjects/
│
├── Application/                         (Use cases)
│   ├── Services/
│   │   ├── CreateOrderService.cs
│   │   ├── GetOrderService.cs
│   ├── Dtos/
│   │   ├── OrderDto.cs
│   └── Interfaces/
│       └── ICreateOrderService.cs
│
├── Infrastructure/                      (Implementations)
│   ├── Persistence/
│   │   ├── EfOrderRepository.cs
│   │   ├── ApplicationDbContext.cs
│   ├── ExternalServices/
│   │   ├── PaymentGatewayAdapter.cs
│   └── Logging/
│       └── FileLogger.cs
│
└── Presentation/                        (UI)
    ├── Controllers/
    │   └── OrderController.cs
    ├── Startup.cs                       (DI setup)
    └── Program.cs
```

## Dependency Injection Setup

```csharp
// Startup.cs - Wire dependencies
public void ConfigureServices(IServiceCollection services)
{
    // Domain layer - no registration needed (no external deps)

    // Application layer
    services.AddScoped<ICreateOrderService, CreateOrderService>();
    services.AddScoped<IGetOrderService, GetOrderService>();

    // Infrastructure layer
    services.AddScoped<IOrderRepository, EfOrderRepository>();
    services.AddScoped<IPaymentService, PaymentGatewayAdapter>();
    services.AddScoped<ILogger, FileLogger>();

    // DbContext
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer("connection_string")
    );
}
```

## Pros and Cons

### Advantages
✅ **Testability** - Pure domain logic with no dependencies  
✅ **Flexibility** - Swap implementations easily  
✅ **Independence** - Layers independent of frameworks  
✅ **Maintainability** - Clear separation of concerns  
✅ **Scalability** - Easy to extend without breaking core  

### Disadvantages
❌ **Complexity** - Lots of abstractions and layers  
❌ **Overkill** - For small apps, simple MVC sufficient  
❌ **Learning Curve** - Requires understanding architecture  
❌ **Performance** - Extra indirection adds overhead  

## When to Use Clean Architecture

### ✅ Use When:
- Complex business logic
- Long-lived applications
- Multiple delivery mechanisms needed (API, UI, Mobile)
- Team scalability important
- Testing critical
- Framework independence desired

### ❌ Don't Use When:
- Simple CRUD applications
- Small projects
- Rapid prototyping
- Single simple use case

## Summary

Clean Architecture organizes code into independent layers with clear dependency directions pointing inward toward core business logic. Domain layer has zero dependencies. Perfect for complex applications where testability and maintainability critical.

**Key Takeaway:** Clean Architecture isolates business logic from infrastructure with dependencies pointing inward.
