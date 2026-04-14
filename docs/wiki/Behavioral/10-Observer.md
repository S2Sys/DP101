# Observer Pattern

## Overview

**Category:** Behavioral Pattern  
**Purpose:** Define a one-to-many dependency so that when one object changes state, all its dependents are notified automatically.  
**Also Called:** Pub-Sub, Event Subscription  
**Complexity:** Medium

## Problem

Need to notify multiple objects when another object's state changes, without tight coupling:

```csharp
// Problem: Tight coupling - Stock knows about all observers
public class Stock
{
    private List<Portfolio> portfolios;
    private List<AlertManager> alerts;
    private List<Logger> loggers;

    public void SetPrice(decimal newPrice)
    {
        // Must notify all manually
        foreach (var portfolio in portfolios)
            portfolio.UpdatePrice(this);
        foreach (var alert in alerts)
            alert.CheckAlert(this);
        foreach (var logger in loggers)
            logger.LogPrice(this);
    }
}
// Adding new observer requires changing Stock class!
```

## Solution

Create observer interface that subjects notify automatically:

```csharp
public interface IObserver
{
    void Update(string message);
}

public class Subject
{
    private List<IObserver> _observers = new();

    public void Subscribe(IObserver observer) => _observers.Add(observer);
    public void Unsubscribe(IObserver observer) => _observers.Remove(observer);

    public void NotifyObservers(string message)
    {
        foreach (var observer in _observers)
            observer.Update(message);
    }
}

// Adding new observer doesn't change Subject!
```

## Implementation Approaches

### 1. Simple Observer Pattern

```csharp
public interface IObserver
{
    void Update(string message);
}

public class Subject
{
    private List<IObserver> _observers = new();

    public void Subscribe(IObserver observer)
    {
        _observers.Add(observer);
    }

    public void Unsubscribe(IObserver observer)
    {
        _observers.Remove(observer);
    }

    public void Notify(string message)
    {
        foreach (var observer in _observers)
        {
            observer.Update(message);
        }
    }
}

// Concrete observers
public class EmailObserver : IObserver
{
    public void Update(string message) =>
        Console.WriteLine($"[Email] {message}");
}

public class SMSObserver : IObserver
{
    public void Update(string message) =>
        Console.WriteLine($"[SMS] {message}");
}

// Usage
var subject = new Subject();
subject.Subscribe(new EmailObserver());
subject.Subscribe(new SMSObserver());
subject.Notify("Important update!");
```

### 2. Stock Price Observer

```csharp
public class Stock
{
    private string _symbol;
    private decimal _price;
    private List<IStockObserver> _observers = new();

    public Stock(string symbol, decimal price)
    {
        _symbol = symbol;
        _price = price;
    }

    public void Attach(IStockObserver observer) => _observers.Add(observer);
    public void Detach(IStockObserver observer) => _observers.Remove(observer);

    public void SetPrice(decimal newPrice)
    {
        if (_price != newPrice)
        {
            _price = newPrice;
            NotifyObservers();
        }
    }

    private void NotifyObservers()
    {
        foreach (var observer in _observers)
        {
            observer.PriceChanged(_symbol, _price);
        }
    }
}

public interface IStockObserver
{
    void PriceChanged(string symbol, decimal newPrice);
}

public class StockPortfolio : IStockObserver
{
    private List<(string symbol, int quantity, decimal buyPrice)> _holdings = new();

    public void BuyStock(string symbol, int quantity, decimal price)
    {
        _holdings.Add((symbol, quantity, price));
    }

    public void PriceChanged(string symbol, decimal newPrice)
    {
        var holding = _holdings.FirstOrDefault(h => h.symbol == symbol);
        if (holding != default)
        {
            var gain = (newPrice - holding.buyPrice) * holding.quantity;
            Console.WriteLine($"Portfolio: {symbol} gain/loss: ${gain}");
        }
    }
}

// Usage
var stock = new Stock("ACME", 100m);
var portfolio = new StockPortfolio();
stock.Attach(portfolio);

portfolio.BuyStock("ACME", 10, 100m);
stock.SetPrice(105m);  // Notifies portfolio automatically
```

### 3. Weather Station Observer

```csharp
public class WeatherData
{
    private decimal _temperature;
    private decimal _humidity;
    private List<IWeatherObserver> _observers = new();

    public void RegisterObserver(IWeatherObserver observer) => _observers.Add(observer);
    public void RemoveObserver(IWeatherObserver observer) => _observers.Remove(observer);

    public void SetMeasurements(decimal temp, decimal humidity, decimal pressure)
    {
        _temperature = temp;
        _humidity = humidity;
        NotifyObservers();
    }

    private void NotifyObservers()
    {
        foreach (var observer in _observers)
        {
            observer.Update(_temperature, _humidity);
        }
    }
}

public interface IWeatherObserver
{
    void Update(decimal temperature, decimal humidity);
}

public class CurrentConditionsDisplay : IWeatherObserver
{
    public void Update(decimal temperature, decimal humidity)
    {
        Console.WriteLine($"Current: {temperature}°F, {humidity}%");
    }
}

public class StatisticsDisplay : IWeatherObserver
{
    private List<decimal> _temperatures = new();

    public void Update(decimal temperature, decimal humidity)
    {
        _temperatures.Add(temperature);
        Console.WriteLine($"Avg: {_temperatures.Average()}°F");
    }
}

// Usage
var weather = new WeatherData();
weather.RegisterObserver(new CurrentConditionsDisplay());
weather.RegisterObserver(new StatisticsDisplay());
weather.SetMeasurements(72m, 65m, 29.9m);
```

### 4. .NET Events Pattern (C# Native)

```csharp
// Using .NET events instead of IObserver
public class Button
{
    // Event declaration
    public event EventHandler Clicked;

    public void Click()
    {
        // Raise event
        Clicked?.Invoke(this, EventArgs.Empty);
    }
}

// Subscribers
public class Logger
{
    public void OnButtonClicked(object sender, EventArgs e)
    {
        Console.WriteLine("[Log] Button clicked");
    }
}

public class Analytics
{
    public void OnButtonClicked(object sender, EventArgs e)
    {
        Console.WriteLine("[Analytics] Button click tracked");
    }
}

// Usage
var button = new Button();
var logger = new Logger();
var analytics = new Analytics();

button.Clicked += logger.OnButtonClicked;
button.Clicked += analytics.OnButtonClicked;

button.Click();  // Both subscribers notified
```

### 5. Property Change Notifier

```csharp
public class PropertyChangeNotifier
{
    private Dictionary<string, object> _properties = new();
    private List<IPropertyObserver> _observers = new();

    public void Subscribe(IPropertyObserver observer) => _observers.Add(observer);

    public void SetProperty(string name, object value)
    {
        var oldValue = _properties.ContainsKey(name) ? _properties[name] : null;
        if (oldValue != value)
        {
            _properties[name] = value;
            NotifyPropertyChanged(name, oldValue, value);
        }
    }

    private void NotifyPropertyChanged(string name, object oldValue, object newValue)
    {
        foreach (var observer in _observers)
        {
            observer.OnPropertyChanged(name, oldValue, newValue);
        }
    }
}

public interface IPropertyObserver
{
    void OnPropertyChanged(string name, object oldValue, object newValue);
}

// Usage
var notifier = new PropertyChangeNotifier();
notifier.Subscribe(new PropertyChangeLogger());
notifier.SetProperty("Username", "john");  // Notifies observers
```

---

## Real-World Examples

### GUI Event Handling
```csharp
button.Click += (sender, e) => MessageBox.Show("Clicked!");
textBox.TextChanged += (sender, e) => UpdatePreview();
```

### Data Binding
```csharp
var person = new Person();
person.PropertyChanged += (sender, e) =>
{
    if (e.PropertyName == "Age")
        UpdateAgeDisplay();
};
```

### MVC Pattern
```csharp
public class Model
{
    public event EventHandler DataChanged;

    public void UpdateData()
    {
        // Update data
        DataChanged?.Invoke(this, EventArgs.Empty);
    }
}

public class View
{
    public View(Model model)
    {
        model.DataChanged += OnDataChanged;
    }

    private void OnDataChanged(object sender, EventArgs e)
    {
        Refresh();
    }
}
```

---

## Observer vs. Pub-Sub

| Aspect | Observer | Pub-Sub |
|--------|----------|---------|
| **Coupling** | Direct | Indirect (via broker) |
| **Communication** | Synchronous | Asynchronous |
| **Scalability** | Limited | High |
| **Complexity** | Simple | Complex |
| **Use Case** | Single subject | Distributed systems |

---

## Pros and Cons

### Advantages
✅ **Loose Coupling** - Subject and observers independent  
✅ **Dynamic Relationships** - Subscribe/unsubscribe at runtime  
✅ **Broadcast Communication** - Notify multiple objects  
✅ **Open/Closed Principle** - Easy to add new observers  

### Disadvantages
❌ **Notification Order** - Undefined observer notification order  
❌ **Memory Leaks** - Must unsubscribe to avoid holding references  
❌ **Performance** - All observers notified regardless of interest  
❌ **Debugging** - Hard to trace notification flow  

---

## When to Use

### ✅ Use Observer When:
- One object's state affects many others
- Don't know upfront how many objects to notify
- Objects should be loosely coupled
- Need dynamic subscription/unsubscription
- Building event-driven systems

### ❌ Don't Use When:
- Simple one-to-one relationships
- Performance is critical
- Notification order matters
- Simple method calls sufficient

---

## Interview Questions

**Q: What's the difference between Observer and Pub-Sub?**
A: Observer is direct subject-observer communication; Pub-Sub uses broker for indirect communication.

**Q: How do you prevent memory leaks in Observer?**
A: Always unsubscribe observers when no longer needed. Use weak references if needed.

**Q: What if observer throws exception?**
A: Use try-catch in notification loop to prevent other observers missing notifications.

**Q: .NET events vs. IObserver interface?**
A: Events are C# implementation of Observer pattern. Events are more convenient but less flexible.

---

## Summary

The Observer pattern is fundamental for event-driven programming. Perfect for GUI frameworks, MVC patterns, and real-time data updates. Use .NET events in C# for convenience, or implement IObserver interface for more control.

**Key Takeaway:** Observer enables loose coupling through automatic notifications when state changes.
