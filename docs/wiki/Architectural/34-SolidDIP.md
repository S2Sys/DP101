# SOLID: Dependency Inversion Principle (DIP)

## Overview

**Category:** Architectural Principle  
**Principle:** High-level modules should not depend on low-level modules. Both should depend on abstractions.  
**Complexity:** Medium-Hard  
**Use Case:** Dependency injection, layered architecture, loose coupling

## Problem

High-level business logic depends directly on low-level implementations:
- Changes to implementations break high-level logic
- Hard to swap implementations
- Difficult to test with mocks
- Tight coupling throughout

```csharp
// BAD: High-level depends on low-level concrete implementations
public class PaymentProcessor
{
    // Depends on concrete implementations
    private CreditCardPaymentGateway _creditCardGateway;
    private PayPalPaymentGateway _paypalGateway;
    private SqlOrderRepository _orderRepository;
    private SmtpEmailService _emailService;
    private FileLogger _logger;

    public PaymentProcessor()
    {
        _creditCardGateway = new CreditCardPaymentGateway();
        _paypalGateway = new PayPalPaymentGateway();
        _orderRepository = new SqlOrderRepository("connection_string");
        _emailService = new SmtpEmailService("smtp.gmail.com");
        _logger = new FileLogger("logs.txt");
    }

    public void ProcessOrder(Order order, string paymentType)
    {
        try
        {
            // Direct dependency on concrete classes
            if (paymentType == "CreditCard")
            {
                _creditCardGateway.Charge(order.Total);
            }
            else
            {
                _paypalGateway.Charge(order.Total);
            }

            // Direct dependency on SQL repository
            _orderRepository.Save(order);
            
            // Direct dependency on email service
            _emailService.SendConfirmation(order.Customer.Email);
        }
        catch (Exception ex)
        {
            // Direct dependency on file logger
            _logger.Log(ex.Message);
            throw;
        }
    }
}

// Problems:
// - PaymentProcessor depends on 5 concrete low-level implementations
// - Must create all dependencies in constructor
// - Cannot test without creating real databases, email servers, etc.
// - Changing payment gateway means modifying PaymentProcessor
// - Tightly coupled to implementation details
// - Cannot swap PostgreSQL for SQL Server without changing code
```

## Solution

Depend on abstractions (interfaces), not concrete implementations:

```csharp
// GOOD: Depend on abstractions, not concrete implementations

// Define abstractions (interfaces)
public interface IPaymentGateway
{
    void Charge(decimal amount);
}

public interface IOrderRepository
{
    void Save(Order order);
}

public interface IEmailService
{
    void SendConfirmation(string email);
}

public interface ILogger
{
    void Log(string message);
}

// High-level business logic depends on abstractions
public class PaymentProcessor
{
    private readonly IPaymentGateway _paymentGateway;
    private readonly IOrderRepository _orderRepository;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    // Dependencies injected - not created here
    public PaymentProcessor(
        IPaymentGateway paymentGateway,
        IOrderRepository orderRepository,
        IEmailService emailService,
        ILogger logger)
    {
        _paymentGateway = paymentGateway;
        _orderRepository = orderRepository;
        _emailService = emailService;
        _logger = logger;
    }

    public void ProcessOrder(Order order)
    {
        try
        {
            // Uses abstractions - can work with any implementation
            _paymentGateway.Charge(order.Total);
            _orderRepository.Save(order);
            _emailService.SendConfirmation(order.Customer.Email);
        }
        catch (Exception ex)
        {
            _logger.Log(ex.Message);
            throw;
        }
    }
}

// Low-level modules implement abstractions
public class CreditCardPaymentGateway : IPaymentGateway
{
    public void Charge(decimal amount)
    {
        Console.WriteLine($"Charging credit card: ${amount}");
    }
}

public class PayPalPaymentGateway : IPaymentGateway
{
    public void Charge(decimal amount)
    {
        Console.WriteLine($"Charging PayPal: ${amount}");
    }
}

public class SqlOrderRepository : IOrderRepository
{
    private readonly string _connectionString;

    public SqlOrderRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    public void Save(Order order)
    {
        Console.WriteLine($"Saving order {order.Id} to SQL Server");
    }
}

public class SmtpEmailService : IEmailService
{
    private readonly string _smtpServer;

    public SmtpEmailService(string smtpServer)
    {
        _smtpServer = smtpServer;
    }

    public void SendConfirmation(string email)
    {
        Console.WriteLine($"Sending email to {email}");
    }
}

public class FileLogger : ILogger
{
    private readonly string _logPath;

    public FileLogger(string logPath)
    {
        _logPath = logPath;
    }

    public void Log(string message)
    {
        File.AppendAllText(_logPath, $"{DateTime.Now}: {message}\n");
    }
}

// Easy to test with mock implementations
public class MockPaymentGateway : IPaymentGateway
{
    public void Charge(decimal amount)
    {
        Console.WriteLine($"[MOCK] Charging: ${amount}");
    }
}

public class MockOrderRepository : IOrderRepository
{
    public void Save(Order order)
    {
        Console.WriteLine($"[MOCK] Saving order {order.Id}");
    }
}

public class MockEmailService : IEmailService
{
    public void SendConfirmation(string email)
    {
        Console.WriteLine($"[MOCK] Email to {email}");
    }
}

public class MockLogger : ILogger
{
    public void Log(string message)
    {
        Console.WriteLine($"[MOCK] {message}");
    }
}

// Usage - easy to swap implementations
public class OrderController
{
    public void ProcessOrderTest()
    {
        // For testing - use mocks
        var processor = new PaymentProcessor(
            new MockPaymentGateway(),
            new MockOrderRepository(),
            new MockEmailService(),
            new MockLogger()
        );

        var order = new Order { Id = 1, Total = 99.99m };
        processor.ProcessOrder(order);
    }

    public void ProcessOrderProduction()
    {
        // For production - use real implementations
        var processor = new PaymentProcessor(
            new CreditCardPaymentGateway(),
            new SqlOrderRepository("Server=localhost"),
            new SmtpEmailService("smtp.gmail.com"),
            new FileLogger("logs.txt")
        );

        var order = new Order { Id = 1, Total = 99.99m };
        processor.ProcessOrder(order);
    }
}

// Dependency Injection Container
public class ServiceContainer
{
    public static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Register implementations
        services.AddScoped<IPaymentGateway, CreditCardPaymentGateway>();
        services.AddScoped<IOrderRepository, SqlOrderRepository>();
        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<ILogger, FileLogger>();
        services.AddScoped<PaymentProcessor>();

        return services.BuildServiceProvider();
    }
}
```

## Implementation Approaches

### 1. Constructor Injection

```csharp
// DIP using Constructor Injection
public interface IDatabase
{
    User GetUser(int id);
    void SaveUser(User user);
}

public interface IEmailSender
{
    void Send(string to, string subject);
}

public class UserService
{
    private readonly IDatabase _database;
    private readonly IEmailSender _emailSender;

    // Dependencies injected through constructor
    public UserService(IDatabase database, IEmailSender emailSender)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
    }

    public void UpdateUserEmail(int userId, string newEmail)
    {
        var user = _database.GetUser(userId);
        user.Email = newEmail;
        _database.SaveUser(user);
        _emailSender.Send(newEmail, "Your email has been updated");
    }
}

// Usage
public class Program
{
    public static void Main()
    {
        // Create implementations
        IDatabase database = new SqlDatabase("connection_string");
        IEmailSender emailSender = new SmtpEmailSender();

        // Inject into high-level module
        var userService = new UserService(database, emailSender);

        userService.UpdateUserEmail(1, "newemail@example.com");

        // For testing - inject mocks
        IDatabase mockDb = new MockDatabase();
        IEmailSender mockEmail = new MockEmailSender();
        var testService = new UserService(mockDb, mockEmail);
    }
}
```

### 2. Property Injection

```csharp
// DIP using Property Injection
public class ReportGenerator
{
    public IDataSource DataSource { get; set; }
    public IReportFormatter Formatter { get; set; }
    public ILogger Logger { get; set; }

    public void GenerateReport()
    {
        try
        {
            var data = DataSource.GetData();
            var formatted = Formatter.Format(data);
            Logger.Log("Report generated");
        }
        catch (Exception ex)
        {
            Logger.Log($"Error: {ex.Message}");
        }
    }
}

// Usage
var generator = new ReportGenerator
{
    DataSource = new DatabaseDataSource(),
    Formatter = new PdfReportFormatter(),
    Logger = new ConsoleLogger()
};

generator.GenerateReport();
```

### 3. Method Injection

```csharp
// DIP using Method Injection
public class PaymentService
{
    public void ProcessPayment(
        decimal amount,
        IPaymentGateway gateway,  // Injected as parameter
        ILogger logger)           // Injected as parameter
    {
        try
        {
            gateway.Charge(amount);
            logger.Log($"Payment processed: ${amount}");
        }
        catch (Exception ex)
        {
            logger.Log($"Payment failed: {ex.Message}");
            throw;
        }
    }
}

// Usage
var service = new PaymentService();
service.ProcessPayment(
    100m,
    new StripePaymentGateway(),
    new ConsoleLogger()
);
```

### 4. Service Locator Pattern (Anti-pattern but sometimes used)

```csharp
// DIP using Service Locator (NOT recommended)
public class ServiceLocator
{
    private static Dictionary<Type, object> _services = new();

    public static void Register<TInterface, TImplementation>(TImplementation implementation)
        where TImplementation : TInterface
    {
        _services[typeof(TInterface)] = implementation;
    }

    public static T Resolve<T>() where T : class
    {
        return _services[typeof(T)] as T;
    }
}

// Usage
ServiceLocator.Register<IDatabase, SqlDatabase>(new SqlDatabase("..."));
ServiceLocator.Register<IEmailSender, SmtpEmailSender>(new SmtpEmailSender());

var database = ServiceLocator.Resolve<IDatabase>();
var emailSender = ServiceLocator.Resolve<IEmailSender>();

// WARNING: Service Locator hides dependencies and makes testing harder
// Constructor Injection is preferred!
```

### 5. Dependency Injection Container

```csharp
// DIP with IoC Container (Recommended)
public class Startup
{
    public IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Register abstractions to implementations
        services.AddScoped<IDatabase, SqlDatabase>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<ILogger, FileLogger>();

        // Register high-level services
        services.AddScoped<UserService>();
        services.AddScoped<OrderService>();
        services.AddScoped<PaymentService>();

        return services.BuildServiceProvider();
    }
}

// Usage
var serviceProvider = new Startup().ConfigureServices();

var userService = serviceProvider.GetRequiredService<UserService>();
var orderService = serviceProvider.GetRequiredService<OrderService>();
var paymentService = serviceProvider.GetRequiredService<PaymentService>();

userService.UpdateUserEmail(1, "new@example.com");
```

## Benefits of DIP

✅ **Loose Coupling** - Depend on abstractions, not implementations  
✅ **Easy Testing** - Inject mocks instead of real implementations  
✅ **Flexibility** - Swap implementations without changing code  
✅ **Reusability** - High-level logic works with any compatible implementation  
✅ **Maintainability** - Changes to implementations don't affect high-level logic  
✅ **Scalability** - Easy to add new implementations  

## Drawbacks

❌ **Complexity** - Requires abstraction layer  
❌ **Learning Curve** - DI concepts take time to understand  
❌ **Over-Engineering** - Can be overkill for simple projects  
❌ **Configuration** - DI container setup adds code  

## Interview Questions

**Q: What is Dependency Inversion Principle?**
A: High-level modules should not depend on low-level modules. Both should depend on abstractions. This inverts traditional dependency direction where high-level depends on low-level.

**Q: What's the difference between Dependency Injection and Dependency Inversion?**
A: Dependency Inversion is a design principle (depend on abstractions). Dependency Injection is a technique to implement it (inject dependencies rather than creating them).

**Q: What are the three types of dependency injection?**
A: Constructor injection (through constructor parameters), Property injection (through properties), and Method injection (through method parameters).

**Q: Is Service Locator pattern the same as Dependency Injection?**
A: No. Service Locator hides dependencies making them implicit. DI makes dependencies explicit through constructor/property parameters. DI is preferred.

**Q: How does DIP enable testing?**
A: With DIP, you inject mock implementations instead of real ones. Without DIP, you're forced to use real implementations making testing difficult.

**Q: What's the most common way to implement DIP?**
A: Constructor injection with a Dependency Injection container (like Microsoft.Extensions.DependencyInjection, Autofac, or Ninject).

## When to Apply DIP

### ✅ Apply When:
- Building layered architectures
- Multiple implementations of same interface needed
- Testing is important
- Code needs flexibility to swap implementations
- Using Dependency Injection container
- Building enterprise applications

### ❌ Don't Apply When:
- Simple, single-implementation scenarios
- Adding unnecessary abstraction
- Overkill for small utilities
- No need for testing

## Code Smell: Hard Dependencies

```csharp
// ANTI-PATTERN: Hard dependencies on concrete classes
public class OrderService
{
    public void ProcessOrder(Order order)
    {
        // Creates database directly - cannot test!
        var db = new SqlDatabase("connection_string");
        db.Save(order);

        // Creates email service directly - cannot mock!
        var emailService = new SmtpEmailService();
        emailService.SendConfirmation(order.Customer.Email);

        // Creates logger directly
        var logger = new FileLogger("logs.txt");
        logger.Log("Order processed");
    }
}

// SOLUTION: Inject dependencies
public class OrderService
{
    private readonly IDatabase _database;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;

    public OrderService(IDatabase database, IEmailService emailService, ILogger logger)
    {
        _database = database;
        _emailService = emailService;
        _logger = logger;
    }

    public void ProcessOrder(Order order)
    {
        _database.Save(order);
        _emailService.SendConfirmation(order.Customer.Email);
        _logger.Log("Order processed");
    }
}
```

## Real-World Patterns Using DIP

**Repository Pattern** - Abstracts data access  
**Decorator Pattern** - Decorates abstracted services  
**Factory Pattern** - Creates implementations based on abstraction  
**Strategy Pattern** - Strategies implement common abstraction  
**Observer Pattern** - Depends on IObserver abstraction  

## Summary

Dependency Inversion Principle requires that both high-level and low-level modules depend on abstractions, not concrete implementations. This is typically implemented through Dependency Injection where dependencies are injected rather than created. DIP is fundamental to building testable, maintainable, and flexible software architectures.

**Key Takeaway:** Depend on interfaces/abstractions, inject dependencies. Never create dependencies directly in high-level code.

---

**Related SOLID Principles:**
- Single Responsibility Principle - Each class has one reason to change
- Open/Closed Principle - Classes open for extension, closed for modification
- Liskov Substitution Principle - Subtypes must be substitutable
- Interface Segregation Principle - Clients shouldn't depend on unused methods
