namespace DP101.Core.CreationalPatterns.Factory;

/// <summary>
/// FACTORY PATTERN (Factory Method)
///
/// Intent: Define an interface for creating an object, but let subclasses decide which class to instantiate.
///
/// Participants:
/// - Product: Interface for objects the factory creates
/// - ConcreteProduct: Objects the concrete factories create
/// - Factory: Interface for creating products
/// - ConcreteFactory: Subclasses that implement creation logic
///
/// PROS:
/// - Encapsulates object creation
/// - Eliminates direct class dependencies
/// - Makes code more flexible and extensible
/// - Centralized creation logic
/// - Adheres to Open/Closed Principle
/// - Allows easy addition of new types
///
/// CONS:
/// - Creates extra classes/interfaces
/// - Can be overkill for simple cases
/// - May complicate code initially
///
/// USE CASES:
/// - Creating objects based on type or parameters
/// - Reducing coupling between classes
/// - Centralizing creation logic
/// - Making code extensible
///
/// WHEN TO USE:
/// - When you need multiple types of objects
/// - When creation logic might change
/// - When you want to abstract instantiation
/// - When subclasses should decide which type to create
///
/// WHEN NOT TO USE:
/// - For simple object creation
/// - When you have only one product type
/// - When complexity isn't justified
/// </summary>

// PRODUCT INTERFACES

/// <summary>Product interface that all factories create</summary>
public interface IShape
{
    void Draw();
    string GetName();
}

/// <summary>Concrete products</summary>
public class Circle : IShape
{
    public void Draw() => Console.WriteLine("Drawing Circle");
    public string GetName() => "Circle";
}

public class Rectangle : IShape
{
    public void Draw() => Console.WriteLine("Drawing Rectangle");
    public string GetName() => "Rectangle";
}

public class Triangle : IShape
{
    public void Draw() => Console.WriteLine("Drawing Triangle");
    public string GetName() => "Triangle";
}

// FACTORY INTERFACE

/// <summary>Factory interface - defines how to create shapes</summary>
public interface IShapeFactory
{
    IShape CreateShape();
}

// CONCRETE FACTORIES

/// <summary>Factory for creating circles</summary>
public class CircleFactory : IShapeFactory
{
    public IShape CreateShape() => new Circle();
}

/// <summary>Factory for creating rectangles</summary>
public class RectangleFactory : IShapeFactory
{
    public IShape CreateShape() => new Rectangle();
}

/// <summary>Factory for creating triangles</summary>
public class TriangleFactory : IShapeFactory
{
    public IShape CreateShape() => new Triangle();
}

// SIMPLE FACTORY (NOT PURE FACTORY METHOD, BUT COMMON VARIATION)

/// <summary>
/// Simple/Static Factory Pattern - A single factory with a method to create objects based on parameter.
/// Less pure than Factory Method but commonly used.
/// </summary>
public class ShapeFactory
{
    public static IShape CreateShape(ShapeType shapeType)
    {
        return shapeType switch
        {
            ShapeType.Circle => new Circle(),
            ShapeType.Rectangle => new Rectangle(),
            ShapeType.Triangle => new Triangle(),
            _ => throw new ArgumentException($"Unknown shape type: {shapeType}")
        };
    }

    public static IShape CreateShape(string shapeTypeName)
    {
        return shapeTypeName.ToLowerInvariant() switch
        {
            "circle" => new Circle(),
            "rectangle" => new Rectangle(),
            "triangle" => new Triangle(),
            _ => throw new ArgumentException($"Unknown shape type: {shapeTypeName}")
        };
    }
}

public enum ShapeType
{
    Circle,
    Rectangle,
    Triangle
}

// PARAMETRIZED FACTORY

/// <summary>
/// Parametrized Factory - A single factory that takes parameters to determine creation.
/// More flexible than Simple Factory.
/// </summary>
public class ParametrizedShapeFactory : IShapeFactory
{
    private readonly ShapeType _shapeType;

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
            _ => throw new InvalidOperationException($"Unknown shape type: {_shapeType}")
        };
    }
}

// GENERIC FACTORY

/// <summary>
/// Generic Factory - A flexible factory that uses reflection to create any type.
/// Useful for plugin architectures and dependency injection containers.
/// </summary>
public class GenericFactory<TProduct> where TProduct : class
{
    public TProduct Create()
    {
        var constructorInfo = typeof(TProduct).GetConstructor(Type.EmptyTypes);

        if (constructorInfo == null)
            throw new InvalidOperationException($"Type {typeof(TProduct).Name} must have a parameterless constructor");

        return (TProduct)constructorInfo.Invoke(null)!;
    }

    public TProduct Create(params object[] args)
    {
        var paramTypes = args.Select(a => a.GetType()).ToArray();
        var constructorInfo = typeof(TProduct).GetConstructor(paramTypes);

        if (constructorInfo == null)
            throw new InvalidOperationException(
                $"Type {typeof(TProduct).Name} does not have a matching constructor");

        return (TProduct)constructorInfo.Invoke(args)!;
    }
}

// ABSTRACT FACTORY PATTERN (Related but separate pattern)

/// <summary>
/// Abstract Factory - Create families of related products.
/// Often confused with Factory Method but creates multiple related products.
/// </summary>

// Product families
public interface IButton
{
    void Click();
}

public interface ICheckbox
{
    void Check();
}

// Windows products
public class WindowsButton : IButton
{
    public void Click() => Console.WriteLine("Windows Button clicked");
}

public class WindowsCheckbox : ICheckbox
{
    public void Check() => Console.WriteLine("Windows Checkbox checked");
}

// Linux products
public class LinuxButton : IButton
{
    public void Click() => Console.WriteLine("Linux Button clicked");
}

public class LinuxCheckbox : ICheckbox
{
    public void Check() => Console.WriteLine("Linux Checkbox checked");
}

// Abstract factory
public interface IThemeFactory
{
    IButton CreateButton();
    ICheckbox CreateCheckbox();
}

// Concrete factories
public class WindowsThemeFactory : IThemeFactory
{
    public IButton CreateButton() => new WindowsButton();
    public ICheckbox CreateCheckbox() => new WindowsCheckbox();
}

public class LinuxThemeFactory : IThemeFactory
{
    public IButton CreateButton() => new LinuxButton();
    public ICheckbox CreateCheckbox() => new LinuxCheckbox();
}

// REGISTRY-BASED FACTORY

/// <summary>
/// Registry-based Factory - Dynamically register and create types.
/// Very flexible for plugin architectures.
/// </summary>
public class RegistryFactory<TKey, TProduct> where TProduct : class
{
    private readonly Dictionary<TKey, Func<TProduct>> _registry = [];

    public void Register(TKey key, Func<TProduct> factory)
    {
        _registry[key] = factory;
    }

    public TProduct Create(TKey key)
    {
        if (_registry.TryGetValue(key, out var factory))
            return factory();

        throw new KeyNotFoundException($"No factory registered for key: {key}");
    }

    public bool CanCreate(TKey key) => _registry.ContainsKey(key);
}
