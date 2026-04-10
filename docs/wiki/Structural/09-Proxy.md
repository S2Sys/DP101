# Proxy Pattern

## Overview

**Category:** Structural Pattern  
**Purpose:** Provide a surrogate or placeholder for another object to control access to it.  
**Also Called:** Surrogate  
**Complexity:** Medium  
**Types:** Virtual, Protection, Smart Reference, Logging, Remote

## Problem

Sometimes you need to control access or behavior of an object:
- Creating object is expensive (lazy loading)
- Need access control
- Want to log access
- Remote objects need special handling
- Need to manage resources

```csharp
// Problem: Loading large image immediately is slow
public class Image
{
    public Image(string filename)
    {
        LoadImageFromDisk();  // SLOW!
    }

    private void LoadImageFromDisk() { /* 10 seconds */ }
    public void Display() { /* Show image */ }
}

// Client must wait even if image isn't displayed
var image = new Image("huge.jpg");  // Waits 10 seconds
// ... do other work ...
image.Display();  // Actually uses image
```

## Solution

Create a proxy that controls access:

```csharp
public interface IImage
{
    void Display();
}

public class ImageProxy : IImage
{
    private RealImage _realImage;
    private string _filename;

    public ImageProxy(string filename) => _filename = filename;

    public void Display()
    {
        // Lazy loading - only load when needed
        if (_realImage == null)
            _realImage = new RealImage(_filename);

        _realImage.Display();
    }
}

// Client can create immediately and load on demand
IImage image = new ImageProxy("huge.jpg");  // Fast, no loading
// ... do other work ...
image.Display();  // Load and display only when needed
```

## Implementation Approaches

### 1. Virtual Proxy (Lazy Loading)

```csharp
public class ImageProxy : IImage
{
    private RealImage _realImage;
    private string _filename;

    public ImageProxy(string filename) => _filename = filename;

    public void Display()
    {
        // Load only when needed
        if (_realImage == null)
        {
            Console.WriteLine($"Loading {_filename} from disk...");
            _realImage = new RealImage(_filename);
        }
        _realImage.Display();
    }
}
```

### 2. Protection Proxy (Access Control)

```csharp
public interface IBank
{
    void Withdraw(decimal amount);
    decimal GetBalance();
}

public class BankProxy : IBank
{
    private RealBank _bank;
    private string _userId;

    public BankProxy(RealBank bank, string userId)
    {
        _bank = bank;
        _userId = userId;
    }

    public void Withdraw(decimal amount)
    {
        if (!IsAuthorized())
        {
            Console.WriteLine("Access denied");
            return;
        }

        if (amount > 1000)
        {
            Console.WriteLine("Withdrawal limit exceeded");
            return;
        }

        _bank.Withdraw(amount);
    }

    public decimal GetBalance()
    {
        if (!IsAuthorized())
            return 0;

        return _bank.GetBalance();
    }

    private bool IsAuthorized() => _userId == "admin";
}
```

### 3. Smart Reference (Caching)

```csharp
public class DataProxy : IData
{
    private ExpensiveData _realData;
    private string _key;
    private static Dictionary<string, ExpensiveData> _cache = new();

    public DataProxy(string key) => _key = key;

    public string GetData()
    {
        // Check cache first
        if (_cache.ContainsKey(_key))
        {
            Console.WriteLine("Using cached data");
            return _cache[_key].GetData();
        }

        // Load and cache
        if (_realData == null)
            _realData = new ExpensiveData(_key);

        _cache[_key] = _realData;
        return _realData.GetData();
    }
}
```

### 4. Logging Proxy

```csharp
public class LoggingProxy : IService
{
    private RealService _service;
    private List<string> _logs = new();

    public LoggingProxy() => _service = new RealService();

    public void DoWork(string task)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        _logs.Add($"[{timestamp}] DoWork: {task}");
        Console.WriteLine($"[LOG] {task}");

        _service.DoWork(task);
    }

    public List<string> GetLogs() => _logs;
}
```

### 5. Security Proxy

```csharp
public class SecurityProxy : ISensitiveOperation
{
    private RealSensitiveOperation _operation;
    private string _userId;
    private Dictionary<string, List<string>> _permissions;

    public SecurityProxy(string userId)
    {
        _operation = new RealSensitiveOperation();
        _userId = userId;
        SetupPermissions();
    }

    public void ExecuteSensitiveOperation(string operation)
    {
        if (HasPermission(operation))
        {
            Console.WriteLine($"[AUDIT] {_userId} executing: {operation}");
            _operation.ExecuteSensitiveOperation(operation);
        }
        else
        {
            Console.WriteLine($"[SECURITY] Access denied for {_userId}");
        }
    }

    private bool HasPermission(string operation) =>
        _permissions[_userId].Contains(operation);

    private void SetupPermissions()
    {
        _permissions = new Dictionary<string, List<string>>
        {
            { "admin", new List<string> { "delete", "create", "modify" } },
            { "user", new List<string> { "modify" } },
            { "guest", new List<string>() }
        };
    }
}
```

---

## Proxy Types Comparison

| Type | Purpose | Use Case |
|------|---------|----------|
| **Virtual** | Lazy loading | Expensive object creation |
| **Protection** | Access control | Restrict operations |
| **Smart Ref** | Resource management | Reference counting, caching |
| **Logging** | Audit trail | Security/debugging |
| **Remote** | Network access | Distributed systems |

---

## Proxy vs. Similar Patterns

| Pattern | Purpose | Structure |
|---------|---------|-----------|
| **Proxy** | Control access | Same interface, restricts operations |
| **Decorator** | Add functionality | Same interface, adds behavior |
| **Facade** | Simplify | Different interface, simpler |
| **Adapter** | Translate | Different interface, translates |

---

## Real-World Examples

### Real Estate Listing
```csharp
public class HouseProxy : IHouse
{
    private RealHouse _house;
    private string _address;

    public HouseProxy(string address) => _address = address;

    public void ShowDetails()
    {
        if (_house == null)
            _house = new RealHouse(_address);  // Load details on demand

        _house.ShowDetails();
    }
}
```

### Remote Service
```csharp
public class RemoteServiceProxy : IRemoteService
{
    public string CallRemoteMethod(string param)
    {
        // Add network overhead handling
        Console.WriteLine("[REMOTE] Calling service...");
        System.Threading.Thread.Sleep(100);  // Network latency

        var service = new RealRemoteService();
        return service.CallRemoteMethod(param);
    }
}
```

### Reference Counting
```csharp
public class ResourceProxy : IResource
{
    private RealResource _resource;
    private static int _refCount = 0;

    public ResourceProxy()
    {
        if (_resource == null)
            _resource = new RealResource();

        _refCount++;
    }

    public void Use() => _resource.Use();

    ~ResourceProxy()
    {
        _refCount--;
        if (_refCount == 0)
            _resource = null;  // Cleanup
    }
}
```

---

## Pros and Cons

### Advantages
✅ **Lazy Loading** - Defer expensive operations  
✅ **Access Control** - Restrict operations  
✅ **Logging/Auditing** - Track access  
✅ **Resource Management** - Control creation/destruction  
✅ **Transparency** - Client sees same interface  

### Disadvantages
❌ **Extra Indirection** - Additional layer  
❌ **Complexity** - More classes  
❌ **Performance** - Slight overhead  
❌ **Debugging** - Harder to trace  

---

## When to Use

### ✅ Use Proxy When:
- Need lazy loading
- Want access control
- Logging/auditing needed
- Remote object access
- Resource management critical
- Need reference counting

### ❌ Don't Use When:
- No special access needed
- Performance critical
- Simple direct access sufficient
- Simplicity valued

---

## Interview Questions

**Q: What's the difference between Proxy and Decorator?**
A: Proxy controls access; Decorator adds functionality. Both wrap objects.

**Q: When would you use Virtual Proxy?**
A: When object creation is expensive and might not be needed (lazy loading).

**Q: How does Protection Proxy enforce access?**
A: By checking permissions before delegating to real object.

**Q: Can a Proxy modify data before passing to real object?**
A: Yes, common in logging/validation proxies.

---

## Summary

Proxy provides controlled access to another object through a surrogate. Perfect for lazy loading (Virtual), access control (Protection), caching (Smart Reference), logging (Logging), and remote access. Adds indirection but provides powerful control mechanisms.

**Key Takeaway:** Proxy controls access and behavior of objects through a surrogate.
