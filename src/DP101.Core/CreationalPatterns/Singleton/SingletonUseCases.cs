namespace DP101.Core.CreationalPatterns.Singleton;

// REAL-WORLD SINGLETON USE CASES

/// <summary>
/// LOGGER SINGLETON
/// A thread-safe logger instance used throughout the application.
/// </summary>
public sealed class Logger
{
    private static readonly Lazy<Logger> _instance = new(() => new Logger());
    private readonly List<string> _logs = [];

    private Logger()
    {
    }

    public static Logger Instance => _instance.Value;

    public void Log(string message)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        var logEntry = $"[{timestamp}] {message}";
        _logs.Add(logEntry);
        Console.WriteLine(logEntry);
    }

    public IReadOnlyList<string> GetAllLogs() => _logs.AsReadOnly();

    public void ClearLogs() => _logs.Clear();
}

/// <summary>
/// DATABASE CONNECTION SINGLETON
/// Manages a single database connection for the application.
/// </summary>
public sealed class DatabaseConnection
{
    private static readonly Lazy<DatabaseConnection> _instance = new(() => new DatabaseConnection());
    private bool _isConnected;

    private DatabaseConnection()
    {
        _isConnected = false;
    }

    public static DatabaseConnection Instance => _instance.Value;

    public bool IsConnected => _isConnected;

    public void Connect()
    {
        _isConnected = true;
        Logger.Instance.Log("Database connected");
    }

    public void Disconnect()
    {
        _isConnected = false;
        Logger.Instance.Log("Database disconnected");
    }

    public void ExecuteQuery(string query)
    {
        if (!_isConnected)
            throw new InvalidOperationException("Database is not connected");

        Logger.Instance.Log($"Executing query: {query}");
    }
}

/// <summary>
/// CONFIGURATION MANAGER SINGLETON
/// Manages application configuration settings centrally.
/// </summary>
public sealed class ConfigurationManager
{
    private static readonly Lazy<ConfigurationManager> _instance = new(() => new ConfigurationManager());
    private readonly Dictionary<string, object> _settings = [];

    private ConfigurationManager()
    {
        // Load default configuration
        _settings["AppName"] = "DP101 Application";
        _settings["Version"] = "1.0.0";
        _settings["Environment"] = "Development";
    }

    public static ConfigurationManager Instance => _instance.Value;

    public object GetSetting(string key)
    {
        return _settings.TryGetValue(key, out var value) ?
            value :
            throw new KeyNotFoundException($"Setting '{key}' not found");
    }

    public void SetSetting(string key, object value)
    {
        _settings[key] = value;
    }

    public bool HasSetting(string key) => _settings.ContainsKey(key);

    public IReadOnlyDictionary<string, object> GetAllSettings() => _settings.AsReadOnly();
}

/// <summary>
/// APPLICATION STATE MANAGER SINGLETON
/// Manages global application state.
/// </summary>
public sealed class ApplicationState
{
    private static readonly Lazy<ApplicationState> _instance = new(() => new ApplicationState());
    private readonly Dictionary<string, object> _state = [];
    private readonly object _lockObject = new();

    private ApplicationState()
    {
    }

    public static ApplicationState Instance => _instance.Value;

    public void SetState(string key, object value)
    {
        lock (_lockObject)
        {
            _state[key] = value;
        }
    }

    public object? GetState(string key)
    {
        lock (_lockObject)
        {
            return _state.TryGetValue(key, out var value) ? value : null;
        }
    }

    public void ClearState()
    {
        lock (_lockObject)
        {
            _state.Clear();
        }
    }

    public int GetStateCount()
    {
        lock (_lockObject)
        {
            return _state.Count;
        }
    }
}

/// <summary>
/// CACHE MANAGER SINGLETON
/// Provides caching functionality across the application.
/// </summary>
public sealed class CacheManager
{
    private static readonly Lazy<CacheManager> _instance = new(() => new CacheManager());
    private readonly Dictionary<string, CacheEntry> _cache = [];
    private readonly object _lockObject = new();

    private CacheManager()
    {
    }

    public static CacheManager Instance => _instance.Value;

    public void Set<T>(string key, T value, TimeSpan? expiration = null)
    {
        lock (_lockObject)
        {
            var entry = new CacheEntry
            {
                Value = value,
                CreatedAt = DateTime.Now,
                ExpiresAt = expiration.HasValue ? DateTime.Now.Add(expiration.Value) : null
            };
            _cache[key] = entry;
        }
    }

    public T? Get<T>(string key)
    {
        lock (_lockObject)
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                // Check if expired
                if (entry.ExpiresAt.HasValue && DateTime.Now > entry.ExpiresAt)
                {
                    _cache.Remove(key);
                    return default;
                }

                return (T?)entry.Value;
            }

            return default;
        }
    }

    public bool Exists(string key)
    {
        lock (_lockObject)
        {
            if (_cache.TryGetValue(key, out var entry))
            {
                if (entry.ExpiresAt.HasValue && DateTime.Now > entry.ExpiresAt)
                {
                    _cache.Remove(key);
                    return false;
                }

                return true;
            }

            return false;
        }
    }

    public void Remove(string key)
    {
        lock (_lockObject)
        {
            _cache.Remove(key);
        }
    }

    public void Clear()
    {
        lock (_lockObject)
        {
            _cache.Clear();
        }
    }

    private class CacheEntry
    {
        public object? Value { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
    }
}
