# Design Patterns Interview Q&A Guide

Common questions and answers for design pattern interviews, organized by pattern.

## Table of Contents

1. [Creational Patterns](#creational-patterns)
2. [Structural Patterns](#structural-patterns)
3. [Behavioral Patterns](#behavioral-patterns)
4. [Architectural Patterns](#architectural-patterns)
5. [General Pattern Questions](#general-pattern-questions)

---

## Creational Patterns

### Singleton

**Q: What is the Singleton pattern?**
A: A creational pattern that ensures a class has only one instance and provides a global point of access to it.

**Q: What are common variations of Singleton in C#?**
A: Basic, ThreadSafe (lock-based), Lazy<T>, Double-Checked Locking, Bill Pugh (static constructor).

**Q: What are the problems with Singleton?**
A: Global state, hard to test, thread safety issues, can hide dependencies.

**Q: How do you implement thread-safe Singleton in C#?**
A: Use Lazy<T> or static constructor approach. Example: `private static readonly Lazy<Singleton> instance = new(() => new Singleton());`

**Q: When should you NOT use Singleton?**
A: When you need multiple instances, when testing without mocks, when it hides dependencies.

### Factory Method

**Q: What is the Factory Method pattern?**
A: A creational pattern that defines an interface for creating objects, letting subclasses decide which class to instantiate.

**Q: How does Factory Method differ from Abstract Factory?**
A: Factory Method creates a single type of object, Abstract Factory creates families of related objects.

**Q: What are the benefits?**
A: Loose coupling, easier to extend, encapsulation of creation logic.

### Abstract Factory

**Q: What is the Abstract Factory pattern?**
A: Provides an interface for creating families of related or dependent objects without specifying their concrete classes.

**Q: When would you use Abstract Factory?**
A: When you have multiple families of objects (e.g., UI themes, database providers).

### Builder

**Q: What is the Builder pattern?**
A: Separates the construction of a complex object from its representation, allowing step-by-step construction.

**Q: What are the advantages over constructors?**
A: Cleaner API, handles many optional parameters, immutable objects possible.

**Q: How does fluent builder work in C#?**
A: Each method returns `this`, allowing method chaining.

### Prototype

**Q: What is the Prototype pattern?**
A: Creates new objects by copying an existing object (prototype) rather than creating from scratch.

**Q: What's the difference between shallow and deep copy?**
A: Shallow copy copies references, deep copy recursively copies all objects.

**Q: When is Prototype useful?**
A: When object creation is expensive, or when you need independent copies.

---

## Structural Patterns

### Adapter

**Q: What is the Adapter pattern?**
A: Converts the interface of a class to another interface clients expect, enabling incompatible interfaces to work together.

**Q: What are two types of Adapter?**
A: Class Adapter (inheritance) and Object Adapter (composition).

**Q: What's the difference from Decorator?**
A: Adapter changes interface, Decorator adds responsibility.

### Decorator

**Q: What is the Decorator pattern?**
A: Attaches additional responsibilities to an object dynamically, providing a flexible alternative to subclassing.

**Q: How does Decorator differ from Inheritance?**
A: Decorator is more flexible, allows runtime composition, avoids class explosion.

**Q: Real-world example in .NET?**
A: `BufferedStream` decorating `FileStream`, or middleware in ASP.NET.

### Facade

**Q: What is the Facade pattern?**
A: Provides a unified, simplified interface to a set of interfaces in a subsystem.

**Q: When should you use Facade?**
A: When you have a complex subsystem and want to simplify access to it.

### Proxy

**Q: What is the Proxy pattern?**
A: Provides a surrogate or placeholder for another object to control access to it.

**Q: What are common types of Proxy?**
A: Virtual (lazy loading), Protection (access control), Smart Reference (resource management), Logging.

**Q: How does Proxy differ from Decorator?**
A: Proxy controls access/creation, Decorator adds functionality.

### Composite

**Q: What is the Composite pattern?**
A: Composes objects into tree structures to represent part-whole hierarchies.

**Q: What problem does it solve?**
A: Treating individual objects and compositions uniformly.

**Q: Real-world example?**
A: File system (folders contain files and folders), UI components (panels contain buttons, panels, etc.).

### Bridge

**Q: What is the Bridge pattern?**
A: Decouples an abstraction from its implementation so they can vary independently.

**Q: When is Bridge useful?**
A: When you have multiple implementations of an interface and want to avoid class explosion.

### Flyweight

**Q: What is the Flyweight pattern?**
A: Uses sharing to support large numbers of fine-grained objects efficiently.

**Q: What's an example?**
A: String interning, character rendering in a text editor.

---

## Behavioral Patterns

### Observer

**Q: What is the Observer pattern?**
A: Defines a one-to-many dependency so that when one object changes state, all dependents are notified automatically.

**Q: How is it implemented in C#?**
A: Events and delegates, or IObservable/IObserver interfaces.

**Q: What's the difference from Pub-Sub?**
A: Observer is direct, synchronous. Pub-Sub is often indirect, asynchronous.

### Strategy

**Q: What is the Strategy pattern?**
A: Defines a family of algorithms, encapsulates each, and makes them interchangeable.

**Q: When would you use this?**
A: When you have multiple ways to do something and want to select at runtime.

### State

**Q: What is the State pattern?**
A: Allows an object to alter its behavior when its internal state changes.

**Q: How does it differ from Strategy?**
A: State is about object behavior based on state, Strategy is about selecting algorithms.

**Q: Real-world example?**
A: Traffic light (Red, Yellow, Green states), TCP connection states.

### Command

**Q: What is the Command pattern?**
A: Encapsulates a request as an object, allowing parametrization of clients with different requests.

**Q: What enables Undo/Redo?**
A: Each Command stores previous state and can execute/unexecute.

### Iterator

**Q: What is the Iterator pattern?**
A: Provides a way to access elements of an object sequentially without exposing its underlying representation.

**Q: How does C# implement this?**
A: IEnumerable/IEnumerator, foreach loop.

### Mediator

**Q: What is the Mediator pattern?**
A: Defines an object that encapsulates how a set of objects interact.

**Q: What problem does it solve?**
A: Reduces coupling between objects by centralizing communication.

**Q: Real-world example?**
A: Dialog box coordinator, air traffic control center.

### Chain of Responsibility

**Q: What is the Chain of Responsibility pattern?**
A: Passes a request along a chain of handlers, where each handler decides either to process or pass it on.

**Q: Real-world examples?**
A: Event handling in UI, logging frameworks, approval workflows.

### Visitor

**Q: What is the Visitor pattern?**
A: Represents an operation to be performed on elements of an object structure, letting you define new operations without changing the classes.

**Q: When is it useful?**
A: When you have a stable object structure but need to add many new operations.

**Q: Example?**
A: AST (Abstract Syntax Tree) traversal in compilers.

### Template Method

**Q: What is the Template Method pattern?**
A: Defines the skeleton of an algorithm, letting subclasses fill in steps.

**Q: How does it differ from Strategy?**
A: Template Method uses inheritance, Strategy uses composition.

### Memento

**Q: What is the Memento pattern?**
A: Captures and externalizes an object's internal state without violating encapsulation.

**Q: Use case?**
A: Undo/Redo functionality, savepoints in games.

### Interpreter

**Q: What is the Interpreter pattern?**
A: Defines a representation for a grammar and an interpreter to interpret sentences.

**Q: Use case?**
A: Query languages, expression evaluators.

---

## Architectural Patterns

### MVC

**Q: What is MVC?**
A: Model-View-Controller separates application into three components.

**Q: What are the responsibilities?**
A: Model (data/logic), View (presentation), Controller (user input).

**Q: Advantages?**
A: Separation of concerns, reusability, testability.

### MVVM

**Q: What is MVVM?**
A: Model-View-ViewModel for data binding-heavy applications.

**Q: When is MVVM preferred over MVC?**
A: WPF, Xamarin, Angular - platforms with strong data binding.

### CQRS

**Q: What is CQRS?**
A: Command Query Responsibility Segregation separates read and write models.

**Q: What are the benefits?**
A: Scalability, different optimization strategies for reads/writes.

**Q: What are the challenges?**
A: Eventual consistency, complexity, debugging.

### Event Sourcing

**Q: What is Event Sourcing?**
A: Stores application state as a sequence of events rather than current state.

**Q: Benefits?**
A: Complete audit trail, temporal queries, recovery.

**Q: Challenges?**
A: Complexity, eventual consistency, event versioning.

### Domain-Driven Design (DDD)

**Q: What is DDD?**
A: Focuses on modeling complex business domains.

**Q: Key concepts?**
A: Entities, Value Objects, Aggregates, Repositories, Domain Services.

**Q: When should you use DDD?**
A: Complex business domains, long-term projects.

### SAGA Pattern

**Q: What is the SAGA pattern?**
A: Manages distributed transactions across microservices.

**Q: What are two types?**
A: Orchestration (centralized), Choreography (event-driven).

**Q: How does compensation work?**
A: Each step has compensating transaction to undo on failure.

**Q: When would you use Saga?**
A: Multi-service business processes, distributed transactions.

**Q: Orchestration vs Choreography?**
A: Orchestration is explicit but central (single point of failure), Choreography is distributed but harder to follow.

### Dependency Injection

**Q: What is Dependency Injection?**
A: Technique to provide objects with their dependencies.

**Q: Three types?**
A: Constructor, Property/Setter, Interface injection.

**Q: Why use DI?**
A: Loose coupling, testability, flexibility.

### Repository Pattern

**Q: What is Repository?**
A: Abstracts data access, treating data source as in-memory collection.

**Q: Benefits?**
A: Abstraction of persistence, easier testing, easier switching data sources.

### Clean Architecture

**Q: What are the layers?**
A: Entities, Use Cases, Interface Adapters, Frameworks & Drivers.

**Q: What's the dependency rule?**
A: Dependencies point inward; nothing in inner layers know about outer layers.

---

## General Pattern Questions

### When to Use Patterns

**Q: Should you always use design patterns?**
A: No. Don't over-engineer. Use patterns when they solve real problems.

**Q: What's the simplest design pattern?**
A: Probably Singleton or Simple Factory for many cases.

**Q: How do you know when to refactor to a pattern?**
A: Code duplication, hard to extend, tight coupling, hard to test.

### Pattern Relationships

**Q: Can patterns be combined?**
A: Yes, often multiple patterns work together.

**Q: Example of pattern combination?**
A: Singleton Factory, Strategy with Decorator, Observer with Mediator.

### Anti-patterns

**Q: What is an anti-pattern?**
A: A pattern that appears to be beneficial but results in negative consequences.

**Q: Examples?**
A: Service Locator (anti-pattern to DI), God Class, Cargo Cult Programming.

### Choosing Between Similar Patterns

**Q: Adapter vs Decorator?**
A: Adapter changes interface, Decorator adds functionality.

**Q: Proxy vs Decorator?**
A: Proxy controls access, Decorator adds responsibility.

**Q: Strategy vs State?**
A: Strategy selects algorithms, State changes behavior based on internal state.

**Q: Factory Method vs Abstract Factory?**
A: Factory Method creates single products, Abstract Factory creates families.

**Q: Observer vs Mediator?**
A: Observer is one-to-many, Mediator is many-to-many with centralized communication.

### Testing with Patterns

**Q: How do patterns help testing?**
A: Dependency Injection, Strategy pattern, Facade simplify mocking.

**Q: What patterns are good for testing?**
A: DI Container, Repository, Abstract Factory, Strategy.

---

## Tips for Pattern Interviews

1. **Understand the problem first** - Don't just mention patterns. Explain why it solves the problem.
2. **Know variations** - Understand different implementations and when to use each.
3. **Real-world examples** - Have examples from your experience.
4. **Trade-offs** - Discuss pros and cons, not just benefits.
5. **When NOT to use** - Understanding when NOT to use a pattern is as important as when to use it.
6. **Implementation details** - Be ready to code examples.
7. **Combinations** - Understand how patterns work together.

---

**Last updated: 2026-04-10**
