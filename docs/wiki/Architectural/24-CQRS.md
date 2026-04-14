# CQRS (Command Query Responsibility Segregation) Architecture

## Overview

**Category:** Architectural Pattern  
**Purpose:** Separate read (Query) and write (Command) models for scalability and optimization.  
**Complexity:** High  
**Common Use:** Event sourcing, microservices, high-traffic systems

## Problem

Single model for both reads and writes becomes:
- Inefficient (same normalization for both)
- Hard to scale (reads and writes different load)
- Complex queries (join multiple entities)
- Difficult to cache

```csharp
// BAD: Single model for reads and writes
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public List<Order> Orders { get; set; }  // Heavy for writes
    public List<Address> Addresses { get; set; }
    public List<Payment> Payments { get; set; }
}

// Complex to read, heavy to write
```

## Solution

Separate read and write models:

```csharp
// COMMAND MODEL (Write): Normalized for writes
public class UserCommand
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

// QUERY MODEL (Read): Denormalized for reads
public class UserReadModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
    public List<string> RecentPurchases { get; set; }
}

// COMMAND HANDLER: Write operations
public class CreateUserCommand
{
    public string Name { get; set; }
    public string Email { get; set; }
}

public class CreateUserCommandHandler
{
    private ICommandRepository _repository;

    public void Handle(CreateUserCommand cmd)
    {
        var user = new UserCommand { Name = cmd.Name, Email = cmd.Email };
        _repository.SaveUser(user);
    }
}

// QUERY HANDLER: Read operations
public class GetUserQuery
{
    public int UserId { get; set; }
}

public class GetUserQueryHandler
{
    private IQueryRepository _queryRepository;

    public UserReadModel Handle(GetUserQuery query)
    {
        return _queryRepository.GetUserReadModel(query.UserId);
    }
}
```

## Implementation

```csharp
// COMMAND SIDE (Normalized)
public class UserCommandService
{
    private ICommandDatabase _writeDb;

    public void CreateUser(CreateUserCommand cmd)
    {
        var user = new User { Name = cmd.Name, Email = cmd.Email };
        _writeDb.Users.Add(user);
        _writeDb.SaveChanges();
        
        PublishEvent(new UserCreatedEvent(user.Id, user.Name));
    }

    public void UpdateUser(int id, string newEmail)
    {
        var user = _writeDb.Users.Find(id);
        user.Email = newEmail;
        _writeDb.SaveChanges();
        
        PublishEvent(new UserEmailChangedEvent(id, newEmail));
    }

    private void PublishEvent(DomainEvent @event)
    {
        // Publish event to event bus
    }
}

// QUERY SIDE (Denormalized, Read-optimized)
public class UserQueryService
{
    private IQueryDatabase _readDb;

    public UserReadModel GetUserById(int id)
    {
        // Pre-calculated, denormalized for fast reads
        return _readDb.UserReadModels
            .FromSql("SELECT Id, Name, Email, TotalOrders, TotalSpent FROM UserReadModels WHERE Id = @id")
            .FirstOrDefault();
    }

    public List<UserReadModel> SearchUsers(string nameFilter)
    {
        // Optimized for search
        return _readDb.UserReadModels
            .Where(u => u.Name.Contains(nameFilter))
            .ToList();
    }
}

// EVENT HANDLER: Keeps read model in sync
public class UserEventProjector
{
    private IQueryDatabase _readDb;

    public void On(UserCreatedEvent @event)
    {
        var readModel = new UserReadModel
        {
            Id = @event.UserId,
            Name = @event.Name,
            TotalOrders = 0,
            TotalSpent = 0m
        };
        _readDb.UserReadModels.Add(readModel);
    }

    public void On(UserEmailChangedEvent @event)
    {
        var readModel = _readDb.UserReadModels.Find(@event.UserId);
        readModel.Email = @event.NewEmail;
    }

    public void On(OrderPlacedEvent @event)
    {
        var readModel = _readDb.UserReadModels.Find(@event.UserId);
        readModel.TotalOrders++;
        readModel.TotalSpent += @event.Amount;
        readModel.RecentPurchases.Add(@event.ItemName);
    }
}

// USAGE
public class UserController
{
    private readonly UserCommandService _commandService;
    private readonly UserQueryService _queryService;

    public void CreateUser(string name, string email)
    {
        _commandService.CreateUser(new CreateUserCommand { Name = name, Email = email });
    }

    public UserReadModel GetUser(int id)
    {
        return _queryService.GetUserById(id);
    }
}
```

---

## Pros and Cons

### Advantages
✅ **Independent Scaling** - Scale reads/writes separately  
✅ **Optimized Models** - Each side optimized for its purpose  
✅ **Performance** - Read model can be cached, indexed optimally  
✅ **Complex Queries** - Denormalized read models simplify queries  
✅ **Event Sourcing** - Works perfectly with event sourcing  

### Disadvantages
❌ **Complexity** - Two models to maintain  
❌ **Consistency** - Eventual consistency (read lag)  
❌ **Synchronization** - Keep models in sync  
❌ **Debugging** - Harder to trace data flow  
❌ **Overkill** - Not needed for simple applications  

---

## When to Use CQRS

### ✅ Use When:
- High read/write ratio imbalance
- Complex domain logic
- Need event sourcing
- Performance optimization critical
- Different teams for read/write
- Microservices architecture

### ❌ Don't Use When:
- Simple CRUD applications
- Balanced read/write patterns
- Strong consistency required
- Small team projects

---

## Summary

CQRS separates read and write models for independent optimization. Commands handle writes, Queries handle reads. Read models denormalized for performance. Perfect for complex systems and event sourcing. Trade-off: eventual consistency and complexity.

**Key Takeaway:** CQRS separates command (write) and query (read) models for independent optimization.
