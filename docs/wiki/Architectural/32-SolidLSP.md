# SOLID: Liskov Substitution Principle (LSP)

## Overview

**Category:** Architectural Principle  
**Principle:** Objects of a superclass should be replaceable with objects of its subclasses without breaking the application.  
**Complexity:** Medium-Hard  
**Use Case:** Inheritance design, polymorphism, contract verification

## Problem

Subclasses that don't properly implement their parent contract:
- Break substitutability
- Cause unexpected runtime behavior
- Violate caller expectations
- Create hidden contracts in documentation

```csharp
// BAD: Violates Liskov Substitution Principle
public class Bird
{
    public virtual void Fly()
    {
        Console.WriteLine("Flying...");
    }
}

public class Sparrow : Bird
{
    public override void Fly()
    {
        Console.WriteLine("Sparrow flying");
    }
}

public class Penguin : Bird
{
    public override void Fly()
    {
        throw new NotSupportedException("Penguins cannot fly!");
    }
}

// Client code assumes all Birds can fly
public class AviaryManagement
{
    public void MakeBirdsFly(List<Bird> birds)
    {
        foreach (var bird in birds)
        {
            bird.Fly();  // BOOM! Penguin throws exception
        }
    }
}

// Usage
var birds = new List<Bird>
{
    new Sparrow(),
    new Penguin(),  // This breaks the contract!
    new Sparrow()
};

var aviary = new AviaryManagement();
aviary.MakeBirdsFly(birds);  // Exception at runtime!

// Problems:
// - Penguin cannot be substituted for Bird
// - Violates Liskov Substitution Principle
// - Client code must check type to avoid exception
// - Hidden contract not declared in signature
```

## Solution

Use proper inheritance hierarchy respecting the contract:

```csharp
// GOOD: Liskov Substitution Principle respected

// Base class - all birds have this ability
public abstract class Bird
{
    public abstract void Move();
}

// Flying birds follow this contract
public abstract class FlyingBird : Bird
{
    public override void Move() => Fly();
    
    public abstract void Fly();
}

// Non-flying birds follow different contract
public abstract class NonFlyingBird : Bird
{
    public override void Move() => Walk();
    
    public abstract void Walk();
}

// Concrete flying bird
public class Sparrow : FlyingBird
{
    public override void Fly()
    {
        Console.WriteLine("Sparrow flying");
    }
}

// Concrete non-flying bird
public class Penguin : NonFlyingBird
{
    public override void Walk()
    {
        Console.WriteLine("Penguin walking");
    }
}

// Client code works with correct abstraction
public class AviaryManagement
{
    public void MakeBirdsMove(List<Bird> birds)
    {
        foreach (var bird in birds)
        {
            bird.Move();  // Works for all birds!
        }
    }

    public void MakeBirdsFlying(List<FlyingBird> birds)
    {
        foreach (var bird in birds)
        {
            bird.Fly();  // Only called with flying birds
        }
    }
}

// Usage
var allBirds = new List<Bird>
{
    new Sparrow(),
    new Penguin(),
    new Sparrow()
};

var aviary = new AviaryManagement();
aviary.MakeBirdsMove(allBirds);  // Works perfectly!

// This won't compile - correct!
// aviary.MakeBirdsFlying(new List<Bird> { new Penguin() });
```

## Implementation Approaches

### 1. Rectangle-Square Problem

```csharp
// BAD: Square violates Rectangle contract
public class Rectangle
{
    public virtual int Width { get; set; }
    public virtual int Height { get; set; }

    public int GetArea() => Width * Height;
}

public class Square : Rectangle
{
    private int _side;

    public override int Width
    {
        get => _side;
        set => _side = value;
    }

    public override int Height
    {
        get => _side;
        set => _side = value;  // Violates Rectangle contract!
    }
}

// Client assumes Rectangle contract: setting Width and Height independently
public void TestRectangleArea(Rectangle rect)
{
    rect.Width = 5;
    rect.Height = 4;
    Assert.AreEqual(20, rect.GetArea());  // Fails for Square!
}

// Problems:
// - Square breaks Rectangle contract
// - Width and Height are not independent
// - Client code cannot substitute Square for Rectangle

// GOOD: Use composition or correct inheritance
public abstract class Shape
{
    public abstract int GetArea();
}

public class Rectangle : Shape
{
    public int Width { get; set; }
    public int Height { get; set; }

    public override int GetArea() => Width * Height;
}

public class Square : Shape
{
    public int Side { get; set; }

    public override int GetArea() => Side * Side;
}

// Now Square is not a Rectangle, but both implement Shape contract
public void TestShapeArea(Shape shape)
{
    // Works for both Rectangle and Square
    Assert.IsTrue(shape.GetArea() >= 0);
}
```

### 2. Collection Contract

```csharp
// BAD: ReadOnlyCollection violates Collection contract
public class Collection<T>
{
    protected List<T> _items = new();

    public virtual void Add(T item) => _items.Add(item);
    public virtual void Remove(T item) => _items.Remove(item);
    public virtual int Count => _items.Count;
}

public class ReadOnlyCollection<T> : Collection<T>
{
    public override void Add(T item)
    {
        throw new NotSupportedException("Collection is read-only");
    }

    public override void Remove(T item)
    {
        throw new NotSupportedException("Collection is read-only");
    }
}

// Client assumes Collection contract
public void ProcessCollection<T>(Collection<T> collection, T item)
{
    collection.Add(item);  // BOOM! Fails for ReadOnlyCollection
    Assert.AreEqual(1, collection.Count);
}

// GOOD: Use proper interface hierarchy
public interface IReadOnlyCollection<out T>
{
    int Count { get; }
    IEnumerable<T> GetItems();
}

public interface ICollection<T> : IReadOnlyCollection<T>
{
    void Add(T item);
    void Remove(T item);
}

public class Collection<T> : ICollection<T>
{
    protected List<T> _items = new();

    public int Count => _items.Count;

    public void Add(T item) => _items.Add(item);
    public void Remove(T item) => _items.Remove(item);
    public IEnumerable<T> GetItems() => _items.AsReadOnly();
}

public class ReadOnlyCollection<T> : IReadOnlyCollection<T>
{
    private List<T> _items = new();

    public int Count => _items.Count;
    public IEnumerable<T> GetItems() => _items.AsReadOnly();
}

// Client code specifies correct contract
public void ProcessReadOnlyCollection<T>(IReadOnlyCollection<T> collection)
{
    Console.WriteLine($"Items: {collection.Count}");
}

public void ProcessMutableCollection<T>(ICollection<T> collection, T item)
{
    collection.Add(item);
    Assert.AreEqual(1, collection.Count);
}
```

### 3. Payment Processor Contract

```csharp
// BAD: CryptoCurrencyProcessor violates PaymentProcessor contract
public abstract class PaymentProcessor
{
    public abstract decimal ProcessPayment(decimal amount);
    // Contract: Always processes payment successfully or throws
    // Returns the processed amount
}

public class CreditCardProcessor : PaymentProcessor
{
    public override decimal ProcessPayment(decimal amount)
    {
        Console.WriteLine($"Processing: ${amount}");
        return amount;
    }
}

public class CryptoCurrencyProcessor : PaymentProcessor
{
    public override decimal ProcessPayment(decimal amount)
    {
        // Violates contract - can fail silently
        if (DateTime.Now.Hour < 9)
        {
            return 0;  // Returns 0 instead of throwing!
        }
        return amount;
    }
}

// Client assumes contract: ProcessPayment always processes or throws
public void ChargeCustomer(PaymentProcessor processor, decimal amount)
{
    var processed = processor.ProcessPayment(amount);
    // Assumes processed == amount or exception
    Console.WriteLine($"Charged: ${processed}");
}

// GOOD: Define contract clearly
public abstract class PaymentProcessor
{
    /// <summary>
    /// Processes a payment.
    /// </summary>
    /// <param name="amount">Amount to process</param>
    /// <returns>Processed amount</returns>
    /// <exception cref="PaymentFailedException">
    /// Thrown if payment processing fails
    /// </exception>
    public abstract decimal ProcessPayment(decimal amount);
}

public class CreditCardProcessor : PaymentProcessor
{
    public override decimal ProcessPayment(decimal amount)
    {
        Console.WriteLine($"Processing: ${amount}");
        return amount;
    }
}

public class CryptoCurrencyProcessor : PaymentProcessor
{
    public override decimal ProcessPayment(decimal amount)
    {
        if (DateTime.Now.Hour < 9)
        {
            throw new PaymentFailedException("Market not open yet");
        }
        return amount;
    }
}

public class PaymentFailedException : Exception
{
    public PaymentFailedException(string message) : base(message) { }
}

// Client code - contract is now respected
public void ChargeCustomer(PaymentProcessor processor, decimal amount)
{
    try
    {
        var processed = processor.ProcessPayment(amount);
        Console.WriteLine($"Charged: ${processed}");
    }
    catch (PaymentFailedException ex)
    {
        Console.WriteLine($"Payment failed: {ex.Message}");
    }
}
```

### 4. Preconditions and Postconditions

```csharp
// BAD: Subclass strengthens preconditions (violates LSP)
public class PaymentService
{
    public virtual bool ValidatePayment(Payment payment)
    {
        return payment.Amount > 0;  // Precondition: Amount > 0
    }

    public virtual bool ProcessPayment(Payment payment)
    {
        if (!ValidatePayment(payment))
            throw new InvalidOperationException();
        return true;
    }
}

public class PremiumPaymentService : PaymentService
{
    public override bool ValidatePayment(Payment payment)
    {
        // VIOLATES LSP: Strengthens precondition
        return payment.Amount > 100;  // Now requires Amount > 100
    }
}

// Client assumes parent contract
public void ProcessUserPayment(PaymentService service, Payment payment)
{
    if (payment.Amount > 0)
    {
        service.ProcessPayment(payment);  // Fails for Premium!
    }
}

// GOOD: Respect contract
public class PaymentService
{
    public virtual bool ValidatePayment(Payment payment)
    {
        return payment.Amount > 0;
    }

    public virtual bool ProcessPayment(Payment payment)
    {
        if (!ValidatePayment(payment))
            throw new InvalidOperationException();
        return true;
    }
}

public class PremiumPaymentService : PaymentService
{
    // GOOD: Can weaken precondition (accept <= 0) but shouldn't!
    // Or can strengthen postcondition (e.g., ensure reward points)
    public override bool ProcessPayment(Payment payment)
    {
        var result = base.ProcessPayment(payment);
        
        // Strengthen postcondition: also award points
        if (result)
        {
            AwardRewardPoints(payment.Amount);
        }
        
        return result;
    }

    private void AwardRewardPoints(decimal amount)
    {
        Console.WriteLine($"Awarded {(int)amount * 10} points");
    }
}
```

## Benefits of LSP

✅ **Predictable Behavior** - Subclasses behave as expected  
✅ **Polymorphism** - Substitution works correctly  
✅ **Reduced Bugs** - No hidden contracts  
✅ **Better Testing** - Can test with any subclass  
✅ **Code Reuse** - Polymorphism enables generic code  
✅ **Maintainability** - Clear inheritance contracts  

## Drawbacks

❌ **Design Complexity** - Requires careful hierarchy design  
❌ **Inheritance Issues** - Difficult to retrofit existing classes  
❌ **Over-Abstraction** - Can create unnecessary base classes  
❌ **Performance** - Virtual calls add overhead  

## Interview Questions

**Q: What is Liskov Substitution Principle?**
A: Subtypes must be substitutable for their base types without breaking the application. A subclass should not violate the contract of its parent class.

**Q: Give an example of LSP violation.**
A: The Rectangle-Square problem. Square inherits from Rectangle but changes its contract - setting Width and Height independently no longer works. Square violates Rectangle's contract.

**Q: How does LSP relate to inheritance?**
A: LSP defines the proper use of inheritance. A subclass can only inherit from a parent if it respects the parent's contract. Violation indicates incorrect inheritance hierarchy.

**Q: What's wrong with throwing NotSupportedException in a subclass?**
A: It violates LSP. The caller expects the operation to work (or throw expected exceptions). Throwing NotSupportedException violates the parent contract and prevents substitution.

**Q: Can subclasses have stronger preconditions?**
A: No, that violates LSP. Subclasses can weaken preconditions (accept more inputs) but not strengthen them. This breaks the contract.

**Q: Can subclasses have weaker postconditions?**
A: No, that violates LSP. Subclasses can strengthen postconditions (provide more) but not weaken them. Weakening breaks the contract.

## When to Apply LSP

### ✅ Apply When:
- Using inheritance or polymorphism
- Creating subclass hierarchies
- Implementing interfaces
- Designing class contracts
- Using base class references

### ❌ Don't Apply When:
- No inheritance involved
- Using composition over inheritance
- Contracts are not clearly defined

## Code Smell: Type Checking

```csharp
// ANTI-PATTERN: Type checking indicates LSP violation
public void ProcessBird(Bird bird)
{
    if (bird is Penguin)
    {
        var penguin = (Penguin)bird;
        penguin.Walk();
    }
    else
    {
        bird.Fly();
    }
}

// SOLUTION: Proper inheritance respecting LSP
public void ProcessBird(Bird bird)
{
    bird.Move();  // Works for all birds correctly
}
```

## Real-World Patterns Supporting LSP

**Strategy Pattern** - Strategies are substitutable implementations  
**Decorator Pattern** - Decorators properly implement wrapped interface  
**Adapter Pattern** - Adapters provide expected interface  
**Template Method Pattern** - Subclasses follow template contract  

## Summary

Liskov Substitution Principle ensures that subclasses can be used wherever their parent types are expected. This enables true polymorphism and prevents unexpected behavior. LSP requires careful design of inheritance hierarchies and clear specification of contracts. Violations typically indicate incorrect hierarchy design.

**Key Takeaway:** Subclasses must respect their parent's contract. Type checking usually indicates an LSP violation.

---

**Related SOLID Principles:**
- Single Responsibility Principle - Each class has one reason to change
- Open/Closed Principle - Classes open for extension, closed for modification
- Interface Segregation Principle - Clients shouldn't depend on unused methods
- Dependency Inversion Principle - Depend on abstractions, not concrete implementations
