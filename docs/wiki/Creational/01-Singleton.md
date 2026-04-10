# Singleton Pattern

## Overview

**Category:** Creational Pattern  
**Purpose:** Ensure a class has only one instance and provide a global point of access to it.

## Problem

Many applications need exactly one instance of a particular class:
- A logger that writes to a single log file
- A database connection pool
- A configuration manager
- An application state manager
- A cache manager

Creating multiple instances can:
- Waste resources
- Lead to inconsistent state
- Cause synchronization problems
- Violate business rules

## Solution

The Singleton pattern ensures that only one instance of a class exists throughout the application's lifetime and provides a global point of access to that instance.

### Key Characteristics

1. **Private Constructor** - Prevents instantiation from outside the class
2. **Static Instance** - Holds the single instance
3. **Static Access Method** - Provides global access (usually `Instance` property)
4. **Thread Safety** - Ensures safe initialization in multi-threaded environments

## Implementation Approaches

### 1. Basic Singleton (Not Thread-Safe)

```csharp
public sealed class BasicSingleton
{
    private static BasicSingleton _instance;

    private BasicSingleton() { }

    public static BasicSingleton Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new BasicSingleton();
            }
            return _instance;
        }
    }
}
```

**When to use:** Single-threaded applications only. DO NOT use in multi-threaded environments.

### 2. Thread-Safe with Lock

```csharp
public sealed class ThreadSafeLockingSingleton
{
    private static ThreadSafeLockingSingleton _instance;
    private static readonly object _lockObject = new();

    private ThreadSafeLockingSingleton() { }

    public static ThreadSafeLockingSingleton Instance
    {
        get
        {
            lock (_lockObject)
            {
                if (_instance == null)
                {
                    _instance = new ThreadSafeLockingSingleton();
                }
            }
            return _instance;
        }
    }
}
```

**Pros:** Simple, thread-safe  
**Cons:** Lock acquisition on every call (performance overhead)

### 3. Double-Checked Locking (Optimized)

```csharp
public sealed class DoubleCheckedLockingSingleton
{
    private static volatile DoubleCheckedLockingSingleton _instance;
    private static readonly object _lockObject = new();

    private DoubleCheckedLockingSingleton() { }

    public static DoubleCheckedLockingSingleton Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lockObject)
                {
                    if (_instance == null)
                    {
                        _instance = new DoubleCheckedLockingSingleton();
                    }
                }
            }
            return _instance;
        }
    }
}
```

**Pros:** Reduces lock contention after initialization  
**Cons:** Complex, requires `volatile` keyword

### 4. Lazy<T> Singleton (Recommended)

```csharp
public sealed class LazySingleton
{
    private static readonly Lazy<LazySingleton> _instance =
        new(() => new LazySingleton());

    private LazySingleton() { }

    public static LazySingleton Instance => _instance.Value;
}
```

**Pros:** Thread-safe, efficient, clean code, built-in .NET feature  
**Cons:** Requires .NET Framework 4.0+

### 5. Bill Pugh Singleton (Static Constructor)

```csharp
public sealed class BillPughSingleton
{
    private static readonly SingletonHolder _holder = new();

    private BillPughSingleton() { }

    public static BillPughSingleton Instance => _holder.Instance;

    private class SingletonHolder
    {
        public BillPughSingleton Instance { get; } = new BillPughSingleton();
        
        static SingletonHolder() { }
    }
}
```

**Pros:** Thread-safe via static constructor, excellent for advanced scenarios  
**Cons:** Requires understanding of static constructor semantics

## Class Diagram

```
┌──────────────────────────────┐
│       Singleton              │
├──────────────────────────────┤
│ - instance: Singleton        │
├──────────────────────────────┤
│ - Singleton()                │
│ + getInstance(): Singleton   │
└──────────────────────────────┘
```

## Pros and Cons

### Advantages

✅ **Controlled Access** - Single, controlled point of access  
✅ **Lazy Initialization** - Instance created only when needed  
✅ **Reduced Namespace Pollution** - No global variables  
✅ **Thread Safety** - Can be implemented safely for multi-threaded environments  
✅ **Subclassing** - Can be subclassed (careful implementation required)  
✅ **Instance Control** - Can extend to control number of instances

### Disadvantages

❌ **Global State** - Effectively a global variable, makes code harder to reason about  
❌ **Testing Difficulties** - Hard to mock or substitute for testing  
❌ **Hidden Dependencies** - Dependencies aren't explicit in method signatures  
❌ **Concurrency Complexity** - Proper thread-safe implementation is complex  
❌ **Easy to Misuse** - Often used when not necessary  
❌ **Hides Design Issues** - May mask problems in your architecture  

## Real-World Examples in .NET

### Logger

```csharp
public sealed class Logger
{
    private static readonly Lazy<Logger> _instance = new(() => new Logger());
    private readonly List<string> _logs = new();

    private Logger() { }

    public static Logger Instance => _instance.Value;

    public void Log(string message) => _logs.Add(message);
}

// Usage
Logger.Instance.Log("Application started");
```

### Database Connection

```csharp
public sealed class DatabaseConnection
{
    private static readonly Lazy<DatabaseConnection> _instance =
        new(() => new DatabaseConnection());
    private bool _isConnected;

    private DatabaseConnection() { }

    public static DatabaseConnection Instance => _instance.Value;

    public void Connect() => _isConnected = true;
    public bool IsConnected => _isConnected;
}

// Usage
DatabaseConnection.Instance.Connect();
if (DatabaseConnection.Instance.IsConnected)
{
    // Execute queries
}
```

### Configuration Manager

```csharp
public sealed class ConfigurationManager
{
    private static readonly Lazy<ConfigurationManager> _instance =
        new(() => new ConfigurationManager());
    private readonly Dictionary<string, object> _settings = new();

    private ConfigurationManager()
    {
        _settings["AppName"] = "MyApp";
        _settings["Version"] = "1.0.0";
    }

    public static ConfigurationManager Instance => _instance.Value;

    public object GetSetting(string key) => _settings[key];
}

// Usage
var appName = ConfigurationManager.Instance.GetSetting("AppName");
```

### Cache Manager

```csharp
public sealed class CacheManager
{
    private static readonly Lazy<CacheManager> _instance =
        new(() => new CacheManager());
    private readonly Dictionary<string, object> _cache = new();

    private CacheManager() { }

    public static CacheManager Instance => _instance.Value;

    public void Set<T>(string key, T value) => _cache[key] = value;

    public T Get<T>(string key) => (T)_cache[key];
}

// Usage
CacheManager.Instance.Set("user:1", new User { Id = 1, Name = "John" });
var user = CacheManager.Instance.Get<User>("user:1");
```

## Use Cases

### When to Use Singleton

1. **Logger instances** - Single log file for entire application
2. **Configuration managers** - Centralized application settings
3. **Database connection pools** - Manage database connections
4. **Thread pools** - Manage worker threads
5. **Caches** - Application-wide caching
6. **Session managers** - Track user sessions
7. **Print spoolers** - Queue and manage print jobs
8. **Window managers** - In GUI applications

### When NOT to Use Singleton

❌ When multiple instances might be needed later  
❌ When using dependency injection containers (prefer DI)  
❌ When testing is important (hard to mock)  
❌ When your instance needs configuration parameters  
❌ When using async/await heavily (consider scoped DI instead)  
❌ When the instance manages external resources  
❌ In domain models or business entities  

## Alternatives and Variations

### 1. Service Locator Pattern
Similar but with a registry to look up services.

### 2. Dependency Injection
Modern alternative - inject dependencies instead of using singletons.

```csharp
// Instead of singleton
public class MyService
{
    private static readonly Lazy<MyService> _instance =
        new(() => new MyService());

    public static MyService Instance => _instance.Value;
}

// Use DI
services.AddSingleton<MyService>();

public class MyController
{
    public MyController(MyService service) { }
}
```

### 3. Registry Pattern
Centralized registry of objects.

## Thread Safety Comparison

| Approach | Thread-Safe | Lazy Init | Performance | Complexity |
|----------|:-----------:|:---------:|:-----------:|:----------:|
| Basic | ❌ | ✅ | ⭐⭐⭐ | Low |
| Lock | ✅ | ✅ | ⭐⭐ | Low |
| Double-Check | ✅ | ✅ | ⭐⭐⭐ | High |
| Lazy<T> | ✅ | ✅ | ⭐⭐⭐ | Low |
| Bill Pugh | ✅ | ✅ | ⭐⭐⭐ | Medium |

## Testing Considerations

### Challenges with Singleton

- **Hard to Mock** - Instance is static
- **Test Isolation** - Tests may interfere with each other
- **State Persistence** - Singleton state carries between tests

### Testing Strategies

```csharp
// 1. Create a test double that implements the same interface
public interface ILogger
{
    void Log(string message);
}

public sealed class TestLogger : ILogger
{
    public void Log(string message) { /* test implementation */ }
}

// 2. Use dependency injection instead of singleton
public class MyService
{
    private readonly ILogger _logger;
    
    public MyService(ILogger logger) => _logger = logger;
}

// 3. Provide a reset mechanism for testing
public sealed class Logger
{
    private static Logger _instance = new();
    
    public static void ResetForTesting() => _instance = new();
}
```

## Best Practices

1. **Use Lazy<T>** - Most modern and recommended approach
2. **Mark as sealed** - Prevent accidental inheritance
3. **Keep it simple** - If complex logic needed, reconsider the pattern
4. **Consider DI instead** - Modern applications prefer dependency injection
5. **Document carefully** - Explain why singleton is necessary
6. **Test thoroughly** - Especially multi-threaded scenarios
7. **Provide reset** - In testing, allow singleton state reset
8. **Use interfaces** - Makes testing and future refactoring easier

## Interview Questions

**Q: What's wrong with using singletons?**  
A: Global state, hard to test, hides dependencies, can be overused. Modern practice prefers dependency injection.

**Q: How do you make a singleton thread-safe in C#?**  
A: Use `Lazy<T>`, double-checked locking, or static constructor (Bill Pugh). `Lazy<T>` is recommended.

**Q: How do you test a singleton?**  
A: Use interfaces to mock, provide reset methods, or refactor to use dependency injection.

**Q: Can a singleton be inherited?**  
A: Not well - mark it sealed. If you need inheritance, reconsider the pattern.

**Q: What's the difference between Singleton and Static Class?**  
A: Singleton is an object instance (can implement interfaces), static class is not (purely procedural). Singleton can be polymorphic.

## Related Patterns

- **Factory Method** - Can use singleton factory
- **Service Locator** - Similar but uses registry instead of static access
- **Facade** - Often implemented as singleton
- **Abstract Factory** - Can return singleton instances
- **Observer** - Often implemented with singleton subject

## References

- Gang of Four Design Patterns
- C# Design Patterns
- Dependency Injection in .NET
- ASP.NET Core Dependency Injection

---

**Recommendation:** In modern .NET applications, use `Lazy<T>` for singleton pattern if needed, but prefer dependency injection with `AddSingleton` in your service container.
