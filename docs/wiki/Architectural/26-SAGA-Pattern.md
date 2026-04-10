# SAGA Pattern - Distributed Transaction Management

## Overview

**Category:** Architectural Pattern  
**Purpose:** Manage a sequence of distributed transactions across multiple services in a microservices architecture.  
**Complexity:** Advanced  
**Use in Systems:** Microservices, Distributed Systems

## Problem

In a monolithic application, ACID transactions ensure consistency. However, in microservices architectures:

- Multiple independent services cannot participate in a single ACID transaction
- Network calls are unreliable
- Services may fail at any point
- Traditional two-phase commit doesn't work well
- Need to maintain data consistency across service boundaries

Example: E-commerce order processing
1. Payment service processes payment
2. Inventory service reserves items
3. Shipping service creates shipment
4. Notification service sends confirmation

If step 3 fails after step 1 succeeds, what happens to the payment?

## Solution

The SAGA pattern breaks a distributed transaction into a sequence of local transactions, each with a compensating transaction. If any step fails, compensating transactions undo previous steps.

### Two Main Approaches

## 1. Orchestration-Based SAGA

### Characteristics
- **Central Orchestrator** - Controls the flow
- **Synchronous Communication** - Often uses request/response
- **Explicit Workflow** - Clear to understand
- **Centralized Logic** - Easier to debug

### Architecture

```
┌─────────────────┐
│  Order Service  │
│  (Orchestrator) │
└────────┬────────┘
         │
    ┌────┴────┬──────────┬────────────┐
    │          │          │            │
    ▼          ▼          ▼            ▼
┌──────────┐ ┌─────────────┐ ┌──────────┐
│ Payment  │ │ Inventory   │ │ Shipping │
│ Service  │ │ Service     │ │ Service  │
└──────────┘ └─────────────┘ └──────────┘
```

### Execution Flow

1. **Orchestrator starts saga**
2. **Call Payment Service**
   - Success → Proceed to next step
   - Failure → Start compensation
3. **Call Inventory Service**
   - Success → Proceed to shipping
   - Failure → Compensate payment
4. **Call Shipping Service**
   - Success → Saga complete
   - Failure → Compensate inventory, then payment

### Implementation Example

```csharp
public class OrderSagaOrchestrator
{
    public bool ProcessOrder(Order order)
    {
        try
        {
            // Step 1: Reserve inventory
            var inventoryResult = _inventoryService
                .ReserveItems(order.OrderId, order.Items);
            if (!inventoryResult.Success)
                return false;

            // Step 2: Process payment
            var paymentResult = _paymentService
                .ProcessPayment(order.OrderId, order.TotalAmount);
            if (!paymentResult.Success)
            {
                _inventoryService.ReleaseReservation(order.OrderId);
                return false;
            }

            // Step 3: Create shipment
            var shippingResult = _shippingService
                .CreateShipment(order.OrderId);
            if (!shippingResult.Success)
            {
                _paymentService.RefundPayment(paymentResult.PaymentId);
                _inventoryService.ReleaseReservation(order.OrderId);
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            // Compensation logic
            return false;
        }
    }
}
```

### Advantages
✅ Explicit workflow - Easy to understand  
✅ Centralized logic - Simpler to implement  
✅ Better error handling - Can implement complex retry logic  
✅ Debugging - Easier to trace flow  

### Disadvantages
❌ Single point of failure - Orchestrator must be resilient  
❌ Tight coupling - Orchestrator knows all services  
❌ Can become monolithic - Orchestrator grows complex  
❌ Network overhead - Direct calls to all services  

### Best For
- Workflows with clear sequence
- Workflows where orchestrator can be stateless
- Systems with smaller number of services
- Complex business logic in orchestration

---

## 2. Choreography-Based SAGA

### Characteristics
- **Event-Driven** - Services react to events
- **Decentralized** - No central orchestrator
- **Asynchronous** - Non-blocking calls
- **Loosely Coupled** - Services don't know each other

### Architecture

```
┌──────────────────────────────────┐
│       Event Bus / Broker         │
└────┬─────────┬────────┬──────────┘
     │         │        │
     ▼         ▼        ▼
┌────────┐ ┌────────┐ ┌──────────┐
│Payment │ │Inventory│ │ Shipping │
│Service │ │Service │ │ Service  │
└────────┘ └────────┘ └──────────┘
     │         │        │
     └────┬────┴────┬───┘
          │         │
     Published   Published
     Events      Events
```

### Execution Flow

1. **Order Service publishes OrderCreatedEvent**
2. **Inventory Service subscribes**
   - Reserves items
   - Publishes InventoryReservedEvent
3. **Payment Service subscribes to InventoryReservedEvent**
   - Processes payment
   - Publishes PaymentCompletedEvent or PaymentFailedEvent
4. **Shipping Service subscribes to PaymentCompletedEvent**
   - Creates shipment
   - Publishes ShipmentCreatedEvent or ShipmentFailedEvent
5. **Services publish compensation events on failure**

### Implementation Example

```csharp
public class PaymentServiceChoreography
{
    public PaymentServiceChoreography(IEventBus eventBus)
    {
        _eventBus = eventBus;
        _eventBus.Subscribe<PaymentStartedEvent>(
            OnPaymentStarted);
        _eventBus.Subscribe<CompensationStartedEvent>(
            OnCompensationStarted);
    }

    private void OnPaymentStarted(PaymentStartedEvent @event)
    {
        try
        {
            ProcessPayment(@event.Amount);
            _eventBus.Publish(
                new PaymentCompletedEvent(@event.OrderId, paymentId));
        }
        catch (Exception ex)
        {
            _eventBus.Publish(
                new PaymentFailedEvent(@event.OrderId, ex.Message));
        }
    }
}
```

### Advantages
✅ No single point of failure  
✅ Loose coupling - Services independent  
✅ Better scalability - Async processing  
✅ Service autonomy - Each service owns its logic  
✅ Resilient - Services continue if one fails  

### Disadvantages
❌ Hard to follow - Flow isn't explicit  
❌ Difficult debugging - Distributed logic  
❌ Event ordering - Events may arrive out of order  
❌ Cyclic events - Risk of infinite loops  
❌ Testing complexity - Need to test event sequences  

### Best For
- Loosely coupled microservices
- High availability requirements
- Systems with many services
- Event-driven architectures

---

## Comparison Matrix

| Aspect | Orchestration | Choreography |
|--------|---------------|--------------|
| **Complexity** | Medium | High |
| **Coupling** | Tight | Loose |
| **Flow Visibility** | Explicit | Implicit |
| **Debugging** | Easier | Harder |
| **Single Point of Failure** | Orchestrator | None |
| **Scalability** | Limited | Better |
| **Event Ordering** | Controlled | Risk |
| **Testing** | Easier | Harder |
| **Best for Small Systems** | Yes | Maybe |
| **Best for Large Systems** | Maybe | Yes |

## Compensating Transactions

### Pattern
Each forward step has a corresponding compensating transaction:

```
Forward Step  ←→  Compensating Transaction
─────────────     ────────────────────────
Payment       ←→  Refund
Reserve Stock ←→  Release Stock
Create Order  ←→  Cancel Order
```

### Implementation

```csharp
public interface ICompensatableTransaction
{
    Task<Result> ExecuteAsync();
    Task<Result> CompensateAsync();
}

public class PaymentTransaction : ICompensatableTransaction
{
    public async Task<Result> ExecuteAsync()
    {
        return await ProcessPaymentAsync();
    }

    public async Task<Result> CompensateAsync()
    {
        return await RefundPaymentAsync();
    }
}
```

### Idempotency
Critical: Compensating transactions must be idempotent
- Safe to execute multiple times
- Should return same result
- Prevents duplicate refunds, double releases, etc.

```csharp
public class IdempotentTransaction : ICompensatableTransaction
{
    private Dictionary<string, Result> _cache = new();

    public async Task<Result> ExecuteAsync()
    {
        string key = "execute";
        if (_cache.ContainsKey(key))
            return _cache[key];

        var result = await _transaction.ExecuteAsync();
        _cache[key] = result;
        return result;
    }
}
```

## Real-World Example: E-Commerce Order Processing

### Happy Path
```
Customer Places Order
       ↓
Order Service Creates Order
       ↓
Inventory Service Reserves Items ✓
       ↓
Payment Service Charges Card ✓
       ↓
Shipping Service Creates Shipment ✓
       ↓
Notification Service Sends Confirmation ✓
       ↓
Order Complete ✓
```

### Failure Path (Payment Fails)
```
Order Service Creates Order
       ↓
Inventory Service Reserves Items ✓
       ↓
Payment Service Charges Card ✗
       ↓
Compensation: Inventory Service Releases Items
       ↓
Compensation: Notification Service Sends Apology
       ↓
Order Failed ✗
```

## Implementation Patterns

### 1. Request/Reply with Compensation
```csharp
// Orchestrator
var payment = await paymentService.ProcessAsync(amount);
if (payment.Success)
{
    var shipping = await shippingService.CreateAsync(orderId);
    if (!shipping.Success)
        await paymentService.RefundAsync(payment.Id);
}
```

### 2. Event-Based with Event Sourcing
```csharp
// Store events instead of state
eventStore.Add(new OrderCreatedEvent { ... });
eventStore.Add(new PaymentCompletedEvent { ... });
eventStore.Add(new InventoryReservedEvent { ... });

// Replay events for state reconstruction
var order = eventStore.GetAggregateState<Order>("ORDER123");
```

### 3. Mixed: Choreography with Orchestrator
```csharp
// Orchestrator still exists but is lighter
// Services publish events (choreography)
// Orchestrator publishes "next step" events
```

## Common Pitfalls

❌ **Not Making Transactions Idempotent**
- Refund twice
- Double charge
- Duplicate shipments

✅ **Solution:** Make all operations idempotent

❌ **Ignoring Event Ordering**
- Compensation before completion
- Out-of-order events causing wrong state

✅ **Solution:** Use event versioning and ordering guarantees

❌ **Missing Timeout Handling**
- Service doesn't respond
- Saga hangs indefinitely

✅ **Solution:** Implement timeouts and circuit breakers

❌ **No Saga History**
- Cannot debug failures
- Cannot audit transactions

✅ **Solution:** Log all saga state changes

## Tools and Frameworks

- **Temporal** - Microservices orchestration
- **Cadence** - Fault-tolerant workflow
- **NServiceBus** - .NET messaging with sagas
- **MassTransit** - .NET distributed application framework
- **Axon Framework** - Java event-driven
- **Kairos** - SAGA support for microservices

## Interview Questions

**Q: What is the SAGA pattern?**
A: A pattern for managing distributed transactions across microservices without relying on two-phase commit.

**Q: When would you use orchestration vs choreography?**
A: Orchestration for simple, sequential flows with clear logic. Choreography for autonomous, loosely-coupled services.

**Q: How do you handle failures in a SAGA?**
A: Through compensating transactions that undo previous steps in reverse order.

**Q: What's the difference between SAGA and distributed transactions?**
A: SAGA uses application-level coordination and eventual consistency. Distributed transactions use database-level ACID.

**Q: What must compensating transactions be?**
A: Idempotent - safe to execute multiple times with same result.

## Metrics and Observability

Key metrics to monitor:
- **Success rate** - % of sagas completing successfully
- **Failure rate** - % of sagas requiring compensation
- **Duration** - How long sagas take
- **Compensation count** - How often each compensation is triggered
- **Deadletter rate** - Sagas that get stuck

## Best Practices

✅ **Make transactions idempotent**  
✅ **Implement comprehensive logging**  
✅ **Use correlation IDs for tracing**  
✅ **Set appropriate timeouts**  
✅ **Plan compensations upfront**  
✅ **Test failure scenarios**  
✅ **Monitor saga metrics**  
✅ **Document the flow clearly**  
✅ **Consider eventual consistency**  
✅ **Handle partial failures gracefully**  

## Related Patterns

- **Event Sourcing** - Store state as events
- **CQRS** - Separate read and write models
- **Circuit Breaker** - Prevent cascading failures
- **Compensating Transaction** - Undo operations
- **Idempotent Receiver** - Handle duplicate messages
- **Dead Letter Queue** - Handle failed messages
- **Outbox Pattern** - Guarantee message delivery

## References

- "Building Microservices" - Sam Newman
- "Microservices Patterns" - Chris Richardson
- "Database Reliability Engineering" - O'Reilly
- Temporal Documentation
- NServiceBus Saga Documentation

---

## Summary

The SAGA pattern is essential for building reliable microservices. Choose orchestration for simple workflows with clear business logic, and choreography for distributed systems requiring loose coupling and high availability. Always implement compensating transactions as idempotent operations and monitor saga execution for reliability and performance.

**Key Takeaway:** SAGA enables distributed data consistency without sacrificing microservices autonomy.
