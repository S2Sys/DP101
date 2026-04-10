namespace DP101.Core.CreationalPatterns.Builder;

/// <summary>
/// BUILDER PATTERN
///
/// Intent: Separate the construction of a complex object from its representation,
/// allowing step-by-step construction.
///
/// Participants:
/// - Product: Complex object to be built
/// - Builder: Interface for building products
/// - ConcreteBuilder: Implements builder interface
/// - Director: Optional - uses builder to construct products
///
/// PROS:
/// - Cleaner API for constructing complex objects
/// - Handles many optional parameters elegantly
/// - Immutable objects possible
/// - Readable and fluent API
/// - Separates construction from representation
/// - Can create different representations with same builder
///
/// CONS:
/// - Extra classes needed
/// - More code for simple objects
/// - Slight performance overhead
///
/// USE CASES:
/// - Building objects with many optional parameters
/// - Creating immutable objects
/// - Building complex domain objects
/// - Configuration objects
/// - Database query builders
/// - Request/Response objects with optional fields
///
/// WHEN TO USE:
/// - When object has many optional parameters
/// - When you want immutability
/// - When construction is complex or multi-step
/// - When you need different representations
///
/// WHEN NOT TO USE:
/// - For simple objects with few parameters
/// - When performance is critical
/// - When simplicity is valued over flexibility
/// </summary>

// SIMPLE BUILDER PATTERN

/// <summary>
/// Product - Complex object being built
/// </summary>
public class House
{
    public string? Foundation { get; set; }
    public string? Walls { get; set; }
    public string? Roof { get; set; }
    public string? Windows { get; set; }
    public string? Doors { get; set; }
    public string? Garden { get; set; }
    public string? Garage { get; set; }
    public bool HasPool { get; set; }
    public string? InteriorDesign { get; set; }

    public override string ToString()
    {
        return $@"House:
  Foundation: {Foundation}
  Walls: {Walls}
  Roof: {Roof}
  Windows: {Windows}
  Doors: {Doors}
  Garden: {Garden}
  Garage: {Garage}
  Pool: {HasPool}
  Interior: {InteriorDesign}";
    }
}

/// <summary>
/// Builder interface
/// </summary>
public interface IHouseBuilder
{
    IHouseBuilder BuildFoundation();
    IHouseBuilder BuildWalls();
    IHouseBuilder BuildRoof();
    IHouseBuilder BuildWindows();
    IHouseBuilder BuildDoors();
    IHouseBuilder BuildGarden();
    IHouseBuilder BuildGarage();
    IHouseBuilder BuildPool();
    IHouseBuilder BuildInterior();
    House Build();
}

/// <summary>
/// Concrete builder
/// </summary>
public class HouseBuilder : IHouseBuilder
{
    private readonly House _house = new();

    public IHouseBuilder BuildFoundation()
    {
        _house.Foundation = "Concrete foundation";
        return this;
    }

    public IHouseBuilder BuildWalls()
    {
        _house.Walls = "Brick walls";
        return this;
    }

    public IHouseBuilder BuildRoof()
    {
        _house.Roof = "Tile roof";
        return this;
    }

    public IHouseBuilder BuildWindows()
    {
        _house.Windows = "Glass windows";
        return this;
    }

    public IHouseBuilder BuildDoors()
    {
        _house.Doors = "Wooden doors";
        return this;
    }

    public IHouseBuilder BuildGarden()
    {
        _house.Garden = "Landscaped garden";
        return this;
    }

    public IHouseBuilder BuildGarage()
    {
        _house.Garage = "Two-car garage";
        return this;
    }

    public IHouseBuilder BuildPool()
    {
        _house.HasPool = true;
        return this;
    }

    public IHouseBuilder BuildInterior()
    {
        _house.InteriorDesign = "Modern interior";
        return this;
    }

    public House Build() => _house;
}

/// <summary>
/// Director - Uses builder to construct objects with specific algorithms
/// </summary>
public class HouseDirector
{
    private readonly IHouseBuilder _builder;

    public HouseDirector(IHouseBuilder builder)
    {
        _builder = builder;
    }

    public House BuildSimpleHouse() =>
        _builder
            .BuildFoundation()
            .BuildWalls()
            .BuildRoof()
            .BuildWindows()
            .BuildDoors()
            .Build();

    public House BuildLuxuryHouse() =>
        _builder
            .BuildFoundation()
            .BuildWalls()
            .BuildRoof()
            .BuildWindows()
            .BuildDoors()
            .BuildGarden()
            .BuildPool()
            .BuildGarage()
            .BuildInterior()
            .Build();
}

// FLUENT BUILDER WITH IMMUTABLE OBJECT

/// <summary>
/// Immutable product
/// </summary>
public class Pizza
{
    public string? Size { get; }
    public string? Crust { get; }
    public List<string> Toppings { get; }
    public bool HasCheese { get; }
    public bool HasSauce { get; }

    public Pizza(string size, string crust, List<string> toppings, bool hasCheese, bool hasSauce)
    {
        Size = size;
        Crust = crust;
        Toppings = toppings;
        HasCheese = hasCheese;
        HasSauce = hasSauce;
    }

    public override string ToString()
    {
        var toppings = string.Join(", ", Toppings);
        return $"Pizza [{Size}] - {Crust} crust, Toppings: {toppings}, Cheese: {HasCheese}, Sauce: {HasSauce}";
    }
}

/// <summary>
/// Fluent builder for Pizza
/// </summary>
public class PizzaBuilder
{
    private string _size = "Medium";
    private string _crust = "Thin";
    private readonly List<string> _toppings = [];
    private bool _hasCheese = true;
    private bool _hasSauce = true;

    public PizzaBuilder WithSize(string size)
    {
        _size = size;
        return this;
    }

    public PizzaBuilder WithCrust(string crust)
    {
        _crust = crust;
        return this;
    }

    public PizzaBuilder AddTopping(string topping)
    {
        _toppings.Add(topping);
        return this;
    }

    public PizzaBuilder WithCheese(bool hasCheese = true)
    {
        _hasCheese = hasCheese;
        return this;
    }

    public PizzaBuilder WithSauce(bool hasSauce = true)
    {
        _hasSauce = hasSauce;
        return this;
    }

    public Pizza Build()
    {
        return new Pizza(_size, _crust, new List<string>(_toppings), _hasCheese, _hasSauce);
    }
}

// REQUEST BUILDER PATTERN

/// <summary>
/// Complex request object with many optional parameters
/// </summary>
public class HttpRequest
{
    public string Url { get; }
    public string Method { get; }
    public Dictionary<string, string> Headers { get; }
    public string? Body { get; }
    public int Timeout { get; }
    public bool IsSecure { get; }
    public Dictionary<string, string> QueryParameters { get; }

    public HttpRequest(
        string url,
        string method,
        Dictionary<string, string> headers,
        string? body,
        int timeout,
        bool isSecure,
        Dictionary<string, string> queryParameters)
    {
        Url = url;
        Method = method;
        Headers = headers;
        Body = body;
        Timeout = timeout;
        IsSecure = isSecure;
        QueryParameters = queryParameters;
    }

    public override string ToString()
    {
        return $"{(IsSecure ? "HTTPS" : "HTTP")} {Method} {Url} (Timeout: {Timeout}ms)";
    }
}

/// <summary>
/// Fluent builder for HTTP requests
/// </summary>
public class HttpRequestBuilder
{
    private string _url = "";
    private string _method = "GET";
    private readonly Dictionary<string, string> _headers = [];
    private string? _body;
    private int _timeout = 5000;
    private bool _isSecure = true;
    private readonly Dictionary<string, string> _queryParameters = [];

    public HttpRequestBuilder WithUrl(string url)
    {
        _url = url;
        return this;
    }

    public HttpRequestBuilder WithMethod(string method)
    {
        _method = method;
        return this;
    }

    public HttpRequestBuilder AddHeader(string key, string value)
    {
        _headers[key] = value;
        return this;
    }

    public HttpRequestBuilder WithBody(string body)
    {
        _body = body;
        return this;
    }

    public HttpRequestBuilder WithTimeout(int milliseconds)
    {
        _timeout = milliseconds;
        return this;
    }

    public HttpRequestBuilder AsSecure(bool secure = true)
    {
        _isSecure = secure;
        return this;
    }

    public HttpRequestBuilder AddQueryParameter(string key, string value)
    {
        _queryParameters[key] = value;
        return this;
    }

    public HttpRequest Build()
    {
        if (string.IsNullOrEmpty(_url))
            throw new InvalidOperationException("URL is required");

        return new HttpRequest(_url, _method, _headers, _body, _timeout, _isSecure, _queryParameters);
    }
}

// CONFIGURATION BUILDER

/// <summary>
/// Configuration object with many optional parameters
/// </summary>
public class DatabaseConfig
{
    public string Server { get; }
    public int Port { get; }
    public string Database { get; }
    public string? UserId { get; }
    public string? Password { get; }
    public int ConnectionTimeout { get; }
    public bool EnablePooling { get; }
    public int MaxPoolSize { get; }
    public bool EnableEncryption { get; }
    public List<string> Certificates { get; }

    public DatabaseConfig(
        string server,
        int port,
        string database,
        string? userId,
        string? password,
        int connectionTimeout,
        bool enablePooling,
        int maxPoolSize,
        bool enableEncryption,
        List<string> certificates)
    {
        Server = server;
        Port = port;
        Database = database;
        UserId = userId;
        Password = password;
        ConnectionTimeout = connectionTimeout;
        EnablePooling = enablePooling;
        MaxPoolSize = maxPoolSize;
        EnableEncryption = enableEncryption;
        Certificates = certificates;
    }
}

/// <summary>
/// Fluent builder for database configuration
/// </summary>
public class DatabaseConfigBuilder
{
    private string _server = "localhost";
    private int _port = 5432;
    private string _database = "";
    private string? _userId;
    private string? _password;
    private int _connectionTimeout = 30;
    private bool _enablePooling = true;
    private int _maxPoolSize = 100;
    private bool _enableEncryption = false;
    private readonly List<string> _certificates = [];

    public DatabaseConfigBuilder WithServer(string server)
    {
        _server = server;
        return this;
    }

    public DatabaseConfigBuilder WithPort(int port)
    {
        _port = port;
        return this;
    }

    public DatabaseConfigBuilder WithDatabase(string database)
    {
        _database = database;
        return this;
    }

    public DatabaseConfigBuilder WithCredentials(string userId, string password)
    {
        _userId = userId;
        _password = password;
        return this;
    }

    public DatabaseConfigBuilder WithConnectionTimeout(int seconds)
    {
        _connectionTimeout = seconds;
        return this;
    }

    public DatabaseConfigBuilder EnablePooling(bool enable = true, int maxSize = 100)
    {
        _enablePooling = enable;
        _maxPoolSize = maxSize;
        return this;
    }

    public DatabaseConfigBuilder EnableEncryption()
    {
        _enableEncryption = true;
        return this;
    }

    public DatabaseConfigBuilder AddCertificate(string path)
    {
        _certificates.Add(path);
        return this;
    }

    public DatabaseConfig Build()
    {
        if (string.IsNullOrEmpty(_database))
            throw new InvalidOperationException("Database name is required");

        return new DatabaseConfig(
            _server,
            _port,
            _database,
            _userId,
            _password,
            _connectionTimeout,
            _enablePooling,
            _maxPoolSize,
            _enableEncryption,
            _certificates);
    }
}
