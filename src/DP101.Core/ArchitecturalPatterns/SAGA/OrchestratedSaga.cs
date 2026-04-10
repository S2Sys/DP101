namespace DP101.Core.ArchitecturalPatterns.SAGA;

/// <summary>
/// ORCHESTRATED SAGA PATTERN
///
/// Intent: Manage a sequence of distributed transactions across multiple services
/// using a central orchestrator that choreographs the workflow.
///
/// Key Characteristics:
/// - Central coordinator/orchestrator
/// - Orchestrator knows all steps and order
/// - Synchronous communication possible
/// - Easier to understand flow
/// - Orchestrator as single point of failure
///
/// PROS:
/// - Explicit workflow visibility
/// - Easier to understand and debug
/// - Centralized decision logic
/// - Better error handling control
/// - Clear responsibility boundaries
///
/// CONS:
/// - Single orchestrator point of failure
/// - Can become complex and monolithic
/// - Tightly couples orchestrator to services
/// - Limited service autonomy
/// - Network overhead
///
/// USE CASES:
/// - E-commerce order processing
/// - Travel booking
/// - Payment processing with multiple steps
/// - Complex business transactions
/// - Workflows with clear sequence
/// </summary>

#region Data Models

public class Order
{
    public string OrderId { get; set; }
    public List<OrderItem> Items { get; set; }
    public decimal TotalAmount { get; set; }
    public string Status { get; set; }
    public string CustomerId { get; set; }

    public Order(string orderId, string customerId, decimal totalAmount)
    {
        OrderId = orderId;
        CustomerId = customerId;
        TotalAmount = totalAmount;
        Items = new List<OrderItem>();
        Status = "Created";
    }
}

public class OrderItem
{
    public string ItemId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }

    public OrderItem(string itemId, int quantity, decimal price)
    {
        ItemId = itemId;
        Quantity = quantity;
        Price = price;
    }
}

public class Payment
{
    public string PaymentId { get; set; }
    public string OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; }

    public Payment(string paymentId, string orderId, decimal amount)
    {
        PaymentId = paymentId;
        OrderId = orderId;
        Amount = amount;
        Status = "Pending";
    }
}

public class Inventory
{
    public string ItemId { get; set; }
    public int Quantity { get; set; }

    public Inventory(string itemId, int quantity)
    {
        ItemId = itemId;
        Quantity = quantity;
    }
}

public class Shipment
{
    public string ShipmentId { get; set; }
    public string OrderId { get; set; }
    public string Status { get; set; }

    public Shipment(string shipmentId, string orderId)
    {
        ShipmentId = shipmentId;
        OrderId = orderId;
        Status = "Pending";
    }
}

#endregion

#region Service Interfaces

public interface IPaymentService
{
    PaymentResult ProcessPayment(string orderId, decimal amount);
    PaymentResult RefundPayment(string paymentId);
}

public interface IInventoryService
{
    InventoryResult ReserveItems(string orderId, List<OrderItem> items);
    InventoryResult ReleaseReservation(string orderId);
}

public interface IShippingService
{
    ShippingResult CreateShipment(string orderId);
    ShippingResult CancelShipment(string shipmentId);
}

#endregion

#region Service Results

public class PaymentResult
{
    public bool Success { get; set; }
    public string PaymentId { get; set; }
    public string Message { get; set; }

    public PaymentResult(bool success, string paymentId, string message)
    {
        Success = success;
        PaymentId = paymentId;
        Message = message;
    }
}

public class InventoryResult
{
    public bool Success { get; set; }
    public string ReservationId { get; set; }
    public string Message { get; set; }

    public InventoryResult(bool success, string reservationId, string message)
    {
        Success = success;
        ReservationId = reservationId;
        Message = message;
    }
}

public class ShippingResult
{
    public bool Success { get; set; }
    public string ShipmentId { get; set; }
    public string Message { get; set; }

    public ShippingResult(bool success, string shipmentId, string message)
    {
        Success = success;
        ShipmentId = shipmentId;
        Message = message;
    }
}

#endregion

#region Concrete Services

public class PaymentServiceImpl : IPaymentService
{
    public PaymentResult ProcessPayment(string orderId, decimal amount)
    {
        Console.WriteLine($"[Payment Service] Processing payment for order {orderId}: ${amount}");
        var paymentId = Guid.NewGuid().ToString();
        // Simulate payment processing
        System.Threading.Thread.Sleep(200);
        Console.WriteLine($"[Payment Service] Payment {paymentId} processed successfully");
        return new PaymentResult(true, paymentId, "Payment processed");
    }

    public PaymentResult RefundPayment(string paymentId)
    {
        Console.WriteLine($"[Payment Service] Refunding payment {paymentId}");
        System.Threading.Thread.Sleep(100);
        return new PaymentResult(true, paymentId, "Payment refunded");
    }
}

public class InventoryServiceImpl : IInventoryService
{
    private Dictionary<string, int> _stock = new()
    {
        { "ITEM001", 100 },
        { "ITEM002", 50 },
        { "ITEM003", 200 }
    };

    public InventoryResult ReserveItems(string orderId, List<OrderItem> items)
    {
        Console.WriteLine($"[Inventory Service] Reserving items for order {orderId}");

        foreach (var item in items)
        {
            if (!_stock.ContainsKey(item.ItemId) || _stock[item.ItemId] < item.Quantity)
            {
                Console.WriteLine($"[Inventory Service] Insufficient stock for {item.ItemId}");
                return new InventoryResult(false, "", "Insufficient stock");
            }

            _stock[item.ItemId] -= item.Quantity;
        }

        var reservationId = Guid.NewGuid().ToString();
        Console.WriteLine($"[Inventory Service] Reservation {reservationId} created");
        return new InventoryResult(true, reservationId, "Items reserved");
    }

    public InventoryResult ReleaseReservation(string orderId)
    {
        Console.WriteLine($"[Inventory Service] Releasing reservation for order {orderId}");
        return new InventoryResult(true, "", "Reservation released");
    }
}

public class ShippingServiceImpl : IShippingService
{
    public ShippingResult CreateShipment(string orderId)
    {
        Console.WriteLine($"[Shipping Service] Creating shipment for order {orderId}");
        var shipmentId = Guid.NewGuid().ToString();
        System.Threading.Thread.Sleep(150);
        Console.WriteLine($"[Shipping Service] Shipment {shipmentId} created");
        return new ShippingResult(true, shipmentId, "Shipment created");
    }

    public ShippingResult CancelShipment(string shipmentId)
    {
        Console.WriteLine($"[Shipping Service] Canceling shipment {shipmentId}");
        return new ShippingResult(true, shipmentId, "Shipment cancelled");
    }
}

#endregion

#region Orchestrator

public class OrderSagaOrchestrator
{
    private readonly IPaymentService _paymentService;
    private readonly IInventoryService _inventoryService;
    private readonly IShippingService _shippingService;

    public OrderSagaOrchestrator(
        IPaymentService paymentService,
        IInventoryService inventoryService,
        IShippingService shippingService)
    {
        _paymentService = paymentService;
        _inventoryService = inventoryService;
        _shippingService = shippingService;
    }

    public bool ProcessOrder(Order order)
    {
        Console.WriteLine($"\n=== Starting Order Saga for {order.OrderId} ===");

        try
        {
            // Step 1: Reserve inventory
            var inventoryResult = _inventoryService.ReserveItems(order.OrderId, order.Items);
            if (!inventoryResult.Success)
            {
                Console.WriteLine($"[Orchestrator] ❌ Inventory reservation failed");
                return false;
            }

            // Step 2: Process payment
            var paymentResult = _paymentService.ProcessPayment(order.OrderId, order.TotalAmount);
            if (!paymentResult.Success)
            {
                Console.WriteLine($"[Orchestrator] ❌ Payment failed, compensating inventory");
                _inventoryService.ReleaseReservation(order.OrderId);
                return false;
            }

            // Step 3: Create shipment
            var shippingResult = _shippingService.CreateShipment(order.OrderId);
            if (!shippingResult.Success)
            {
                Console.WriteLine($"[Orchestrator] ❌ Shipment creation failed, compensating");
                _paymentService.RefundPayment(paymentResult.PaymentId);
                _inventoryService.ReleaseReservation(order.OrderId);
                return false;
            }

            Console.WriteLine($"[Orchestrator] ✅ Order {order.OrderId} completed successfully!");
            order.Status = "Completed";
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Orchestrator] ❌ Unexpected error: {ex.Message}");
            return false;
        }
    }
}

#endregion

#region Example Usage

public class OrchestratedSagaExample
{
    public static void Run()
    {
        var paymentService = new PaymentServiceImpl();
        var inventoryService = new InventoryServiceImpl();
        var shippingService = new ShippingServiceImpl();

        var orchestrator = new OrderSagaOrchestrator(
            paymentService,
            inventoryService,
            shippingService);

        // Successful order
        var order1 = new Order(
            "ORD001",
            "CUST001",
            150.00m);

        order1.Items.Add(new OrderItem("ITEM001", 2, 50.00m));
        order1.Items.Add(new OrderItem("ITEM002", 1, 50.00m));

        orchestrator.ProcessOrder(order1);

        // Order that will fail at payment stage (simulate)
        Console.WriteLine("\n" + new string('=', 50));
        var order2 = new Order(
            "ORD002",
            "CUST002",
            500.00m);

        order2.Items.Add(new OrderItem("ITEM001", 5, 100.00m));
        orchestrator.ProcessOrder(order2);
    }
}

#endregion
