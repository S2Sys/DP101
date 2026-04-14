# MVP (Model-View-Presenter) Architecture

## Overview

**Category:** Architectural Pattern  
**Purpose:** Separate presentation from business logic with passive View, allowing complete UI testing.  
**Complexity:** Medium  
**Key Difference from MVC:** Presenter controls View; View has no intelligence

## Problem

In MVC, Controller doesn't fully isolate View from Model, making UI testing difficult. View still handles some logic and can update itself.

## Solution

Presenter (like Controller) but with completely passive View:

```csharp
// MODEL: Business logic only
public class BankAccount
{
    public decimal Balance { get; private set; }
    
    public void Deposit(decimal amount)
    {
        if (amount <= 0) throw new ArgumentException();
        Balance += amount;
    }
}

// VIEW: Completely passive - just displays data
public interface IAccountView
{
    void DisplayBalance(decimal balance);
    void DisplayMessage(string message);
}

// PRESENTER: All UI logic and orchestration
public class AccountPresenter
{
    private readonly BankAccount _model;
    private readonly IAccountView _view;

    public void Deposit(decimal amount)
    {
        try
        {
            _model.Deposit(amount);
            _view.DisplayBalance(_model.Balance);
            _view.DisplayMessage("✅ Deposit successful");
        }
        catch (Exception ex)
        {
            _view.DisplayMessage($"❌ Error: {ex.Message}");
        }
    }
}
```

## Key Characteristics

- **Passive View**: View only displays data, no logic
- **Testable UI Logic**: Presenter can be tested with mock View
- **Two-Way Communication**: View calls Presenter, Presenter calls View
- **View Independence**: View can be completely replaced

## Implementations

### 1. Windows Forms MVP

```csharp
// MODEL
public class Task
{
    public int Id { get; set; }
    public string Title { get; set; }
    public bool IsCompleted { get; set; }
}

public class TaskService
{
    private List<Task> _tasks = new();

    public List<Task> GetAllTasks() => _tasks.ToList();

    public void AddTask(string title)
    {
        _tasks.Add(new Task { Id = _tasks.Count + 1, Title = title });
    }

    public void CompleteTask(int id)
    {
        var task = _tasks.FirstOrDefault(t => t.Id == id);
        if (task != null) task.IsCompleted = true;
    }
}

// VIEW INTERFACE
public interface ITaskView
{
    void ShowTasks(List<Task> tasks);
    void ShowMessage(string message);
    void ClearInput();
    event EventHandler<EventArgs> AddTaskClicked;
    event EventHandler<int> CompleteTaskClicked;
    string GetTaskInput { get; }
}

// VIEW: Windows Form (implements interface)
public partial class TaskForm : Form, ITaskView
{
    private TaskPresenter _presenter;

    public string GetTaskInput => taskInput.Text;

    public event EventHandler<EventArgs> AddTaskClicked;
    public event EventHandler<int> CompleteTaskClicked;

    public TaskForm()
    {
        InitializeComponent();
        _presenter = new TaskPresenter(new TaskService(), this);
        _presenter.LoadTasks();

        addButton.Click += (s, e) => AddTaskClicked?.Invoke(this, EventArgs.Empty);
    }

    public void ShowTasks(List<Task> tasks)
    {
        taskGrid.DataSource = tasks;
    }

    public void ShowMessage(string message)
    {
        MessageBox.Show(message);
    }

    public void ClearInput()
    {
        taskInput.Clear();
    }
}

// PRESENTER: All logic
public class TaskPresenter
{
    private readonly TaskService _service;
    private readonly ITaskView _view;

    public TaskPresenter(TaskService service, ITaskView view)
    {
        _service = service;
        _view = view;

        _view.AddTaskClicked += (s, e) => OnAddTask();
        _view.CompleteTaskClicked += (s, id) => OnCompleteTask(id);
    }

    public void LoadTasks()
    {
        var tasks = _service.GetAllTasks();
        _view.ShowTasks(tasks);
    }

    private void OnAddTask()
    {
        var title = _view.GetTaskInput;
        if (string.IsNullOrEmpty(title))
        {
            _view.ShowMessage("❌ Title required");
            return;
        }

        _service.AddTask(title);
        _view.ClearInput();
        LoadTasks();
        _view.ShowMessage("✅ Task added");
    }

    private void OnCompleteTask(int id)
    {
        _service.CompleteTask(id);
        LoadTasks();
        _view.ShowMessage("✅ Task completed");
    }
}
```

### 2. Testing MVP

```csharp
// MOCK VIEW FOR TESTING
public class MockAccountView : IAccountView
{
    public string LastMessage { get; set; }
    public decimal LastBalance { get; set; }

    public void DisplayBalance(decimal balance) => LastBalance = balance;
    public void DisplayMessage(string message) => LastMessage = message;
}

// PRESENTER TEST
[TestFixture]
public class AccountPresenterTests
{
    private AccountPresenter _presenter;
    private MockAccountView _view;
    private BankAccount _account;

    [SetUp]
    public void Setup()
    {
        _account = new BankAccount();
        _view = new MockAccountView();
        _presenter = new AccountPresenter(_account, _view);
    }

    [Test]
    public void Deposit_ValidAmount_UpdatesViewBalance()
    {
        // Arrange
        decimal amount = 100m;

        // Act
        _presenter.Deposit(amount);

        // Assert
        Assert.AreEqual(100m, _view.LastBalance);
        Assert.That(_view.LastMessage, Does.Contain("successful"));
    }

    [Test]
    public void Deposit_InvalidAmount_ShowsError()
    {
        // Act
        _presenter.Deposit(-50m);

        // Assert
        Assert.That(_view.LastMessage, Does.Contain("Error"));
    }
}
```

---

## MVP vs MVC vs MVVM

| Aspect | MVP | MVC | MVVM |
|--------|-----|-----|------|
| **View** | Passive | Active | Passive |
| **Controller/Presenter** | Presenter controls View | Controller orchestrates | ViewModel handles binding |
| **Binding** | Manual | Manual | Automatic |
| **Testability** | Very high | Medium | High |
| **UI Framework** | Windows Forms, Android | Web, Desktop | WPF, UWP |

---

## When to Use MVP

### ✅ Use MVP When:
- Building testable UI applications
- View needs to be completely independent
- Manual control over View updates
- Legacy UI frameworks without binding
- Testing UI logic critical

### ❌ Don't Use When:
- Automatic data binding beneficial
- Simple applications
- Web applications (use MVC)

---

## Summary

MVP separates presentation through passive View and Presenter orchestration. Perfect for highly testable applications with complex UI logic. All UI logic in Presenter, View just displays data. Most testable of presentation patterns but requires manual updates.

**Key Takeaway:** MVP uses passive View with Presenter controlling all UI logic for maximum testability.
