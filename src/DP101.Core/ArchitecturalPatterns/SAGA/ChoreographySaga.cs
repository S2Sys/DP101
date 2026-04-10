namespace DP101.Core.ArchitecturalPatterns.SAGA;

/// <summary>
/// CHOREOGRAPHY-BASED SAGA PATTERN
///
/// Intent: Manage distributed transactions through event-driven choreography
/// where each service listens to events and triggers the next step.
///
/// Key Characteristics:
/// - Event-driven, no central orchestrator
/// - Services react to events
/// - Services publish events
/// - Decentralized decision making
/// - More autonomous services
/// - Complex event flow
///
/// PROS:
/// - No single point of failure
/// - Services more independent
/// - Better scalability
/// - Loose coupling between services
/// - More resilient
///
/// CONS:
/// - Hard to follow the flow
/// - Difficult to debug
/// - Event handling can be complex
/// - Potential for cyclic events
/// - Testing is challenging
/// - Event ordering issues
///
/// USE CASES:
/// - Microservices with loose coupling
/// - Event-driven architectures
/// - Systems with high availability requirements
/// - Decentralized workflows
/// </summary>

#region Events

public abstract class DomainEvent
{
    public string EventId { get; }
    public string OrderId { get; }
    public DateTime Timestamp { get; }

    protected DomainEvent(string orderId)
    {
        EventId = Guid.NewGuid().ToString();
        OrderId = orderId;
        Timestamp = DateTime.Now;
    }
}

public class OrderCreatedEvent : DomainEvent
{
    public string CustomerId { get; }
    public List<OrderItem> Items { get; }
    public decimal TotalAmount { get; }

    public OrderCreatedEvent(string orderId, string customerId, List<OrderItem> items, decimal totalAmount)
        : base(orderId)
    {
        CustomerId = customerId;
        Items = items;
        TotalAmount = totalAmount;
    }
}

public class PaymentStartedEvent : DomainEvent
{
    public decimal Amount { get; }

    public PaymentStartedEvent(string orderId, decimal amount) : base(orderId)
    {
        Amount = amount;
    }
}

public class PaymentCompletedEvent : DomainEvent
{
    public string PaymentId { get; }

    public PaymentCompletedEvent(string orderId, string paymentId) : base(orderId)
    {
        PaymentId = paymentId;
    }
}

public class PaymentFailedEvent : DomainEvent
{
    public string Reason { get; }

    public PaymentFailedEvent(string orderId, string reason) : base(orderId)
    {
        Reason = reason;
    }
}

public class InventoryReservedEvent : DomainEvent
{
    public string ReservationId { get; }

    public InventoryReservedEvent(string orderId, string reservationId) : base(orderId)
    {
        ReservationId = reservationId;
    }
}

public class InventoryReservationFailedEvent : DomainEvent
{
    public string Reason { get; }

    public InventoryReservationFailedEvent(string orderId, string reason) : base(orderId)
    {
        Reason = reason;
    }
}

public class ShipmentCreatedEvent : DomainEvent
{
    public string ShipmentId { get; }

    public ShipmentCreatedEvent(string orderId, string shipmentId) : base(orderId)
    {
        ShipmentId = shipmentId;
    }
}

public class ShipmentFailedEvent : DomainEvent
{
    public string Reason { get; }

    public ShipmentFailedEvent(string orderId, string reason) : base(orderId)
    {
        Reason = reason;
    }
}

public class OrderCompletedEvent : DomainEvent
{
    public OrderCompletedEvent(string orderId) : base(orderId) { }
}

public class OrderFailedEvent : DomainEvent
{
    public string Reason { get; }

    public OrderFailedEvent(string orderId, string reason) : base(orderId)
    {
        Reason = reason;
    }
}

public class CompensationStartedEvent : DomainEvent
{
    public CompensationStartedEvent(string orderId) : base(orderId) { }
}

#endregion

#region Event Bus

public interface IEventBus
{
    void Publish(DomainEvent @event);
    void Subscribe<T>(Action<T> handler) where T : DomainEvent;
}

public class SimpleEventBus : IEventBus
{
    private Dictionary<Type, List<Delegate>> _subscribers = new();

    public void Publish(DomainEvent @event)
    {
        var eventType = @event.GetType();
        if (_subscribers.ContainsKey(eventType))
        {
            foreach (var handler in _subscribers[eventType])
            {
                handler.DynamicInvoke(@event);
            }
        }
    }

    public void Subscribe<T>(Action<T> handler) where T : DomainEvent
    {
        var eventType = typeof(T);
        if (!_subscribers.ContainsKey(eventType))
        {
            _subscribers[eventType] = new List<Delegate>();
        }
        _subscribers[eventType].Add(handler);
    }
}

#endregion

#region Choreography Services

public class PaymentServiceChoreography
{
    private IEventBus _eventBus;

    public PaymentServiceChoreography(IEventBus eventBus)
    {
        _eventBus = eventBus;
        _eventBus.Subscribe<PaymentStartedEvent>(OnPaymentStarted);
        _eventBus.Subscribe<CompensationStartedEvent>(OnCompensationStarted);
    }

    private void OnPaymentStarted(PaymentStartedEvent @event)
    {
        Console.WriteLine($"[Payment Service] Processing payment for order {@event.OrderId}: ${@event.Amount}");

        // Simulate payment processing
        System.Threading.Thread.Sleep(200);

        // In reality, success/failure would depend on business logic
        bool paymentSuccessful = @event.Amount < 10000; // Fail if amount too high

        if (paymentSuccessful)
        {
            var paymentId = Guid.NewGuid().ToString();
            Console.WriteLine($"[Payment Service] ✅ Payment {@event.OrderId} completed");
            _eventBus.Publish(new PaymentCompletedEvent(@event.OrderId, paymentId));
        }
        else
        {
            Console.WriteLine($"[Payment Service] ❌ Payment {@event.OrderId} failed");
            _eventBus.Publish(new PaymentFailedEvent(@event.OrderId, "Insufficient funds"));
        }
    }

    private void OnCompensationStarted(CompensationStartedEvent @event)
    {
        Console.WriteLine($"[Payment Service] Compensating payment for order {@event.OrderId}");
        // Refund would happen here
    }
}

public class InventoryServiceChoreography
{
    private IEventBus _eventBus;
    private Dictionary<string, int> _stock = new()
    {
        { "ITEM001", 100 },
        { "ITEM002", 50 },
        { "ITEM003", 200 }
    };

    public InventoryServiceChoreography(IEventBus eventBus)
    {
        _eventBus = eventBus;
        _eventBus.Subscribe<OrderCreatedEvent>(OnOrderCreated);
        _eventBus.Subscribe<PaymentFailedEvent>(OnPaymentFailed);
        _eventBus.Subscribe<ShipmentFailedEvent>(OnShipmentFailed);
    }

    private void OnOrderCreated(OrderCreatedEvent @event)
    {
        Console.WriteLine($"[Inventory Service] Attempting to reserve items for order {@event.OrderId}");

        bool canReserve = @event.Items.All(item =>
            _stock.ContainsKey(item.ItemId) && _stock[item.ItemId] >= item.Quantity);

        if (canReserve)
        {
            foreach (var item in @event.Items)
            {
                _stock[item.ItemId] -= item.Quantity;
            }

            var reservationId = Guid.NewGuid().ToString();
            Console.WriteLine($"[Inventory Service] ✅ Items reserved for {@event.OrderId}");
            _eventBus.Publish(new InventoryReservedEvent(@event.OrderId, reservationId));
        }
        else
        {
            Console.WriteLine($"[Inventory Service] ❌ Insufficient stock for {@event.OrderId}");
            _eventBus.Publish(new InventoryReservationFailedEvent(@event.OrderId, "Insufficient stock"));
        }
    }

    private void OnPaymentFailed(PaymentFailedEvent @event)
    {
        Console.WriteLine($"[Inventory Service] Releasing reservation for {@event.OrderId}");
        // Release inventory reservation
    }

    private void OnShipmentFailed(ShipmentFailedEvent @event)
    {
        Console.WriteLine($"[Inventory Service] Releasing items for {@event.OrderId}");
        // Release inventory
    }
}

public class ShippingServiceChoreography
{
    private IEventBus _eventBus;

    public ShippingServiceChoreography(IEventBus eventBus)
    {
        _eventBus = eventBus;
        _eventBus.Subscribe<PaymentCompletedEvent>(OnPaymentCompleted);
        _eventBus.Subscribe<PaymentFailedEvent>(OnPaymentFailed);
        _eventBus.Subscribe<InventoryReservationFailedEvent>(OnInventoryFailed);
    }

    private void OnPaymentCompleted(PaymentCompletedEvent @event)
    {
        Console.WriteLine($"[Shipping Service] Creating shipment for order {@event.OrderId}");

        // Simulate shipment creation
        System.Threading.Thread.Sleep(150);

        var shipmentId = Guid.NewGuid().ToString();
        Console.WriteLine($"[Shipping Service] ✅ Shipment {@event.OrderId} created");
        _eventBus.Publish(new ShipmentCreatedEvent(@event.OrderId, shipmentId));
    }

    private void OnPaymentFailed(PaymentFailedEvent @event)
    {
        Console.WriteLine($"[Shipping Service] Payment failed, cannot ship {@event.OrderId}");
    }

    private void OnInventoryFailed(InventoryReservationFailedEvent @event)
    {
        Console.WriteLine($"[Shipping Service] Inventory reservation failed, cannot ship {@event.OrderId}");
    }
}

public class OrderServiceChoreography
{
    private IEventBus _eventBus;

    public OrderServiceChoreography(IEventBus eventBus)
    {
        _eventBus = eventBus;
        _eventBus.Subscribe<InventoryReservedEvent>(OnInventoryReserved);
        _eventBus.Subscribe<PaymentFailedEvent>(OnPaymentFailed);
        _eventBus.Subscribe<InventoryReservationFailedEvent>(OnInventoryFailed);
        _eventBus.Subscribe<ShipmentCreatedEvent>(OnShipmentCreated);
        _eventBus.Subscribe<ShipmentFailedEvent>(OnShipmentFailed);
    }

    public void CreateOrder(Order order)
    {
        Console.WriteLine($"\n=== Creating Order {@order.OrderId} ===");
        _eventBus.Publish(new OrderCreatedEvent(
            order.OrderId,
            order.CustomerId,
            order.Items,
            order.TotalAmount));
    }

    private void OnInventoryReserved(InventoryReservedEvent @event)
    {
        Console.WriteLine($"[Order Service] Inventory reserved, starting payment");
        _eventBus.Publish(new PaymentStartedEvent(@event.OrderId, 0)); // Amount would come from order data
    }

    private void OnPaymentFailed(PaymentFailedEvent @event)
    {
        Console.WriteLine($"[Order Service] ❌ Order {@event.OrderId} failed: {@event.Reason}");
        _eventBus.Publish(new CompensationStartedEvent(@event.OrderId));
        _eventBus.Publish(new OrderFailedEvent(@event.OrderId, @event.Reason));
    }

    private void OnInventoryFailed(InventoryReservationFailedEvent @event)
    {
        Console.WriteLine($"[Order Service] ❌ Order {@event.OrderId} failed: {@event.Reason}");
        _eventBus.Publish(new OrderFailedEvent(@event.OrderId, @event.Reason));
    }

    private void OnShipmentCreated(ShipmentCreatedEvent @event)
    {
        Console.WriteLine($"[Order Service] ✅ Order {@event.OrderId} completed successfully!");
        _eventBus.Publish(new OrderCompletedEvent(@event.OrderId));
    }

    private void OnShipmentFailed(ShipmentFailedEvent @event)
    {
        Console.WriteLine($"[Order Service] ❌ Order {@event.OrderId} failed: {@event.Reason}");
        _eventBus.Publish(new CompensationStartedEvent(@event.OrderId));
        _eventBus.Publish(new OrderFailedEvent(@event.OrderId, @event.Reason));
    }
}

#endregion

#region Example Usage

public class ChoreographySagaExample
{
    public static void Run()
    {
        var eventBus = new SimpleEventBus();

        // Initialize services
        var paymentService = new PaymentServiceChoreography(eventBus);
        var inventoryService = new InventoryServiceChoreography(eventBus);
        var shippingService = new ShippingServiceChoreography(eventBus);
        var orderService = new OrderServiceChoreography(eventBus);

        // Create and process order
        var order = new Order("ORD001", "CUST001", 150.00m);
        order.Items.Add(new OrderItem("ITEM001", 2, 50.00m));
        order.Items.Add(new OrderItem("ITEM002", 1, 50.00m));

        orderService.CreateOrder(order);

        System.Threading.Thread.Sleep(1000); // Allow events to propagate

        // Try another order with high amount that fails
        Console.WriteLine("\n" + new string('=', 50));
        var order2 = new Order("ORD002", "CUST002", 15000.00m);
        order2.Items.Add(new OrderItem("ITEM001", 5, 100.00m));

        orderService.CreateOrder(order2);

        System.Threading.Thread.Sleep(1000);
    }
}

#endregion
