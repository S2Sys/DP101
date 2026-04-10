namespace DP101.Core.ArchitecturalPatterns.SAGA;

/// <summary>
/// COMPENSATING TRANSACTIONS PATTERN
///
/// Intent: Rollback distributed transactions by executing compensating actions
/// for each step that succeeded before a failure occurred.
///
/// Key Characteristics:
/// - Each transaction has a compensating action
/// - Executed in reverse order on failure
/// - Idempotency is critical
/// - May not achieve true consistency (eventual consistency)
/// - Cannot undo all operations (e.g., email sent)
///
/// PROS:
/// - Allows undo of distributed transactions
/// - More realistic than ACID in distributed systems
/// - Can handle long-running transactions
/// - Supports eventual consistency
/// - Works across service boundaries
///
/// CONS:
/// - Some operations cannot be fully undone
/// - Requires careful implementation
/// - Idempotency essential
/// - Compensation logic can be complex
/// - Difficult to test
/// - Business logic becomes complex
///
/// USE CASES:
/// - Multi-service transactions
/// - Long-running business processes
/// - Distributed payments
/// - Booking and reservation systems
/// - Any multi-step workflow with possible failures
/// </summary>

#region Transaction and Compensation

public interface ICompensatableTransaction
{
    Task<TransactionResult> ExecuteAsync();
    Task<TransactionResult> CompensateAsync();
    string GetTransactionName();
}

public class TransactionResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
    public string TransactionId { get; set; }

    public TransactionResult(bool success, string message, string transactionId)
    {
        Success = success;
        Message = message;
        TransactionId = transactionId;
    }
}

#endregion

#region Concrete Compensating Transactions

// Payment Transaction and Compensation
public class PaymentTransaction : ICompensatableTransaction
{
    private string _paymentId = "";
    private decimal _amount;
    private string _orderId;

    public PaymentTransaction(string orderId, decimal amount)
    {
        _orderId = orderId;
        _amount = amount;
    }

    public async Task<TransactionResult> ExecuteAsync()
    {
        Console.WriteLine($"[Payment] Processing ${_amount} for order {_orderId}");
        await Task.Delay(100); // Simulate processing

        // Simulate occasional failures
        if (_amount > 5000)
        {
            return new TransactionResult(false, "Payment amount exceeds limit", "");
        }

        _paymentId = Guid.NewGuid().ToString();
        Console.WriteLine($"[Payment] ✅ Payment {_paymentId} processed");
        return new TransactionResult(true, "Payment processed", _paymentId);
    }

    public async Task<TransactionResult> CompensateAsync()
    {
        if (string.IsNullOrEmpty(_paymentId))
            return new TransactionResult(true, "No payment to refund", "");

        Console.WriteLine($"[Payment] Refunding payment {_paymentId}");
        await Task.Delay(50);
        Console.WriteLine($"[Payment] ✅ Refund completed");
        return new TransactionResult(true, "Payment refunded", _paymentId);
    }

    public string GetTransactionName() => "Payment";
}

// Inventory Reservation and Release
public class InventoryReservationTransaction : ICompensatableTransaction
{
    private string _reservationId = "";
    private string _orderId;
    private List<(string itemId, int quantity)> _items;

    public InventoryReservationTransaction(string orderId, List<(string itemId, int quantity)> items)
    {
        _orderId = orderId;
        _items = items;
    }

    public async Task<TransactionResult> ExecuteAsync()
    {
        Console.WriteLine($"[Inventory] Reserving items for order {_orderId}");
        await Task.Delay(100);

        _reservationId = Guid.NewGuid().ToString();
        Console.WriteLine($"[Inventory] ✅ Reservation {_reservationId} created");
        return new TransactionResult(true, "Items reserved", _reservationId);
    }

    public async Task<TransactionResult> CompensateAsync()
    {
        if (string.IsNullOrEmpty(_reservationId))
            return new TransactionResult(true, "No reservation to release", "");

        Console.WriteLine($"[Inventory] Releasing reservation {_reservationId}");
        await Task.Delay(50);
        Console.WriteLine($"[Inventory] ✅ Items released back to stock");
        return new TransactionResult(true, "Inventory released", _reservationId);
    }

    public string GetTransactionName() => "Inventory";
}

// Shipment Creation and Cancellation
public class ShipmentTransaction : ICompensatableTransaction
{
    private string _shipmentId = "";
    private string _orderId;

    public ShipmentTransaction(string orderId)
    {
        _orderId = orderId;
    }

    public async Task<TransactionResult> ExecuteAsync()
    {
        Console.WriteLine($"[Shipping] Creating shipment for order {_orderId}");
        await Task.Delay(150);

        _shipmentId = Guid.NewGuid().ToString();
        Console.WriteLine($"[Shipping] ✅ Shipment {_shipmentId} created");
        return new TransactionResult(true, "Shipment created", _shipmentId);
    }

    public async Task<TransactionResult> CompensateAsync()
    {
        if (string.IsNullOrEmpty(_shipmentId))
            return new TransactionResult(true, "No shipment to cancel", "");

        Console.WriteLine($"[Shipping] Canceling shipment {_shipmentId}");
        await Task.Delay(50);
        Console.WriteLine($"[Shipping] ✅ Shipment cancelled");
        return new TransactionResult(true, "Shipment cancelled", _shipmentId);
    }

    public string GetTransactionName() => "Shipment";
}

// Email Notification (Cannot be fully undone)
public class NotificationTransaction : ICompensatableTransaction
{
    private string _notificationId = "";
    private string _email;
    private string _message;

    public NotificationTransaction(string email, string message)
    {
        _email = email;
        _message = message;
    }

    public async Task<TransactionResult> ExecuteAsync()
    {
        Console.WriteLine($"[Notification] Sending email to {_email}");
        await Task.Delay(80);

        _notificationId = Guid.NewGuid().ToString();
        Console.WriteLine($"[Notification] ✅ Email sent (ID: {_notificationId})");
        return new TransactionResult(true, "Email sent", _notificationId);
    }

    public async Task<TransactionResult> CompensateAsync()
    {
        // Emails cannot be unsent, but we can log it
        Console.WriteLine($"[Notification] ⚠️  Cannot unsend email {_notificationId}");
        Console.WriteLine($"[Notification] Sending apology email to {_email}");
        await Task.Delay(50);
        return new TransactionResult(true, "Apology email sent", _notificationId);
    }

    public string GetTransactionName() => "Notification";
}

#endregion

#region Saga Coordinator

public class SagaCoordinator
{
    private List<ICompensatableTransaction> _transactions = [];
    private List<TransactionResult> _results = [];

    public void RegisterTransaction(ICompensatableTransaction transaction)
    {
        _transactions.Add(transaction);
    }

    public async Task<bool> ExecuteSagaAsync()
    {
        Console.WriteLine($"\n=== Starting Saga with {_transactions.Count} steps ===\n");

        for (int i = 0; i < _transactions.Count; i++)
        {
            var transaction = _transactions[i];
            Console.WriteLine($"Step {i + 1}/{_transactions.Count}: {transaction.GetTransactionName()}");

            var result = await transaction.ExecuteAsync();
            _results.Add(result);

            if (!result.Success)
            {
                Console.WriteLine($"\n❌ Transaction failed: {result.Message}");
                Console.WriteLine("Starting compensation...\n");
                await CompensateAsync();
                return false;
            }

            Console.WriteLine();
        }

        Console.WriteLine("✅ Saga completed successfully!\n");
        return true;
    }

    private async Task CompensateAsync()
    {
        // Execute compensations in reverse order
        for (int i = _results.Count - 1; i >= 0; i--)
        {
            var transaction = _transactions[i];
            Console.WriteLine($"Compensating: {transaction.GetTransactionName()}");
            await transaction.CompensateAsync();
            Console.WriteLine();
        }
    }

    public void DisplayResults()
    {
        Console.WriteLine("\n=== Transaction Results ===");
        for (int i = 0; i < _results.Count; i++)
        {
            var result = _results[i];
            var status = result.Success ? "✅" : "❌";
            Console.WriteLine($"{status} {_transactions[i].GetTransactionName()}: {result.Message}");
        }
    }
}

#endregion

#region Example Usage

public class CompensatingTransactionsExample
{
    public static async Task RunAsync()
    {
        Console.WriteLine("=== Example 1: Successful Saga ===");
        await RunSuccessfulSagaAsync();

        await Task.Delay(1000);

        Console.WriteLine("\n\n=== Example 2: Failed Saga with Compensation ===");
        await RunFailedSagaAsync();
    }

    private static async Task RunSuccessfulSagaAsync()
    {
        var coordinator = new SagaCoordinator();

        // Register transactions
        coordinator.RegisterTransaction(
            new PaymentTransaction("ORD001", 150.00m));

        coordinator.RegisterTransaction(
            new InventoryReservationTransaction("ORD001",
                new List<(string, int)> { ("ITEM001", 2), ("ITEM002", 1) }));

        coordinator.RegisterTransaction(
            new ShipmentTransaction("ORD001"));

        coordinator.RegisterTransaction(
            new NotificationTransaction("customer@example.com", "Order confirmed"));

        var success = await coordinator.ExecuteSagaAsync();
        coordinator.DisplayResults();
    }

    private static async Task RunFailedSagaAsync()
    {
        var coordinator = new SagaCoordinator();

        // Register transactions - this will fail at payment step
        coordinator.RegisterTransaction(
            new PaymentTransaction("ORD002", 7500.00m)); // This will fail

        coordinator.RegisterTransaction(
            new InventoryReservationTransaction("ORD002",
                new List<(string, int)> { ("ITEM001", 5) }));

        coordinator.RegisterTransaction(
            new ShipmentTransaction("ORD002"));

        coordinator.RegisterTransaction(
            new NotificationTransaction("customer@example.com", "Order confirmed"));

        var success = await coordinator.ExecuteSagaAsync();
        coordinator.DisplayResults();
    }
}

#endregion

#region Idempotency Decorator

/// <summary>
/// Wrapper to make transactions idempotent
/// Idempotent = Safe to execute multiple times with same result
/// </summary>
public class IdempotentTransactionDecorator : ICompensatableTransaction
{
    private ICompensatableTransaction _transaction;
    private Dictionary<string, TransactionResult> _executionCache = new();

    public IdempotentTransactionDecorator(ICompensatableTransaction transaction)
    {
        _transaction = transaction;
    }

    public async Task<TransactionResult> ExecuteAsync()
    {
        string cacheKey = $"execute_{GetTransactionName()}";

        if (_executionCache.ContainsKey(cacheKey))
        {
            Console.WriteLine($"[Idempotent] Using cached result for {GetTransactionName()}");
            return _executionCache[cacheKey];
        }

        var result = await _transaction.ExecuteAsync();
        _executionCache[cacheKey] = result;
        return result;
    }

    public async Task<TransactionResult> CompensateAsync()
    {
        string cacheKey = $"compensate_{GetTransactionName()}";

        if (_executionCache.ContainsKey(cacheKey))
        {
            Console.WriteLine($"[Idempotent] Using cached compensation for {GetTransactionName()}");
            return _executionCache[cacheKey];
        }

        var result = await _transaction.CompensateAsync();
        _executionCache[cacheKey] = result;
        return result;
    }

    public string GetTransactionName() => _transaction.GetTransactionName();
}

#endregion
