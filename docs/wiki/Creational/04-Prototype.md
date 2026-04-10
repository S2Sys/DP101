# Prototype Pattern

## Overview

**Category:** Creational Pattern  
**Purpose:** Create new objects by copying an existing object (prototype) rather than creating from scratch.  
**Complexity:** Medium  
**Key Concept:** Object cloning and copying

## Problem

When object creation is expensive:
- Creating from scratch involves complex initialization
- Loading data from database/network is slow
- Expensive computations needed
- You need independent copies of objects
- Want to avoid subclass explosion

Example: Clone a complex document template instead of creating from scratch

## Solution

Make objects cloneable and copy existing objects instead of creating new ones:

```csharp
var original = new Document { Title = "Template", Content = "..." };
var copy = original.Clone();  // Fast copy instead of expensive creation
```

## Implementation Approaches

### 1. Shallow Copy (MemberwiseClone)

```csharp
public class Person : ICloneable<Person>
{
    public string Name { get; set; }
    public int Age { get; set; }

    // Shallow copy - references are shared
    public Person ShallowClone()
    {
        return (Person)MemberwiseClone();
    }

    public Person Clone()
    {
        return new Person 
        { 
            Name = Name, 
            Age = Age 
        };
    }
}
```

**Pros:** Fast, simple  
**Cons:** Reference fields shared between copies

### 2. Deep Copy (Full Clone)

```csharp
public class Document : ICloneable<Document>
{
    public string Title { get; set; }
    public List<string> Tags { get; set; }

    // Deep copy - all fields copied
    public Document Clone()
    {
        return new Document
        {
            Title = Title,
            Tags = new List<string>(Tags)  // Copy collection
        };
    }

    public Document DeepClone()
    {
        var clone = (Document)MemberwiseClone();
        clone.Tags = new List<string>(Tags);
        return clone;
    }
}
```

**Pros:** Independent copies  
**Cons:** More complex, more overhead

### 3. Prototype Registry

```csharp
public class PrototypeRegistry<T> where T : ICloneable<T>
{
    private Dictionary<string, T> _prototypes = new();

    public void Register(string name, T prototype)
    {
        _prototypes[name] = prototype;
    }

    public T CreateFromPrototype(string name)
    {
        var prototype = _prototypes[name];
        if (prototype == null)
            throw new KeyNotFoundException($"Prototype '{name}' not found");

        return prototype.Clone();
    }
}

// Usage
var registry = new PrototypeRegistry<Shape>();
var circle = new Circle { X = 0, Y = 0, Radius = 10 };
registry.Register("default-circle", circle);

var copy = registry.CreateFromPrototype("default-circle");
```

### 4. Generic Deep Copy with Recursion

```csharp
public class SerializablePrototype : ICloneable<SerializablePrototype>
{
    public string Name { get; set; }
    public List<string> Items { get; set; }
    public NestedData Nested { get; set; }

    public SerializablePrototype DeepClone()
    {
        return new SerializablePrototype
        {
            Name = Name,
            Items = new List<string>(Items),
            Nested = Nested?.DeepClone()  // Recursive copy
        };
    }

    public SerializablePrototype Clone()
    {
        return (SerializablePrototype)MemberwiseClone();
    }
}

public class NestedData
{
    public string Value { get; set; }
    public List<int> Numbers { get; set; }

    public NestedData DeepClone() => new NestedData
    {
        Value = Value,
        Numbers = new List<int>(Numbers)
    };
}
```

### 5. ICloneable Interface

```csharp
// Standard .NET interface
public interface ICloneable
{
    object Clone();
}

// Generic version (better practice)
public interface ICloneable<T>
{
    T Clone();
    T DeepClone();
}
```

---

## Clone vs. Constructor Comparison

| Aspect | New Constructor | Clone |
|--------|-----------------|-------|
| **Performance** | Slower (initialization) | Faster (copy) |
| **Complexity** | Simple init logic | Complex object copy |
| **Dependencies** | Needs all constructors | Needs copy logic |
| **Shallow Copy** | N/A | Supported |
| **Deep Copy** | Must build manually | Can automate |

---

## Shallow vs. Deep Copy

```csharp
public class Address { public string City { get; set; } }

public class Person
{
    public string Name { get; set; }
    public Address Address { get; set; }
}

var original = new Person 
{ 
    Name = "John",
    Address = new Address { City = "NYC" }
};

// Shallow copy - Address is same object
var shallow = original.ShallowClone();
shallow.Address.City = "LA";
Console.WriteLine(original.Address.City);  // "LA" - SHARED!

// Deep copy - Address is new object
var deep = original.DeepClone();
deep.Address.City = "LA";
Console.WriteLine(original.Address.City);  // "NYC" - INDEPENDENT!
```

---

## Real-World Examples

### Template Cloning
```csharp
public class EmailTemplate
{
    public string Subject { get; set; }
    public string Body { get; set; }
    public List<string> Recipients { get; set; }

    public EmailTemplate Clone()
    {
        return new EmailTemplate
        {
            Subject = Subject,
            Body = Body,
            Recipients = new List<string>(Recipients)
        };
    }
}

// Usage
var template = _repository.GetTemplate("Welcome");
var emailCopy = template.Clone();
emailCopy.Recipients.Clear();
emailCopy.Recipients.Add("user@example.com");
// Original template unchanged
```

### Configuration Cloning
```csharp
public class AppConfig
{
    public Dictionary<string, string> Settings { get; set; }

    public AppConfig Clone()
    {
        return new AppConfig
        {
            Settings = new Dictionary<string, string>(Settings)
        };
    }
}

// Usage - create variations without affecting original
var production = _configs.Get("production");
var staging = production.Clone();
staging.Settings["database"] = "staging-db";
```

### Shape Cloning
```csharp
var original = new Circle { X = 10, Y = 20, Radius = 5, Color = "Red" };
var copy = original.Clone();
copy.X = 30;  // Original X = 10, Copy X = 30

// Apply transformations to copies
var shapes = new List<Shape>();
for (int i = 0; i < 10; i++)
{
    var shape = original.Clone();
    shape.X += i * 10;
    shapes.Add(shape);
}
```

---

## Pros and Cons

### Advantages
✅ **Performance** - Cloning faster than re-creation for complex objects  
✅ **Avoiding Dependencies** - Don't need all constructor parameters  
✅ **Polymorphism** - Can clone unknown types if implementing ICloneable  
✅ **Variations** - Easily create object variants  
✅ **Simplicity** - No complex factory logic  
✅ **Speed** - Faster than deserializing from storage  

### Disadvantages
❌ **Deep Copy Complexity** - Manual deep copy logic needed  
❌ **Circular References** - Can cause infinite loops  
❌ **Shallow Copy Confusion** - Easy to forget and share references  
❌ **ICloneable Issues** - Returns object type, not strongly typed  
❌ **Memory** - Clones use memory immediately  
❌ **Maintenance** - Clone logic must stay in sync with class  

---

## When to Use

### ✅ Use Prototype When:
- Creating objects is expensive
- Need many similar objects
- Creating variations of existing objects
- Avoid constructor parameter complexity
- Need to copy complex object graphs
- Prototype instances serve as templates

### ❌ Don't Use When:
- Object creation is cheap
- Objects are immutable
- Only need one instance
- Deep copy logic is too complex
- Prefer explicit construction
- Circular references likely

---

## Interview Questions

**Q: What's the difference between shallow and deep copy?**
A: Shallow copy shares references to nested objects; deep copy creates independent copies of everything.

**Q: When would you use Prototype over Factory?**
A: Prototype for expensive creation or creating variations; Factory for creating new objects from scratch.

**Q: How do you handle circular references in deep copy?**
A: Keep a visited set to detect cycles and reuse references instead of copying.

**Q: Is ICloneable a good interface?**
A: Microsoft recommends against it; create custom ICloneable<T> instead for type safety.

**Q: Can you clone immutable objects?**
A: Yes, but it's often unnecessary since they can't change. The clone is functionally identical.

---

## Common Pitfalls

❌ **Forgetting Deep Copy**
```csharp
// BAD - Shallow copy, references shared
public Person Clone()
{
    var copy = (Person)MemberwiseClone();
    return copy;  // Collections still shared!
}
```

✅ **Proper Deep Copy**
```csharp
// GOOD - Independent copy
public Person Clone()
{
    return new Person
    {
        Name = Name,
        Hobbies = new List<string>(Hobbies)
    };
}
```

---

## Best Practices

1. **Use Custom ICloneable<T>** - Type-safe alternative to ICloneable
2. **Document Shallow vs. Deep** - Be explicit about clone behavior
3. **Handle Collections** - Always copy collections in deep clone
4. **Test Clones** - Verify independence of clones
5. **Consider Immutability** - Simplifies clone logic
6. **Watch Circular References** - Can cause infinite loops
7. **Keep Clone Logic Updated** - Sync with class changes

---

## Summary

The Prototype pattern enables creating new objects by copying existing ones, which is faster than creating from scratch for complex objects. Use shallow copy for simple objects; use deep copy for complex object graphs with collections. Document clone behavior clearly to avoid confusion about object independence.

**Key Takeaway:** Prototype is perfect for expensive object creation or creating variations of existing objects.
