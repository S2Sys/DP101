# Object Pool Pattern

## Overview

**Category:** Creational Pattern  
**Purpose:** Reuse expensive objects by pooling them instead of creating/destroying repeatedly.  
**Complexity:** Medium  
**Key Concept:** Resource management and reusability

## Problem

Creating and destroying expensive objects repeatedly wastes resources:
- **Database Connections** - Opening/closing is slow
- **Thread Creation** - OS resource allocation is expensive
- **Socket Connections** - Network setup overhead
- **Large Buffers** - Memory allocation expensive
- **Graphics Resources** - GPU memory allocation slow

Example: Creating new database connection for each query is inefficient

## Solution

Maintain a pool of reusable objects. Acquire from pool when needed, return when done:

```csharp
var pool = new ObjectPool<Connection>(() => new Connection(), 10, 50);
var connection = pool.Acquire();
try
{
    connection.Query("SELECT ...");
}
finally
{
    pool.Release(connection);
}
```

## Implementation Approaches

### 1. Generic Object Pool

```csharp
public interface IPoolable
{
    void Reset();
    bool IsValid();
}

public class ObjectPool<T> where T : IPoolable
{
    private Stack<T> _available;
    private HashSet<T> _inUse;
    private Func<T> _factory;
    private int _maxSize;
    private object _lockObject = new();

    public ObjectPool(Func<T> factory, int initialSize = 10, int maxSize = 100)
    {
        _factory = factory;
        _maxSize = maxSize;
        _available = new Stack<T>(initialSize);
        _inUse = new HashSet<T>();

        // Pre-allocate pool
        for (int i = 0; i < initialSize; i++)
        {
            _available.Push(_factory());
        }
    }

    public T Acquire()
    {
        lock (_lockObject)
        {
            T obj;

            if (_available.Count > 0)
            {
                obj = _available.Pop();
            }
            else if (_inUse.Count < _maxSize)
            {
                obj = _factory();
            }
            else
            {
                throw new InvalidOperationException("Pool exhausted");
            }

            _inUse.Add(obj);
            return obj;
        }
    }

    public void Release(T obj)
    {
        lock (_lockObject)
        {
            if (_inUse.Remove(obj))
            {
                obj.Reset();
                _available.Push(obj);
            }
        }
    }

    public int AvailableCount => _available.Count;
    public int InUseCount => _inUse.Count;
    public int TotalCount => _available.Count + _inUse.Count;
}
```

### 2. Database Connection Pool

```csharp
public class PoolableConnection : IPoolable
{
    public string ConnectionString { get; set; }
    public bool IsOpen { get; private set; }

    public PoolableConnection(string connectionString)
    {
        ConnectionString = connectionString;
        IsOpen = true;
    }

    public void Query(string sql)
    {
        if (!IsOpen)
            throw new InvalidOperationException("Connection closed");
    }

    public void Reset()
    {
        // Clear state, keep connection open
    }

    public bool IsValid() => IsOpen;
}

public class ConnectionPool
{
    private ObjectPool<PoolableConnection> _pool;

    public ConnectionPool(string connectionString, int poolSize = 5)
    {
        _pool = new ObjectPool<PoolableConnection>(
            () => new PoolableConnection(connectionString),
            poolSize,
            poolSize * 2
        );
    }

    public PoolableConnection GetConnection() => _pool.Acquire();
    public void ReleaseConnection(PoolableConnection conn) => _pool.Release(conn);
}

// Usage
var pool = new ConnectionPool("Server=localhost;Database=MyDb", 5);
var conn = pool.GetConnection();
try
{
    conn.Query("SELECT * FROM Users");
}
finally
{
    pool.ReleaseConnection(conn);
}
```

### 3. IDisposable Pattern Integration

```csharp
public class PooledResource<T> : IDisposable where T : IPoolable
{
    private ObjectPool<T> _pool;
    private T _resource;

    public PooledResource(ObjectPool<T> pool)
    {
        _pool = pool;
        _resource = _pool.Acquire();
    }

    public T Resource => _resource;

    public void Dispose()
    {
        if (_resource != null)
        {
            _pool.Release(_resource);
            _resource = null;
        }
    }
}

// Usage with using statement
using (var pooled = new PooledResource<ByteBuffer>(bufferPool))
{
    var buffer = pooled.Resource;
    // Use buffer
}  // Automatically returned to pool
```

### 4. Thread Pool

```csharp
public class PoolableWorker : IPoolable
{
    public Action<string> Work { get; set; }

    public void DoWork(string id)
    {
        Work?.Invoke(id);
    }

    public void Reset()
    {
        Work = null;
    }

    public bool IsValid() => true;
}

public class SimpleThreadPool
{
    private ObjectPool<PoolableWorker> _workerPool;
    private Queue<Action<string>> _taskQueue = new();

    public SimpleThreadPool(int numWorkers = 4)
    {
        _workerPool = new ObjectPool<PoolableWorker>(
            () => new PoolableWorker(),
            numWorkers,
            numWorkers
        );

        for (int i = 0; i < numWorkers; i++)
        {
            var thread = new Thread(ProcessTasks);
            thread.Start();
        }
    }

    public void QueueTask(Action<string> task)
    {
        lock (_taskQueue)
        {
            _taskQueue.Enqueue(task);
            Monitor.PulseAll(_taskQueue);
        }
    }

    private void ProcessTasks()
    {
        while (true)
        {
            Action<string> task;
            lock (_taskQueue)
            {
                while (_taskQueue.Count == 0)
                    Monitor.Wait(_taskQueue);
                task = _taskQueue.Dequeue();
            }

            var worker = _workerPool.Acquire();
            try
            {
                worker.DoWork(Guid.NewGuid().ToString());
            }
            finally
            {
                _workerPool.Release(worker);
            }
        }
    }
}
```

### 5. Buffer Pool

```csharp
public class ByteBuffer : IPoolable
{
    public byte[] Data { get; }
    public int Size => Data.Length;

    public ByteBuffer(int size)
    {
        Data = new byte[size];
    }

    public void Reset()
    {
        Array.Clear(Data, 0, Data.Length);
    }

    public bool IsValid() => Data != null;
}

// Usage
var bufferPool = new ObjectPool<ByteBuffer>(() => new ByteBuffer(4096), 10, 20);
var buffer = bufferPool.Acquire();
try
{
    // Use buffer
}
finally
{
    bufferPool.Release(buffer);
}
```

---

## Pool Configurations

```
┌─────────────────────────────┐
│      ObjectPool<T>          │
├─────────────────────────────┤
│ Available Stack (Pre-allocated)
│ • Reused objects           │
│ • Reset before use         │
│ • Fast access (LIFO)       │
│                             │
│ InUse HashSet (Active)     │
│ • Current users            │
│ • Tracked for cleanup      │
│ • Prevents double release  │
│                             │
│ Factory (Creation)         │
│ • Creates new objects      │
│ • Called when pool empty   │
│ • Respects max size        │
└─────────────────────────────┘
```

---

## Real-World Examples

### Database Connection Pool
```csharp
public class ConnectionPool
{
    private ObjectPool<DbConnection> _pool;

    public ConnectionPool(string connectionString)
    {
        _pool = new ObjectPool<DbConnection>(
            () => new SqlConnection(connectionString),
            initialSize: 5,
            maxSize: 20
        );
    }

    public void ExecuteQuery(string query)
    {
        var conn = _pool.Acquire();
        try
        {
            conn.Open();
            var cmd = conn.CreateCommand();
            cmd.CommandText = query;
            cmd.ExecuteNonQuery();
        }
        finally
        {
            _pool.Release(conn);
        }
    }
}
```

### HTTP Connection Pool
```csharp
public class HttpConnectionPool
{
    private ObjectPool<HttpClient> _pool;

    public HttpConnectionPool(int poolSize = 5)
    {
        _pool = new ObjectPool<HttpClient>(
            () => new HttpClient(),
            poolSize,
            poolSize * 2
        );
    }

    public async Task<string> GetAsync(string url)
    {
        var client = _pool.Acquire();
        try
        {
            return await client.GetStringAsync(url);
        }
        finally
        {
            _pool.Release(client);
        }
    }
}
```

### Memory Buffer Pool
```csharp
public class ByteBufferPool
{
    private ObjectPool<ByteBuffer> _pool;

    public ByteBufferPool(int bufferSize = 4096, int poolSize = 10)
    {
        _pool = new ObjectPool<ByteBuffer>(
            () => new ByteBuffer(bufferSize),
            poolSize,
            poolSize * 2
        );
    }

    public ByteBuffer GetBuffer() => _pool.Acquire();
    public void ReturnBuffer(ByteBuffer buffer) => _pool.Release(buffer);
}
```

---

## Pros and Cons

### Advantages
✅ **Performance** - Reuses expensive objects  
✅ **Memory** - Reduces garbage collection pressure  
✅ **Scalability** - Controls resource usage  
✅ **Predictability** - Resource limits known  
✅ **Speed** - Faster than creating new objects  

### Disadvantages
❌ **Complexity** - Thread safety needed  
❌ **State Management** - Objects must be reset  
❌ **Memory** - Pre-allocated objects take memory  
❌ **Maintenance** - Must manage pool lifecycle  
❌ **Stale State** - Can hold onto old state if reset fails  

---

## When to Use

### ✅ Use Object Pool When:
- Object creation is expensive (DB, network, threads)
- Creating many objects frequently
- Resource limits are important
- Performance is critical
- Object is reusable and stateless (after reset)

### ❌ Don't Use When:
- Object creation is cheap
- Objects not reusable
- Simplicity is valued
- Unpredictable usage patterns
- Pool management overhead > creation cost

---

## Thread Safety

Object pools must be thread-safe:

```csharp
// Acquire and Release use locks
public T Acquire()
{
    lock (_lockObject)  // Thread-safe acquisition
    {
        // Get object safely
    }
}

public void Release(T obj)
{
    lock (_lockObject)  // Thread-safe release
    {
        // Return object safely
    }
}
```

---

## Interview Questions

**Q: Why use Object Pool instead of creating new objects?**
A: Creating expensive objects (connections, threads) is slower than reusing pooled objects.

**Q: How do you prevent state leakage in a pool?**
A: Call Reset() on each object before returning to pool to clear state.

**Q: What happens if pool is exhausted?**
A: Depends on implementation - can create more (if under max), wait, or throw exception.

**Q: How do you track pooled objects?**
A: Use InUse set to track checked-out objects; prevent double-release and enable cleanup.

**Q: Is Object Pool relevant with GC?**
A: Yes - reduces GC pressure by reusing objects; improves performance in high-throughput systems.

---

## Best Practices

1. **Implement Reset()** - Clear state completely
2. **Use Thread Safety** - Locks for Acquire/Release
3. **Set Reasonable Limits** - Max size prevents resource exhaustion
4. **Monitor Pool** - Track available/inuse counts
5. **Validate Objects** - Check IsValid() before use
6. **Handle Exceptions** - Return to pool in finally blocks
7. **Use IDisposable** - Simplifies release with using statement
8. **Test Pool Exhaustion** - Verify behavior when pool is full

---

## Summary

Object Pool is essential for managing expensive resources like database connections, threads, and network sockets. Properly implemented pools significantly improve performance in high-throughput systems by reusing objects instead of creating/destroying them repeatedly. Always ensure proper reset logic and thread safety.

**Key Takeaway:** Object Pool dramatically improves performance when creating objects is expensive.
