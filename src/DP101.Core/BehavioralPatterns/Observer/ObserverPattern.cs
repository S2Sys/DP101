namespace DP101.Core.BehavioralPatterns.Observer;

/// <summary>
/// OBSERVER PATTERN
///
/// Intent: Define a one-to-many dependency so that when one object changes state,
/// all its dependents are notified automatically.
///
/// Participants:
/// - Subject: Defines interface for attaching/detaching observers
/// - ConcreteSubject: Sends notifications to observers
/// - Observer: Defines update interface
/// - ConcreteObserver: Implements observer interface
///
/// PROS:
/// - Loose coupling between objects
/// - Dynamic subscription/unsubscription
/// - Supports broadcast communication
/// - Changes propagate automatically
///
/// CONS:
/// - All observers notified regardless of relevance
/// - Order of notifications is undefined
/// - Can lead to memory leaks if not unsubscribed
/// - Performance impact with many observers
/// </summary>

// ========== EXAMPLE 1: EVENT-BASED OBSERVER ==========

// Observer interface
public interface IObserver
{
    void Update(string message);
}

// Subject
public class EventPublisher
{
    private List<IObserver> _observers = [];

    public void Subscribe(IObserver observer)
    {
        _observers.Add(observer);
    }

    public void Unsubscribe(IObserver observer)
    {
        _observers.Remove(observer);
    }

    public void Publish(string message)
    {
        Console.WriteLine($"[Publisher] Publishing: {message}");
        foreach (var observer in _observers)
        {
            observer.Update(message);
        }
    }
}

// Concrete observers
public class EmailObserver : IObserver
{
    public void Update(string message)
    {
        Console.WriteLine($"[Email] Sending email notification: {message}");
    }
}

public class SMSObserver : IObserver
{
    public void Update(string message)
    {
        Console.WriteLine($"[SMS] Sending SMS: {message}");
    }
}

public class LogObserver : IObserver
{
    private List<string> _logs = [];

    public void Update(string message)
    {
        _logs.Add(message);
        Console.WriteLine($"[Log] Logged message #{_logs.Count}: {message}");
    }

    public List<string> GetLogs() => _logs;
}

// ========== EXAMPLE 2: STOCK PRICE OBSERVER ==========

public class Stock
{
    private string _symbol;
    private decimal _price;
    private List<IStockObserver> _observers = [];

    public Stock(string symbol, decimal price)
    {
        _symbol = symbol;
        _price = price;
    }

    public void Attach(IStockObserver observer)
    {
        _observers.Add(observer);
    }

    public void Detach(IStockObserver observer)
    {
        _observers.Remove(observer);
    }

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

    public decimal GetPrice() => _price;
    public string GetSymbol() => _symbol;
}

public interface IStockObserver
{
    void PriceChanged(string symbol, decimal newPrice);
}

public class StockPortfolio : IStockObserver
{
    private List<(string symbol, int quantity, decimal buyPrice)> _holdings = [];

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
            Console.WriteLine($"[Portfolio] {symbol} price changed to ${newPrice}. Unrealized gain/loss: ${gain}");
        }
    }
}

public class AlertObserver : IStockObserver
{
    private decimal _threshold;

    public AlertObserver(decimal threshold)
    {
        _threshold = threshold;
    }

    public void PriceChanged(string symbol, decimal newPrice)
    {
        if (newPrice > _threshold)
        {
            Console.WriteLine($"[Alert] {symbol} has exceeded threshold! Current price: ${newPrice}");
        }
    }
}

// ========== EXAMPLE 3: WEATHER STATION ==========

public class WeatherData
{
    private decimal _temperature;
    private decimal _humidity;
    private decimal _pressure;
    private List<IWeatherObserver> _observers = [];

    public void RegisterObserver(IWeatherObserver observer) => _observers.Add(observer);
    public void RemoveObserver(IWeatherObserver observer) => _observers.Remove(observer);

    public void SetMeasurements(decimal temp, decimal humidity, decimal pressure)
    {
        _temperature = temp;
        _humidity = humidity;
        _pressure = pressure;
        NotifyObservers();
    }

    private void NotifyObservers()
    {
        foreach (var observer in _observers)
        {
            observer.Update(_temperature, _humidity, _pressure);
        }
    }
}

public interface IWeatherObserver
{
    void Update(decimal temperature, decimal humidity, decimal pressure);
}

public class CurrentConditionsDisplay : IWeatherObserver
{
    public void Update(decimal temperature, decimal humidity, decimal pressure)
    {
        Console.WriteLine($"[Current Conditions] Temp: {temperature}°F, Humidity: {humidity}%, Pressure: {pressure}");
    }
}

public class StatisticsDisplay : IWeatherObserver
{
    private List<decimal> _temperatures = [];

    public void Update(decimal temperature, decimal humidity, decimal pressure)
    {
        _temperatures.Add(temperature);
        var avgTemp = _temperatures.Average();
        var maxTemp = _temperatures.Max();
        var minTemp = _temperatures.Min();

        Console.WriteLine($"[Statistics] Avg: {avgTemp}°F, Max: {maxTemp}°F, Min: {minTemp}°F");
    }
}

// ========== EXAMPLE 4: MODEL-VIEW UPDATE ==========

public class Model
{
    private string _data;
    private List<IView> _views = [];

    public Model(string initialData)
    {
        _data = initialData;
    }

    public void AttachView(IView view)
    {
        _views.Add(view);
    }

    public void DetachView(IView view)
    {
        _views.Remove(view);
    }

    public void SetData(string newData)
    {
        _data = newData;
        NotifyViews();
    }

    private void NotifyViews()
    {
        foreach (var view in _views)
        {
            view.Update(_data);
        }
    }

    public string GetData() => _data;
}

public interface IView
{
    void Update(string data);
}

public class TextViewDisplay : IView
{
    public void Update(string data)
    {
        Console.WriteLine($"[TextView] Displaying: {data}");
    }
}

public class GraphicalViewDisplay : IView
{
    public void Update(string data)
    {
        Console.WriteLine($"[GraphView] Rendering graph for: {data}");
    }
}

// ========== EXAMPLE 5: PROPERTY CHANGE NOTIFIER ==========

public class PropertyChangeNotifier
{
    private Dictionary<string, object> _properties = [];
    private List<IPropertyObserver> _observers = [];

    public void Subscribe(IPropertyObserver observer) => _observers.Add(observer);
    public void Unsubscribe(IPropertyObserver observer) => _observers.Remove(observer);

    public void SetProperty(string propertyName, object value)
    {
        var oldValue = _properties.ContainsKey(propertyName) ? _properties[propertyName] : null;

        if (oldValue != value)
        {
            _properties[propertyName] = value;
            NotifyPropertyChanged(propertyName, oldValue, value);
        }
    }

    public object? GetProperty(string propertyName)
    {
        return _properties.ContainsKey(propertyName) ? _properties[propertyName] : null;
    }

    private void NotifyPropertyChanged(string propertyName, object? oldValue, object? newValue)
    {
        foreach (var observer in _observers)
        {
            observer.OnPropertyChanged(propertyName, oldValue, newValue);
        }
    }
}

public interface IPropertyObserver
{
    void OnPropertyChanged(string propertyName, object? oldValue, object? newValue);
}

public class PropertyChangeLogger : IPropertyObserver
{
    public void OnPropertyChanged(string propertyName, object? oldValue, object? newValue)
    {
        Console.WriteLine($"[PropertyLog] {propertyName} changed from '{oldValue}' to '{newValue}'");
    }
}

public class PropertyChangeValidator : IPropertyObserver
{
    public void OnPropertyChanged(string propertyName, object? oldValue, object? newValue)
    {
        Console.WriteLine($"[Validator] Validating property {propertyName}");
        if (propertyName == "Age" && newValue is int age && age < 0)
        {
            Console.WriteLine($"[Validator] Invalid age: {age}");
        }
    }
}
