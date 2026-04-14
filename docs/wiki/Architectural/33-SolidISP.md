# SOLID: Interface Segregation Principle (ISP)

## Overview

**Category:** Architectural Principle  
**Principle:** Clients should not be forced to depend on interfaces they do not use.  
**Complexity:** Medium  
**Use Case:** Interface design, client dependencies, role-based interfaces

## Problem

Large interfaces with many methods force implementing classes to:
- Implement methods they don't use
- Depend on functionality they don't need
- Have poor cohesion
- Create tight coupling

```csharp
// BAD: Fat interface with many unrelated methods
public interface IEmployee
{
    // Personal information
    string GetName();
    void SetName(string name);

    // Financial
    decimal GetSalary();
    void SetSalary(decimal salary);

    // Time tracking
    void ClockIn();
    void ClockOut();
    TimeSpan GetWorkingHours();

    // Reporting
    void GenerateReport();
    void SubmitTimesheet();

    // Benefits
    int GetVacationDays();
    void ApproveLeave(int days);

    // Management
    void AssignProject(Project project);
    List<Project> GetAssignedProjects();

    // Performance
    decimal GetPerformanceScore();
    void SetPerformanceScore(decimal score);
}

// Implementing class forced to implement many unrelated methods
public class ContractorEmployee : IEmployee
{
    // Must implement all methods, even though contractor:
    // - Doesn't have vacation days
    // - Doesn't have performance scores
    // - Doesn't have salary (hourly instead)
    // - Doesn't get benefits

    public void ApproveLeave(int days)
    {
        throw new NotImplementedException("Contractors cannot take leave");
    }

    public int GetVacationDays()
    {
        throw new NotImplementedException("Contractors don't have vacation");
    }

    // ... many other unused methods
}

// Client forced to depend on unused methods
public class HRManagementSystem
{
    public void ManageEmployee(IEmployee employee)
    {
        // This system needs only name and salary
        // But depends on entire IEmployee interface
        var name = employee.GetName();
        var salary = employee.GetSalary();

        // Has access to unrelated methods (bad!)
        employee.ClockIn();
        employee.GenerateReport();
    }
}

// Problems:
// - Fat interface with 13+ methods
// - Contractors must implement methods that don't apply
// - Tight coupling to unrelated functionality
// - Client depends on too much
// - Changes to unrelated methods affect all implementing classes
```

## Solution

Segregate large interfaces into smaller, focused interfaces:

```csharp
// GOOD: Segregated interfaces with single, focused responsibility

// Role-based interfaces
public interface IEmployee
{
    string GetName();
    void SetName(string name);
}

public interface ISalaried
{
    decimal GetSalary();
    void SetSalary(decimal salary);
}

public interface ITimeTracked
{
    void ClockIn();
    void ClockOut();
    TimeSpan GetWorkingHours();
}

public interface IReporter
{
    void GenerateReport();
    void SubmitTimesheet();
}

public interface IBenefitsEligible
{
    int GetVacationDays();
    void ApproveLeave(int days);
}

public interface IProjectAssignable
{
    void AssignProject(Project project);
    List<Project> GetAssignedProjects();
}

public interface IPerformanceRated
{
    decimal GetPerformanceScore();
    void SetPerformanceScore(decimal score);
}

// Implementing only needed interfaces
public class FullTimeEmployee : IEmployee, ISalaried, ITimeTracked, IReporter, 
                                IBenefitsEligible, IProjectAssignable, IPerformanceRated
{
    private string _name;
    private decimal _salary;
    private List<Project> _projects = new();
    private DateTime _clockInTime;

    public string GetName() => _name;
    public void SetName(string name) => _name = name;

    public decimal GetSalary() => _salary;
    public void SetSalary(decimal salary) => _salary = salary;

    public void ClockIn() => _clockInTime = DateTime.Now;
    public void ClockOut() => Console.WriteLine("Clocked out");
    public TimeSpan GetWorkingHours() => DateTime.Now - _clockInTime;

    public void GenerateReport() => Console.WriteLine("Generating report...");
    public void SubmitTimesheet() => Console.WriteLine("Submitting timesheet");

    public int GetVacationDays() => 20;
    public void ApproveLeave(int days) => Console.WriteLine($"Leave approved: {days} days");

    public void AssignProject(Project project) => _projects.Add(project);
    public List<Project> GetAssignedProjects() => _projects;

    public decimal GetPerformanceScore() => 4.5m;
    public void SetPerformanceScore(decimal score) => Console.WriteLine($"Score: {score}");
}

// Contractor implements only relevant interfaces
public class ContractorEmployee : IEmployee
{
    private string _name;

    public string GetName() => _name;
    public void SetName(string name) => _name = name;
    
    // No vacation, salary, or performance interfaces needed!
}

// Freelancer implements only time and project interfaces
public class FreelanceEmployee : IEmployee, ITimeTracked, IProjectAssignable
{
    private string _name;
    private List<Project> _projects = new();
    private DateTime _clockInTime;

    public string GetName() => _name;
    public void SetName(string name) => _name = name;

    public void ClockIn() => _clockInTime = DateTime.Now;
    public void ClockOut() => Console.WriteLine("Clocked out");
    public TimeSpan GetWorkingHours() => DateTime.Now - _clockInTime;

    public void AssignProject(Project project) => _projects.Add(project);
    public List<Project> GetAssignedProjects() => _projects;
}

// Clients depend only on what they use
public class HRManagementSystem
{
    public void ManageFullTimeEmployee(FullTimeEmployee employee)
    {
        var name = employee.GetName();
        var salary = employee.GetSalary();  // Specific to salaried employees
        Console.WriteLine($"{name}: ${salary}");
    }
}

public class TimeTrackingSystem
{
    public void TrackTime(ITimeTracked employee)
    {
        employee.ClockIn();
        // Only depends on time tracking interface
    }
}

public class ProjectManagementSystem
{
    public void AssignWork(IProjectAssignable worker, Project project)
    {
        worker.AssignProject(project);
        // Only depends on project assignment interface
    }
}
```

## Implementation Approaches

### 1. Role-Based Interfaces

```csharp
// Segregate by role/responsibility
public interface IReader
{
    void Read();
}

public interface IWriter
{
    void Write();
}

public interface ICloseable
{
    void Close();
}

// Classes implement only needed roles
public class ConsoleInput : IReader
{
    public void Read()
    {
        Console.WriteLine("Reading from console");
    }
}

public class FileWriter : IWriter, ICloseable
{
    public void Write()
    {
        Console.WriteLine("Writing to file");
    }

    public void Close()
    {
        Console.WriteLine("Closing file");
    }
}

// Client depends only on needed interface
public class DataProcessor
{
    public void ProcessInput(IReader reader)
    {
        reader.Read();  // Works with any reader
    }

    public void OutputData(IWriter writer)
    {
        writer.Write();  // Works with any writer
    }
}
```

### 2. Service Interfaces

```csharp
// BAD: Fat service interface
public interface IUserService
{
    User GetUser(int id);
    void UpdateUser(User user);
    void DeleteUser(int id);
    void SendEmail(User user, string subject);
    void LogActivity(User user, string activity);
    void GenerateReport(List<User> users);
}

// GOOD: Segregated service interfaces
public interface IUserRepository
{
    User GetUser(int id);
    void UpdateUser(User user);
    void DeleteUser(int id);
}

public interface IEmailService
{
    void SendEmail(string to, string subject, string body);
}

public interface IActivityLogger
{
    void LogActivity(int userId, string activity);
}

public interface IReportGenerator
{
    void GenerateReport(List<User> users);
}

// Implementation uses composition
public class UserService
{
    private readonly IUserRepository _repository;
    private readonly IEmailService _emailService;
    private readonly IActivityLogger _logger;

    public UserService(
        IUserRepository repository,
        IEmailService emailService,
        IActivityLogger logger)
    {
        _repository = repository;
        _emailService = emailService;
        _logger = logger;
    }

    public void UpdateUserAndNotify(User user, string emailSubject)
    {
        _repository.UpdateUser(user);
        _emailService.SendEmail(user.Email, emailSubject, "Your profile was updated");
        _logger.LogActivity(user.Id, "Profile updated");
    }
}

// Client depends only on what it uses
public class ReportController
{
    private readonly IReportGenerator _reportGenerator;

    public ReportController(IReportGenerator reportGenerator)
    {
        _reportGenerator = reportGenerator;
    }

    public void GenerateUserReport(List<User> users)
    {
        _reportGenerator.GenerateReport(users);
    }
}
```

### 3. Adapter Pattern for Legacy Interfaces

```csharp
// Old fat interface from legacy system
public interface ILegacyDocument
{
    void Open(string path);
    void Close();
    void Save();
    void SaveAs(string path);
    void Print();
    void ExportToPdf(string path);
    void ExportToXml(string path);
    void FindAndReplace(string find, string replace);
    void Undo();
    void Redo();
    void Cut();
    void Copy();
    void Paste();
    // ... many more methods
}

// Create focused interfaces
public interface IDocumentIO
{
    void Open(string path);
    void Close();
    void Save();
    void SaveAs(string path);
}

public interface IDocumentPrinting
{
    void Print();
    void ExportToPdf(string path);
}

public interface IDocumentExport
{
    void ExportToXml(string path);
    void ExportToPdf(string path);
}

public interface IDocumentEditing
{
    void FindAndReplace(string find, string replace);
    void Cut();
    void Copy();
    void Paste();
    void Undo();
    void Redo();
}

// Adapter bridges legacy interface to segregated interfaces
public class LegacyDocumentAdapter : 
    IDocumentIO, IDocumentPrinting, IDocumentExport, IDocumentEditing
{
    private readonly ILegacyDocument _legacyDoc;

    public LegacyDocumentAdapter(ILegacyDocument legacyDoc)
    {
        _legacyDoc = legacyDoc;
    }

    // IO operations
    public void Open(string path) => _legacyDoc.Open(path);
    public void Close() => _legacyDoc.Close();
    public void Save() => _legacyDoc.Save();
    public void SaveAs(string path) => _legacyDoc.SaveAs(path);

    // Printing operations
    public void Print() => _legacyDoc.Print();

    // Export operations
    public void ExportToXml(string path) => _legacyDoc.ExportToXml(path);
    public void ExportToPdf(string path) => _legacyDoc.ExportToPdf(path);

    // Editing operations
    public void FindAndReplace(string find, string replace) => 
        _legacyDoc.FindAndReplace(find, replace);
    public void Cut() => _legacyDoc.Cut();
    public void Copy() => _legacyDoc.Copy();
    public void Paste() => _legacyDoc.Paste();
    public void Undo() => _legacyDoc.Undo();
    public void Redo() => _legacyDoc.Redo();
}

// New code depends on focused interfaces
public class DocumentPrinter
{
    private readonly IDocumentPrinting _document;

    public DocumentPrinter(IDocumentPrinting document)
    {
        _document = document;
    }

    public void PrintAndExport(string pdfPath)
    {
        _document.Print();
        _document.ExportToPdf(pdfPath);
    }
}
```

## Benefits of ISP

✅ **Focused Contracts** - Interfaces specify exactly what's needed  
✅ **Reduced Coupling** - Clients depend only on used methods  
✅ **Better Cohesion** - Interface members are related  
✅ **Flexibility** - Classes can implement multiple focused interfaces  
✅ **Easier Testing** - Mock only needed behavior  
✅ **Clearer Intent** - Interface name expresses purpose  

## Drawbacks

❌ **More Interfaces** - Can create many small interfaces  
❌ **Complexity** - More types to manage  
❌ **Overhead** - Multiple interface inheritance  
❌ **Over-Segregation** - Risk of too many tiny interfaces  

## Interview Questions

**Q: What is Interface Segregation Principle?**
A: Clients should not be forced to depend on interfaces they do not use. It's better to have many specific interfaces than one general-purpose interface.

**Q: How does ISP differ from SRP?**
A: SRP focuses on class responsibilities. ISP focuses on client dependencies. A class can have one responsibility but expose many interface roles.

**Q: What's a sign of ISP violation?**
A: Implementing classes throw NotImplementedException or have empty implementations of methods they don't use. This indicates the interface is too fat.

**Q: Can a class implement multiple interfaces?**
A: Yes, that's encouraged! ISP recommends many specific interfaces over one fat interface, allowing classes to implement only needed roles.

**Q: How does ISP relate to inheritance?**
A: ISP applies to interface design. Inheritance hierarchies should also be segregated - avoid inheriting from classes with unneeded methods.

**Q: What's role-based interface design?**
A: Designing interfaces around client roles (IReader, IWriter, ICloseable) rather than object types. Each role represents a specific capability.

## When to Apply ISP

### ✅ Apply When:
- Interface has many unrelated methods
- Implementing classes don't use all methods
- Clients use only subset of interface methods
- Creating reusable interface contracts
- Supporting multiple roles

### ❌ Don't Over-Apply When:
- Interface is cohesive and focused
- All methods are closely related
- Creating too many micro-interfaces
- Adds unnecessary complexity

## Code Smell: NotImplementedException

```csharp
// ANTI-PATTERN: Class forced to implement unused interface methods
public class SimpleReader : IDocument
{
    public void Read() { /* ... */ }

    public void Write()
    {
        throw new NotImplementedException();
    }

    public void Print()
    {
        throw new NotImplementedException();
    }

    public void Export()
    {
        throw new NotImplementedException();
    }
}

// SOLUTION: Implement only needed interfaces
public interface IReadable
{
    void Read();
}

public class SimpleReader : IReadable
{
    public void Read() { /* ... */ }
}
```

## Real-World Patterns Using ISP

**Adapter Pattern** - Creates focused interfaces for incompatible objects  
**Decorator Pattern** - Decorators implement specific interfaces  
**Facade Pattern** - Provides focused interface to complex system  
**Strategy Pattern** - Each strategy implements specific interface  

## Summary

Interface Segregation Principle requires that interfaces be specific to client needs. Rather than creating fat interfaces with many unrelated methods, create multiple focused interfaces. This reduces coupling, improves testability, and clarifies intent. ISP is particularly important in object-oriented systems with complex contracts.

**Key Takeaway:** Design specific interfaces for specific client needs. Implement only needed interfaces, never force implementations into unused methods.

---

**Related SOLID Principles:**
- Single Responsibility Principle - Each class has one reason to change
- Open/Closed Principle - Classes open for extension, closed for modification
- Liskov Substitution Principle - Subtypes must be substitutable
- Dependency Inversion Principle - Depend on abstractions, not concrete implementations
