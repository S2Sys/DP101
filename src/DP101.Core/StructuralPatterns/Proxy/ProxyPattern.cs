namespace DP101.Core.StructuralPatterns.Proxy;

/// <summary>
/// PROXY PATTERN
///
/// Intent: Provide a surrogate or placeholder for another object to control access to it.
///
/// Types of Proxy:
/// - Virtual Proxy: Lazy initialization
/// - Protection Proxy: Access control
/// - Smart Reference Proxy: Reference counting, caching
/// - Logging Proxy: Log access
///
/// PROS:
/// - Control over original object access
/// - Lazy initialization possible
/// - Can log or audit access
/// - Can add security checks
/// - Decouples client from subject
///
/// CONS:
/// - Extra indirection
/// - Can slow down performance
/// - More complexity
/// </summary>

// ========== EXAMPLE 1: VIRTUAL PROXY (LAZY LOADING) ==========

public interface IImage
{
    void Display();
}

public class RealImage : IImage
{
    private string _filename;

    public RealImage(string filename)
    {
        _filename = filename;
        LoadImageFromDisk();
    }

    private void LoadImageFromDisk()
    {
        Console.WriteLine($"Loading image from disk: {_filename}");
        System.Threading.Thread.Sleep(1000); // Simulate expensive operation
    }

    public void Display()
    {
        Console.WriteLine($"Displaying image: {_filename}");
    }
}

public class ImageProxy : IImage
{
    private string _filename;
    private RealImage? _realImage;

    public ImageProxy(string filename)
    {
        _filename = filename;
        _realImage = null; // Not loaded yet
    }

    public void Display()
    {
        // Lazy loading - only load when needed
        if (_realImage == null)
        {
            _realImage = new RealImage(_filename);
        }
        _realImage.Display();
    }
}

// ========== EXAMPLE 2: PROTECTION PROXY (ACCESS CONTROL) ==========

public interface IBank
{
    void Withdraw(decimal amount);
    void Deposit(decimal amount);
    decimal GetBalance();
}

public class RealBank : IBank
{
    private decimal _balance;

    public RealBank(decimal initialBalance)
    {
        _balance = initialBalance;
    }

    public void Withdraw(decimal amount)
    {
        _balance -= amount;
        Console.WriteLine($"Withdrew: ${amount}. New balance: ${_balance}");
    }

    public void Deposit(decimal amount)
    {
        _balance += amount;
        Console.WriteLine($"Deposited: ${amount}. New balance: ${_balance}");
    }

    public decimal GetBalance() => _balance;
}

public class BankProxy : IBank
{
    private RealBank _bank;
    private string _userId;
    private List<string> _allowedUsers;

    public BankProxy(RealBank bank, string userId)
    {
        _bank = bank;
        _userId = userId;
        _allowedUsers = new List<string> { "admin", "owner" };
    }

    public void Withdraw(decimal amount)
    {
        if (!IsAuthorized())
        {
            Console.WriteLine($"Access denied: {_userId} is not authorized to withdraw");
            return;
        }

        if (amount > 1000)
        {
            Console.WriteLine($"Access denied: Withdrawal amount ${amount} exceeds limit");
            return;
        }

        _bank.Withdraw(amount);
    }

    public void Deposit(decimal amount)
    {
        if (!IsAuthorized())
        {
            Console.WriteLine($"Access denied: {_userId} is not authorized to deposit");
            return;
        }

        _bank.Deposit(amount);
    }

    public decimal GetBalance()
    {
        if (!IsAuthorized())
        {
            Console.WriteLine($"Access denied: {_userId} is not authorized to view balance");
            return 0;
        }

        return _bank.GetBalance();
    }

    private bool IsAuthorized() => _allowedUsers.Contains(_userId);
}

// ========== EXAMPLE 3: SMART REFERENCE PROXY (CACHING) ==========

public interface IData
{
    string GetData();
}

public class ExpensiveData : IData
{
    private string _data;

    public ExpensiveData(string key)
    {
        _data = FetchDataFromServer(key);
    }

    private string FetchDataFromServer(string key)
    {
        Console.WriteLine($"Fetching data from server for key: {key}");
        System.Threading.Thread.Sleep(500);
        return $"Data for {key}";
    }

    public string GetData() => _data;
}

public class DataProxy : IData
{
    private string _key;
    private ExpensiveData? _realData;
    private static Dictionary<string, ExpensiveData> _cache = [];

    public DataProxy(string key)
    {
        _key = key;
    }

    public string GetData()
    {
        if (_realData == null)
        {
            // Check cache first
            if (_cache.ContainsKey(_key))
            {
                Console.WriteLine($"Getting data from cache: {_key}");
                _realData = _cache[_key];
            }
            else
            {
                _realData = new ExpensiveData(_key);
                _cache[_key] = _realData;
            }
        }

        return _realData.GetData();
    }
}

// ========== EXAMPLE 4: LOGGING PROXY ==========

public interface IService
{
    void DoWork(string task);
    string GetStatus();
}

public class RealService : IService
{
    public void DoWork(string task)
    {
        Console.WriteLine($"Performing work: {task}");
    }

    public string GetStatus() => "Service is running";
}

public class LoggingProxy : IService
{
    private RealService _service;
    private List<string> _logs = [];

    public LoggingProxy()
    {
        _service = new RealService();
    }

    public void DoWork(string task)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        _logs.Add($"[{timestamp}] DoWork called with task: {task}");
        Console.WriteLine($"[LOG] {timestamp} - DoWork: {task}");
        _service.DoWork(task);
    }

    public string GetStatus()
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        _logs.Add($"[{timestamp}] GetStatus called");
        return _service.GetStatus();
    }

    public List<string> GetLogs() => _logs;
}

// ========== EXAMPLE 5: REMOTE PROXY ==========

public interface IRemoteService
{
    string CallRemoteMethod(string parameter);
}

public class RealRemoteService : IRemoteService
{
    public string CallRemoteMethod(string parameter)
    {
        // Simulate remote call
        return $"Remote response for: {parameter}";
    }
}

public class RemoteServiceProxy : IRemoteService
{
    private RealRemoteService _service;
    private int _callCount;

    public RemoteServiceProxy()
    {
        _service = new RealRemoteService();
        _callCount = 0;
    }

    public string CallRemoteMethod(string parameter)
    {
        _callCount++;
        Console.WriteLine($"[REMOTE PROXY] Call #{_callCount}: {parameter}");

        // Add network overhead simulation
        System.Threading.Thread.Sleep(100);

        return _service.CallRemoteMethod(parameter);
    }

    public int GetCallCount() => _callCount;
}

// ========== EXAMPLE 6: REFERENCE COUNTING PROXY ==========

public interface IResource
{
    void Use();
}

public class RealResource : IResource
{
    public void Use() => Console.WriteLine("Using resource");
}

public class ResourceProxy : IResource
{
    private RealResource _resource;
    private static int _referenceCount;

    public ResourceProxy()
    {
        if (_resource == null)
        {
            Console.WriteLine("Creating resource");
            _resource = new RealResource();
        }
        _referenceCount++;
        Console.WriteLine($"Reference count: {_referenceCount}");
    }

    public void Use() => _resource.Use();

    ~ResourceProxy()
    {
        _referenceCount--;
        Console.WriteLine($"Destroying proxy. Reference count: {_referenceCount}");
        if (_referenceCount == 0)
        {
            _resource = null;
        }
    }
}

// ========== EXAMPLE 7: SECURITY PROXY ==========

public interface ISensitiveOperation
{
    void ExecuteSensitiveOperation(string operation);
}

public class RealSensitiveOperation : ISensitiveOperation
{
    public void ExecuteSensitiveOperation(string operation)
    {
        Console.WriteLine($"Executing sensitive operation: {operation}");
    }
}

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

    private void SetupPermissions()
    {
        _permissions = new Dictionary<string, List<string>>
        {
            { "admin", new List<string> { "delete", "create", "modify", "view" } },
            { "user", new List<string> { "view", "modify" } },
            { "guest", new List<string> { "view" } }
        };
    }

    public void ExecuteSensitiveOperation(string operation)
    {
        if (HasPermission(operation))
        {
            Console.WriteLine($"[AUDIT] User {_userId} executing: {operation}");
            _operation.ExecuteSensitiveOperation(operation);
        }
        else
        {
            Console.WriteLine($"[SECURITY] Access denied for user {_userId} to operation: {operation}");
        }
    }

    private bool HasPermission(string operation)
    {
        if (!_permissions.ContainsKey(_userId))
            return false;

        return _permissions[_userId].Contains(operation);
    }
}
