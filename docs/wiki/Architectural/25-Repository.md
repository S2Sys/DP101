# Repository Pattern

## Overview

**Category:** Architectural Pattern  
**Purpose:** Mediate between domain and data mapping layers, acting like an in-memory collection of aggregate roots.  
**Complexity:** Medium  
**Use Case:** Data access abstraction, testing, swapping data sources

## Problem

Business logic couples directly to data access code:

```csharp
// BAD: Business logic mixed with data access
public class OrderService
{
    public void ProcessOrder(int orderId)
    {
        // Data access mixed in business logic!
        using (var conn = new SqlConnection("..."))
        {
            var order = conn.QuerySingle<Order>(
                "SELECT * FROM Orders WHERE Id = @id", 
                new { id = orderId }
            );
            
            // Business logic
            order.Status = OrderStatus.Processing;
            
            // More data access
            conn.Execute(
                "UPDATE Orders SET Status = @status WHERE Id = @id",
                new { status = order.Status, id = orderId }
            );
        }
    }
}

// Hard to test, hard to change data source, violates SRP
```

## Solution

Abstract data access through Repository interface:

```csharp
// Repository interface
public interface IOrderRepository
{
    Order GetById(int id);
    void Save(Order order);
    void Delete(int id);
    IQueryable<Order> GetAll();
}

// Business logic - pure, testable
public class OrderService
{
    private readonly IOrderRepository _repository;

    public OrderService(IOrderRepository repository)
    {
        _repository = repository;
    }

    public void ProcessOrder(int orderId)
    {
        var order = _repository.GetById(orderId);
        order.Status = OrderStatus.Processing;
        _repository.Save(order);
    }
}

// Data access implementation
public class SqlOrderRepository : IOrderRepository
{
    private readonly string _connectionString;

    public Order GetById(int id)
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            return conn.QuerySingle<Order>(
                "SELECT * FROM Orders WHERE Id = @id",
                new { id }
            );
        }
    }

    public void Save(Order order)
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Execute(
                "UPDATE Orders SET Status = @status WHERE Id = @id",
                new { status = order.Status, id = order.Id }
            );
        }
    }
}
```

## Implementation Approaches

### 1. Generic Repository Pattern

```csharp
// Generic base repository
public interface IRepository<T> where T : class
{
    T GetById(int id);
    IQueryable<T> GetAll();
    void Add(T entity);
    void Update(T entity);
    void Delete(T entity);
    void SaveChanges();
}

public class GenericRepository<T> : IRepository<T> where T : class
{
    protected readonly DbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(DbContext context)
    {
        _context = context;
        _dbSet = context.Set<T>();
    }

    public T GetById(int id) => _dbSet.Find(id);

    public IQueryable<T> GetAll() => _dbSet.AsQueryable();

    public void Add(T entity)
    {
        _dbSet.Add(entity);
        _context.SaveChanges();
    }

    public void Update(T entity)
    {
        _dbSet.Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
        _context.SaveChanges();
    }

    public void Delete(T entity)
    {
        _dbSet.Remove(entity);
        _context.SaveChanges();
    }

    public void SaveChanges() => _context.SaveChanges();
}

// Specific repository
public interface IProductRepository : IRepository<Product>
{
    List<Product> GetByCategory(string category);
    List<Product> GetLowStock();
}

public class ProductRepository : GenericRepository<Product>, IProductRepository
{
    public ProductRepository(DbContext context) : base(context) { }

    public List<Product> GetByCategory(string category)
    {
        return _dbSet.Where(p => p.Category == category).ToList();
    }

    public List<Product> GetLowStock()
    {
        return _dbSet.Where(p => p.Quantity < 10).ToList();
    }
}
```

### 2. Entity Framework Repository

```csharp
// Domain model
public class Customer
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public List<Order> Orders { get; set; } = new();
}

public class Order
{
    public int Id { get; set; }
    public int CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal TotalAmount { get; set; }
    public Customer Customer { get; set; }
}

// Repository interface
public interface ICustomerRepository
{
    Customer GetById(int id);
    Customer GetByEmail(string email);
    List<Customer> GetAll();
    List<Customer> GetByOrderAmount(decimal minAmount);
    void Add(Customer customer);
    void Update(Customer customer);
    void Delete(int id);
}

// EF Implementation
public class EfCustomerRepository : ICustomerRepository
{
    private readonly ApplicationDbContext _context;

    public EfCustomerRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public Customer GetById(int id)
    {
        return _context.Customers
            .Include(c => c.Orders)
            .FirstOrDefault(c => c.Id == id);
    }

    public Customer GetByEmail(string email)
    {
        return _context.Customers.FirstOrDefault(c => c.Email == email);
    }

    public List<Customer> GetAll()
    {
        return _context.Customers.Include(c => c.Orders).ToList();
    }

    public List<Customer> GetByOrderAmount(decimal minAmount)
    {
        return _context.Customers
            .Where(c => c.Orders.Sum(o => o.TotalAmount) >= minAmount)
            .Include(c => c.Orders)
            .ToList();
    }

    public void Add(Customer customer)
    {
        _context.Customers.Add(customer);
        _context.SaveChanges();
    }

    public void Update(Customer customer)
    {
        _context.Customers.Update(customer);
        _context.SaveChanges();
    }

    public void Delete(int id)
    {
        var customer = _context.Customers.Find(id);
        if (customer != null)
        {
            _context.Customers.Remove(customer);
            _context.SaveChanges();
        }
    }
}

// Service using repository
public class CustomerService
{
    private readonly ICustomerRepository _repository;

    public CustomerService(ICustomerRepository repository)
    {
        _repository = repository;
    }

    public Customer RegisterCustomer(string name, string email)
    {
        var existing = _repository.GetByEmail(email);
        if (existing != null)
            throw new InvalidOperationException("Customer exists");

        var customer = new Customer { Name = name, Email = email };
        _repository.Add(customer);
        return customer;
    }

    public List<Customer> GetVIPCustomers()
    {
        return _repository.GetByOrderAmount(10000m);
    }
}
```

### 3. Testing with Repository Mock

```csharp
// Mock repository for testing
public class MockCustomerRepository : ICustomerRepository
{
    private List<Customer> _customers = new();

    public Customer GetById(int id) => _customers.FirstOrDefault(c => c.Id == id);
    public Customer GetByEmail(string email) => _customers.FirstOrDefault(c => c.Email == email);
    public List<Customer> GetAll() => _customers.ToList();
    public List<Customer> GetByOrderAmount(decimal minAmount) =>
        _customers.Where(c => c.Orders.Sum(o => o.TotalAmount) >= minAmount).ToList();

    public void Add(Customer customer)
    {
        customer.Id = _customers.Max(c => c.Id) + 1;
        _customers.Add(customer);
    }

    public void Update(Customer customer)
    {
        var existing = GetById(customer.Id);
        if (existing != null)
        {
            _customers.Remove(existing);
            _customers.Add(customer);
        }
    }

    public void Delete(int id)
    {
        var customer = GetById(id);
        if (customer != null)
            _customers.Remove(customer);
    }
}

// Unit test
[TestFixture]
public class CustomerServiceTests
{
    private CustomerService _service;
    private ICustomerRepository _repository;

    [SetUp]
    public void Setup()
    {
        _repository = new MockCustomerRepository();
        _service = new CustomerService(_repository);
    }

    [Test]
    public void RegisterCustomer_ValidData_CreatesCustomer()
    {
        // Act
        var customer = _service.RegisterCustomer("John", "john@example.com");

        // Assert
        Assert.IsNotNull(customer);
        Assert.AreEqual("John", customer.Name);
    }

    [Test]
    public void RegisterCustomer_DuplicateEmail_ThrowsException()
    {
        // Arrange
        _repository.Add(new Customer { Name = "Existing", Email = "john@example.com" });

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() =>
            _service.RegisterCustomer("John", "john@example.com")
        );
    }
}
```

### 4. MongoDB Repository

```csharp
// MongoDB implementation
public class MongoCustomerRepository : ICustomerRepository
{
    private readonly IMongoCollection<Customer> _collection;

    public MongoCustomerRepository(IMongoDatabase database)
    {
        _collection = database.GetCollection<Customer>("customers");
    }

    public Customer GetById(int id)
    {
        return _collection.Find(c => c.Id == id).FirstOrDefault();
    }

    public Customer GetByEmail(string email)
    {
        return _collection.Find(c => c.Email == email).FirstOrDefault();
    }

    public List<Customer> GetAll()
    {
        return _collection.Find(_ => true).ToList();
    }

    public List<Customer> GetByOrderAmount(decimal minAmount)
    {
        return _collection.Find(c => 
            c.Orders.Sum(o => o.TotalAmount) >= minAmount
        ).ToList();
    }

    public void Add(Customer customer)
    {
        _collection.InsertOne(customer);
    }

    public void Update(Customer customer)
    {
        _collection.ReplaceOne(c => c.Id == customer.Id, customer);
    }

    public void Delete(int id)
    {
        _collection.DeleteOne(c => c.Id == id);
    }
}
```

---

## Repository Pattern Benefits

✅ **Testability** - Mock repository for unit tests  
✅ **Flexibility** - Swap data sources without changing business logic  
✅ **Separation of Concerns** - Business logic separated from data access  
✅ **Consistency** - Centralized data access logic  
✅ **DDD Support** - Works well with aggregate roots

---

## When to Use Repository

### ✅ Use When:
- Data access needs to be abstracted
- Multiple data sources possible
- Extensive unit testing needed
- Domain-driven design
- Need to swap implementations

### ❌ Don't Use When:
- CRUD operations only (EF DbContext sufficient)
- Simple data access
- No testability requirements
- Over-engineering simple apps

---

## Repository vs DbContext

| Aspect | Repository | DbContext |
|--------|-----------|-----------|
| **Abstraction** | Yes | No |
| **Testability** | High | Medium |
| **Complexity** | More | Less |
| **Flexibility** | High | Low |
| **Overkill for CRUD** | Yes | No |

---

## Summary

Repository pattern abstracts data access through a collection-like interface. Perfect for testable, flexible applications with changing data sources. Use with Unit of Work for transaction management. Trade-off: extra layer of indirection.

**Key Takeaway:** Repository abstracts data access, enabling testability and flexibility.
