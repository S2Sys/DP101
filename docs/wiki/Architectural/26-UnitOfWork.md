# Unit of Work Pattern

## Overview

**Category:** Architectural Pattern  
**Purpose:** Maintain a list of objects affected by a business transaction and coordinate writing changes and resolving concurrency problems.  
**Complexity:** Medium  
**Typical Use:** Database transactions, coordinating multiple repositories

## Problem

Multiple repositories make coordinating transactions difficult:

```csharp
// BAD: Uncoordinated transactions
public class OrderService
{
    private readonly IOrderRepository _orderRepo;
    private readonly IPaymentRepository _paymentRepo;
    private readonly IInventoryRepository _inventoryRepo;

    public void PlaceOrder(Order order, Payment payment, List<InventoryItem> items)
    {
        try
        {
            _orderRepo.Add(order);      // First transaction
            _orderRepo.SaveChanges();
            
            _paymentRepo.Add(payment);  // Second transaction - if fails, order still exists!
            _paymentRepo.SaveChanges();
            
            foreach (var item in items)
            {
                _inventoryRepo.Update(item);  // Third transaction
            }
            _inventoryRepo.SaveChanges();
        }
        catch
        {
            // Inconsistent state - some changes saved, some not!
        }
    }
}
```

## Solution

Coordinate all repositories in single transaction:

```csharp
// Unit of Work interface
public interface IUnitOfWork : IDisposable
{
    IOrderRepository Orders { get; }
    IPaymentRepository Payments { get; }
    IInventoryRepository Inventory { get; }
    
    void SaveChanges();
    void Rollback();
}

// Service using Unit of Work
public class OrderService
{
    private readonly IUnitOfWork _unitOfWork;

    public void PlaceOrder(Order order, Payment payment, List<InventoryItem> items)
    {
        try
        {
            _unitOfWork.Orders.Add(order);
            _unitOfWork.Payments.Add(payment);
            
            foreach (var item in items)
                _unitOfWork.Inventory.Update(item);
            
            _unitOfWork.SaveChanges();  // Single transaction!
        }
        catch
        {
            _unitOfWork.Rollback();  // All rolled back
        }
    }
}
```

## Implementation Approaches

### 1. EF Core Unit of Work

```csharp
// Repository interface
public interface IRepository<T> where T : class
{
    T GetById(int id);
    IQueryable<T> GetAll();
    void Add(T entity);
    void Update(T entity);
    void Delete(T entity);
}

// Generic EF repository
public class EfRepository<T> : IRepository<T> where T : class
{
    protected readonly DbContext _context;
    protected readonly DbSet<T> _dbSet;

    public EfRepository(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public T GetById(int id) => _dbSet.Find(id);
    public IQueryable<T> GetAll() => _dbSet.AsQueryable();
    public void Add(T entity) => _dbSet.Add(entity);
    public void Update(T entity) => _dbSet.Update(entity);
    public void Delete(T entity) => _dbSet.Remove(entity);
}

// Domain repositories
public interface IOrderRepository : IRepository<Order>
{
    List<Order> GetByCustomer(int customerId);
}

public class EfOrderRepository : EfRepository<Order>, IOrderRepository
{
    public EfOrderRepository(DbContext context) : base(context) { }

    public List<Order> GetByCustomer(int customerId)
    {
        return _dbSet.Where(o => o.CustomerId == customerId).ToList();
    }
}

public interface IPaymentRepository : IRepository<Payment>
{
    List<Payment> GetByOrder(int orderId);
}

public class EfPaymentRepository : EfRepository<Payment>, IPaymentRepository
{
    public EfPaymentRepository(DbContext context) : base(context) { }

    public List<Payment> GetByOrder(int orderId)
    {
        return _dbSet.Where(p => p.OrderId == orderId).ToList();
    }
}

// Unit of Work
public interface IUnitOfWork : IDisposable
{
    IOrderRepository Orders { get; }
    IPaymentRepository Payments { get; }
    void SaveChanges();
}

public class EfUnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IOrderRepository _orderRepository;
    private IPaymentRepository _paymentRepository;

    public EfUnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public IOrderRepository Orders =>
        _orderRepository ??= new EfOrderRepository(_context);

    public IPaymentRepository Payments =>
        _paymentRepository ??= new EfPaymentRepository(_context);

    public void SaveChanges()
    {
        using (var transaction = _context.Database.BeginTransaction())
        {
            try
            {
                _context.SaveChanges();
                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}

// Service using Unit of Work
public class OrderService
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public void PlaceOrder(Order order, Payment payment)
    {
        try
        {
            _unitOfWork.Orders.Add(order);
            _unitOfWork.Payments.Add(payment);
            _unitOfWork.SaveChanges();  // Single transaction
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Order failed: {ex.Message}");
            throw;
        }
    }
}
```

### 2. Dependency Injection Setup

```csharp
// Startup configuration
public void ConfigureServices(IServiceCollection services)
{
    services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer("connection_string")
    );

    services.AddScoped<IUnitOfWork, EfUnitOfWork>();
    services.AddScoped<OrderService>();

    // In ASP.NET, Unit of Work lifetime matches request lifetime
    // Ensures same DbContext instance throughout request
}

// Usage in controller
public class OrderController
{
    private readonly OrderService _orderService;

    public OrderController(OrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpPost]
    public ActionResult PlaceOrder(CreateOrderRequest request)
    {
        var order = new Order { CustomerId = request.CustomerId };
        var payment = new Payment { Amount = request.TotalAmount };

        try
        {
            _orderService.PlaceOrder(order, payment);
            return Ok("Order placed successfully");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
```

### 3. Advanced: Change Tracking

```csharp
// Unit of Work with change tracking
public interface IUnitOfWork : IDisposable
{
    IOrderRepository Orders { get; }
    IPaymentRepository Payments { get; }

    void RegisterNew<T>(T entity);
    void RegisterDirty<T>(T entity);
    void RegisterClean<T>(T entity);
    void RegisterRemoved<T>(T entity);

    void SaveChanges();
    void Rollback();
}

public class ChangeTrackingUnitOfWork : IUnitOfWork
{
    private readonly DbContext _context;
    private List<object> _newObjects = new();
    private List<object> _dirtyObjects = new();
    private List<object> _removedObjects = new();

    public void RegisterNew<T>(T entity)
    {
        _newObjects.Add(entity);
    }

    public void RegisterDirty<T>(T entity)
    {
        _dirtyObjects.Add(entity);
    }

    public void RegisterRemoved<T>(T entity)
    {
        _removedObjects.Add(entity);
    }

    public void SaveChanges()
    {
        using (var transaction = _context.Database.BeginTransaction())
        {
            try
            {
                foreach (var obj in _newObjects)
                    _context.Add(obj);

                foreach (var obj in _dirtyObjects)
                    _context.Update(obj);

                foreach (var obj in _removedObjects)
                    _context.Remove(obj);

                _context.SaveChanges();
                transaction.Commit();

                _newObjects.Clear();
                _dirtyObjects.Clear();
                _removedObjects.Clear();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }

    public void Rollback()
    {
        _newObjects.Clear();
        _dirtyObjects.Clear();
        _removedObjects.Clear();
    }

    public void Dispose() => _context?.Dispose();
}
```

### 4. Testing Unit of Work

```csharp
// Mock Unit of Work for testing
public class MockUnitOfWork : IUnitOfWork
{
    public IOrderRepository Orders { get; }
    public IPaymentRepository Payments { get; }

    private bool _committed = false;

    public MockUnitOfWork()
    {
        Orders = new MockOrderRepository();
        Payments = new MockPaymentRepository();
    }

    public void SaveChanges()
    {
        _committed = true;
    }

    public bool WasCommitted => _committed;

    public void Dispose() { }
}

// Unit test
[TestFixture]
public class OrderServiceTests
{
    private OrderService _service;
    private IUnitOfWork _unitOfWork;

    [SetUp]
    public void Setup()
    {
        _unitOfWork = new MockUnitOfWork();
        _service = new OrderService(_unitOfWork);
    }

    [Test]
    public void PlaceOrder_ValidData_CommitsTransaction()
    {
        // Arrange
        var order = new Order { CustomerId = 1 };
        var payment = new Payment { Amount = 100m };

        // Act
        _service.PlaceOrder(order, payment);

        // Assert
        Assert.IsTrue(((MockUnitOfWork)_unitOfWork).WasCommitted);
    }
}
```

---

## Unit of Work Diagram

```
┌──────────────────────────────────────────────┐
│            Business Service                   │
│        (OrderService, UserService)            │
└────────────────────┬─────────────────────────┘
                     │
┌────────────────────▼─────────────────────────┐
│         UNIT OF WORK                          │
│  ┌────────────────────────────────────────┐  │
│  │ OrderRepository                        │  │
│  │ PaymentRepository                      │  │
│  │ InventoryRepository                    │  │
│  └────────────────────────────────────────┘  │
│                                              │
│  SaveChanges() { Begin Transaction...}      │
│  Rollback() { Rollback Transaction...}      │
└────────────────────┬─────────────────────────┘
                     │
┌────────────────────▼─────────────────────────┐
│        DATABASE (Single Transaction)         │
│    All changes commit together or rollback   │
└──────────────────────────────────────────────┘
```

---

## Pros and Cons

### Advantages
✅ **Atomicity** - All changes committed together  
✅ **Consistency** - Prevents partial updates  
✅ **Testability** - Mock Unit of Work for testing  
✅ **Centralized** - Single place for transaction logic  
✅ **Easy Rollback** - Automatic on exception  

### Disadvantages
❌ **Complexity** - Extra layer of abstraction  
❌ **Overhead** - Performance cost for coordination  
❌ **DbContext Already Does This** - EF Core DbContext is already UoW  
❌ **Over-Engineering** - Overkill for simple apps  

---

## When to Use Unit of Work

### ✅ Use When:
- Multiple repositories in single transaction
- Need transaction coordination
- Complex multi-step operations
- Consistency critical

### ❌ Don't Use When:
- Using EF DbContext (already UoW)
- Single repository operations
- Simple CRUD apps
- DbContext sufficient

---

## Real-World Example

```csharp
public class BankTransferService
{
    private readonly IUnitOfWork _unitOfWork;

    public void TransferMoney(int fromAccountId, int toAccountId, decimal amount)
    {
        try
        {
            var fromAccount = _unitOfWork.Accounts.GetById(fromAccountId);
            var toAccount = _unitOfWork.Accounts.GetById(toAccountId);

            if (fromAccount.Balance < amount)
                throw new InvalidOperationException("Insufficient funds");

            fromAccount.Balance -= amount;
            toAccount.Balance += amount;

            _unitOfWork.Accounts.Update(fromAccount);
            _unitOfWork.Accounts.Update(toAccount);

            var transaction = new Transaction
            {
                FromAccountId = fromAccountId,
                ToAccountId = toAccountId,
                Amount = amount,
                Date = DateTime.Now
            };

            _unitOfWork.Transactions.Add(transaction);

            _unitOfWork.SaveChanges();  // All-or-nothing!
        }
        catch
        {
            _unitOfWork.Rollback();
            throw;
        }
    }
}
```

---

## Summary

Unit of Work coordinates multiple repositories in single transaction, ensuring consistency. Perfect for complex operations spanning multiple entities. Works with Repository pattern. Modern ORMs like EF Core DbContext already implement this pattern.

**Key Takeaway:** Unit of Work ensures all repository changes commit together or rollback together.
