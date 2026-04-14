# MVC (Model-View-Controller) Architecture

## Overview

**Category:** Architectural Pattern  
**Purpose:** Separate application into three interconnected components to separate concerns and enable parallel development.  
**Complexity:** Medium  
**Use Case:** Web applications, desktop applications, mobile apps

## Problem

Applications with mixed business logic, UI, and data handling become:
- Hard to maintain and test
- Difficult to reuse components
- Impossible to work on features independently
- Brittle when requirements change

```csharp
// BAD: Mixed concerns in single class
public class UserForm
{
    private TextBox nameInput;
    private TextBox emailInput;
    
    public void SaveUser()
    {
        // UI logic
        string name = nameInput.Text;
        
        // Validation logic
        if (string.IsNullOrEmpty(name)) return;
        
        // Business logic
        var user = new User { Name = name, Email = emailInput.Text };
        
        // Data access
        using (var db = new SqlConnection(...))
        {
            db.Execute("INSERT INTO Users...", user);
        }
        
        MessageBox.Show("Saved!");  // Back to UI
    }
}
// Everything mixed together!
```

## Solution

Separate into three distinct layers:

```csharp
// MODEL: Business logic and data
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

public class UserService
{
    private IUserRepository _repository;
    
    public void CreateUser(string name, string email)
    {
        // Validation
        if (string.IsNullOrEmpty(name)) throw new ArgumentException();
        
        // Business logic
        var user = new User { Name = name, Email = email };
        _repository.Save(user);
    }
}

// VIEW: Presentation only
public partial class UserForm : Form
{
    private UserController _controller;
    
    private void SaveButton_Click(object sender, EventArgs e)
    {
        _controller.SaveUser(nameInput.Text, emailInput.Text);
        MessageBox.Show("Saved!");
    }
}

// CONTROLLER: Orchestrates Model and View
public class UserController
{
    private UserService _userService;
    private UserForm _view;
    
    public void SaveUser(string name, string email)
    {
        _userService.CreateUser(name, email);
        _view.RefreshUserList();
    }
}
```

## Implementation Approaches

### 1. ASP.NET MVC Web Application

```csharp
// MODEL: User domain model
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
    public DateTime CreatedAt { get; set; }
}

// REPOSITORY: Data access layer
public interface IUserRepository
{
    User GetById(int id);
    List<User> GetAll();
    void Save(User user);
    void Delete(int id);
}

public class UserRepository : IUserRepository
{
    private List<User> _users = new();

    public User GetById(int id) => _users.FirstOrDefault(u => u.Id == id);
    public List<User> GetAll() => _users.ToList();
    
    public void Save(User user)
    {
        if (user.Id == 0)
            user.Id = _users.Max(u => u.Id) + 1;
        
        var existing = GetById(user.Id);
        if (existing != null)
            _users.Remove(existing);
        
        _users.Add(user);
    }

    public void Delete(int id)
    {
        var user = GetById(id);
        if (user != null)
            _users.Remove(user);
    }
}

// SERVICE/BUSINESS LOGIC: Application services
public class UserService
{
    private readonly IUserRepository _repository;

    public UserService(IUserRepository repository)
    {
        _repository = repository;
    }

    public User CreateUser(string name, string email)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name required");
        
        if (!email.Contains("@"))
            throw new ArgumentException("Invalid email");

        var user = new User
        {
            Name = name,
            Email = email,
            CreatedAt = DateTime.Now
        };

        _repository.Save(user);
        return user;
    }

    public List<User> GetAllUsers() => _repository.GetAll();
}

// CONTROLLER: Request handling and orchestration
public class UserController
{
    private readonly UserService _userService;

    public UserController(UserService userService)
    {
        _userService = userService;
    }

    public ActionResult List()
    {
        var users = _userService.GetAllUsers();
        return View(users);  // Passes to View
    }

    [HttpPost]
    public ActionResult Create(string name, string email)
    {
        try
        {
            var user = _userService.CreateUser(name, email);
            return RedirectToAction("List");
        }
        catch (ArgumentException ex)
        {
            return View("Error", ex.Message);
        }
    }

    public ActionResult Delete(int id)
    {
        // Business logic in service
        // Controller just orchestrates
        return RedirectToAction("List");
    }
}

// VIEW: HTML template (Razor)
// Users.cshtml
@model List<User>

<table>
    <thead>
        <tr>
            <th>Name</th>
            <th>Email</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var user in Model)
        {
            <tr>
                <td>@user.Name</td>
                <td>@user.Email</td>
            </tr>
        }
    </tbody>
</table>

<form method="post" action="/user/create">
    <input type="text" name="name" placeholder="Name"/>
    <input type="email" name="email" placeholder="Email"/>
    <button type="submit">Add User</button>
</form>
```

### 2. WinForms MVC Desktop Application

```csharp
// MODEL
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public int Quantity { get; set; }
}

// MODEL SERVICE
public class ProductService
{
    private List<Product> _products = new();

    public void AddProduct(string name, decimal price)
    {
        var product = new Product
        {
            Id = _products.Count + 1,
            Name = name,
            Price = price,
            Quantity = 0
        };
        _products.Add(product);
    }

    public List<Product> GetAllProducts() => _products.ToList();

    public void UpdatePrice(int id, decimal newPrice)
    {
        var product = _products.FirstOrDefault(p => p.Id == id);
        if (product != null)
            product.Price = newPrice;
    }
}

// VIEW
public partial class ProductForm : Form
{
    private ProductController _controller;

    public ProductForm()
    {
        InitializeComponent();
        _controller = new ProductController(this);
    }

    public void DisplayProducts(List<Product> products)
    {
        productGrid.DataSource = products;
    }

    public void ShowMessage(string message)
    {
        MessageBox.Show(message);
    }

    private void AddButton_Click(object sender, EventArgs e)
    {
        _controller.AddProduct(nameInput.Text, decimal.Parse(priceInput.Text));
        nameInput.Clear();
        priceInput.Clear();
    }

    private void RefreshButton_Click(object sender, EventArgs e)
    {
        _controller.RefreshProductList();
    }
}

// CONTROLLER
public class ProductController
{
    private ProductService _service;
    private ProductForm _view;

    public ProductController(ProductForm view)
    {
        _service = new ProductService();
        _view = view;
    }

    public void AddProduct(string name, decimal price)
    {
        try
        {
            _service.AddProduct(name, price);
            _view.ShowMessage("✅ Product added");
            RefreshProductList();
        }
        catch (Exception ex)
        {
            _view.ShowMessage($"❌ Error: {ex.Message}");
        }
    }

    public void RefreshProductList()
    {
        var products = _service.GetAllProducts();
        _view.DisplayProducts(products);
    }
}
```

### 3. E-Commerce MVC System

```csharp
// MODEL: Order domain
public class Order
{
    public int Id { get; set; }
    public DateTime OrderDate { get; set; }
    public List<OrderItem> Items { get; set; } = new();
    public OrderStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
}

public class OrderItem
{
    public int ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public enum OrderStatus { Pending, Processing, Shipped, Delivered }

// MODEL: Service layer
public class OrderService
{
    private readonly IOrderRepository _orderRepository;
    private readonly IProductRepository _productRepository;

    public OrderService(IOrderRepository orderRepo, IProductRepository productRepo)
    {
        _orderRepository = orderRepo;
        _productRepository = productRepo;
    }

    public Order CreateOrder(List<int> productIds, List<int> quantities)
    {
        var order = new Order { OrderDate = DateTime.Now, Status = OrderStatus.Pending };

        for (int i = 0; i < productIds.Count; i++)
        {
            var product = _productRepository.GetById(productIds[i]);
            if (product == null)
                throw new InvalidOperationException("Product not found");

            order.Items.Add(new OrderItem
            {
                ProductId = product.Id,
                Quantity = quantities[i],
                UnitPrice = product.Price
            });

            order.TotalAmount += product.Price * quantities[i];
        }

        _orderRepository.Save(order);
        return order;
    }

    public void ProcessOrder(int orderId)
    {
        var order = _orderRepository.GetById(orderId);
        if (order == null)
            throw new InvalidOperationException("Order not found");

        order.Status = OrderStatus.Processing;
        _orderRepository.Update(order);
    }

    public List<Order> GetUserOrders(int userId)
    {
        return _orderRepository.GetUserOrders(userId);
    }
}

// CONTROLLER
public class OrderController
{
    private OrderService _orderService;

    public OrderController(OrderService orderService)
    {
        _orderService = orderService;
    }

    public ActionResult CreateOrder(int[] productIds, int[] quantities)
    {
        try
        {
            var order = _orderService.CreateOrder(productIds.ToList(), quantities.ToList());
            return Json(new { success = true, orderId = order.Id });
        }
        catch (Exception ex)
        {
            return Json(new { success = false, error = ex.Message });
        }
    }

    public ActionResult ViewOrder(int orderId)
    {
        var order = _orderService.GetById(orderId);
        return View(order);
    }
}

// VIEW: Order confirmation page
// OrderConfirmation.cshtml
@model Order

<div class="order-confirmation">
    <h2>Order #@Model.Id Confirmed</h2>
    
    <table>
        <tr>
            <th>Product</th>
            <th>Qty</th>
            <th>Price</th>
            <th>Total</th>
        </tr>
        @foreach (var item in Model.Items)
        {
            <tr>
                <td>@item.ProductId</td>
                <td>@item.Quantity</td>
                <td>$@item.UnitPrice</td>
                <td>$@(item.UnitPrice * item.Quantity)</td>
            </tr>
        }
    </table>
    
    <h3>Total: $@Model.TotalAmount</h3>
    <p>Status: @Model.Status</p>
</div>
```

---

## MVC Flow Diagram

```
┌─────────────┐
│   User      │
│ (Browser)   │
└──────┬──────┘
       │ 1. Request
       ▼
┌─────────────────────────────────────────┐
│ CONTROLLER (Request Handler)             │
│ - Parse request                          │
│ - Call business logic                    │
│ - Select view to render                  │
└──────────────────┬──────────────────────┘
                   │ 2. Call service
                   │
       ┌───────────▼──────────────┐
       │ MODEL (Business Logic)    │
       │ - Apply business rules    │
       │ - Validate data           │
       │ - Access database         │
       └───────────┬───────────────┘
                   │ 3. Return data
                   │
┌──────────────────▼──────────────────────┐
│ VIEW (Presentation)                      │
│ - Format data for display                │
│ - Render HTML/UI                         │
│ - Handle user input                      │
└──────────────────┬──────────────────────┘
                   │ 4. Response
                   ▼
          ┌────────────────┐
          │  User Browser  │
          │ (Display HTML) │
          └────────────────┘
```

---

## Pros and Cons

### Advantages
✅ **Separation of Concerns** - Each layer has single responsibility  
✅ **Testability** - Can test business logic independently  
✅ **Reusability** - Models can be reused across views  
✅ **Parallel Development** - Teams work on different layers  
✅ **Maintainability** - Clear structure and organization  
✅ **Scalability** - Easy to extend with new features  

### Disadvantages
❌ **Complexity** - More files and classes to manage  
❌ **Learning Curve** - New developers need to understand pattern  
❌ **Overhead** - Extra abstraction layers add overhead  
❌ **Tight Coupling** - Controller tightly couples Model and View  
❌ **Testing View** - Views still hard to unit test  

---

## MVC vs. Other Patterns

| Pattern | Controller | Model | View |
|---------|-----------|-------|------|
| **MVC** | Orchestrator | Business logic | Presentation |
| **MVVM** | ViewModel | Model | View (with binding) |
| **MVP** | Presenter | Model | View (passive) |

---

## When to Use MVC

### ✅ Use MVC When:
- Building web applications (ASP.NET MVC, Spring, Django)
- Need clear separation of concerns
- Multiple developers working on same codebase
- Want independent testing of business logic
- Traditional request-response architecture
- Building UI with forms and navigation

### ❌ Don't Use MVC When:
- Real-time bidirectional updates needed (use MVVM/WebSockets)
- Simple single-page apps (use SPA framework)
- Tightly integrated data binding required (use MVVM)
- Highly interactive UI (consider MVVM)

---

## Interview Questions

**Q: What is the role of Controller in MVC?**
A: Controller receives user input, calls appropriate business logic in Model, and selects View to render response. Orchestrates Model and View.

**Q: How does MVC improve testability?**
A: Business logic in Model is independent of UI, so can be unit tested without rendering Views or user interaction.

**Q: What's the difference between MVC and MVVM?**
A: MVC has active View with Controller orchestration. MVVM has passive View with ViewModel handling updates. MVVM better for rich client apps.

**Q: Can Model directly update View?**
A: Ideally no. Controller should mediate. Some variations allow Model to notify View of changes, but tightly couples them.

**Q: What about Model-View-Update (MVU) pattern?**
A: Modern variation used in React, Elm. Pure functional approach with unidirectional data flow. More testable than traditional MVC.

---

## Real-World Examples

- **ASP.NET MVC** - Microsoft web framework
- **Spring MVC** - Java web framework
- **Django** - Python web framework
- **Ruby on Rails** - Ruby web framework
- **Laravel** - PHP web framework
- **WinForms** - Desktop applications
- **WPF** with MVVM variation - Rich clients

---

## Summary

MVC separates application into Model (business logic), View (presentation), and Controller (orchestration). Perfect for web applications with clear separation between data, logic, and display. Enables parallel development, independent testing, and code reuse. Trade-off: more files and complexity compared to monolithic design.

**Key Takeaway:** MVC separates concerns into Model (logic), View (UI), and Controller (orchestration).
