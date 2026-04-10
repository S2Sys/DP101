# Builder Pattern

## Overview

**Category:** Creational Pattern  
**Purpose:** Separate the construction of a complex object from its representation, allowing step-by-step construction.  
**Complexity:** Medium  
**Best Used For:** Complex objects with many optional parameters

## Problem

Creating complex objects with many optional parameters:

```csharp
// BAD: Constructor with many parameters
public class Pizza
{
    public Pizza(string size, string crust, bool cheese, bool sauce, 
                 List<string> toppings, bool extra, bool gluten...)
    {
        // Complex initialization
    }
}

// Usage is unclear
var pizza = new Pizza("Large", "Thick", true, true, 
                      new List<string> { "Pepperoni" }, false, true);
```

**Problems:**
- Too many constructor parameters
- Hard to understand what parameters mean
- Object might be in inconsistent state during construction
- Difficult to add new options
- Same constructor for different configurations

## Solution

Use a builder to construct objects step-by-step with a clean, fluent API:

```csharp
var pizza = new PizzaBuilder()
    .WithSize("Large")
    .WithCrust("Thick")
    .AddTopping("Pepperoni")
    .AddTopping("Mushrooms")
    .WithCheese()
    .Build();
```

## Implementation Approaches

### 1. Classic Builder

```csharp
public class House
{
    public string Foundation { get; set; }
    public string Walls { get; set; }
    public string Roof { get; set; }
    public string Windows { get; set; }
    public string Doors { get; set; }
}

public class HouseBuilder
{
    private House _house = new House();

    public HouseBuilder BuildFoundation()
    {
        _house.Foundation = "Concrete";
        return this;
    }

    public HouseBuilder BuildWalls()
    {
        _house.Walls = "Brick";
        return this;
    }

    public HouseBuilder BuildRoof()
    {
        _house.Roof = "Tile";
        return this;
    }

    public House Build() => _house;
}

// Usage
var house = new HouseBuilder()
    .BuildFoundation()
    .BuildWalls()
    .BuildRoof()
    .Build();
```

### 2. Fluent Builder (Most Popular)

```csharp
public class PizzaBuilder
{
    private string _size = "Medium";
    private string _crust = "Thin";
    private List<string> _toppings = new();
    private bool _cheese = true;

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

    public Pizza Build()
    {
        return new Pizza(_size, _crust, _toppings, _cheese);
    }
}

// Fluent usage
var pizza = new PizzaBuilder()
    .WithSize("Large")
    .WithCrust("Thick")
    .AddTopping("Pepperoni")
    .AddTopping("Mushrooms")
    .Build();
```

### 3. Builder with Director

```csharp
public interface IHouseBuilder
{
    IHouseBuilder BuildFoundation();
    IHouseBuilder BuildWalls();
    IHouseBuilder BuildRoof();
    House Build();
}

public class HouseDirector
{
    private IHouseBuilder _builder;

    public HouseDirector(IHouseBuilder builder)
    {
        _builder = builder;
    }

    public House BuildSimpleHouse()
    {
        return _builder
            .BuildFoundation()
            .BuildWalls()
            .BuildRoof()
            .Build();
    }

    public House BuildLuxuryHouse()
    {
        return _builder
            .BuildFoundation()
            .BuildWalls()
            .BuildRoof()
            .BuildGarage()
            .BuildPool()
            .Build();
    }
}
```

### 4. Immutable Object Builder

```csharp
public class ImmutableHttpRequest
{
    public string Url { get; }
    public string Method { get; }
    public Dictionary<string, string> Headers { get; }
    public string Body { get; }

    private ImmutableHttpRequest(string url, string method, 
                                 Dictionary<string, string> headers, string body)
    {
        Url = url;
        Method = method;
        Headers = new Dictionary<string, string>(headers);
        Body = body;
    }

    public class Builder
    {
        private string _url = "";
        private string _method = "GET";
        private Dictionary<string, string> _headers = new();
        private string _body = "";

        public Builder WithUrl(string url) { _url = url; return this; }
        public Builder WithMethod(string method) { _method = method; return this; }
        public Builder AddHeader(string key, string value) 
        { 
            _headers[key] = value; 
            return this; 
        }
        public Builder WithBody(string body) { _body = body; return this; }

        public ImmutableHttpRequest Build()
        {
            if (string.IsNullOrEmpty(_url))
                throw new InvalidOperationException("URL required");

            return new ImmutableHttpRequest(_url, _method, _headers, _body);
        }
    }
}

// Usage - creates immutable object
var request = new ImmutableHttpRequest.Builder()
    .WithUrl("https://api.example.com/users")
    .WithMethod("POST")
    .AddHeader("Content-Type", "application/json")
    .WithBody("{\"name\": \"John\"}")
    .Build();
```

### 5. Configuration Builder

```csharp
public class DatabaseConfigBuilder
{
    private string _server = "localhost";
    private int _port = 5432;
    private string _database = "";
    private string _userId = "";
    private string _password = "";

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

    public DatabaseConfig Build()
    {
        if (string.IsNullOrEmpty(_database))
            throw new InvalidOperationException("Database required");

        return new DatabaseConfig(_server, _port, _database, _userId, _password);
    }
}

// Usage
var config = new DatabaseConfigBuilder()
    .WithServer("db.example.com")
    .WithDatabase("production")
    .WithCredentials("admin", "password")
    .Build();
```

---

## Builder vs Constructor Comparison

| Aspect | Constructor | Builder |
|--------|-----------|---------|
| **Readability** | Poor (many params) | Excellent (fluent) |
| **Optional Params** | Difficult | Easy |
| **Parameter Order** | Fixed | Flexible |
| **Complex Logic** | Scattered | Centralized |
| **Immutability** | Possible | Natural |
| **Type Safety** | Compile-time | Compile-time |

---

## Real-World Examples

### HTTP Request Builder
```csharp
var request = new HttpRequestBuilder()
    .WithUrl("https://api.github.com/users")
    .WithMethod("POST")
    .WithTimeout(5000)
    .AddHeader("Authorization", "Bearer token")
    .WithBody(jsonPayload)
    .Build();
```

### Database Connection Builder
```csharp
var connection = new ConnectionBuilder()
    .WithServer("prod-db.example.com")
    .WithPort(5432)
    .WithDatabase("myapp")
    .WithCredentials("user", "pass")
    .WithTimeout(30)
    .EnablePooling(100)
    .Build();
```

### Query Builder
```csharp
var query = new QueryBuilder()
    .Select("id", "name", "email")
    .From("users")
    .Where("status", "active")
    .OrderBy("name")
    .Limit(10)
    .Build();
```

---

## Pros and Cons

### Advantages
✅ **Clear API** - Self-documenting code  
✅ **Optional Parameters** - Easy to handle many options  
✅ **Immutability** - Can create immutable objects  
✅ **Step-by-Step** - Build complex objects gradually  
✅ **Flexible** - Easy to add new options  
✅ **Readable** - Method chaining improves readability  
✅ **Validation** - Can validate in Build() method  

### Disadvantages
❌ **More Code** - Extra builder class  
❌ **Overhead** - Extra object creation  
❌ **Complexity** - Overkill for simple objects  
❌ **Learning Curve** - Developers need to know pattern  

---

## When to Use

### ✅ Use Builder When:
- Object has many optional parameters (5+)
- Creating complex objects with variations
- Want to make API more readable
- Need to validate state before creation
- Creating immutable objects
- Object has many configuration options

### ❌ Don't Use When:
- Simple objects with few parameters
- All parameters are required
- Object creation is rare
- Performance is critical (minor overhead)
- Simplicity is valued

---

## Best Practices

1. **Use Fluent Interface** - Return `this` for chaining
2. **Validate in Build()** - Check required fields
3. **Set Defaults** - Provide sensible defaults
4. **Immutable Results** - Make built objects immutable
5. **Clear Naming** - Use descriptive method names (With, Add, Set)
6. **Handle Required Fields** - Throw if required fields missing
7. **Optional Validation** - Validate constraints in Build()
8. **Consider Thread Safety** - Builders might be used concurrently

---

## Interview Questions

**Q: When would you use Builder pattern over multiple constructors?**
A: When you have many optional parameters (typically 4+). Builder makes the code more readable and flexible.

**Q: Can you chain builders?**
A: Yes, each method returns `this`, enabling method chaining (fluent interface).

**Q: What's the difference between Builder and Factory?**
A: Builder constructs complex objects step-by-step; Factory creates objects with single method call.

**Q: Should builders be thread-safe?**
A: No, builders are typically used in single thread. Create new builder for each object.

**Q: Can builder create mutable or immutable objects?**
A: Both - builder returns whatever object type you want to create.

---

## Summary

The Builder pattern excels at creating complex objects with many optional parameters. It makes code more readable and maintainable compared to telescoping constructors. Use fluent interface for clean method chaining. Validate in the Build() method to ensure consistent objects.

**Key Takeaway:** Builder makes complex object construction readable, flexible, and maintainable.
