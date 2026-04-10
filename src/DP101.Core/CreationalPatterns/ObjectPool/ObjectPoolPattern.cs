namespace DP101.Core.CreationalPatterns.ObjectPool;

/// <summary>
/// OBJECT POOL PATTERN
///
/// Intent: Use a pool of reusable objects to avoid expensive allocation and deallocation.
///
/// Participants:
/// - Reusable: Objects that are expensive to create
/// - ObjectPool: Manages pool of reusable objects
/// - Client: Uses objects from the pool
///
/// PROS:
/// - Improves performance by reusing objects
/// - Reduces garbage collection pressure
/// - Provides control over resource usage
/// - Predictable memory usage
/// - Useful for expensive resources (DB connections, threads)
///
/// CONS:
/// - Increased complexity
/// - Synchronization overhead
/// - Must manage object state properly
/// - Not suitable for all objects
/// - Harder to debug
///
/// USE CASES:
/// - Database connections
/// - Thread pools
/// - Network connections
/// - Socket pools
/// - Large buffer allocations
/// - Graphics resources
///
/// WHEN TO USE:
/// - Object creation is expensive
/// - High frequency of allocation/deallocation
/// - Need to limit resource usage
/// - Performance is critical
///
/// WHEN NOT TO USE:
/// - Object creation is cheap
/// - Objects don't hold resources
/// - Memory is not a concern
/// - Simplicity is valued
/// </summary>

// POOLABLE INTERFACE

/// <summary>
/// Interface for objects that can be pooled
/// </summary>
public interface IPoolable
{
    void Reset();
    bool IsValid();
}

// SIMPLE OBJECT POOL

/// <summary>
/// Generic object pool implementation
/// </summary>
public class ObjectPool<T> where T : IPoolable
{
    private readonly Stack<T> _available;
    private readonly HashSet<T> _inUse;
    private readonly Func<T> _factory;
    private readonly int _maxSize;
    private readonly object _lockObject = new();

    public ObjectPool(Func<T> factory, int initialSize = 10, int maxSize = 100)
    {
        _factory = factory;
        _maxSize = maxSize;
        _available = new Stack<T>(initialSize);
        _inUse = [];

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
                throw new InvalidOperationException("Object pool exhausted");
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

    public int AvailableCount
    {
        get
        {
            lock (_lockObject)
            {
                return _available.Count;
            }
        }
    }

    public int InUseCount
    {
        get
        {
            lock (_lockObject)
            {
                return _inUse.Count;
            }
        }
    }

    public int TotalCount
    {
        get
        {
            lock (_lockObject)
            {
                return _available.Count + _inUse.Count;
            }
        }
    }
}

// POOLABLE BUFFER

/// <summary>
/// Expensive poolable resource - large buffer
/// </summary>
public class ByteBuffer : IPoolable
{
    private byte[] _data;
    public int Size { get; }

    public ByteBuffer(int size)
    {
        Size = size;
        _data = new byte[size];
    }

    public void Write(byte[] data)
    {
        if (data.Length > Size)
            throw new ArgumentException("Data too large for buffer");

        Array.Copy(data, _data, data.Length);
    }

    public byte[] Read()
    {
        return _data;
    }

    public void Reset()
    {
        Array.Clear(_data, 0, _data.Length);
    }

    public bool IsValid()
    {
        return _data != null && _data.Length == Size;
    }
}

// DATABASE CONNECTION POOL

/// <summary>
/// Poolable database connection
/// </summary>
public class PoolableConnection : IPoolable
{
    public string ConnectionString { get; set; }
    public bool IsOpen { get; private set; }
    private int _usageCount;

    public PoolableConnection(string connectionString)
    {
        ConnectionString = connectionString;
        IsOpen = true;
        _usageCount = 0;
    }

    public void ExecuteQuery(string query)
    {
        if (!IsOpen)
            throw new InvalidOperationException("Connection is not open");

        _usageCount++;
        // Simulate query execution
    }

    public void Reset()
    {
        _usageCount = 0;
        // Clear any state
    }

    public bool IsValid()
    {
        return IsOpen;
    }

    public int GetUsageCount() => _usageCount;
}

/// <summary>
/// Connection pool manager
/// </summary>
public class ConnectionPool
{
    private readonly ObjectPool<PoolableConnection> _pool;
    private readonly string _connectionString;

    public ConnectionPool(string connectionString, int poolSize = 5)
    {
        _connectionString = connectionString;
        _pool = new ObjectPool<PoolableConnection>(
            () => new PoolableConnection(connectionString),
            poolSize,
            poolSize * 2);
    }

    public PoolableConnection GetConnection()
    {
        return _pool.Acquire();
    }

    public void ReleaseConnection(PoolableConnection connection)
    {
        _pool.Release(connection);
    }

    public int AvailableConnections => _pool.AvailableCount;
    public int InUseConnections => _pool.InUseCount;
}

// USING STATEMENT WRAPPER

/// <summary>
/// Wrapper to automatically return object to pool
/// </summary>
public class PooledResource<T> : IDisposable where T : IPoolable
{
    private readonly ObjectPool<T> _pool;
    private T? _resource;
    private bool _disposed;

    public PooledResource(ObjectPool<T> pool)
    {
        _pool = pool;
        _resource = _pool.Acquire();
    }

    public T Resource
    {
        get
        {
            if (_disposed)
                throw new ObjectDisposedException("PooledResource");
            return _resource!;
        }
    }

    public void Dispose()
    {
        if (!_disposed && _resource != null)
        {
            _pool.Release(_resource);
            _resource = default;
            _disposed = true;
        }

        GC.SuppressFinalize(this);
    }

    ~PooledResource()
    {
        Dispose();
    }
}

// THREAD POOL

/// <summary>
/// Poolable worker task
/// </summary>
public class PoolableWorker : IPoolable
{
    public Action<string>? Work { get; set; }
    public string? WorkerId { get; private set; }
    private bool _isWorking;

    public void SetWork(Action<string>? work)
    {
        Work = work;
    }

    public void DoWork(string id)
    {
        WorkerId = id;
        _isWorking = true;
        Work?.Invoke(id);
        _isWorking = false;
    }

    public void Reset()
    {
        Work = null;
        WorkerId = null;
        _isWorking = false;
    }

    public bool IsValid()
    {
        return !_isWorking;
    }
}

/// <summary>
/// Simple thread pool implementation
/// </summary>
public class SimpleThreadPool
{
    private readonly ObjectPool<PoolableWorker> _workerPool;
    private readonly Queue<Action<string>> _taskQueue = [];
    private readonly object _queueLock = new();

    public SimpleThreadPool(int numWorkers = 4)
    {
        _workerPool = new ObjectPool<PoolableWorker>(
            () => new PoolableWorker(),
            numWorkers,
            numWorkers);

        // Start worker threads
        for (int i = 0; i < numWorkers; i++)
        {
            var thread = new Thread(ProcessTasks) { IsBackground = true };
            thread.Start();
        }
    }

    public void QueueTask(Action<string> task)
    {
        lock (_queueLock)
        {
            _taskQueue.Enqueue(task);
            Monitor.PulseAll(_queueLock);
        }
    }

    private void ProcessTasks()
    {
        while (true)
        {
            Action<string>? task = null;

            lock (_queueLock)
            {
                while (_taskQueue.Count == 0)
                {
                    Monitor.Wait(_queueLock);
                }

                task = _taskQueue.Dequeue();
            }

            if (task != null)
            {
                var worker = _workerPool.Acquire();
                try
                {
                    worker.SetWork(task);
                    worker.DoWork(Guid.NewGuid().ToString());
                }
                finally
                {
                    _workerPool.Release(worker);
                }
            }
        }
    }
}

// POOLABLE ARRAY

/// <summary>
/// Array wrapper with pooling support
/// </summary>
public class PoolableArray<T> : IPoolable
{
    public T[] Data { get; }
    public int Length => Data.Length;

    public PoolableArray(int size)
    {
        Data = new T[size];
    }

    public void Reset()
    {
        Array.Clear(Data, 0, Data.Length);
    }

    public bool IsValid()
    {
        return Data != null;
    }

    public void Set(int index, T value)
    {
        Data[index] = value;
    }

    public T Get(int index)
    {
        return Data[index];
    }
}
