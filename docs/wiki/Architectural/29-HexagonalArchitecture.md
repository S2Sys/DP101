# Hexagonal Architecture (Ports & Adapters)

## Overview

**Category:** Architectural Pattern  
**Purpose:** Isolate application core from external systems through abstract ports and concrete adapters.  
**Also Called:** Ports & Adapters  
**Complexity:** High  
**Key Concept:** Invert dependencies - external systems depend on application, not vice versa

## Problem

Application tightly coupled to external systems:

```csharp
// BAD: Direct dependencies on external systems
public class OrderService
{
    private SqlDatabase _database;      // Tightly coupled
    private EmailProvider _email;       // Tightly coupled
    private PaymentGateway _payment;    // Tightly coupled

    public void ProcessOrder(Order order)
    {
        _database.SaveOrder(order);     // Can't test without real DB
        _email.Send(order.CustomerEmail);  // Can't test without real email
        _payment.Charge(order.Amount);     // Can't test without real payment
    }
}

// Testing is impossible without external systems!
```

## Solution

Define ports (interfaces) that application exposes and requires:

```
┌──────────────────────────────────────────────┐
│                 APPLICATION                  │
│                                              │
│  ┌──────────────────────────────────────┐  │
│  │  Order Service (Core Business Logic) │  │
│  └──────────────────────────────────────┘  │
│                                              │
│  Ports (Interfaces):                        │
│  ┌─────────────┐  ┌─────────────┐         │
│  │ IOrderStore │  │ IMailSender │         │
│  └─────────────┘  └─────────────┘         │
└──────────────────────────────────────────────┘
    │                        │
    │ Adapters               │
    ▼                        ▼
┌──────────────┐      ┌─────────────────┐
│ SqlAdapter   │      │ SmtpAdapter     │
└──────────────┘      └─────────────────┘
    │                        │
    ▼                        ▼
┌──────────────┐      ┌─────────────────┐
│ SQL Database │      │ SMTP Server     │
└──────────────┘      └─────────────────┘
```

## Implementation

### 1. Core Application (No External Dependencies)

```csharp
// Core domain - doesn't know about external systems
public class Order
{
    public int Id { get; set; }
    public string CustomerEmail { get; set; }
    public decimal Amount { get; set; }
}

// Port (Interface) - application exposes
public interface IOrderStore
{
    void Save(Order order);
    Order GetById(int id);
}

public interface IMailSender
{
    void SendEmail(string to, string subject, string body);
}

public interface IPaymentProcessor
{
    bool ProcessPayment(decimal amount, string cardToken);
}

// Core Service - depends on ports, not implementations
public class OrderService
{
    private readonly IOrderStore _orderStore;
    private readonly IMailSender _mailSender;
    private readonly IPaymentProcessor _paymentProcessor;

    public OrderService(
        IOrderStore orderStore,
        IMailSender mailSender,
        IPaymentProcessor paymentProcessor)
    {
        _orderStore = orderStore;
        _mailSender = mailSender;
        _paymentProcessor = paymentProcessor;
    }

    public void ProcessOrder(Order order, string cardToken)
    {
        if (!_paymentProcessor.ProcessPayment(order.Amount, cardToken))
            throw new PaymentFailedException();

        _orderStore.Save(order);
        _mailSender.SendEmail(order.CustomerEmail, "Order Confirmed", "...");
    }
}
```

### 2. Adapters (Implementations)

```csharp
// Database Adapter
public class SqlOrderStoreAdapter : IOrderStore
{
    private readonly string _connectionString;

    public void Save(Order order)
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            conn.Execute("INSERT INTO Orders...", order);
        }
    }

    public Order GetById(int id)
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            return conn.QuerySingle<Order>("SELECT * FROM Orders WHERE Id = @id", new { id });
        }
    }
}

// Email Adapter
public class SmtpMailSenderAdapter : IMailSender
{
    private readonly string _smtpServer;
    private readonly int _smtpPort;

    public void SendEmail(string to, string subject, string body)
    {
        using (var client = new SmtpClient(_smtpServer, _smtpPort))
        {
            var message = new MailMessage("noreply@company.com", to)
            {
                Subject = subject,
                Body = body
            };
            client.Send(message);
        }
    }
}

// Payment Adapter
public class StripePaymentAdapter : IPaymentProcessor
{
    private readonly string _stripeApiKey;

    public bool ProcessPayment(decimal amount, string cardToken)
    {
        try
        {
            var service = new ChargeService();
            var options = new ChargeCreateOptions
            {
                Amount = (long)(amount * 100),  // Convert to cents
                Currency = "usd",
                Source = cardToken
            };
            var charge = service.Create(options);
            return charge.Paid;
        }
        catch
        {
            return false;
        }
    }
}

// Mock Adapters for Testing
public class InMemoryOrderStoreAdapter : IOrderStore
{
    private Dictionary<int, Order> _orders = new();

    public void Save(Order order) => _orders[order.Id] = order;
    public Order GetById(int id) => _orders.GetValueOrDefault(id);
}

public class MockMailSenderAdapter : IMailSender
{
    public List<EmailMessage> SentEmails { get; } = new();

    public void SendEmail(string to, string subject, string body)
    {
        SentEmails.Add(new EmailMessage { To = to, Subject = subject, Body = body });
    }
}

public class MockPaymentProcessorAdapter : IPaymentProcessor
{
    public bool WillSucceed { get; set; } = true;
    public bool ProcessPayment(decimal amount, string cardToken) => WillSucceed;
}
```

### 3. Dependency Injection Setup

```csharp
// Production setup
public void ConfigureServicesProduction(IServiceCollection services)
{
    services.AddScoped<IOrderStore, SqlOrderStoreAdapter>();
    services.AddScoped<IMailSender, SmtpMailSenderAdapter>();
    services.AddScoped<IPaymentProcessor, StripePaymentAdapter>();
    services.AddScoped<OrderService>();
}

// Testing setup
public class OrderServiceTests
{
    private OrderService _service;
    private InMemoryOrderStoreAdapter _orderStore;
    private MockMailSenderAdapter _mailSender;
    private MockPaymentProcessorAdapter _paymentProcessor;

    [SetUp]
    public void Setup()
    {
        _orderStore = new InMemoryOrderStoreAdapter();
        _mailSender = new MockMailSenderAdapter();
        _paymentProcessor = new MockPaymentProcessorAdapter();

        _service = new OrderService(_orderStore, _mailSender, _paymentProcessor);
    }

    [Test]
    public void ProcessOrder_ValidPayment_SavesAndSendsEmail()
    {
        // Arrange
        var order = new Order { Id = 1, CustomerEmail = "test@example.com", Amount = 100m };
        _paymentProcessor.WillSucceed = true;

        // Act
        _service.ProcessOrder(order, "token");

        // Assert
        Assert.IsNotNull(_orderStore.GetById(1));
        Assert.AreEqual(1, _mailSender.SentEmails.Count);
        Assert.AreEqual("test@example.com", _mailSender.SentEmails[0].To);
    }

    [Test]
    public void ProcessOrder_PaymentFails_ThrowsException()
    {
        // Arrange
        var order = new Order { Id = 1, CustomerEmail = "test@example.com", Amount = 100m };
        _paymentProcessor.WillSucceed = false;

        // Act & Assert
        Assert.Throws<PaymentFailedException>(() =>
            _service.ProcessOrder(order, "token")
        );
    }
}
```

### 4. API Controller (Driving Adapter)

```csharp
// Primary Adapter - HTTP requests driving the application
[ApiController]
[Route("api/[controller]")]
public class OrderController
{
    private readonly OrderService _orderService;

    public OrderController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public ActionResult<OrderResponse> CreateOrder(CreateOrderRequest request)
    {
        var order = new Order
        {
            CustomerEmail = request.Email,
            Amount = request.Amount
        };

        _orderService.ProcessOrder(order, request.CardToken);

        return Ok(new OrderResponse { OrderId = order.Id });
    }
}
```

## Ports vs Adapters

| Part | Role | Example |
|------|------|---------|
| **Port** | Interface defined by application | IOrderStore, IMailSender |
| **Adapter** | Concrete implementation of port | SqlOrderStoreAdapter, SmtpMailSenderAdapter |
| **Driving** | External system calls application | HTTP controller, Console app |
| **Driven** | Application calls external system | Database, Email, Payment gateway |

## Architecture Layers

```
┌────────────────────────────────────────────┐
│       Driving Adapters (Primary)           │
│  (HTTP API, CLI, Message Queue Listener)   │
└────────────────────────────────────────────┘
         │
         ▼ (Driving)
┌────────────────────────────────────────────┐
│      APPLICATION CORE (Hexagon)            │
│   - Business Logic                         │
│   - Domain Models                          │
│   - Ports (Interfaces)                     │
└────────────────────────────────────────────┘
         │ (Driven)
         ▼
┌────────────────────────────────────────────┐
│      Driven Adapters (Secondary)           │
│  (Database, Email, Payment, Cache)         │
└────────────────────────────────────────────┘
```

## Advantages

✅ **Testability** - Mock adapters for testing  
✅ **Independence** - Application independent of external systems  
✅ **Flexibility** - Swap implementations easily  
✅ **Clarity** - Clear separation of concerns  
✅ **Framework Agnostic** - Change HTTP to CLI or message queue  

## When to Use Hexagonal Architecture

### ✅ Use When:
- Complex business logic to protect
- Multiple external systems needed
- Testing critical
- Potential to change external systems
- Multiple interfaces (API, CLI, Message queue)

### ❌ Don't Use When:
- Simple CRUD applications
- Single external system
- No testing requirements
- Rapid prototyping

## Summary

Hexagonal Architecture isolates application core through ports and adapters, making it independent of external systems. Perfect for testable, flexible applications where business logic is complex and external systems may change.

**Key Takeaway:** Hexagonal Architecture inverts dependencies so external systems depend on application through abstract ports.
