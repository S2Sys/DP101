namespace DP101.Core.CreationalPatterns.Singleton;

/// <summary>
/// SINGLETON PATTERN
///
/// Intent: Ensure a class has only one instance and provide a global point of access to it.
///
/// Participants:
/// - Singleton: Defines an instance operation that lets clients access its unique instance.
///
/// PROS:
/// - Controlled access to sole instance
/// - Lazy initialization possible
/// - Reduces global namespace pollution
/// - Can control number of instances
/// - Thread-safe implementation possible
///
/// CONS:
/// - Global state can make testing difficult
/// - Hides dependencies
/// - Can be overused
/// - Not suitable for all scenarios requiring single instances
/// - Can mask design problems
///
/// USE CASES:
/// - Logger instances
/// - Configuration managers
/// - Database connection pools
/// - Thread pools
/// - Caches
/// - Application settings managers
///
/// WHEN TO USE:
/// - When exactly one instance of a class is needed
/// - When creation is expensive
/// - When global access is acceptable
/// - When lazy initialization is beneficial
///
/// WHEN NOT TO USE:
/// - When multiple instances are needed or might be needed later
/// - When dependency injection is available
/// - When state management is complex
/// - For testability, prefer DI over Singleton
/// </summary>

// 1. BASIC SINGLETON (NOT THREAD-SAFE - DO NOT USE IN MULTI-THREADED ENVIRONMENTS)
public sealed class BasicSingleton
{
    private static BasicSingleton? _instance;

    private BasicSingleton()
    {
    }

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

    public string GetInfo() => "Basic Singleton Instance";
}

// 2. THREAD-SAFE SINGLETON WITH LOCK
public sealed class ThreadSafeLockingSingleton
{
    private static ThreadSafeLockingSingleton? _instance;
    private static readonly object _lockObject = new();

    private ThreadSafeLockingSingleton()
    {
    }

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

    public string GetInfo() => "Thread-Safe Lock Singleton Instance";
}

// 3. DOUBLE-CHECKED LOCKING SINGLETON (OPTIMIZED FOR PERFORMANCE)
public sealed class DoubleCheckedLockingSingleton
{
    private static volatile DoubleCheckedLockingSingleton? _instance;
    private static readonly object _lockObject = new();

    private DoubleCheckedLockingSingleton()
    {
    }

    public static DoubleCheckedLockingSingleton Instance
    {
        get
        {
            // First check without lock (for performance)
            if (_instance == null)
            {
                lock (_lockObject)
                {
                    // Second check with lock (for thread safety)
                    if (_instance == null)
                    {
                        _instance = new DoubleCheckedLockingSingleton();
                    }
                }
            }

            return _instance;
        }
    }

    public string GetInfo() => "Double-Checked Locking Singleton Instance";
}

// 4. LAZY<T> SINGLETON (RECOMMENDED - THREAD-SAFE WITH LAZY INITIALIZATION)
public sealed class LazySingleton
{
    private static readonly Lazy<LazySingleton> _instance =
        new(() => new LazySingleton());

    private LazySingleton()
    {
    }

    public static LazySingleton Instance => _instance.Value;

    public string GetInfo() => "Lazy<T> Singleton Instance";
}

// 5. BILL PUGH SINGLETON (USING STATIC CONSTRUCTOR)
public sealed class BillPughSingleton
{
    private static readonly SingletonHolder _holder = new();

    private BillPughSingleton()
    {
    }

    public static BillPughSingleton Instance => _holder.Instance;

    public string GetInfo() => "Bill Pugh Singleton Instance";

    // Inner class holder - ensures thread-safe lazy initialization
    private class SingletonHolder
    {
        public BillPughSingleton Instance { get; } = new BillPughSingleton();

        // Static constructor ensures thread safety
        static SingletonHolder()
        {
        }
    }
}

// 6. GENERIC SINGLETON BASE CLASS (REUSABLE)
public abstract class SingletonBase<T> where T : SingletonBase<T>
{
    private static readonly Lazy<T> _instance =
        new(() =>
        {
            var constructor = typeof(T).GetConstructor(Type.EmptyTypes);
            if (constructor == null)
                throw new InvalidOperationException($"Class {typeof(T).Name} must have a parameterless constructor");

            return (T)constructor.Invoke(null)!;
        });

    protected SingletonBase()
    {
    }

    public static T Instance => _instance.Value;
}

// Example usage of generic singleton
public sealed class GenericSingletonExample : SingletonBase<GenericSingletonExample>
{
    public string GetInfo() => "Generic Singleton Instance";
}
