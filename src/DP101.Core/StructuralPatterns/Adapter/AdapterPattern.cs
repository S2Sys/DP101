namespace DP101.Core.StructuralPatterns.Adapter;

/// <summary>
/// ADAPTER PATTERN
///
/// Intent: Convert the interface of a class into another interface clients expect,
/// allowing incompatible interfaces to work together.
///
/// Participants:
/// - Target: Interface expected by clients
/// - Adapter: Converts interface to Target interface
/// - Adaptee: Existing interface that needs adapting
///
/// PROS:
/// - Makes incompatible interfaces compatible
/// - Allows reuse of existing code
/// - Increases class reusability
/// - Decouples client from implementation
///
/// CONS:
/// - Adds extra layer of indirection
/// - Can complicate code
/// - Multiple adapters can be confusing
///
/// USE CASES:
/// - Third-party library integration
/// - Legacy system integration
/// - Different interface standards
/// - API versioning
/// </summary>

// ========== EXAMPLE 1: CLASS ADAPTER (INHERITANCE) ==========

// Target interface that client expects
public interface ITarget
{
    string Request();
}

// Adaptee - existing class with different interface
public class Adaptee
{
    public string SpecificRequest()
    {
        return "Specific request response from Adaptee";
    }
}

// Class Adapter using inheritance
public class ClassAdapter : Adaptee, ITarget
{
    public string Request()
    {
        return SpecificRequest();
    }
}

// ========== EXAMPLE 2: OBJECT ADAPTER (COMPOSITION) ==========

// Object Adapter using composition
public class ObjectAdapter : ITarget
{
    private readonly Adaptee _adaptee;

    public ObjectAdapter(Adaptee adaptee)
    {
        _adaptee = adaptee;
    }

    public string Request()
    {
        return $"Adapted: {_adaptee.SpecificRequest()}";
    }
}

// ========== EXAMPLE 3: POWER ADAPTER ==========

// 220V electrical system (Adaptee)
public class Voltage220V
{
    public double GetVoltage()
    {
        return 220.0;
    }
}

// 110V system interface (Target)
public interface IVoltage110V
{
    double GetVoltage();
}

// Adapter to convert 220V to 110V
public class VoltageAdapter : IVoltage110V
{
    private readonly Voltage220V _voltage220;

    public VoltageAdapter(Voltage220V voltage220)
    {
        _voltage220 = voltage220;
    }

    public double GetVoltage()
    {
        return _voltage220.GetVoltage() / 2;
    }
}

// ========== EXAMPLE 4: PAYMENT ADAPTER ==========

// Existing payment system
public class PaymentProcessor
{
    public bool ProcessPayment(double amount)
    {
        Console.WriteLine($"Processing payment: ${amount}");
        return true;
    }
}

// New payment interface expected by application
public interface IPaymentGateway
{
    PaymentResponse Pay(PaymentRequest request);
}

public class PaymentRequest
{
    public string CardNumber { get; set; }
    public double Amount { get; set; }
    public string Currency { get; set; }

    public PaymentRequest(string cardNumber, double amount, string currency = "USD")
    {
        CardNumber = cardNumber;
        Amount = amount;
        Currency = currency;
    }
}

public class PaymentResponse
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string TransactionId { get; set; }

    public PaymentResponse(bool success, string message, string transactionId)
    {
        Success = success;
        Message = message;
        TransactionId = transactionId;
    }
}

// Adapter for old payment processor
public class PaymentProcessorAdapter : IPaymentGateway
{
    private readonly PaymentProcessor _processor;

    public PaymentProcessorAdapter(PaymentProcessor processor)
    {
        _processor = processor;
    }

    public PaymentResponse Pay(PaymentRequest request)
    {
        try
        {
            var success = _processor.ProcessPayment(request.Amount);
            return new PaymentResponse(
                success,
                "Payment processed successfully",
                Guid.NewGuid().ToString());
        }
        catch (Exception ex)
        {
            return new PaymentResponse(false, $"Payment failed: {ex.Message}", "");
        }
    }
}

// ========== EXAMPLE 5: JSON TO XML ADAPTER ==========

// Existing JSON interface
public class JsonDataProvider
{
    public string GetJsonData()
    {
        return "{\"name\": \"John\", \"age\": 30}";
    }
}

// Required XML interface
public interface IXmlDataProvider
{
    string GetXmlData();
}

// Adapter from JSON to XML
public class JsonToXmlAdapter : IXmlDataProvider
{
    private readonly JsonDataProvider _jsonProvider;

    public JsonToXmlAdapter(JsonDataProvider jsonProvider)
    {
        _jsonProvider = jsonProvider;
    }

    public string GetXmlData()
    {
        var json = _jsonProvider.GetJsonData();
        // Simplified conversion - in real code, use proper JSON parsing
        return "<root><name>John</name><age>30</age></root>";
    }
}

// ========== EXAMPLE 6: COLLECTION ADAPTER ==========

// Old-style list interface
public class LegacyArray
{
    private string[] _items;

    public LegacyArray(string[] items)
    {
        _items = items;
    }

    public int Count => _items.Length;

    public string GetItem(int index)
    {
        return _items[index];
    }
}

// Modern collection interface
public interface IModernCollection : IEnumerable<string>
{
    void Add(string item);
    void Remove(string item);
    string Get(int index);
}

// Adapter to make old array work with modern interface
public class LegacyArrayAdapter : IModernCollection
{
    private readonly LegacyArray _legacyArray;
    private List<string> _items;

    public LegacyArrayAdapter(LegacyArray legacyArray)
    {
        _legacyArray = legacyArray;
        _items = new List<string>();

        // Copy items from legacy array
        for (int i = 0; i < legacyArray.Count; i++)
        {
            _items.Add(legacyArray.GetItem(i));
        }
    }

    public void Add(string item) => _items.Add(item);
    public void Remove(string item) => _items.Remove(item);
    public string Get(int index) => _items[index];

    public IEnumerator<string> GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

// ========== EXAMPLE 7: DATABASE ADAPTER ==========

// Old database interface
public class OldDatabase
{
    public bool InsertRecord(string tableName, Dictionary<string, object> data)
    {
        Console.WriteLine($"Old DB: Inserting into {tableName}");
        return true;
    }
}

// New database interface
public interface IDatabase
{
    void Insert<T>(T entity) where T : class;
    void Update<T>(T entity) where T : class;
    void Delete<T>(T entity) where T : class;
    T? Select<T>(int id) where T : class;
}

// Simple entity for example
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }

    public User(int id, string name)
    {
        Id = id;
        Name = name;
    }
}

// Adapter for old database
public class OldDatabaseAdapter : IDatabase
{
    private readonly OldDatabase _oldDatabase;

    public OldDatabaseAdapter(OldDatabase oldDatabase)
    {
        _oldDatabase = oldDatabase;
    }

    public void Insert<T>(T entity) where T : class
    {
        var tableName = typeof(T).Name;
        var data = new Dictionary<string, object> { { "data", entity } };
        _oldDatabase.InsertRecord(tableName, data);
    }

    public void Update<T>(T entity) where T : class
    {
        var tableName = typeof(T).Name;
        var data = new Dictionary<string, object> { { "data", entity } };
        _oldDatabase.InsertRecord(tableName, data);
    }

    public void Delete<T>(T entity) where T : class { }

    public T? Select<T>(int id) where T : class
    {
        return null;
    }
}
