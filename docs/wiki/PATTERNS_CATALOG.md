# Design Patterns Catalog

Comprehensive index of all ~99 design patterns covered in DP101 project, organized by category.

## Creational Patterns (Object Creation)

Patterns that provide object creation mechanisms that increase flexibility and reuse of existing code.

| # | Pattern | Intent | Variations | Status |
|---|---------|--------|-----------|--------|
| 1 | Singleton | Ensure a class has only one instance | 5 variations | ✓ |
| 2 | Factory Method | Define interface for object creation | With abstract class | ✓ |
| 3 | Abstract Factory | Create families of related objects | Parameterized | ✓ |
| 4 | Static Factory | Create objects through static method | - | ✓ |
| 5 | Builder | Construct complex objects step by step | Fluent builder | ✓ |
| 6 | Prototype | Create objects by copying prototype | Shallow/Deep copy | ✓ |
| 7 | Object Pool | Reuse expensive objects | Thread-safe pool | ✓ |

## Structural Patterns (Object Composition)

Patterns that deal with object composition, creating relationships between entities to form larger structures.

| # | Pattern | Intent | Variations | Status |
|---|---------|--------|-----------|--------|
| 8 | Adapter | Convert interface to another clients expect | Class/Object adapter | ✓ |
| 9 | Bridge | Decouple abstraction from implementation | - | ✓ |
| 10 | Composite | Compose objects into tree structures | - | ✓ |
| 11 | Decorator | Attach responsibilities dynamically | Multiple decorators | ✓ |
| 12 | Facade | Provide unified interface to subsystem | - | ✓ |
| 13 | Flyweight | Share fine-grained objects efficiently | String interning | ✓ |
| 14 | Proxy | Provide surrogate for another object | Virtual/Protection/Logging | ✓ |

## Behavioral Patterns (Object Interaction)

Patterns that define how objects interact and distribute responsibility.

| # | Pattern | Intent | Variations | Status |
|---|---------|--------|-----------|--------|
| 15 | Chain of Responsibility | Pass request along chain of handlers | Dynamic chain | ✓ |
| 16 | Command | Encapsulate request as object | Undo/Redo | ✓ |
| 17 | Interpreter | Define grammar representation | - | ✓ |
| 18 | Iterator | Access elements sequentially | Reverse iterator | ✓ |
| 19 | Mediator | Reduce coupling between objects | Event-driven | ✓ |
| 20 | Memento | Capture/restore object state | Serialization | ✓ |
| 21 | Observer | Notify objects of state changes | Weak references | ✓ |
| 22 | State | Allow object to change behavior | Nested states | ✓ |
| 23 | Strategy | Define family of algorithms | Runtime selection | ✓ |
| 24 | Template Method | Define algorithm skeleton | Hooks | ✓ |
| 25 | Visitor | Represent operation on object structure | - | ✓ |

## Architectural Patterns

Patterns that define system-wide structures and high-level organization.

### Presentation Layer
| # | Pattern | Intent | Focus | Status |
|---|---------|--------|-------|--------|
| 26 | MVC | Separate model, view, controller | Web/Desktop | ✓ |
| 27 | MVVM | Separate view, model, view-model | Data binding | ✓ |
| 28 | MVP | Separate view, presenter, model | Testability | ✓ |

### Application Layer
| # | Pattern | Intent | Focus | Status |
|---|---------|--------|-------|--------|
| 29 | Dependency Injection | Inject dependencies | Loose coupling | ✓ |
| 30 | Service Locator | Locate services dynamically | Service discovery | ✓ |
| 31 | Repository | Abstract data access | Persistence | ✓ |
| 32 | Unit of Work | Coordinate transactions | Data consistency | ✓ |

### Domain Layer
| # | Pattern | Intent | Focus | Status |
|---|---------|--------|-------|--------|
| 33 | Domain-Driven Design | Model complex domains | Business logic | ✓ |
| 34 | CQRS | Separate read/write models | Scalability | ✓ |
| 35 | Event Sourcing | Store state as events | Audit trail | ✓ |

### Distributed Systems
| # | Pattern | Intent | Focus | Status |
|---|---------|--------|-------|--------|
| **36-39** | **SAGA Pattern** | **Manage distributed transactions** | **4 implementations** | ✓ |
| 36 | Orchestration SAGA | Central coordinator | Order processing | ✓ |
| 37 | Choreography SAGA | Event-driven | Microservices | ✓ |
| 38 | Compensating Transactions | Handle rollbacks | Distributed TX | ✓ |
| 39 | SAGA Comparison | When to use each | Decision guide | ✓ |
| 40 | API Gateway | Single entry point | API management | ✓ |
| 41 | Circuit Breaker | Prevent cascading failures | Resilience | ✓ |
| 42 | Retry Pattern | Retry failed operations | Fault tolerance | ✓ |
| 43 | Bulkhead Pattern | Isolate resources | Stability | ✓ |
| 44 | Strangler Pattern | Gradually replace systems | Migration | ✓ |

### Architecture Styles
| # | Pattern | Intent | Focus | Status |
|---|---------|--------|-------|--------|
| 45 | Clean Architecture | Organize by layers | Maintainability | ✓ |
| 46 | Hexagonal Architecture | Isolate domain | Testability | ✓ |
| 47 | Layered Architecture | Horizontal layers | Separation | ✓ |
| 48 | Microservices | Small independent services | Scalability | ✓ |

### Design Principles
| # | Pattern | Intent | Focus | Status |
|---|---------|--------|-------|--------|
| 49 | Single Responsibility | One reason to change | SOLID-S | ✓ |
| 50 | Open/Closed | Open for extension | SOLID-O | ✓ |
| 51 | Liskov Substitution | Substitutable subtypes | SOLID-L | ✓ |
| 52 | Interface Segregation | Many specific interfaces | SOLID-I | ✓ |
| 53 | Dependency Inversion | Depend on abstractions | SOLID-D | ✓ |

## Concurrency Patterns

Patterns for multi-threaded and asynchronous programming.

| # | Pattern | Intent | Use Case | Status |
|---|---------|--------|----------|--------|
| 54 | Active Object | Encapsulate async execution | Concurrent objects | ✓ |
| 55 | Monitor Object | Synchronize method execution | Thread safety | ✓ |
| 56 | Thread Pool | Manage thread lifecycle | Worker threads | ✓ |
| 57 | Producer-Consumer | Separate production/consumption | Message queues | ✓ |
| 58 | Reader-Writer Lock | Multiple readers, single writer | Concurrent reads | ✓ |
| 59 | Barrier | Synchronize threads | Coordination | ✓ |
| 60 | Mutex | Mutual exclusion | Critical sections | ✓ |
| 61 | Semaphore | Count-based access | Resource pools | ✓ |
| 62 | Read-Write Lock (Advanced) | Optimized read access | High-read scenarios | ✓ |
| 63 | Async/Await Patterns | Async operations | I/O operations | ✓ |
| 64 | Task Parallel Library | Task-based parallelism | Data parallelism | ✓ |
| 65 | Reactive Extensions | Observable streams | Event streams | ✓ |

## Integration Patterns

Patterns for system integration and inter-process communication.

| # | Pattern | Intent | Protocol | Status |
|---|---------|--------|----------|--------|
| 66 | Pipes and Filters | Chain processing | Sequential | ✓ |
| 67 | Publish-Subscribe | Event distribution | Topics | ✓ |
| 68 | Message Queue | Async messaging | Queue | ✓ |
| 69 | Request-Reply | Synchronous messaging | RPC-style | ✓ |
| 70 | Event-Driven Architecture | React to events | Events | ✓ |
| 71 | Polling Consumer | Periodically check | Polling | ✓ |
| 72 | Selective Consumer | Filter messages | Routing | ✓ |
| 73 | Durable Subscriber | Guarantee delivery | Persistence | ✓ |
| 74 | Idempotent Receiver | Handle duplicates | Reliability | ✓ |
| 75 | Dead Letter Channel | Handle errors | Error handling | ✓ |

## Enterprise Patterns

Patterns for enterprise application development.

| # | Pattern | Intent | Focus | Status |
|---|---------|--------|-------|--------|
| 76 | Inversion of Control | Invert control flow | Framework design | ✓ |
| 77 | DI Container | Manage dependencies | Wiring | ✓ |
| 78 | Registry Pattern | Central registry | Service discovery | ✓ |
| 79 | Interceptor Pattern | Intercept operations | Cross-cutting | ✓ |
| 80 | Aspect-Oriented Programming | Separate concerns | Modularity | ✓ |
| 81 | Transaction Script | Organize procedures | Business logic | ✓ |
| 82 | Domain Model | Object-oriented domain | Domain logic | ✓ |
| 83 | Table Module | Operate on recordsets | Data operations | ✓ |
| 84 | Service Layer | Boundary for services | API design | ✓ |
| 85 | Data Transfer Object | Transfer data | Communication | ✓ |
| 86 | Value Object | Identity-less objects | Domain modeling | ✓ |
| 87 | Aggregate | Cluster entities | Consistency | ✓ |
| 88 | Entity | Identity-based objects | Domain modeling | ✓ |

## Data Access Patterns

Patterns for data access and persistence.

| # | Pattern | Intent | Technology | Status |
|---|---------|--------|-----------|--------|
| 89 | Active Record | Domain + DB mapping | ORM | ✓ |
| 90 | Data Mapper | Separation of mapping | ORM | ✓ |
| 91 | Query Object | Build queries | Dynamic SQL | ✓ |
| 92 | Repository Pattern | Abstraction over DAL | Persistence | ✓ |
| 93 | Identity Map | Cache loaded objects | Memory | ✓ |

## Testing Patterns

Patterns for unit and integration testing.

| # | Pattern | Intent | Use | Status |
|---|---------|--------|-----|--------|
| 94 | Mock Object | Simulate dependencies | Behavior testing | ✓ |
| 95 | Stub Object | Provide fixed data | State testing | ✓ |
| 96 | Spy Object | Record interactions | Verification | ✓ |
| 97 | Fake Object | Working implementation | Testing | ✓ |
| 98 | Dummy Object | Placeholder | Compilation | ✓ |

## Advanced Patterns

| # | Pattern | Intent | Focus | Status |
|---|---------|--------|-------|--------|
| 99 | Double Dispatch | Polymorphic behavior | Type resolution | ✓ |

---

## Pattern Selection Guide

### By Problem Type

**Object Creation?**
- Simple: Factory Method, Static Factory
- Complex: Abstract Factory, Builder
- Singleton: Single instance required
- Reuse: Prototype, Object Pool

**Object Composition?**
- Combine: Composite, Decorator
- Adapt: Adapter, Facade
- Substitute: Proxy, Bridge
- Optimize: Flyweight

**Object Interaction?**
- Chain: Chain of Responsibility
- Sequence: Iterator
- Communication: Observer, Mediator
- State: State, Strategy
- Operations: Visitor, Template Method
- Request: Command
- Context: Interpreter, Memento

**System Structure?**
- Web UI: MVC, MVVM, MVP
- Enterprise: Layered, Clean, Hexagonal
- Microservices: Saga, CQRS, Event Sourcing
- Distributed: Circuit Breaker, Retry, Bulkhead

**Multi-threading?**
- Synchronization: Monitor, Mutex, Semaphore
- Async: Active Object, Task Parallel, Reactive
- Patterns: Producer-Consumer, Reader-Writer

**Integration?**
- Messaging: Pipes, Publish-Subscribe, Message Queue
- Communication: Request-Reply, Event-Driven
- Reliability: Idempotent, Durable, Dead Letter

---

**Total: ~99 Design Patterns**
