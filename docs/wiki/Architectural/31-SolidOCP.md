# SOLID: Open/Closed Principle (OCP)

## Overview

**Category:** Architectural Principle  
**Principle:** Software entities should be open for extension but closed for modification.  
**Complexity:** Medium  
**Use Case:** Design patterns, framework design, plugin systems

## Problem

When code must be modified for each new requirement:
- Risk of breaking existing functionality
- Requires recompiling and redeploying
- Violates stable interface contracts
- Difficult to extend for new scenarios

```csharp
// BAD: Closed for extension, modifications break existing code
public class PaymentProcessor
{
    public decimal ProcessPayment(string paymentType, decimal amount)
    {
        decimal fee = 0;

        // Must modify this method for each new payment type
        if (paymentType == "CreditCard")
        {
            fee = amount * 0.03m;  // 3% fee
        }
        else if (paymentType == "PayPal")
        {
            fee = amount * 0.04m;  // 4% fee
        }
        else if (paymentType == "ApplePay")
        {
            fee = amount * 0.02m;  // 2% fee
        }
        else if (paymentType == "Bitcoin")
        {
            fee = amount * 0.01m;  // 1% fee
        }
        // Adding new payment type requires modifying this class!

        return amount + fee;
    }
}

// Problems:
// - Must modify PaymentProcessor for each new payment type
// - Risk of breaking existing functionality
// - Requires recompiling entire assembly
// - Hard to test new types without modifying existing tests
// - Violates Open/Closed Principle
```

## Solution

Use abstraction to allow extension without modification:

```csharp
// GOOD: Open for extension, closed for modification

// Abstract payment processor
public interface IPaymentMethod
{
    decimal CalculateFee(decimal amount);
    void ProcessPayment(decimal amount);
}

// Specific implementations for each payment type
public class CreditCardProcessor : IPaymentMethod
{
    public decimal CalculateFee(decimal amount) => amount * 0.03m;

    public void ProcessPayment(decimal amount)
    {
        Console.WriteLine($"Processing credit card payment: ${amount}");
    }
}

public class PayPalProcessor : IPaymentMethod
{
    public decimal CalculateFee(decimal amount) => amount * 0.04m;

    public void ProcessPayment(decimal amount)
    {
        Console.WriteLine($"Processing PayPal payment: ${amount}");
    }
}

public class ApplePayProcessor : IPaymentMethod
{
    public decimal CalculateFee(decimal amount) => amount * 0.02m;

    public void ProcessPayment(decimal amount)
    {
        Console.WriteLine($"Processing Apple Pay payment: ${amount}");
    }
}

// Adding new payment type - NO MODIFICATION needed!
public class BitcoinProcessor : IPaymentMethod
{
    public decimal CalculateFee(decimal amount) => amount * 0.01m;

    public void ProcessPayment(decimal amount)
    {
        Console.WriteLine($"Processing Bitcoin payment: ${amount}");
    }
}

// Main processor - closed for modification
public class PaymentProcessor
{
    private readonly IPaymentMethod _paymentMethod;

    public PaymentProcessor(IPaymentMethod paymentMethod)
    {
        _paymentMethod = paymentMethod;
    }

    public decimal ProcessPayment(decimal amount)
    {
        var fee = _paymentMethod.CalculateFee(amount);
        _paymentMethod.ProcessPayment(amount + fee);
        return amount + fee;
    }
}

// Usage - easy to add new payment types
var processor = new PaymentProcessor(new CreditCardProcessor());
processor.ProcessPayment(100m);

// NEW payment type - just add new class, no modifications needed!
var bitcoinProcessor = new PaymentProcessor(new BitcoinProcessor());
bitcoinProcessor.ProcessPayment(100m);
```

## Implementation Approaches

### 1. Strategy Pattern

```csharp
// OCP using Strategy Pattern
public interface IDiscountStrategy
{
    decimal ApplyDiscount(decimal originalPrice);
}

public class NoDiscountStrategy : IDiscountStrategy
{
    public decimal ApplyDiscount(decimal originalPrice) => originalPrice;
}

public class PercentageDiscountStrategy : IDiscountStrategy
{
    private readonly decimal _percentage;

    public PercentageDiscountStrategy(decimal percentage)
    {
        _percentage = percentage;
    }

    public decimal ApplyDiscount(decimal originalPrice)
    {
        return originalPrice * (1 - _percentage / 100);
    }
}

public class FixedAmountDiscountStrategy : IDiscountStrategy
{
    private readonly decimal _amount;

    public FixedAmountDiscountStrategy(decimal amount)
    {
        _amount = amount;
    }

    public decimal ApplyDiscount(decimal originalPrice)
    {
        return Math.Max(0, originalPrice - _amount);
    }
}

// NEW: Time-based discount - no modifications to existing code!
public class SeasonalDiscountStrategy : IDiscountStrategy
{
    public decimal ApplyDiscount(decimal originalPrice)
    {
        var season = GetCurrentSeason();
        return season == "Winter" ? originalPrice * 0.8m : originalPrice;
    }

    private string GetCurrentSeason()
    {
        var month = DateTime.Now.Month;
        if (month >= 12 || month <= 2) return "Winter";
        if (month >= 3 && month <= 5) return "Spring";
        if (month >= 6 && month <= 8) return "Summer";
        return "Fall";
    }
}

public class PricingEngine
{
    private readonly IDiscountStrategy _discountStrategy;

    public PricingEngine(IDiscountStrategy discountStrategy)
    {
        _discountStrategy = discountStrategy;
    }

    public decimal CalculatePrice(decimal originalPrice)
    {
        return _discountStrategy.ApplyDiscount(originalPrice);
    }
}

// Usage
var engine = new PricingEngine(new PercentageDiscountStrategy(10));
var price = engine.CalculatePrice(100);  // $90

// NEW discount type - just add new class!
var seasonalEngine = new PricingEngine(new SeasonalDiscountStrategy());
var seasonalPrice = seasonalEngine.CalculatePrice(100);
```

### 2. Template Method Pattern

```csharp
// OCP using Template Method
public abstract class ReportGenerator
{
    // Template method - open for extension through abstract methods
    public final void Generate(List<Employee> employees)
    {
        var data = GatherData(employees);
        var formatted = FormatData(data);
        var output = BuildOutput(formatted);
        SaveReport(output);
    }

    protected abstract List<string> GatherData(List<Employee> employees);
    protected abstract string FormatData(List<string> data);
    protected abstract string BuildOutput(string formatted);
    protected abstract void SaveReport(string output);
}

public class CsvReportGenerator : ReportGenerator
{
    protected override List<string> GatherData(List<Employee> employees)
    {
        return employees.Select(e => $"{e.Name},{e.Department},{e.Salary}").ToList();
    }

    protected override string FormatData(List<string> data)
    {
        return string.Join("\n", data);
    }

    protected override string BuildOutput(string formatted)
    {
        return $"NAME,DEPARTMENT,SALARY\n{formatted}";
    }

    protected override void SaveReport(string output)
    {
        File.WriteAllText("report.csv", output);
    }
}

public class JsonReportGenerator : ReportGenerator
{
    protected override List<string> GatherData(List<Employee> employees)
    {
        return employees.Select(e => JsonConvert.SerializeObject(e)).ToList();
    }

    protected override string FormatData(List<string> data)
    {
        return string.Join(",", data);
    }

    protected override string BuildOutput(string formatted)
    {
        return $"[{formatted}]";
    }

    protected override void SaveReport(string output)
    {
        File.WriteAllText("report.json", output);
    }
}

// NEW report type - extends abstract class, no modifications
public class HtmlReportGenerator : ReportGenerator
{
    protected override List<string> GatherData(List<Employee> employees)
    {
        return employees.Select(e => 
            $"<tr><td>{e.Name}</td><td>{e.Department}</td><td>${e.Salary}</td></tr>"
        ).ToList();
    }

    protected override string FormatData(List<string> data)
    {
        return string.Join("\n", data);
    }

    protected override string BuildOutput(string formatted)
    {
        return $"<table><thead><tr><th>Name</th><th>Department</th><th>Salary</th></thead><tbody>{formatted}</tbody></table>";
    }

    protected override void SaveReport(string output)
    {
        File.WriteAllText("report.html", $"<html><body>{output}</body></html>");
    }
}
```

### 3. Decorator Pattern

```csharp
// OCP using Decorator Pattern
public interface IDataProcessor
{
    string Process(string data);
}

public class BaseDataProcessor : IDataProcessor
{
    public string Process(string data)
    {
        Console.WriteLine("Processing data...");
        return data;
    }
}

// Decorators add functionality without modifying base class
public class EncryptionDecorator : IDataProcessor
{
    private readonly IDataProcessor _innerProcessor;

    public EncryptionDecorator(IDataProcessor innerProcessor)
    {
        _innerProcessor = innerProcessor;
    }

    public string Process(string data)
    {
        var processed = _innerProcessor.Process(data);
        var encrypted = EncryptData(processed);
        Console.WriteLine("Encryption applied");
        return encrypted;
    }

    private string EncryptData(string data)
    {
        // Encryption logic
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(data));
    }
}

public class CompressionDecorator : IDataProcessor
{
    private readonly IDataProcessor _innerProcessor;

    public CompressionDecorator(IDataProcessor innerProcessor)
    {
        _innerProcessor = innerProcessor;
    }

    public string Process(string data)
    {
        var processed = _innerProcessor.Process(data);
        var compressed = CompressData(processed);
        Console.WriteLine("Compression applied");
        return compressed;
    }

    private string CompressData(string data)
    {
        // Compression logic
        return data.Length.ToString();  // Simplified
    }
}

public class LoggingDecorator : IDataProcessor
{
    private readonly IDataProcessor _innerProcessor;

    public LoggingDecorator(IDataProcessor innerProcessor)
    {
        _innerProcessor = innerProcessor;
    }

    public string Process(string data)
    {
        Console.WriteLine($"Processing: {data.Substring(0, Math.Min(20, data.Length))}...");
        var result = _innerProcessor.Process(data);
        Console.WriteLine($"Result length: {result.Length}");
        return result;
    }
}

// Usage - compose functionality without modification
var processor = new LoggingDecorator(
    new CompressionDecorator(
        new EncryptionDecorator(
            new BaseDataProcessor()
        )
    )
);

var result = processor.Process("Sensitive data here");
// Output:
// Processing: Sensitive data here...
// Processing data...
// Encryption applied
// Compression applied
// Result length: 28
```

### 4. Observer Pattern

```csharp
// OCP using Observer Pattern
public interface IOrderEventListener
{
    void OnOrderCreated(Order order);
}

public class Order
{
    public int Id { get; set; }
    public string CustomerName { get; set; }
    public decimal Total { get; set; }

    private List<IOrderEventListener> _listeners = new();

    public void Subscribe(IOrderEventListener listener)
    {
        _listeners.Add(listener);
    }

    public void Create()
    {
        Console.WriteLine("Order created");
        
        // Notify all listeners - system is open for extension
        foreach (var listener in _listeners)
        {
            listener.OnOrderCreated(this);
        }
    }
}

// Existing listener
public class EmailNotificationListener : IOrderEventListener
{
    public void OnOrderCreated(Order order)
    {
        Console.WriteLine($"Sending email to {order.CustomerName}");
    }
}

public class InventoryListener : IOrderEventListener
{
    public void OnOrderCreated(Order order)
    {
        Console.WriteLine($"Updating inventory for order {order.Id}");
    }
}

// NEW listener - no modification to existing code!
public class RewardPointsListener : IOrderEventListener
{
    public void OnOrderCreated(Order order)
    {
        var points = (int)(order.Total * 10);
        Console.WriteLine($"Awarding {points} reward points");
    }
}

public class AnalyticsListener : IOrderEventListener
{
    public void OnOrderCreated(Order order)
    {
        Console.WriteLine($"Recording order in analytics: ${order.Total}");
    }
}

// Usage
var order = new Order { Id = 1, CustomerName = "John", Total = 99.99m };
order.Subscribe(new EmailNotificationListener());
order.Subscribe(new InventoryListener());
order.Subscribe(new RewardPointsListener());  // NEW - no modifications!
order.Subscribe(new AnalyticsListener());     // NEW - no modifications!

order.Create();
```

## Benefits of OCP

✅ **Extensibility** - Add new functionality without modification  
✅ **Stability** - Existing code remains stable and tested  
✅ **Reduced Risk** - No risk of breaking existing functionality  
✅ **Reusability** - Abstractions can be reused for multiple extensions  
✅ **Testability** - New implementations can be tested in isolation  
✅ **Maintainability** - Changes isolated to new classes  

## Drawbacks

❌ **Complexity** - Requires abstraction layers  
❌ **Over-Engineering** - Can create unnecessary abstractions  
❌ **Performance** - Abstraction adds indirection  
❌ **Learning Curve** - Developers must understand abstraction pattern  

## Interview Questions

**Q: What is the Open/Closed Principle?**
A: Software entities should be open for extension but closed for modification. New functionality should be added through inheritance, composition, or abstraction without changing existing code.

**Q: How do you make code open for extension?**
A: Use abstraction (interfaces, abstract classes), inheritance, composition, and design patterns like Strategy, Decorator, and Observer to allow behavior changes without modifying core logic.

**Q: What's the difference between OCP and SRP?**
A: SRP focuses on classes having one responsibility. OCP focuses on classes being extensible. A class can follow SRP (one reason to change) but still violate OCP if it requires modification for new variations.

**Q: Give an example of OCP violation.**
A: A switch statement that handles multiple types and requires modification for each new type. Example: PaymentProcessor with if/else for each payment type.

**Q: How does polymorphism enable OCP?**
A: Polymorphism allows you to define common interface (abstract class or interface), and implementations can vary. New implementations extend functionality without modifying the interface or existing implementations.

**Q: What design patterns support OCP?**
A: Strategy, Template Method, Decorator, Factory, Observer, Chain of Responsibility patterns all support OCP by providing extension points.

## When to Apply OCP

### ✅ Apply When:
- You anticipate new variations of functionality
- New requirements arrive frequently
- You want to avoid modifying tested code
- Functionality needs to be pluggable
- Multiple implementations of same interface needed

### ❌ Don't Over-Apply When:
- Single, simple implementation is sufficient
- Requirements unlikely to change
- Abstraction adds unnecessary complexity
- No clear extension points identified

## Real-World Patterns Using OCP

**Strategy Pattern** - Encapsulates algorithms for easy extension  
**Decorator Pattern** - Adds responsibilities without modification  
**Observer Pattern** - Extends notification behavior  
**Factory Pattern** - Abstracts object creation  
**Template Method Pattern** - Defines extension points  

## Code Smell: Modification-Based Extension

```csharp
// ANTI-PATTERN: Requires modification for each new type
public class ShapeCalculator
{
    public decimal CalculateArea(Shape shape)
    {
        if (shape is Circle)
        {
            var circle = (Circle)shape;
            return Math.PI * circle.Radius * circle.Radius;
        }
        else if (shape is Rectangle)
        {
            var rect = (Rectangle)shape;
            return rect.Width * rect.Height;
        }
        else if (shape is Triangle)
        {
            var tri = (Triangle)shape;
            return (tri.Base * tri.Height) / 2;
        }
        // Adding new shape requires modifying this method!
        throw new NotImplementedException();
    }
}

// SOLUTION: Use polymorphism
public abstract class Shape
{
    public abstract decimal CalculateArea();
}

public class Circle : Shape
{
    public decimal Radius { get; set; }
    public override decimal CalculateArea() => Math.PI * Radius * Radius;
}

public class Rectangle : Shape
{
    public decimal Width { get; set; }
    public decimal Height { get; set; }
    public override decimal CalculateArea() => Width * Height;
}

public class Triangle : Shape
{
    public decimal Base { get; set; }
    public decimal Height { get; set; }
    public override decimal CalculateArea() => (Base * Height) / 2;
}

// NEW shape - no modifications to ShapeCalculator!
public class Pentagon : Shape
{
    public decimal Side { get; set; }
    public override decimal CalculateArea() => /* Pentagon formula */;
}

public class ShapeCalculator
{
    public decimal CalculateArea(Shape shape)
    {
        return shape.CalculateArea();  // Polymorphism handles all types
    }
}
```

## Summary

Open/Closed Principle requires that software be open for extension but closed for modification. This is achieved through abstraction, polymorphism, and composition. OCP reduces risk, improves stability, and enables easy extension without touching tested code. Combined with other SOLID principles, OCP creates flexible, maintainable architectures.

**Key Takeaway:** Extend through abstraction, not modification. Use interfaces and inheritance to allow new functionality without changing existing code.

---

**Related SOLID Principles:**
- Single Responsibility Principle - Each class has one reason to change
- Liskov Substitution Principle - Subtypes must be substitutable
- Interface Segregation Principle - Clients shouldn't depend on unused methods
- Dependency Inversion Principle - Depend on abstractions, not concrete implementations
