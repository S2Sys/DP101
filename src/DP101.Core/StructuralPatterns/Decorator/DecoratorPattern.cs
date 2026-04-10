namespace DP101.Core.StructuralPatterns.Decorator;

/// <summary>
/// DECORATOR PATTERN
///
/// Intent: Attach additional responsibilities to an object dynamically,
/// providing a flexible alternative to subclassing.
///
/// PROS:
/// - Single Responsibility Principle
/// - Open/Closed Principle
/// - Flexible composition
/// - Avoid class explosion
/// - Runtime behavior modification
///
/// CONS:
/// - Many decorator classes can be created
/// - Order of decorators matters
/// - Harder to trace execution
/// </summary>

// ========== EXAMPLE 1: COFFEE DECORATOR ==========

// Component interface
public interface ICoffee
{
    string GetDescription();
    decimal GetCost();
}

// Concrete component
public class BasicCoffee : ICoffee
{
    public string GetDescription() => "Coffee";
    public decimal GetCost() => 2.0m;
}

// Abstract decorator
public abstract class CoffeeDecorator : ICoffee
{
    protected ICoffee _coffee;

    protected CoffeeDecorator(ICoffee coffee)
    {
        _coffee = coffee;
    }

    public virtual string GetDescription() => _coffee.GetDescription();
    public virtual decimal GetCost() => _coffee.GetCost();
}

// Concrete decorators
public class MilkDecorator : CoffeeDecorator
{
    public MilkDecorator(ICoffee coffee) : base(coffee) { }

    public override string GetDescription() => _coffee.GetDescription() + ", milk";
    public override decimal GetCost() => _coffee.GetCost() + 0.5m;
}

public class SugarDecorator : CoffeeDecorator
{
    public SugarDecorator(ICoffee coffee) : base(coffee) { }

    public override string GetDescription() => _coffee.GetDescription() + ", sugar";
    public override decimal GetCost() => _coffee.GetCost() + 0.2m;
}

public class ChocolateDecorator : CoffeeDecorator
{
    public ChocolateDecorator(ICoffee coffee) : base(coffee) { }

    public override string GetDescription() => _coffee.GetDescription() + ", chocolate";
    public override decimal GetCost() => _coffee.GetCost() + 0.7m;
}

public class WhippedCreamDecorator : CoffeeDecorator
{
    public WhippedCreamDecorator(ICoffee coffee) : base(coffee) { }

    public override string GetDescription() => _coffee.GetDescription() + ", whipped cream";
    public override decimal GetCost() => _coffee.GetCost() + 0.3m;
}

// ========== EXAMPLE 2: STREAM DECORATOR ==========

// Component
public interface IDataStream
{
    void Write(string data);
    string Read();
}

// Concrete component
public class FileStream : IDataStream
{
    private string _data = "";

    public void Write(string data) => _data = data;
    public string Read() => _data;
}

// Abstract decorator
public abstract class StreamDecorator : IDataStream
{
    protected IDataStream _stream;

    protected StreamDecorator(IDataStream stream)
    {
        _stream = stream;
    }

    public virtual void Write(string data) => _stream.Write(data);
    public virtual string Read() => _stream.Read();
}

// Concrete decorators
public class EncryptionStreamDecorator : StreamDecorator
{
    public EncryptionStreamDecorator(IDataStream stream) : base(stream) { }

    public override void Write(string data)
    {
        var encrypted = EncryptData(data);
        _stream.Write(encrypted);
    }

    public override string Read()
    {
        var encrypted = _stream.Read();
        return DecryptData(encrypted);
    }

    private string EncryptData(string data) => Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(data));
    private string DecryptData(string data) => System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(data));
}

public class CompressionStreamDecorator : StreamDecorator
{
    public CompressionStreamDecorator(IDataStream stream) : base(stream) { }

    public override void Write(string data)
    {
        var compressed = CompressData(data);
        _stream.Write(compressed);
    }

    public override string Read()
    {
        var compressed = _stream.Read();
        return DecompressData(compressed);
    }

    private string CompressData(string data) => $"[COMPRESSED]{data.Substring(0, Math.Min(5, data.Length))}...";
    private string DecompressData(string data) => data.Replace("[COMPRESSED]", "");
}

public class LoggingStreamDecorator : StreamDecorator
{
    public LoggingStreamDecorator(IDataStream stream) : base(stream) { }

    public override void Write(string data)
    {
        Console.WriteLine($"Writing: {data}");
        _stream.Write(data);
    }

    public override string Read()
    {
        var result = _stream.Read();
        Console.WriteLine($"Reading: {result}");
        return result;
    }
}

// ========== EXAMPLE 3: UI COMPONENT DECORATOR ==========

// Component interface
public interface IUIComponent
{
    void Render();
}

// Concrete component
public class Button : IUIComponent
{
    public void Render() => Console.WriteLine("Rendering Button");
}

// Abstract decorator
public abstract class UIComponentDecorator : IUIComponent
{
    protected IUIComponent _component;

    protected UIComponentDecorator(IUIComponent component)
    {
        _component = component;
    }

    public virtual void Render() => _component.Render();
}

// Concrete decorators
public class BorderDecorator : UIComponentDecorator
{
    public BorderDecorator(IUIComponent component) : base(component) { }

    public override void Render()
    {
        Console.WriteLine("Drawing border");
        _component.Render();
        Console.WriteLine("Border complete");
    }
}

public class ShadowDecorator : UIComponentDecorator
{
    public ShadowDecorator(IUIComponent component) : base(component) { }

    public override void Render()
    {
        Console.WriteLine("Drawing shadow");
        _component.Render();
    }
}

public class ScrollBarDecorator : UIComponentDecorator
{
    public ScrollBarDecorator(IUIComponent component) : base(component) { }

    public override void Render()
    {
        _component.Render();
        Console.WriteLine("Adding scroll bars");
    }
}

// ========== EXAMPLE 4: NOTIFICATION DECORATOR ==========

// Component
public interface INotification
{
    void Send(string message);
}

// Concrete component
public class BasicNotification : INotification
{
    public void Send(string message) => Console.WriteLine($"Sending notification: {message}");
}

// Decorators for different notification channels
public class EmailNotificationDecorator : INotification
{
    private readonly INotification _notification;

    public EmailNotificationDecorator(INotification notification)
    {
        _notification = notification;
    }

    public void Send(string message)
    {
        _notification.Send(message);
        Console.WriteLine($"Also sending via email: {message}");
    }
}

public class SMSNotificationDecorator : INotification
{
    private readonly INotification _notification;

    public SMSNotificationDecorator(INotification notification)
    {
        _notification = notification;
    }

    public void Send(string message)
    {
        _notification.Send(message);
        Console.WriteLine($"Also sending via SMS: {message}");
    }
}

public class PushNotificationDecorator : INotification
{
    private readonly INotification _notification;

    public PushNotificationDecorator(INotification notification)
    {
        _notification = notification;
    }

    public void Send(string message)
    {
        _notification.Send(message);
        Console.WriteLine($"Also sending push notification: {message}");
    }
}

// ========== EXAMPLE 5: PIZZA DECORATOR ==========

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

    protected PizzaDecorator(IPizza pizza)
    {
        _pizza = pizza;
    }

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

    public override string GetDescription() => _pizza.GetDescription() + " + Extra Cheese";
    public override decimal GetPrice() => _pizza.GetPrice() + 1.0m;
}

public class MushroomDecorator : PizzaDecorator
{
    public MushroomDecorator(IPizza pizza) : base(pizza) { }

    public override string GetDescription() => _pizza.GetDescription() + " + Mushroom";
    public override decimal GetPrice() => _pizza.GetPrice() + 0.75m;
}

public class OnionDecorator : PizzaDecorator
{
    public OnionDecorator(IPizza pizza) : base(pizza) { }

    public override string GetDescription() => _pizza.GetDescription() + " + Onion";
    public override decimal GetPrice() => _pizza.GetPrice() + 0.5m;
}
