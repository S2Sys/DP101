namespace DP101.Core.CreationalPatterns.Prototype;

/// <summary>
/// PROTOTYPE PATTERN
///
/// Intent: Create new objects by copying an existing object (prototype)
/// rather than creating from scratch.
///
/// Participants:
/// - Prototype: Interface for cloning
/// - ConcretePrototype: Implements cloning
/// - Client: Uses prototypes to create new objects
///
/// PROS:
/// - Avoids expensive object creation
/// - Avoids subclassing
/// - Flexible object creation
/// - Can clone complex objects easily
/// - Works well with factory pattern
///
/// CONS:
/// - Cloning can be complex (deep copy)
/// - All classes must implement cloning
/// - Circular references complicate cloning
/// - Can hide constructor logic
///
/// USE CASES:
/// - Expensive object creation
/// - Unknown concrete types
/// - Avoid subclass explosion
/// - Clone complex objects
///
/// WHEN TO USE:
/// - Object creation is expensive
/// - Need to create object variants
/// - Want to avoid subclassing
/// - Have complex object graphs
///
/// WHEN NOT TO USE:
/// - Simple objects with cheap creation
/// - Only one instance needed
/// - Cloning behavior is complex
/// </summary>

// CLONEABLE INTERFACE
public interface ICloneable<T>
{
    T Clone();
    T DeepClone();
}

// SIMPLE PROTOTYPE

/// <summary>
/// Simple prototype that implements shallow cloning
/// </summary>
public class Person : ICloneable<Person>
{
    public string? Name { get; set; }
    public int Age { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }

    public Person(string name, int age, string email, string phone)
    {
        Name = name;
        Age = age;
        Email = email;
        Phone = phone;
    }

    // Copy constructor for cloning
    private Person(Person original)
    {
        Name = original.Name;
        Age = original.Age;
        Email = original.Email;
        Phone = original.Phone;
    }

    public Person Clone()
    {
        return new Person(this);
    }

    public Person DeepClone()
    {
        return new Person(this);
    }

    public override string ToString()
    {
        return $"Person: {Name}, Age: {Age}, Email: {Email}, Phone: {Phone}";
    }
}

// COMPLEX PROTOTYPE WITH COLLECTIONS

/// <summary>
/// Complex object with collections that needs deep cloning
/// </summary>
public class Document : ICloneable<Document>
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public List<string> Tags { get; set; }
    public Dictionary<string, object> Metadata { get; set; }
    public DateTime CreatedDate { get; set; }

    public Document()
    {
        Tags = [];
        Metadata = [];
        CreatedDate = DateTime.Now;
    }

    public Document Clone()
    {
        return (Document)MemberwiseClone();
    }

    public Document DeepClone()
    {
        var clone = (Document)MemberwiseClone();
        clone.Tags = new List<string>(Tags);
        clone.Metadata = new Dictionary<string, object>(Metadata);
        return clone;
    }

    public override string ToString()
    {
        return $"Document: {Title} (Tags: {string.Join(", ", Tags)})";
    }
}

// PROTOTYPE REGISTRY

/// <summary>
/// Registry to store and manage prototype instances
/// </summary>
public class PrototypeRegistry<T> where T : ICloneable<T>
{
    private readonly Dictionary<string, T> _prototypes = [];

    public void Register(string name, T prototype)
    {
        _prototypes[name] = prototype;
    }

    public T? GetPrototype(string name)
    {
        return _prototypes.TryGetValue(name, out var prototype) ? prototype : default;
    }

    public T CreateFromPrototype(string name)
    {
        var prototype = GetPrototype(name);
        if (prototype == null)
            throw new KeyNotFoundException($"Prototype '{name}' not found");

        return prototype.Clone();
    }

    public List<string> GetRegisteredNames()
    {
        return _prototypes.Keys.ToList();
    }
}

// ADVANCED: CLONE WITH GENERIC DEEP COPY

/// <summary>
/// Advanced prototype with generic deep cloning using serialization
/// (Requires objects to be serializable)
/// </summary>
public class SerializablePrototype : ICloneable<SerializablePrototype>
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public List<string> Items { get; set; }
    public NestedData? Nested { get; set; }

    public SerializablePrototype()
    {
        Items = [];
    }

    public SerializablePrototype Clone()
    {
        return (SerializablePrototype)MemberwiseClone();
    }

    public SerializablePrototype DeepClone()
    {
        var clone = new SerializablePrototype
        {
            Id = Id,
            Name = Name,
            Items = new List<string>(Items),
            Nested = Nested?.DeepClone()
        };
        return clone;
    }

    public override string ToString()
    {
        return $"SerializablePrototype: {Name} ({Items.Count} items)";
    }
}

public class NestedData
{
    public string? Value { get; set; }
    public List<int> Numbers { get; set; }

    public NestedData()
    {
        Numbers = [];
    }

    public NestedData DeepClone()
    {
        return new NestedData
        {
            Value = Value,
            Numbers = new List<int>(Numbers)
        };
    }
}

// PATTERN: PROTOTYPE FACTORY

/// <summary>
/// Factory using prototypes for object creation
/// </summary>
public class PrototypeFactory
{
    private readonly Dictionary<string, Person> _personPrototypes = [];

    public void RegisterPersonPrototype(string key, Person prototype)
    {
        _personPrototypes[key] = prototype;
    }

    public Person CreatePerson(string prototypeKey)
    {
        if (!_personPrototypes.TryGetValue(prototypeKey, out var prototype))
            throw new KeyNotFoundException($"Prototype '{prototypeKey}' not found");

        return prototype.Clone();
    }

    public Person CreatePersonWithDefaults(string prototypeKey, string newName, int newAge)
    {
        var person = CreatePerson(prototypeKey);
        person.Name = newName;
        person.Age = newAge;
        return person;
    }
}

// SHAPE HIERARCHY WITH PROTOTYPE

public abstract class Shape : ICloneable<Shape>
{
    public int X { get; set; }
    public int Y { get; set; }
    public string? Color { get; set; }

    protected Shape()
    {
        Color = "Black";
    }

    protected Shape(Shape original)
    {
        X = original.X;
        Y = original.Y;
        Color = original.Color;
    }

    public abstract Shape Clone();
    public abstract Shape DeepClone();
    public abstract void Draw();
    public abstract string GetInfo();
}

public class CircleShape : Shape
{
    public int Radius { get; set; }

    public CircleShape()
    {
        Radius = 0;
    }

    public CircleShape(CircleShape original) : base(original)
    {
        Radius = original.Radius;
    }

    public override Shape Clone()
    {
        return new CircleShape(this);
    }

    public override Shape DeepClone()
    {
        return new CircleShape(this);
    }

    public override void Draw()
    {
        Console.WriteLine($"Drawing Circle at ({X}, {Y}) with radius {Radius}, color {Color}");
    }

    public override string GetInfo()
    {
        return $"Circle(X:{X}, Y:{Y}, Radius:{Radius}, Color:{Color})";
    }
}

public class RectangleShape : Shape
{
    public int Width { get; set; }
    public int Height { get; set; }

    public RectangleShape()
    {
        Width = 0;
        Height = 0;
    }

    public RectangleShape(RectangleShape original) : base(original)
    {
        Width = original.Width;
        Height = original.Height;
    }

    public override Shape Clone()
    {
        return new RectangleShape(this);
    }

    public override Shape DeepClone()
    {
        return new RectangleShape(this);
    }

    public override void Draw()
    {
        Console.WriteLine($"Drawing Rectangle at ({X}, {Y}), {Width}x{Height}, color {Color}");
    }

    public override string GetInfo()
    {
        return $"Rectangle(X:{X}, Y:{Y}, Width:{Width}, Height:{Height}, Color:{Color})";
    }
}
