# SOLID: Single Responsibility Principle (SRP)

## Overview

**Category:** Architectural Principle  
**Principle:** A class should have one and only one reason to change.  
**Complexity:** Easy-Medium  
**Use Case:** Class design, separation of concerns, maintainability

## Problem

Classes with multiple responsibilities become:
- Hard to understand and test
- Prone to bugs when one responsibility changes
- Difficult to reuse
- Violate single purpose design

```csharp
// BAD: Multiple responsibilities in one class
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }

    // Responsibility 1: User data
    public void Save()
    {
        using (var db = new SqlConnection("..."))
        {
            db.Execute("INSERT INTO Users...", this);
        }
    }

    // Responsibility 2: Email notification
    public void SendWelcomeEmail()
    {
        var smtpClient = new SmtpClient("smtp.gmail.com");
        smtpClient.Send(Email, "Welcome!");
    }

    // Responsibility 3: Logging
    public void LogActivity(string activity)
    {
        File.AppendAllText("log.txt", $"{DateTime.Now}: {activity}");
    }

    // Responsibility 4: Validation
    public bool IsValidEmail()
    {
        return Email.Contains("@");
    }
}

// Problems:
// - Changes to database schema affect User class
// - Changes to email service affect User class
// - Changes to logging affect User class
// - Hard to test each responsibility independently
// - High coupling to database, email, file system
```

## Solution

Separate each responsibility into its own class:

```csharp
// GOOD: Each class has single responsibility

// Responsibility 1: User data
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

// Responsibility 2: Data persistence
public interface IUserRepository
{
    void Save(User user);
    User GetById(int id);
}

public class UserRepository : IUserRepository
{
    private readonly string _connectionString;

    public void Save(User user)
    {
        using (var db = new SqlConnection(_connectionString))
        {
            db.Execute("INSERT INTO Users...", user);
        }
    }

    public User GetById(int id)
    {
        using (var db = new SqlConnection(_connectionString))
        {
            return db.QuerySingle<User>("SELECT * FROM Users WHERE Id=@id", new { id });
        }
    }
}

// Responsibility 3: Email notifications
public interface IEmailService
{
    void SendWelcomeEmail(string email, string name);
}

public class EmailService : IEmailService
{
    public void SendWelcomeEmail(string email, string name)
    {
        using (var smtpClient = new SmtpClient("smtp.gmail.com"))
        {
            var mailMessage = new MailMessage("noreply@example.com", email)
            {
                Subject = "Welcome!",
                Body = $"Welcome {name}!"
            };
            smtpClient.Send(mailMessage);
        }
    }
}

// Responsibility 4: Logging
public interface ILogger
{
    void Log(string message);
}

public class FileLogger : ILogger
{
    public void Log(string message)
    {
        File.AppendAllText("log.txt", $"{DateTime.Now}: {message}\n");
    }
}

// Responsibility 5: Validation
public interface IEmailValidator
{
    bool IsValid(string email);
}

public class EmailValidator : IEmailValidator
{
    public bool IsValid(string email)
    {
        return email.Contains("@") && email.Contains(".");
    }
}

// Service orchestrating separated concerns
public class UserService
{
    private readonly IUserRepository _repository;
    private readonly IEmailService _emailService;
    private readonly ILogger _logger;
    private readonly IEmailValidator _emailValidator;

    public UserService(
        IUserRepository repository,
        IEmailService emailService,
        ILogger logger,
        IEmailValidator emailValidator)
    {
        _repository = repository;
        _emailService = emailService;
        _logger = logger;
        _emailValidator = emailValidator;
    }

    public void RegisterUser(string name, string email)
    {
        // Validate
        if (!_emailValidator.IsValid(email))
            throw new ArgumentException("Invalid email");

        // Create user
        var user = new User { Name = name, Email = email };

        // Save
        _repository.Save(user);
        _logger.Log($"User {name} saved");

        // Notify
        _emailService.SendWelcomeEmail(email, name);
        _logger.Log($"Welcome email sent to {email}");
    }
}

// Usage
var repository = new UserRepository("connection_string");
var emailService = new EmailService();
var logger = new FileLogger();
var validator = new EmailValidator();

var userService = new UserService(repository, emailService, logger, validator);
userService.RegisterUser("John Doe", "john@example.com");
```

## Implementation Approaches

### 1. Identifying Responsibilities

```csharp
// Question: What could cause this class to change?

// BAD: Multiple reasons to change
public class OrderProcessor
{
    // 1. Order processing logic might change
    public void ProcessOrder(Order order) { }

    // 2. Payment processing might change
    public void ProcessPayment(Payment payment) { }

    // 3. Shipping logic might change
    public void ArrangeShipping(Address address) { }

    // 4. Email notifications might change
    public void SendOrderConfirmation(Order order) { }

    // 5. Database might change
    public void SaveOrder(Order order) { }
}

// GOOD: One reason to change - order processing logic
public class OrderProcessor
{
    private readonly IPaymentProcessor _paymentProcessor;
    private readonly IShippingService _shippingService;
    private readonly IEmailService _emailService;
    private readonly IOrderRepository _repository;

    public void ProcessOrder(Order order)
    {
        // Business logic for processing order
        order.Status = OrderStatus.Processing;

        _paymentProcessor.Process(order.Payment);
        _shippingService.Arrange(order.ShippingAddress);
        _emailService.SendConfirmation(order);
        _repository.Save(order);
    }
}
```

### 2. Report Generation Example

```csharp
// BAD: Mixed concerns
public class ReportGenerator
{
    public void GenerateReport(List<Employee> employees)
    {
        // Report logic
        var report = new StringBuilder();
        report.AppendLine("=== Employee Report ===");
        
        foreach (var emp in employees)
        {
            report.AppendLine($"{emp.Name}: ${emp.Salary}");
        }

        // Formatting for different outputs (HTML, PDF, Excel, etc.)
        // This creates many reasons to change
        OutputAsHtml(report.ToString());
        OutputAsPdf(report.ToString());
        OutputAsExcel(report.ToString());
    }

    private void OutputAsHtml(string content) { }
    private void OutputAsPdf(string content) { }
    private void OutputAsExcel(string content) { }
}

// GOOD: Separated concerns
public class EmployeeReportGenerator
{
    private readonly IReportFormatter _formatter;

    public EmployeeReportGenerator(IReportFormatter formatter)
    {
        _formatter = formatter;
    }

    public void GenerateReport(List<Employee> employees)
    {
        var reportData = new StringBuilder();
        reportData.AppendLine("=== Employee Report ===");
        
        foreach (var emp in employees)
        {
            reportData.AppendLine($"{emp.Name}: ${emp.Salary}");
        }

        _formatter.Format(reportData.ToString());
    }
}

public interface IReportFormatter
{
    void Format(string content);
}

public class HtmlReportFormatter : IReportFormatter
{
    public void Format(string content)
    {
        var html = $"<html><body><pre>{content}</pre></body></html>";
        File.WriteAllText("report.html", html);
    }
}

public class PdfReportFormatter : IReportFormatter
{
    public void Format(string content)
    {
        // Generate PDF
    }
}

public class ExcelReportFormatter : IReportFormatter
{
    public void Format(string content)
    {
        // Generate Excel
    }
}
```

### 3. Authentication Service

```csharp
// BAD: Multiple responsibilities
public class AuthenticationService
{
    // Responsibility 1: User validation
    public bool ValidateUser(string username, string password)
    {
        var user = GetUserFromDatabase(username);
        return BCrypt.Verify(password, user.PasswordHash);
    }

    // Responsibility 2: Token generation
    public string GenerateToken(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes("secret_key");
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] 
            { 
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    // Responsibility 3: Database access
    private User GetUserFromDatabase(string username)
    {
        using (var db = new SqlConnection("..."))
        {
            return db.QuerySingle<User>(
                "SELECT * FROM Users WHERE Username=@username", 
                new { username });
        }
    }

    // Responsibility 4: Logging
    public void LogLoginAttempt(string username, bool success)
    {
        File.AppendAllText("logs.txt", 
            $"{DateTime.Now}: {username} - {(success ? "Success" : "Failed")}\n");
    }
}

// GOOD: Separated concerns
public interface IUserValidator
{
    bool ValidateCredentials(string username, string password);
}

public class PasswordValidator : IUserValidator
{
    private readonly IUserRepository _repository;

    public PasswordValidator(IUserRepository repository)
    {
        _repository = repository;
    }

    public bool ValidateCredentials(string username, string password)
    {
        var user = _repository.GetByUsername(username);
        return user != null && BCrypt.Verify(password, user.PasswordHash);
    }
}

public interface ITokenGenerator
{
    string Generate(User user);
}

public class JwtTokenGenerator : ITokenGenerator
{
    private readonly string _secretKey;

    public JwtTokenGenerator(string secretKey)
    {
        _secretKey = secretKey;
    }

    public string Generate(User user)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.ASCII.GetBytes(_secretKey);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[] 
            { 
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            }),
            Expires = DateTime.UtcNow.AddHours(1),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), 
                SecurityAlgorithms.HmacSha256Signature)
        };
        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }
}

public interface IAuthenticationLogger
{
    void LogAttempt(string username, bool success);
}

public class FileAuthenticationLogger : IAuthenticationLogger
{
    private readonly string _logPath;

    public FileAuthenticationLogger(string logPath)
    {
        _logPath = logPath;
    }

    public void LogAttempt(string username, bool success)
    {
        File.AppendAllText(_logPath, 
            $"{DateTime.Now}: {username} - {(success ? "Success" : "Failed")}\n");
    }
}

public class AuthenticationService
{
    private readonly IUserValidator _validator;
    private readonly ITokenGenerator _tokenGenerator;
    private readonly IAuthenticationLogger _logger;

    public AuthenticationService(
        IUserValidator validator,
        ITokenGenerator tokenGenerator,
        IAuthenticationLogger logger)
    {
        _validator = validator;
        _tokenGenerator = tokenGenerator;
        _logger = logger;
    }

    public string Authenticate(string username, string password)
    {
        var isValid = _validator.ValidateCredentials(username, password);
        _logger.LogAttempt(username, isValid);

        if (!isValid)
            throw new UnauthorizedAccessException("Invalid credentials");

        var user = new User { Id = 1, Username = username };
        return _tokenGenerator.Generate(user);
    }
}
```

## Benefits of SRP

✅ **Single Reason to Change** - Only one aspect to modify  
✅ **Easier Testing** - Test each responsibility independently  
✅ **Better Reusability** - Classes do one thing well  
✅ **Improved Maintainability** - Changes localized  
✅ **Clearer Intent** - Class name clearly states purpose  
✅ **Reduced Coupling** - Dependencies are explicit  

## Drawbacks

❌ **More Classes** - Can lead to many small classes  
❌ **Complexity** - Orchestration becomes more complex  
❌ **Over-Engineering** - Simple tasks become multi-class  
❌ **Navigation** - Harder to understand full flow  

## Interview Questions

**Q: What does Single Responsibility Principle mean?**
A: A class should have one and only one reason to change. It should have one primary purpose or responsibility. This makes classes more testable, reusable, and maintainable.

**Q: How do you identify responsibilities in a class?**
A: Ask "What could cause this class to change?" Each distinct answer is a responsibility. If there are multiple, the class violates SRP.

**Q: What's the difference between SRP and Separation of Concerns?**
A: SRP focuses on class design - each class has one reason to change. Separation of Concerns is broader - it means different aspects (UI, business logic, data) shouldn't be mixed.

**Q: Can a class have multiple related responsibilities?**
A: Ideally no, but in practice, related responsibilities in a cohesive class can be acceptable. The key is that changes to unrelated aspects shouldn't affect the class.

**Q: How does SRP relate to other SOLID principles?**
A: SRP is foundational. When you follow SRP, you naturally end up with classes designed for single purposes, which makes following other SOLID principles (OCP, LSP, ISP, DIP) easier.

**Q: What's a real-world example of SRP violation?**
A: A User class that handles data, validation, persistence, email notifications, and logging. Should be split into User (data), UserRepository (persistence), EmailService (notifications), etc.

## When to Apply SRP

### ✅ Apply When:
- Class has more than one reason to change
- Testing requires mocking multiple unrelated dependencies
- Class name doesn't clearly describe single responsibility
- Different developers change different aspects of same class
- Class methods operate on different objects

### ❌ Don't Over-Apply When:
- It creates unnecessary complexity
- Related responsibilities are tightly coupled
- Creates too many single-method classes
- Reduces code readability

## Real-World Patterns Using SRP

**Repository Pattern** - Separates data access from business logic  
**Adapter Pattern** - Separates interface translation from core logic  
**Decorator Pattern** - Separates added responsibilities from core functionality  
**Chain of Responsibility** - Separates handling into discrete handlers  

## Anti-Pattern: God Class

```csharp
// ANTI-PATTERN: God Class - everything in one place
public class Application
{
    // Data access
    public List<User> GetUsers() { }
    public void SaveUser(User user) { }

    // Business logic
    public void RegisterUser(string name, string email) { }
    public void ProcessPayment(decimal amount) { }

    // Email
    public void SendEmail(string to, string subject) { }

    // Logging
    public void Log(string message) { }

    // Validation
    public bool ValidateEmail(string email) { }

    // Security
    public string HashPassword(string password) { }

    // Configuration
    public string GetConfigValue(string key) { }

    // Reporting
    public void GenerateReport() { }

    // ... 50+ other methods
}

// SOLUTION: Extract to separate classes with single responsibilities
```

## Summary

Single Responsibility Principle requires each class to have one reason to change. This fundamental principle leads to better code organization, easier testing, improved reusability, and clearer intent. While it increases the number of classes, it dramatically improves maintainability and flexibility.

**Key Takeaway:** One class, one reason to change. Extract responsibilities into separate, focused classes.

---

**Related SOLID Principles:**
- Open/Closed Principle - Classes open for extension, closed for modification
- Liskov Substitution Principle - Subtypes must be substitutable
- Interface Segregation Principle - Clients shouldn't depend on unused methods
- Dependency Inversion Principle - Depend on abstractions, not concrete implementations
