# DP101 Design Patterns Wiki

Complete reference guide for ~99 design patterns in C# with implementations, examples, and interview preparation materials.

## Documentation Structure

### [Patterns Catalog](PATTERNS_CATALOG.md)
Complete index of all design patterns with categories and descriptions.

### [Interview Q&A Guide](INTERVIEW_QA.md)
Common interview questions and answers for all design patterns.

## Categories

### [Creational Patterns](Creational/README.md)
Patterns for object creation mechanisms

- Singleton
- Factory Method
- Abstract Factory
- Builder
- Prototype
- Object Pool

### [Structural Patterns](Structural/README.md)
Patterns for object composition and relationships

- Adapter
- Bridge
- Composite
- Decorator
- Facade
- Flyweight
- Proxy

### [Behavioral Patterns](Behavioral/README.md)
Patterns for object collaboration and responsibility distribution

- Chain of Responsibility
- Command
- Interpreter
- Iterator
- Mediator
- Memento
- Observer
- State
- Strategy
- Template Method
- Visitor

### [Architectural Patterns](Architectural/README.md)
Patterns for system organization and high-level structure

- MVC / MVVM / MVP
- CQRS
- Event Sourcing
- Domain-Driven Design
- **SAGA Pattern**
  - Orchestration-based
  - Choreography-based
  - Compensating Transactions
- Clean Architecture
- Layered Architecture
- And more...

### [Concurrency Patterns](Concurrency/README.md)
Patterns for multi-threaded and async programming

- Active Object
- Monitor Object
- Thread Pool
- Producer-Consumer
- Reader-Writer Lock
- And more...

### [Integration Patterns](Integration/README.md)
Patterns for system integration and communication

- Pipes and Filters
- Publish-Subscribe
- Message Queue
- Request-Reply
- And more...

## How to Use This Wiki

1. **Learning a pattern?** Start with the pattern's dedicated page (e.g., `Creational/01-Singleton.md`)
2. **Preparing for interviews?** Check [Interview Q&A Guide](INTERVIEW_QA.md)
3. **Need pattern comparison?** See [Patterns Catalog](PATTERNS_CATALOG.md)
4. **Looking for code examples?** Each pattern page includes implementation walkthrough

## Pattern Documentation Template

Each pattern documentation includes:

- **Intent** - What the pattern does and why
- **Problem** - The problem it solves
- **Solution** - How the pattern solves it
- **Structure** - Class/object diagram (ASCII or description)
- **Implementation** - Code walkthrough with explanations
- **Pros and Cons** - Advantages and disadvantages
- **Real-world Examples** - Where it's used in practice
- **Related Patterns** - Similar or complementary patterns
- **Use Cases** - When to use this pattern
- **Anti-patterns** - When NOT to use it
- **Variations** - Alternative implementations
- **Interview Questions** - Common questions about the pattern

## Code Examples

All code examples in this wiki are actual implementations from the `DP101.Core` project. You can:

- Review the source code in `src/DP101.Core/`
- Run unit tests in `src/DP101.Tests/`
- Execute examples in `src/DP101.Examples/`

## Quick Navigation

- Pattern Catalog: [PATTERNS_CATALOG.md](PATTERNS_CATALOG.md)
- Interview Q&A: [INTERVIEW_QA.md](INTERVIEW_QA.md)
- Diagrams: [diagrams/](diagrams/)

---

*Last updated: 2026-04-10*
