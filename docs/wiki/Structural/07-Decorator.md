# Decorator Pattern

## Overview

**Category:** Structural Pattern  
**Purpose:** Attach additional responsibilities to an object dynamically, providing a flexible alternative to subclassing.  
**Also Called:** Wrapper  
**Complexity:** Medium

## Problem

Subclassing creates class explosion:

```csharp
// BAD: Class explosion for combinations
public class BasicCoffee { }
public class CoffeeWithMilk : BasicCoffee { }
public class CoffeeWithMilkAndSugar : CoffeeWithMilk { }
public class CoffeeWithMilkAndSugarAndChocolate : CoffeeWithMilkAndSugar { }
// How many combinations? 2^n classes!
```

Need flexible way to add behaviors at runtime.

## Solution

Use composition to wrap objects and add behavior dynamically:

```csharp
public interface ICoffee
{
    string GetDescription();
    decimal GetCost();
}

public class BasicCoffee : ICoffee
{
    public string GetDescription() => "Coffee";
    public decimal GetCost() => 2.0m;
}

public abstract class CoffeeDecorator : ICoffee
{
    protected ICoffee _coffee;

    protected CoffeeDecorator(ICoffee coffee) => _coffee = coffee;

    public virtual string GetDescription() => _coffee.GetDescription();
    public virtual decimal GetCost() => _coffee.GetCost();
}

public class MilkDecorator : CoffeeDecorator
{
    public MilkDecorator(ICoffee coffee) : base(coffee) { }

    public override string GetDescription() => _coffee.GetDescription() + ", milk";
    public override decimal GetCost() => _coffee.GetCost() + 0.5m;
}

// Usage - unlimited combinations!
var coffee = new BasicCoffee();
coffee = new MilkDecorator(coffee);
coffee = new SugarDecorator(coffee);
coffee = new ChocolateDecorator(coffee);
```

## Implementation Approaches

### 1. Simple Decorator

```csharp
public interface IUIComponent
{
    void Render();
}

public class Button : IUIComponent
{
    public void Render() => Console.WriteLine("Rendering Button");
}

public abstract class ComponentDecorator : IUIComponent
{
    protected IUIComponent _component;

    protected ComponentDecorator(IUIComponent component) => _component = component;

    public virtual void Render() => _component.Render();
}

public class BorderDecorator : ComponentDecorator
{
    public BorderDecorator(IUIComponent component) : base(component) { }

    public override void Render()
    {
        Console.WriteLine("Drawing border");
        _component.Render();
        Console.WriteLine("Border complete");
    }
}
```

### 2. Stream Decorator (Chaining)

```csharp
public interface IStream
{
    void Write(string data);
    string Read();
}

public class FileStream : IStream
{
    private string _data = "";
    public void Write(string data) => _data = data;
    public string Read() => _data;
}

public abstract class StreamDecorator : IStream
{
    protected IStream _stream;
    protected StreamDecorator(IStream stream) => _stream = stream;
    public virtual void Write(string data) => _stream.Write(data);
    public virtual string Read() => _stream.Read();
}

public class EncryptionDecorator : StreamDecorator
{
    public EncryptionDecorator(IStream stream) : base(stream) { }

    public override void Write(string data)
    {
        var encrypted = Encrypt(data);
        _stream.Write(encrypted);
    }

    public override string Read()
    {
        var encrypted = _stream.Read();
        return Decrypt(encrypted);
    }

    private string Encrypt(string data) => 
        Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(data));

    private string Decrypt(string data) => 
        System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(data));
}

public class CompressionDecorator : StreamDecorator
{
    public CompressionDecorator(IStream stream) : base(stream) { }

    public override void Write(string data)
    {
        var compressed = Compress(data);
        _stream.Write(compressed);
    }

    private string Compress(string data) => $"[COMPRESSED]{data.Substring(0, 5)}...";
}

// Usage: Compose decorators in any order
IStream stream = new FileStream();
stream = new EncryptionDecorator(stream);
stream = new CompressionDecorator(stream);
stream.Write("Secret data");  // Encrypted then compressed
```

### 3. Feature Stacking

```csharp
public interface IPizza
{
    string GetDescription();
    decimal GetPrice();
}

public class BasicPizza : IPizza
{
    public string GetDescription() => "Pizza";
    public decimal GetPrice() => 5.0m;
}

public abstract class PizzaDecorator : IPizza
{
    protected IPizza _pizza;
    protected PizzaDecorator(IPizza pizza) => _pizza = pizza;
    public virtual string GetDescription() => _pizza.GetDescription();
    public virtual decimal GetPrice() => _pizza.GetPrice();
}

public class PepperoniDecorator : PizzaDecorator
{
    public PepperoniDecorator(IPizza pizza) : base(pizza) { }
    public override string GetDescription() => _pizza.GetDescription() + " + Pepperoni";
    public override decimal GetPrice() => _pizza.GetPrice() + 1.5m;
}

public class CheeseDecorator : PizzaDecorator
{
    public CheeseDecorator(IPizza pizza) : base(pizza) { }
    public override string GetDescription() => _pizza.GetDescription() + " + Cheese";
    public override decimal GetPrice() => _pizza.GetPrice() + 1.0m;
}

// Unlimited combinations
var pizza = new BasicPizza();
pizza = new PepperoniDecorator(pizza);
pizza = new CheeseDecorator(pizza);
pizza = new MushroomDecorator(pizza);
Console.WriteLine(pizza.GetDescription());  // "Pizza + Pepperoni + Cheese + Mushroom"
Console.WriteLine(pizza.GetPrice());         // 8.5
```

### 4. Notification Decorator

```csharp
public interface INotification
{
    void Send(string message);
}

public class BasicNotification : INotification
{
    public void Send(string message) => 
        Console.WriteLine($"Notification: {message}");
}

public class EmailNotificationDecorator : INotification
{
    private INotification _notification;

    public EmailNotificationDecorator(INotification notification) =>
        _notification = notification;

    public void Send(string message)
    {
        _notification.Send(message);
        Console.WriteLine($"Also sending email: {message}");
    }
}

public class SMSNotificationDecorator : INotification
{
    private INotification _notification;

    public SMSNotificationDecorator(INotification notification) =>
        _notification = notification;

    public void Send(string message)
    {
        _notification.Send(message);
        Console.WriteLine($"Also sending SMS: {message}");
    }
}

// Usage
INotification notifier = new BasicNotification();
notifier = new EmailNotificationDecorator(notifier);
notifier = new SMSNotificationDecorator(notifier);
notifier.Send("Order confirmed");  // Sends to all channels
```

---

## Decorator vs. Inheritance

```csharp
// With Inheritance (BAD - class explosion)
public class Coffee { }
public class CoffeeWithMilk : Coffee { }
public class CoffeeWithMilkAndSugar : CoffeeWithMilk { }
public class CoffeeWithMilkAndSugarAndChocolate : CoffeeWithMilkAndSugar { }
// 2^n possible combinations!

// With Decorator (GOOD - flexible)
var coffee = new BasicCoffee();
coffee = new MilkDecorator(coffee);
coffee = new SugarDecorator(coffee);
coffee = new ChocolateDecorator(coffee);
// Unlimited combinations, no new classes!
```

---

## Real-World Examples

### GUI Rendering
```csharp
var button = new Button();
button = new BorderDecorator(button);
button = new ShadowDecorator(button);
button.Render();  // Renders button with border and shadow
```

### HTTP Requests
```csharp
IHttpHandler handler = new BaseHttpHandler();
handler = new AuthenticationHandler(handler);
handler = new LoggingHandler(handler);
handler = new CompressionHandler(handler);
handler.Handle(request);  // Authenticated, logged, compressed
```

### Data Formatting
```csharp
var data = new RawData("12345");
data = new FormattingDecorator(data);      // Add formatting
data = new ValidationDecorator(data);      // Add validation
data = new SanitizationDecorator(data);    // Add sanitization
```

---

## Pros and Cons

### Advantages
✅ **Avoids Class Explosion** - Composition instead of inheritance  
✅ **Flexible** - Add/remove features at runtime  
✅ **Single Responsibility** - Each decorator handles one concern  
✅ **Open/Closed Principle** - Open for extension  
✅ **Composable** - Decorators can be combined in any order  

### Disadvantages
❌ **Complexity** - More classes and composition  
❌ **Order Matters** - Decorator order affects behavior  
❌ **Debugging** - Wrapped objects harder to trace  
❌ **Performance** - Extra method calls  
❌ **Confusion** - Type confusion from wrapping  

---

## When to Use

### ✅ Use Decorator When:
- Need many optional features
- Would create too many subclasses
- Feature combinations are numerous
- Features added dynamically
- Single Responsibility Principle needed
- Open/Closed Principle desired

### ❌ Don't Use When:
- Simple object with few features
- Inheritance hierarchy is small
- Performance critical
- Simplicity valued
- Features not combinable

---

## Decorator Pitfalls

❌ **Bad: Uncontrolled Decoration**
```csharp
// Unclear what decorators applied
var obj = new Decorator1(new Decorator2(new Decorator3(original)));
```

✅ **Good: Clear Composition**
```csharp
var obj = original;
obj = new Decorator1(obj);
obj = new Decorator2(obj);
obj = new Decorator3(obj);
// Clear order and intent
```

---

## Interview Questions

**Q: What's the difference between Decorator and Inheritance?**
A: Inheritance adds features at compile-time to classes; Decorator adds at runtime to objects.

**Q: Can decorators be applied in any order?**
A: Usually yes, but order can affect results. Each decorator should be independent.

**Q: How is Decorator different from Proxy?**
A: Decorator adds behavior; Proxy controls access. Both wrap objects.

**Q: What's the decorator pattern in Java's I/O?**
A: `BufferedInputStream` decorating `FileInputStream` - core Java example!

---

## Summary

Decorator avoids class explosion by composing behavior at runtime. Perfect for optional features and feature combinations. Provides elegant alternative to complex inheritance hierarchies. Use for any scenario where you'd otherwise create many subclass combinations.

**Key Takeaway:** Decorator adds functionality dynamically without subclassing explosion.
