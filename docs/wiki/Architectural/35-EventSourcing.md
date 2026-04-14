# Event Sourcing Pattern

## Overview

**Category:** Architectural Pattern  
**Purpose:** Store the state of an entity as a sequence of state-changing events instead of storing just the current state.  
**Complexity:** Hard  
**Use Case:** Audit trails, temporal queries, complex state management, event-driven systems

## Problem

Traditional state-based storage has limitations:
- Only current state is stored, history is lost
- Audit trails require separate logging
- Temporal queries (what was state at time T?) are difficult
- Concurrency conflicts hard to detect
- State transitions are implicit in updates

```csharp
// BAD: State-based storage - history lost
public class BankAccount
{
    public int Id { get; set; }
    public decimal Balance { get; set; }
    public DateTime LastModified { get; set; }

    public void Withdraw(decimal amount)
    {
        Balance -= amount;
        LastModified = DateTime.Now;
    }

    public void Deposit(decimal amount)
    {
        Balance += amount;
        LastModified = DateTime.Now;
    }
}

// Database:
// Id | Balance | LastModified
// 1  | 500     | 2024-04-14 10:00

// Problems:
// - Original balance (1000) is lost
// - Number of transactions unknown
// - Cannot query state at specific time
// - No audit trail of what happened
// - Concurrent updates cause conflicts
```

## Solution

Store all state changes as immutable events:

```csharp
// GOOD: Event Sourcing - complete history preserved

// Domain events
public abstract class DomainEvent
{
    public int AggregateId { get; set; }
    public DateTime OccurredAt { get; set; }
    public int Version { get; set; }
}

public class MoneyDepositedEvent : DomainEvent
{
    public decimal Amount { get; set; }
}

public class MoneyWithdrawnEvent : DomainEvent
{
    public decimal Amount { get; set; }
}

public class AccountCreatedEvent : DomainEvent
{
    public decimal InitialBalance { get; set; }
}

// Aggregate that rebuilds state from events
public class BankAccount
{
    private List<DomainEvent> _events = new();

    public int Id { get; set; }
    public decimal Balance { get; private set; }
    public int Version { get; private set; }

    // Rebuild account from event history
    public void LoadFromHistory(List<DomainEvent> events)
    {
        foreach (var @event in events.OrderBy(e => e.Version))
        {
            ApplyEvent(@event);
        }
    }

    // Apply event and record it
    public void Deposit(decimal amount)
    {
        var @event = new MoneyDepositedEvent
        {
            AggregateId = Id,
            Amount = amount,
            OccurredAt = DateTime.Now,
            Version = Version + 1
        };

        ApplyEvent(@event);
        _events.Add(@event);
    }

    public void Withdraw(decimal amount)
    {
        if (Balance < amount)
            throw new InvalidOperationException("Insufficient funds");

        var @event = new MoneyWithdrawnEvent
        {
            AggregateId = Id,
            Amount = amount,
            OccurredAt = DateTime.Now,
            Version = Version + 1
        };

        ApplyEvent(@event);
        _events.Add(@event);
    }

    // Apply event to current state
    private void ApplyEvent(DomainEvent @event)
    {
        switch (@event)
        {
            case AccountCreatedEvent ace:
                Id = ace.AggregateId;
                Balance = ace.InitialBalance;
                Version = ace.Version;
                break;

            case MoneyDepositedEvent mde:
                Balance += mde.Amount;
                Version = mde.Version;
                break;

            case MoneyWithdrawnEvent mwe:
                Balance -= mwe.Amount;
                Version = mwe.Version;
                break;
        }
    }

    public List<DomainEvent> GetUncommittedEvents() => _events;
}

// Event store
public interface IEventStore
{
    void SaveEvents(int aggregateId, List<DomainEvent> events, int expectedVersion);
    List<DomainEvent> GetEvents(int aggregateId);
    List<DomainEvent> GetAllEvents();
}

public class EventStore : IEventStore
{
    private List<DomainEvent> _events = new();
    private Dictionary<int, int> _versions = new();

    public void SaveEvents(int aggregateId, List<DomainEvent> events, int expectedVersion)
    {
        // Optimistic concurrency check
        if (_versions.ContainsKey(aggregateId) && _versions[aggregateId] != expectedVersion)
            throw new ConcurrencyException("Events have been updated");

        _events.AddRange(events);
        _versions[aggregateId] = events.Last().Version;
    }

    public List<DomainEvent> GetEvents(int aggregateId)
    {
        return _events.Where(e => e.AggregateId == aggregateId).ToList();
    }

    public List<DomainEvent> GetAllEvents()
    {
        return _events;
    }
}

// Repository using Event Sourcing
public class EventSourcedBankAccountRepository
{
    private readonly IEventStore _eventStore;

    public EventSourcedBankAccountRepository(IEventStore eventStore)
    {
        _eventStore = eventStore;
    }

    public BankAccount GetById(int id)
    {
        var events = _eventStore.GetEvents(id);
        var account = new BankAccount();
        account.LoadFromHistory(events);
        return account;
    }

    public void Save(BankAccount account)
    {
        var uncommittedEvents = account.GetUncommittedEvents();
        _eventStore.SaveEvents(account.Id, uncommittedEvents, account.Version - uncommittedEvents.Count);
    }
}

// Usage
var eventStore = new EventStore();
var repository = new EventSourcedBankAccountRepository(eventStore);

var account = new BankAccount();
account.Deposit(1000m);
repository.Save(account);

account.Withdraw(200m);
account.Deposit(500m);
repository.Save(account);

// Retrieve account - rebuilt from events
var loadedAccount = repository.GetById(account.Id);
Console.WriteLine($"Balance: {loadedAccount.Balance}");  // 1300

// View complete history
var allEvents = eventStore.GetAllEvents();
foreach (var @event in allEvents)
{
    Console.WriteLine($"{@event.OccurredAt}: {@event.GetType().Name}");
}
```

## Implementation Approaches

### 1. Append-Only Log

```csharp
// Event sourcing with append-only log
public class AppendOnlyEventLog
{
    private List<DomainEvent> _events = new();

    public void Append(DomainEvent @event)
    {
        @event.OccurredAt = DateTime.Now;
        @event.Version = _events.Count + 1;
        _events.Add(@event);
    }

    public List<DomainEvent> ReadAll() => _events.AsReadOnly().ToList();

    public List<DomainEvent> ReadFrom(int version)
    {
        return _events.Where(e => e.Version >= version).ToList();
    }

    public List<DomainEvent> ReadBetween(DateTime start, DateTime end)
    {
        return _events.Where(e => e.OccurredAt >= start && e.OccurredAt <= end).ToList();
    }
}

// Aggregate with temporal queries
public class Order
{
    private List<DomainEvent> _events = new();
    public int Id { get; set; }
    public OrderStatus Status { get; set; }

    public List<DomainEvent> GetHistory() => _events;

    public OrderStatus GetStatusAt(DateTime time)
    {
        var eventsUntilTime = _events.Where(e => e.OccurredAt <= time).ToList();
        var status = OrderStatus.Created;

        foreach (var @event in eventsUntilTime)
        {
            if (@event is OrderConfirmedEvent) status = OrderStatus.Confirmed;
            if (@event is OrderShippedEvent) status = OrderStatus.Shipped;
            if (@event is OrderDeliveredEvent) status = OrderStatus.Delivered;
        }

        return status;
    }
}
```

### 2. Snapshots for Performance

```csharp
// Snapshots reduce event replay time
public class AccountSnapshot
{
    public int AggregateId { get; set; }
    public decimal Balance { get; set; }
    public int SnapshotVersion { get; set; }
}

public class AccountWithSnapshots
{
    private List<DomainEvent> _events = new();
    public decimal Balance { get; private set; }
    public int Version { get; private set; }

    public void LoadFromSnapshot(AccountSnapshot snapshot, List<DomainEvent> eventsSinceSnapshot)
    {
        // Start from snapshot state
        Balance = snapshot.Balance;
        Version = snapshot.SnapshotVersion;

        // Apply only events after snapshot
        foreach (var @event in eventsSinceSnapshot.OrderBy(e => e.Version))
        {
            ApplyEvent(@event);
        }
    }

    private void ApplyEvent(DomainEvent @event)
    {
        if (@event is MoneyDepositedEvent mde)
            Balance += mde.Amount;
        if (@event is MoneyWithdrawnEvent mwe)
            Balance -= mwe.Amount;

        Version = @event.Version;
    }
}

public class SnapshotEventStore
{
    private Dictionary<int, AccountSnapshot> _snapshots = new();
    private List<DomainEvent> _events = new();

    public void CreateSnapshot(int aggregateId, AccountSnapshot snapshot)
    {
        _snapshots[aggregateId] = snapshot;
    }

    public (AccountSnapshot, List<DomainEvent>) GetAccountWithSnapshot(int aggregateId)
    {
        var snapshot = _snapshots.ContainsKey(aggregateId) 
            ? _snapshots[aggregateId] 
            : null;

        var allEvents = _events.Where(e => e.AggregateId == aggregateId).ToList();

        if (snapshot != null)
        {
            var eventsSinceSnapshot = allEvents
                .Where(e => e.Version > snapshot.SnapshotVersion)
                .ToList();
            return (snapshot, eventsSinceSnapshot);
        }

        return (null, allEvents);
    }
}
```

### 3. Event Projections

```csharp
// Projections create read models from events
public interface IProjection
{
    void Handle(DomainEvent @event);
}

public class AccountBalanceProjection : IProjection
{
    private Dictionary<int, decimal> _balances = new();

    public void Handle(DomainEvent @event)
    {
        if (@event is MoneyDepositedEvent mde)
        {
            if (!_balances.ContainsKey(mde.AggregateId))
                _balances[mde.AggregateId] = 0;
            _balances[mde.AggregateId] += mde.Amount;
        }

        if (@event is MoneyWithdrawnEvent mwe)
        {
            _balances[mwe.AggregateId] -= mwe.Amount;
        }
    }

    public decimal GetBalance(int accountId) => 
        _balances.ContainsKey(accountId) ? _balances[accountId] : 0;
}

public class TransactionHistoryProjection : IProjection
{
    private Dictionary<int, List<string>> _history = new();

    public void Handle(DomainEvent @event)
    {
        if (!_history.ContainsKey(@event.AggregateId))
            _history[@event.AggregateId] = new();

        if (@event is MoneyDepositedEvent mde)
            _history[@event.AggregateId].Add($"Deposited ${mde.Amount}");

        if (@event is MoneyWithdrawnEvent mwe)
            _history[@event.AggregateId].Add($"Withdrew ${mwe.Amount}");
    }

    public List<string> GetHistory(int accountId) =>
        _history.ContainsKey(accountId) ? _history[accountId] : new();
}

// Projection manager
public class ProjectionManager
{
    private List<IProjection> _projections = new();

    public void RegisterProjection(IProjection projection) => _projections.Add(projection);

    public void ProcessEvent(DomainEvent @event)
    {
        foreach (var projection in _projections)
        {
            projection.Handle(@event);
        }
    }
}
```

## Benefits of Event Sourcing

✅ **Complete Audit Trail** - Every change recorded  
✅ **Temporal Queries** - Can query state at any point in time  
✅ **Replay Events** - Rebuild state from history  
✅ **Debugging** - Understand exactly what happened  
✅ **Event-Driven** - Natural fit for event-driven architectures  
✅ **Scalability** - Read and write models can scale independently  

## Drawbacks

❌ **Complexity** - Significant additional complexity  
❌ **Event Storage** - Append-only log grows indefinitely  
❌ **Eventual Consistency** - Projections may lag behind events  
❌ **Snapshot Management** - Snapshots add operational overhead  
❌ **Testing** - Event-driven systems harder to test  
❌ **Debugging** - Event streams can be hard to debug  

## Interview Questions

**Q: What is Event Sourcing?**
A: Event Sourcing stores the state of an entity as a sequence of state-changing events, rather than just the current state. The current state is derived by replaying all events.

**Q: How does Event Sourcing differ from traditional database storage?**
A: Traditional storage keeps only current state. Event Sourcing keeps all events, allowing reconstruction of state at any point in time and providing complete audit trails.

**Q: What are snapshots in Event Sourcing?**
A: Snapshots capture the state at a specific version to avoid replaying all events from the beginning. This improves performance for aggregates with long histories.

**Q: How does Event Sourcing handle concurrency?**
A: Optimistic locking uses version numbers. If expected version doesn't match current version, the write fails with a concurrency exception.

**Q: What is CQRS and how does it relate to Event Sourcing?**
A: CQRS separates reads from writes. Event Sourcing often accompanies CQRS - events drive write model, projections create read models.

**Q: What are event projections?**
A: Projections are read models built by processing events. Multiple projections can exist for different query patterns, all built from the same event log.

## When to Use Event Sourcing

### ✅ Use When:
- Complete audit trail required
- Temporal queries needed
- Event-driven architecture
- High-volume writes with complex logic
- Need to replay history for debugging

### ❌ Don't Use When:
- Simple CRUD operations
- No need for audit trail
- Real-time consistency required
- Limited storage
- Team unfamiliar with pattern

## Real-World Examples

**Financial Systems** - Account transactions with complete history  
**Supply Chain** - Order status changes with timestamps  
**Document Management** - Document version history  
**Collaboration Tools** - Change tracking and undo/redo  
**Audit Logs** - Regulatory compliance requirements  

## Summary

Event Sourcing stores state as immutable events rather than mutable state, providing complete audit trails, temporal queries, and natural event-driven architecture support. While complex to implement, it's invaluable for systems requiring complete history, audit trails, or event-driven patterns. Often paired with CQRS for optimal scalability.

**Key Takeaway:** Event Sourcing trades storage and complexity for complete history and audit trails. Essential for event-driven systems and regulatory requirements.

---

**Related Patterns:**
- CQRS Pattern - Separates reads from writes
- Saga Pattern - Manages distributed transactions
- Snapshot Pattern - Optimizes event replay
