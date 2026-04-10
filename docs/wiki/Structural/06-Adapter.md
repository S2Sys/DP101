# Adapter Pattern

## Overview

**Category:** Structural Pattern  
**Purpose:** Convert the interface of a class into another interface clients expect, allowing incompatible interfaces to work together.  
**Also Called:** Wrapper, Translator  
**Complexity:** Low-Medium

## Problem

Two components need to work together but have incompatible interfaces:

```csharp
// Existing class with different interface
public class OldPaymentSystem
{
    public bool ProcessPayment(double amount) { ... }
}

// New interface expected by application
public interface IPaymentGateway
{
    PaymentResponse Pay(PaymentRequest request);
}

// Can't use old system with new interface
IPaymentGateway gateway = new OldPaymentSystem();  // COMPILE ERROR!
```

## Solution

Create an adapter that translates between the two interfaces:

```csharp
public class PaymentAdapter : IPaymentGateway
{
    private OldPaymentSystem _oldSystem;

    public PaymentAdapter(OldPaymentSystem oldSystem)
    {
        _oldSystem = oldSystem;
    }

    public PaymentResponse Pay(PaymentRequest request)
    {
        bool success = _oldSystem.ProcessPayment(request.Amount);
        return new PaymentResponse(success, "Processed", Guid.NewGuid().ToString());
    }
}

// Now it works!
IPaymentGateway gateway = new PaymentAdapter(oldSystem);
```

## Implementation Approaches

### 1. Class Adapter (Inheritance)

```csharp
// Uses inheritance
public class Adaptee
{
    public void SpecificMethod() => Console.WriteLine("Specific");
}

public interface ITarget
{
    void Request();
}

// Class adapter inherits from Adaptee
public class ClassAdapter : Adaptee, ITarget
{
    public void Request()
    {
        SpecificMethod();  // Call inherited method
    }
}

// Usage
ITarget target = new ClassAdapter();
target.Request();  // Works!
```

**Pros:** Can override Adaptee methods  
**Cons:** Java/C# single inheritance limitation

### 2. Object Adapter (Composition)

```csharp
// Uses composition (recommended)
public class ObjectAdapter : ITarget
{
    private readonly Adaptee _adaptee;

    public ObjectAdapter(Adaptee adaptee)
    {
        _adaptee = adaptee;
    }

    public void Request()
    {
        _adaptee.SpecificMethod();  // Delegate to adaptee
    }
}

// Usage
var adaptee = new Adaptee();
ITarget target = new ObjectAdapter(adaptee);
target.Request();
```

**Pros:**
✅ More flexible  
✅ Can adapt subclasses too  
✅ Preferred design  

**Cons:**
❌ Extra level of indirection

### 3. Payment Gateway Adapter

```csharp
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
                "Payment processed",
                Guid.NewGuid().ToString()
            );
        }
        catch (Exception ex)
        {
            return new PaymentResponse(false, $"Error: {ex.Message}", "");
        }
    }
}
```

### 4. Data Format Adapter

```csharp
// Old JSON provider
public class JsonDataProvider
{
    public string GetJsonData() => "{\"name\": \"John\", \"age\": 30}";
}

// New XML interface
public interface IXmlDataProvider
{
    string GetXmlData();
}

// Adapter
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
        // Parse JSON and convert to XML
        return "<root><name>John</name><age>30</age></root>";
    }
}
```

### 5. Collection Adapter

```csharp
// Old-style array
public class LegacyArray
{
    private string[] _items;

    public int Count => _items.Length;
    public string GetItem(int index) => _items[index];
}

// New collection interface
public interface IModernCollection : IEnumerable<string>
{
    void Add(string item);
    void Remove(string item);
}

// Adapter
public class LegacyArrayAdapter : IModernCollection
{
    private readonly LegacyArray _legacy;
    private List<string> _items = new();

    public LegacyArrayAdapter(LegacyArray legacy)
    {
        _legacy = legacy;
        for (int i = 0; i < legacy.Count; i++)
            _items.Add(legacy.GetItem(i));
    }

    public void Add(string item) => _items.Add(item);
    public void Remove(string item) => _items.Remove(item);

    public IEnumerator<string> GetEnumerator() => _items.GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
```

---

## Real-World Examples

### Voltage Conversion
```csharp
// Old system: 220V
public class Voltage220V
{
    public double GetVoltage() => 220.0;
}

// New interface: 110V
public interface IVoltage110V
{
    double GetVoltage();
}

// Adapter
public class VoltageAdapter : IVoltage110V
{
    private readonly Voltage220V _voltage220;

    public VoltageAdapter(Voltage220V voltage220)
    {
        _voltage220 = voltage220;
    }

    public double GetVoltage() => _voltage220.GetVoltage() / 2;
}
```

### Legacy Database
```csharp
// Old database interface
public class OldDatabase
{
    public bool Insert(string table, Dictionary<string, object> data) { ... }
}

// New interface
public interface IDatabase
{
    void Insert<T>(T entity) where T : class;
}

// Adapter
public class OldDatabaseAdapter : IDatabase
{
    private readonly OldDatabase _oldDb;

    public OldDatabaseAdapter(OldDatabase oldDb)
    {
        _oldDb = oldDb;
    }

    public void Insert<T>(T entity) where T : class
    {
        var table = typeof(T).Name;
        var data = new Dictionary<string, object> { { "entity", entity } };
        _oldDb.Insert(table, data);
    }
}
```

---

## Class vs. Object Adapter

| Aspect | Class Adapter | Object Adapter |
|--------|---------------|----------------|
| **Mechanism** | Inheritance | Composition |
| **Flexibility** | Limited | High |
| **Adapting Subclasses** | No | Yes |
| **Method Override** | Possible | Limited |
| **Dependencies** | Tight (inheritance) | Loose (composition) |
| **Performance** | Slightly faster | Slight overhead |

---

## Pros and Cons

### Advantages
✅ **Reuse Legacy Code** - Use old code with new interfaces  
✅ **No Modification** - Don't need to change existing code  
✅ **Single Responsibility** - Translation logic isolated  
✅ **Compatibility** - Makes incompatible code work together  
✅ **Flexibility** - Compose adapters  

### Disadvantages
❌ **Extra Complexity** - Additional class  
❌ **Indirection** - Extra layer between client and target  
❌ **Performance** - Minor overhead  
❌ **Too Many Adapters** - Can become tangled  

---

## When to Use

### ✅ Use Adapter When:
- Integrating third-party libraries
- Using legacy code
- Working with different interfaces
- Can't modify existing code
- Need compatibility layer
- Connecting incompatible systems

### ❌ Don't Use When:
- Can modify code directly
- Interfaces are compatible
- Extra complexity not justified
- Performance critical
- Simple wrapper not needed

---

## Related Patterns

- **Bridge** - Similar structure, but different intent
- **Decorator** - Similar structure, adds functionality
- **Facade** - Simplifies complex interface
- **Proxy** - Controls access to object

---

## Interview Questions

**Q: What's the difference between Adapter and Decorator?**
A: Adapter converts interfaces; Decorator adds functionality to existing interface.

**Q: When would you use Object Adapter over Class Adapter?**
A: Object Adapter is more flexible and works with subclasses. Preferred in most cases.

**Q: Can an Adapter work with multiple Adaptees?**
A: Yes, an adapter can adapt multiple compatible objects.

**Q: Is Adapter related to Facade?**
A: Similar structure but different intent. Facade simplifies; Adapter translates.

---

## Summary

The Adapter pattern solves incompatibility between interfaces by introducing a translation layer. Use Object Adapter (composition) over Class Adapter (inheritance) for flexibility. Perfect for integrating legacy code and third-party libraries without modifying them.

**Key Takeaway:** Adapter makes incompatible interfaces work together through translation.
