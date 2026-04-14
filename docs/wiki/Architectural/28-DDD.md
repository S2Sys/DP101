# Domain-Driven Design (DDD)

## Overview

**Category:** Architectural Pattern & Methodology  
**Purpose:** Design software around business domain, using shared language between developers and domain experts.  
**Complexity:** Very High  
**Key Concept:** Domain Model - central to design, containing business rules

## Core Concepts

### 1. Ubiquitous Language
Shared language between developers and domain experts:

```csharp
// Instead of generic "User" class, use domain language
public class Customer  // Domain term
{
    public CustomerId Id { get; set; }
    public CustomerName Name { get; set; }
    public Email Email { get; set; }
    
    // Domain logic
    public void PlaceOrder(Order order)
    {
        if (order.TotalAmount > this.CreditLimit)
            throw new OrderExceedsCreditLimitException();
    }
}

// Value Objects - domain concepts with identity
public class Money
{
    public decimal Amount { get; }
    public Currency Currency { get; }

    public Money Add(Money other)
    {
        if (this.Currency != other.Currency)
            throw new InvalidOperationException("Currencies must match");
        return new Money(this.Amount + other.Amount, this.Currency);
    }
}
```

### 2. Aggregate & Aggregate Root

```csharp
// Aggregate Root - controls access to aggregate
public class Order  // Aggregate Root
{
    public OrderId Id { get; private set; }
    public CustomerId CustomerId { get; private set; }
    private List<OrderLine> _lines = new();
    public IReadOnlyList<OrderLine> Lines => _lines.AsReadOnly();
    public OrderStatus Status { get; private set; }

    // Only access order state through aggregate root
    public void AddLine(Product product, Quantity quantity)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Cannot modify closed order");

        _lines.Add(new OrderLine(product, quantity));
    }

    public void Confirm()
    {
        if (_lines.Count == 0)
            throw new InvalidOperationException("Order must have items");

        Status = OrderStatus.Confirmed;
        RaiseDomainEvent(new OrderConfirmedEvent(this.Id));
    }
}

// Entity - has identity but part of aggregate
public class OrderLine  // Entity (not aggregate root)
{
    public OrderLineId Id { get; set; }
    public Product Product { get; set; }
    public Quantity Quantity { get; set; }

    public OrderLine(Product product, Quantity quantity)
    {
        Product = product;
        Quantity = quantity;
    }
}

// Value Object - no identity, immutable
public class Quantity
{
    public int Value { get; }

    public Quantity(int value)
    {
        if (value <= 0)
            throw new ArgumentException("Quantity must be positive");
        Value = value;
    }

    public override bool Equals(object obj) =>
        obj is Quantity qty && qty.Value == Value;

    public override int GetHashCode() => Value.GetHashCode();
}
```

### 3. Repositories

```csharp
// Repository - collection-like interface for aggregate root
public interface IOrderRepository
{
    void Save(Order order);
    Order GetById(OrderId id);
    void Delete(OrderId id);
}

public class EfOrderRepository : IOrderRepository
{
    private readonly DbContext _context;

    public void Save(Order order)
    {
        _context.Orders.Add(order);
        _context.SaveChanges();
    }

    public Order GetById(OrderId id)
    {
        return _context.Orders
            .Include(o => o.Lines)
            .FirstOrDefault(o => o.Id == id);
    }

    public void Delete(OrderId id)
    {
        var order = GetById(id);
        if (order != null)
        {
            _context.Orders.Remove(order);
            _context.SaveChanges();
        }
    }
}
```

### 4. Domain Services

```csharp
// Domain Service - business logic that doesn't belong in entities
public class PaymentService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IPaymentGateway _paymentGateway;

    public void ProcessOrderPayment(OrderId orderId, Money amount)
    {
        var order = _orderRepository.GetById(orderId);
        
        if (!_paymentGateway.Charge(amount))
            throw new PaymentFailedException();

        order.MarkAsPaid();
        _orderRepository.Save(order);
    }
}
```

### 5. Domain Events

```csharp
// Domain Event - something that happened in the domain
public abstract class DomainEvent
{
    public DateTime OccurredOn { get; }
    
    protected DomainEvent()
    {
        OccurredOn = DateTime.Now;
    }
}

public class OrderConfirmedEvent : DomainEvent
{
    public OrderId OrderId { get; }
    public DateTime ConfirmedAt { get; }

    public OrderConfirmedEvent(OrderId orderId)
    {
        OrderId = orderId;
        ConfirmedAt = DateTime.Now;
    }
}

// Event Handler
public class OrderConfirmedEventHandler
{
    private readonly INotificationService _notifications;

    public void Handle(OrderConfirmedEvent @event)
    {
        _notifications.NotifyCustomer(@event.OrderId, "Order confirmed!");
    }
}

// Entity raises event
public class Order
{
    private List<DomainEvent> _events = new();

    public void Confirm()
    {
        Status = OrderStatus.Confirmed;
        _events.Add(new OrderConfirmedEvent(this.Id));
    }

    public IReadOnlyList<DomainEvent> GetEvents() => _events.AsReadOnly();
}
```

### 6. Bounded Contexts

```
┌─────────────────────────────────────────┐
│      Ordering Bounded Context           │
│  ┌─────────────────────────────────────┐│
│  │ Order, Customer, OrderRepository   ││
│  └─────────────────────────────────────┘│
└─────────────────────────────────────────┘
            │
            │ (Anti-Corruption Layer)
            │
┌─────────────────────────────────────────┐
│      Shipping Bounded Context           │
│  ┌─────────────────────────────────────┐│
│  │ Shipment, Package, Tracking        ││
│  └─────────────────────────────────────┘│
└─────────────────────────────────────────┘

// Anti-Corruption Layer - translate between contexts
public class OrderToShipmentTranslator
{
    public ShipmentRequest TranslateOrder(Order order)
    {
        return new ShipmentRequest
        {
            TrackingId = Guid.NewGuid(),
            Items = order.Lines.Select(l => 
                new ShipmentItem(l.Product.Sku, l.Quantity.Value)
            ).ToList()
        };
    }
}
```

## DDD Architecture Layers

```
┌──────────────────────────────────────┐
│  Presentation Layer (UI/API)         │
├──────────────────────────────────────┤
│  Application Layer (Use Cases)       │
│  - OrderService                      │
│  - CustomerService                   │
├──────────────────────────────────────┤
│  Domain Layer (Business Rules) ★★★  │
│  - Order Aggregate                   │
│  - Customer Entity                   │
│  - Money Value Object                │
│  - Domain Events                     │
├──────────────────────────────────────┤
│  Infrastructure Layer (Persistence)  │
│  - EfOrderRepository                 │
│  - Database Access                   │
└──────────────────────────────────────┘
```

## Example: Order Management Domain

```csharp
// DOMAIN LAYER - Pure business logic
public class Order
{
    public OrderId Id { get; private set; }
    public CustomerId CustomerId { get; private set; }
    private List<OrderLine> _lines = new();
    public Money TotalAmount { get; private set; }
    public OrderStatus Status { get; private set; }
    private List<DomainEvent> _events = new();

    public Order(OrderId id, CustomerId customerId)
    {
        Id = id;
        CustomerId = customerId;
        Status = OrderStatus.Draft;
        TotalAmount = new Money(0, Currency.USD);
    }

    public void AddItem(Product product, Quantity quantity)
    {
        if (Status != OrderStatus.Draft)
            throw new InvalidOperationException("Cannot modify non-draft orders");

        var line = new OrderLine(product, quantity);
        _lines.Add(line);
        RecalculateTotal();
    }

    public void Confirm()
    {
        if (_lines.Count == 0)
            throw new InvalidOperationException("Order must have items");

        Status = OrderStatus.Confirmed;
        _events.Add(new OrderConfirmedEvent(this.Id, this.TotalAmount));
    }

    private void RecalculateTotal()
    {
        TotalAmount = _lines.Aggregate(
            new Money(0, Currency.USD),
            (acc, line) => acc.Add(line.LineTotal)
        );
    }

    public IReadOnlyList<DomainEvent> GetUncommittedEvents() => _events.AsReadOnly();
    public void MarkEventsAsCommitted() => _events.Clear();
}

// APPLICATION LAYER - Use cases
public class CreateOrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;

    public OrderId CreateOrder(CreateOrderRequest request)
    {
        var order = new Order(
            OrderId.Create(),
            request.CustomerId
        );

        foreach (var item in request.Items)
        {
            var product = _productRepository.GetById(item.ProductId);
            order.AddItem(product, item.Quantity);
        }

        order.Confirm();
        _orderRepository.Save(order);

        return order.Id;
    }
}

// INFRASTRUCTURE LAYER - Persistence
public class EfOrderRepository : IOrderRepository
{
    private readonly DbContext _context;
    private readonly IEventPublisher _eventPublisher;

    public void Save(Order order)
    {
        _context.Orders.Add(order);
        _context.SaveChanges();

        // Publish events
        foreach (var @event in order.GetUncommittedEvents())
        {
            _eventPublisher.Publish(@event);
        }

        order.MarkEventsAsCommitted();
    }
}
```

## When to Use DDD

### ✅ Use When:
- Complex business domain
- Long-lived applications
- Requirements frequently change
- Business logic is core to value
- Team includes domain experts
- Multi-team projects with bounded contexts

### ❌ Don't Use When:
- Simple CRUD applications
- Rapid prototyping
- Small applications
- Business logic minimal
- Framework/database changes frequent

## Summary

DDD focuses on modeling the business domain accurately, using shared language and clear separation of domain, application, and infrastructure concerns. Perfect for complex enterprise applications where business rules are complex and evolve frequently.

**Key Takeaway:** DDD puts the domain model at the center, using ubiquitous language and aggregates to model business logic.
