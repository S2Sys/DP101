# DP101 - Comprehensive C# Design Patterns Project

A complete reference for ~99 design patterns in C# covering creational, structural, behavioral, architectural, concurrency, and integration patterns with detailed implementations, examples, tests, and wiki documentation.

## Project Structure

```
DP101/
├── src/
│   ├── DP101.Core/          # Core pattern implementations
│   ├── DP101.Examples/      # Console application with usage examples
│   └── DP101.Tests/         # Unit tests for all patterns
├── docs/
│   └── wiki/                # Comprehensive wiki documentation
└── DP101.sln               # Solution file
```

## Quick Start

### Building the Project

```bash
dotnet restore
dotnet build
```

### Running Tests

```bash
dotnet test
```

### Running Examples

```bash
dotnet run --project src/DP101.Examples
```

## Design Patterns Covered

### Creational Patterns
- Singleton (5 variations)
- Factory Method
- Abstract Factory
- Static Factory
- Builder
- Prototype
- Object Pool

### Structural Patterns
- Adapter
- Bridge
- Composite
- Decorator
- Facade
- Flyweight
- Proxy

### Behavioral Patterns
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

### Architectural Patterns
- MVC, MVVM, MVP
- CQRS
- Event Sourcing
- Domain-Driven Design (DDD)
- **SAGA Pattern (4 implementations)**
  - Orchestration-based
  - Choreography-based
  - Compensating transactions
  - Comparison guide
- Dependency Injection
- Repository
- Unit of Work
- And more...

### Concurrency Patterns
- Active Object
- Monitor Object
- Thread Pool
- Producer-Consumer
- Reader-Writer Lock
- Barrier
- And more...

### Integration Patterns
- Pipes and Filters
- Publish-Subscribe
- Message Queue
- Request-Reply
- Event-Driven Architecture
- And more...

## Features

✅ **Comprehensive implementations** - Each pattern includes:
- Core implementation
- Variations and alternative approaches
- Real-world use cases
- Pros and cons analysis
- When to use/avoid

✅ **Full test coverage** - Unit tests for all pattern implementations

✅ **Extensive wiki** - Detailed markdown documentation for each pattern

✅ **Interview preparation** - Common Q&A for all patterns

✅ **Executable examples** - Console app demonstrating pattern usage

## Technology Stack

- **Language:** C# 11+
- **Framework:** .NET 8+
- **Testing:** xUnit, Moq
- **Documentation:** Markdown

## Documentation

See the [wiki](docs/wiki/README.md) for comprehensive pattern documentation including:
- Pattern catalog and index
- Detailed code walkthroughs
- Architecture diagrams
- Interview Q&A sections
- Real-world examples

## Contributing

This project is part of S2Sys DP101 initiative.

## License

Educational project
