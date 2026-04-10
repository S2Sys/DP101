# Factory Pattern

## Overview

**Category:** Creational Pattern  
**Purpose:** Define an interface for creating objects, letting subclasses decide which class to instantiate.  
**Type:** Class creation pattern  
**Complexity:** Medium

## Problem

When to use Factory Pattern:
- A class can't anticipate the type of objects it needs to create
- Creation logic is complex and should be isolated
- You want to avoid coupling to concrete classes
- You need flexibility in object creation
- You want to centralize object creation logic

Example: UI toolkit needs to create different button types (Windows, Mac, Linux) but shouldn't care about concrete implementation.

## Solution

The Factory Pattern provides an abstraction for object creation, delegating the actual instantiation to subclasses or factory methods.

## Implementation Approaches

### 1. Simple Factory (Static Factory)

**Also called:** Static Factory Method

```csharp
public class ShapeFactory
{
    public static IShape CreateShape(ShapeType type)
    {
        return type switch
        {
            ShapeType.Circle => new Circle(),
            ShapeType.Rectangle => new Rectangle(),
            ShapeType.Triangle => new Triangle(),
            _ => throw new ArgumentException("Unknown shape")
        };
    }
}

// Usage
IShape circle = ShapeFactory.CreateShape(ShapeType.Circle);
```

**Pros:**
✅ Simple and straightforward  
✅ Centralized creation logic  
✅ Easy to add new types  

**Cons:**
❌ Violates Open/Closed Principle (need to modify factory for new types)  
❌ Switch statement can become large  
❌ Not truly polymorphic  

**When to Use:**
- Simple object creation
- Limited number of types
- Creation logic is straightforward
- Occasional new types

---

### 2. Factory Method Pattern (Pure)

**Also called:** Virtual Constructor

```csharp
// Abstract factory
public abstract class ShapeFactory
{
    public abstract IShape CreateShape();
}

// Concrete factories
public class CircleFactory : ShapeFactory
{
    public override IShape CreateShape() => new Circle();
}

public class RectangleFactory : ShapeFactory
{
    public override IShape CreateShape() => new Rectangle();
}

// Usage
ShapeFactory factory = new CircleFactory();
IShape shape = factory.CreateShape();
```

**Pros:**
✅ Follows Open/Closed Principle  
✅ Subclasses decide what to create  
✅ Each type has dedicated factory  
✅ Easy to extend  

**Cons:**
❌ Creates many factory classes  
❌ More complex for simple scenarios  
❌ Extra indirection  

**When to Use:**
- Creating related objects
- Complex creation logic
- Want to avoid client coupling
- Subclasses should control creation
- Many object types

---

### 3. Abstract Factory

```csharp
// Product families
public interface IButton { void Click(); }
public interface ICheckbox { void Check(); }

// Concrete products
public class WindowsButton : IButton { public void Click() => Console.WriteLine("Windows button"); }
public class LinuxButton : IButton { public void Click() => Console.WriteLine("Linux button"); }

// Abstract factory
public interface IUIFactory
{
    IButton CreateButton();
    ICheckbox CreateCheckbox();
}

// Concrete factories
public class WindowsFactory : IUIFactory
{
    public IButton CreateButton() => new WindowsButton();
    public ICheckbox CreateCheckbox() => new WindowsCheckbox();
}

public class LinuxFactory : IUIFactory
{
    public IButton CreateButton() => new LinuxButton();
    public ICheckbox CreateCheckbox() => new LinuxCheckbox();
}

// Usage
IUIFactory factory = GetFactory(operatingSystem);
var button = factory.CreateButton();
var checkbox = factory.CreateCheckbox();
```

**Pros:**
✅ Creates families of related objects  
✅ Ensures consistency  
✅ Easy to switch families  
✅ Isolates concrete classes  

**Cons:**
❌ Complex for simple requirements  
❌ Many classes to maintain  
❌ Hard to add new product types  

**When to Use:**
- Multiple families of products
- Need to ensure consistency
- Want to switch entire families
- Complex UI themes, database providers

---

### 4. Parametrized Factory

```csharp
public class ParametrizedShapeFactory : IShapeFactory
{
    private ShapeType _shapeType;

    public ParametrizedShapeFactory(ShapeType shapeType)
    {
        _shapeType = shapeType;
    }

    public IShape CreateShape()
    {
        return _shapeType switch
        {
            ShapeType.Circle => new Circle(),
            ShapeType.Rectangle => new Rectangle(),
            ShapeType.Triangle => new Triangle(),
            _ => throw new InvalidOperationException()
        };
    }
}
```

**Best for:** Runtime configuration of factories

---

### 5. Generic Factory

```csharp
public class GenericFactory<T> where T : class
{
    public T Create()
    {
        var constructor = typeof(T).GetConstructor(Type.EmptyTypes);
        if (constructor == null)
            throw new InvalidOperationException("No parameterless constructor");
        
        return (T)constructor.Invoke(null)!;
    }

    public T Create(params object[] args)
    {
        var paramTypes = args.Select(a => a.GetType()).ToArray();
        var constructor = typeof(T).GetConstructor(paramTypes);
        
        if (constructor == null)
            throw new InvalidOperationException("No matching constructor");
        
        return (T)constructor.Invoke(args)!;
    }
}

// Usage
var circleFactory = new GenericFactory<Circle>();
var circle = circleFactory.Create();
```

**Pros:**
✅ Highly reusable  
✅ Works with any type  
✅ Uses reflection  

**Cons:**
❌ Reflection performance overhead  
❌ Requires parameterless constructor or manual args  
❌ Type safety at runtime only  

**When to Use:**
- Generic plugin systems
- Dependency injection containers
- Framework-level code

---

### 6. Registry-Based Factory

```csharp
public class RegistryFactory<TKey, TProduct> where TProduct : class
{
    private Dictionary<TKey, Func<TProduct>> _registry = [];

    public void Register(TKey key, Func<TProduct> factory)
    {
        _registry[key] = factory;
    }

    public TProduct Create(TKey key)
    {
        if (!_registry.TryGetValue(key, out var factory))
            throw new KeyNotFoundException($"No factory for key: {key}");
        
        return factory();
    }
}

// Usage
var factory = new RegistryFactory<string, IShape>();
factory.Register("circle", () => new Circle());
factory.Register("rectangle", () => new Rectangle());

var shape = factory.Create("circle");
```

**Pros:**
✅ Dynamic registration  
✅ No modification for new types  
✅ Flexible and extensible  

**Cons:**
❌ Runtime type safety  
❌ Registration order matters  
❌ Harder to debug  

**When to Use:**
- Plugin architectures
- Dynamic type discovery
- Configuration-based creation

---

## Comparison Matrix

| Approach | Complexity | Extensibility | Coupling | Use Cases |
|----------|-----------|---------------|----------|-----------|
| Simple Factory | Low | Low | High | Simple, stable types |
| Factory Method | Medium | High | Low | Related object families |
| Abstract Factory | High | High | Very Low | Product families |
| Parametrized | Low | Medium | Medium | Configuration-based |
| Generic | Medium | Very High | Very Low | Framework code |
| Registry | Medium | Very High | Low | Plugin systems |

---

## Real-World Examples

### Database Connection Factory
```csharp
public interface IConnectionFactory
{
    IDbConnection CreateConnection(string connectionString);
}

public class SqlServerFactory : IConnectionFactory
{
    public IDbConnection CreateConnection(string connectionString)
        => new SqlConnection(connectionString);
}

public class MySqlFactory : IConnectionFactory
{
    public IDbConnection CreateConnection(string connectionString)
        => new MySqlConnection(connectionString);
}
```

### Document Format Factory
```csharp
public abstract class DocumentFactory
{
    public abstract Document CreateDocument();
}

public class PDFFactory : DocumentFactory
{
    public override Document CreateDocument() => new PDFDocument();
}

public class WordFactory : DocumentFactory
{
    public override Document CreateDocument() => new WordDocument();
}
```

### Payment Processor Factory
```csharp
public interface IPaymentProcessorFactory
{
    IPaymentProcessor CreateProcessor(string type);
}

public class PaymentProcessorFactory : IPaymentProcessorFactory
{
    public IPaymentProcessor CreateProcessor(string type) =>
        type switch
        {
            "stripe" => new StripeProcessor(),
            "paypal" => new PayPalProcessor(),
            "square" => new SquareProcessor(),
            _ => throw new ArgumentException($"Unknown processor: {type}")
        };
}
```

---

## Pros and Cons

### Advantages
✅ **Loose Coupling** - Clients don't know concrete classes  
✅ **Encapsulation** - Creation logic hidden  
✅ **Single Responsibility** - Creation separated from usage  
✅ **Flexibility** - Easy to add new types  
✅ **Maintainability** - Centralized creation logic  
✅ **Testability** - Easy to mock factories  

### Disadvantages
❌ **Extra Classes** - More code to maintain  
❌ **Complexity** - Overkill for simple cases  
❌ **Indirection** - Extra layer of abstraction  
❌ **Performance** - Slight overhead  
❌ **Debugging** - Harder to trace object creation  

---

## When to Use / When NOT to Use

### ✅ Use Factory When:
- Object creation is complex
- Multiple related types to create
- Creation logic might change
- Want to avoid coupling to concrete classes
- Need to switch implementations
- Creating objects based on configuration
- Testing requires easy mocking

### ❌ Don't Use Factory When:
- Simple object creation (just use `new`)
- Only one type of object
- Creation is straightforward
- Performance is critical
- Simplicity is valued over flexibility
- Object creation is obvious from context

---

## Relationship with Other Patterns

- **Abstract Factory** - Creates families of related objects
- **Builder** - Constructs complex objects step-by-step
- **Singleton** - Factory can return singleton instances
- **Prototype** - Factory can clone prototypes
- **Dependency Injection** - Factories implement IoC principle
- **Strategy** - Factory selects strategies

---

## Interview Questions

**Q: What's the difference between Factory Method and Abstract Factory?**
A: Factory Method creates a single product, Abstract Factory creates families of related products.

**Q: When would you use a factory instead of just calling `new`?**
A: When creation logic is complex, you want to hide implementation, or need flexibility in object types.

**Q: How does Factory Pattern relate to Dependency Injection?**
A: Factories encapsulate creation; DI containers use factories to manage object lifecycles.

**Q: Can a factory return different types each time it's called?**
A: Yes, simple factories often return different types based on parameters.

**Q: What's the performance impact of using factories?**
A: Usually negligible unless using reflection; the benefits outweigh minor overhead.

---

## Best Practices

1. **Keep Creation Logic Simple** - Move complex logic to separate classes
2. **Use Interfaces** - Always return interfaces, not concrete classes
3. **Document Supported Types** - Make it clear what can be created
4. **Handle Invalid Input** - Throw meaningful exceptions for unknown types
5. **Consider Performance** - Use caching for expensive creations
6. **Avoid God Factories** - Don't put all creation in one factory
7. **Test Thoroughly** - Verify all factory paths work
8. **Use Naming Conventions** - Name clearly (Factory, Creator, Builder suffix)

---

## Common Pitfalls

❌ **Creating God Factory**
```csharp
// BAD: Factory that creates everything
public class GodFactory
{
    public object Create(string type) { /* huge switch statement */ }
}
```

✅ **Use Multiple Focused Factories**
```csharp
// GOOD: Separate factories by concern
public interface IShapeFactory { ... }
public interface IColorFactory { ... }
```

❌ **Exposing Concrete Types**
```csharp
// BAD: Returns concrete type
public Circle CreateCircle() { ... }
```

✅ **Return Interfaces**
```csharp
// GOOD: Returns interface
public IShape CreateCircle() { ... }
```

---

## References

- Gang of Four Design Patterns
- Design Patterns in C#
- Refactoring Guru - Factory Pattern
- Microsoft Design Patterns

---

## Summary

The Factory Pattern is fundamental for creating flexible, maintainable object creation. Choose the right factory approach based on complexity and extensibility needs. Use simple factories for straightforward cases, factory methods for related families, and abstract factories for product families. Always prioritize simplicity and avoid over-engineering simple scenarios.

**Key Takeaway:** Factories encapsulate object creation, making code more flexible and maintainable.
