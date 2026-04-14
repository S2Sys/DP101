# Command Pattern

## Overview

**Category:** Behavioral Pattern  
**Purpose:** Encapsulate a request as an object, allowing parameterization of clients with different requests, queuing of requests, and logging of requests.  
**Also Called:** Action, Transaction  
**Complexity:** Medium

## Problem

Need to encapsulate requests as objects for queuing, logging, and undo/redo:

```csharp
// Problem: Tightly coupled requests to invocation
public class Light
{
    public void TurnOn() => Console.WriteLine("Light on");
    public void TurnOff() => Console.WriteLine("Light off");
}

public class Button
{
    private Light _light;

    public Button(Light light) => _light = light;

    public void Click() => _light.TurnOn();  // Tightly coupled!
}

// Cannot queue, undo, log, or reuse requests
```

## Solution

Encapsulate request as object with common interface:

```csharp
public interface ICommand
{
    void Execute();
    void Undo();
}

public class TurnOnCommand : ICommand
{
    private Light _light;

    public TurnOnCommand(Light light) => _light = light;

    public void Execute() => _light.TurnOn();
    public void Undo() => _light.TurnOff();
}

public class Button
{
    private ICommand _command;

    public void SetCommand(ICommand command) => _command = command;
    public void Click() => _command?.Execute();
}

// Now can queue, log, undo, replay, and reuse!
```

## Implementation Approaches

### 1. Light Control Commands

```csharp
public interface ICommand
{
    void Execute();
    void Undo();
}

public class Light
{
    public void TurnOn() => Console.WriteLine("💡 Light turned on");
    public void TurnOff() => Console.WriteLine("💡 Light turned off");
    public void Dim(int level) => Console.WriteLine($"💡 Light dimmed to {level}%");
}

public class TurnOnCommand : ICommand
{
    private Light _light;

    public TurnOnCommand(Light light) => _light = light;

    public void Execute() => _light.TurnOn();
    public void Undo() => _light.TurnOff();
}

public class TurnOffCommand : ICommand
{
    private Light _light;

    public TurnOffCommand(Light light) => _light = light;

    public void Execute() => _light.TurnOff();
    public void Undo() => _light.TurnOn();
}

public class DimCommand : ICommand
{
    private Light _light;
    private int _level;
    private int _previousLevel = 100;

    public DimCommand(Light light, int level)
    {
        _light = light;
        _level = level;
    }

    public void Execute()
    {
        _previousLevel = 100;  // Store previous
        _light.Dim(_level);
    }

    public void Undo() => _light.Dim(_previousLevel);
}

public class RemoteControl
{
    private Stack<ICommand> _commandHistory = new();

    public void ExecuteCommand(ICommand command)
    {
        command.Execute();
        _commandHistory.Push(command);
    }

    public void Undo()
    {
        if (_commandHistory.Count > 0)
        {
            var command = _commandHistory.Pop();
            command.Undo();
        }
    }
}

// Usage
var light = new Light();
var remote = new RemoteControl();

remote.ExecuteCommand(new TurnOnCommand(light));   // Light on
remote.ExecuteCommand(new DimCommand(light, 50));   // Dim to 50%
remote.Undo();                                      // Back to on
remote.Undo();                                      // Light off
```

### 2. Document Commands

```csharp
public interface ICommand
{
    void Execute();
    void Undo();
}

public class Document
{
    private string _content = "";

    public void Append(string text) => _content += text;
    public void Clear() => _content = "";
    public void Print() => Console.WriteLine($"📄 Content: {_content}");
}

public class AppendTextCommand : ICommand
{
    private Document _doc;
    private string _text;

    public AppendTextCommand(Document doc, string text)
    {
        _doc = doc;
        _text = text;
    }

    public void Execute() => _doc.Append(_text);
    public void Undo()
    {
        _doc.Clear();
        // In real implementation, track all appends
    }
}

public class ClearDocumentCommand : ICommand
{
    private Document _doc;
    private string _backup = "";

    public void Execute()
    {
        _backup = _doc.ToString();  // Save before clear
        _doc.Clear();
    }

    public void Undo()
    {
        _doc.Append(_backup);  // Restore from backup
    }
}

public class DocumentEditor
{
    private Stack<ICommand> _undo = new();
    private Stack<ICommand> _redo = new();

    public void Execute(ICommand command)
    {
        command.Execute();
        _undo.Push(command);
        _redo.Clear();  // Clear redo stack on new command
    }

    public void Undo()
    {
        if (_undo.Count > 0)
        {
            var command = _undo.Pop();
            command.Undo();
            _redo.Push(command);
        }
    }

    public void Redo()
    {
        if (_redo.Count > 0)
        {
            var command = _redo.Pop();
            command.Execute();
            _undo.Push(command);
        }
    }
}

// Usage
var doc = new Document();
var editor = new DocumentEditor();

editor.Execute(new AppendTextCommand(doc, "Hello "));
editor.Execute(new AppendTextCommand(doc, "World"));
doc.Print();  // Hello World
editor.Undo();
doc.Print();  // Hello
editor.Redo();
doc.Print();  // Hello World
```

### 3. Database Commands

```csharp
public interface ICommand
{
    void Execute();
    void Undo();
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
}

public class UserRepository
{
    private List<User> _users = new();

    public void Add(User user)
    {
        _users.Add(user);
        Console.WriteLine($"Added user: {user.Name}");
    }

    public void Remove(User user)
    {
        _users.Remove(user);
        Console.WriteLine($"Removed user: {user.Name}");
    }

    public void Update(User user)
    {
        Console.WriteLine($"Updated user: {user.Name}");
    }
}

public class CreateUserCommand : ICommand
{
    private UserRepository _repo;
    private User _user;

    public CreateUserCommand(UserRepository repo, User user)
    {
        _repo = repo;
        _user = user;
    }

    public void Execute() => _repo.Add(_user);
    public void Undo() => _repo.Remove(_user);
}

public class DeleteUserCommand : ICommand
{
    private UserRepository _repo;
    private User _user;

    public DeleteUserCommand(UserRepository repo, User user)
    {
        _repo = repo;
        _user = user;
    }

    public void Execute() => _repo.Remove(_user);
    public void Undo() => _repo.Add(_user);
}

public class Transaction
{
    private List<ICommand> _commands = new();

    public void AddCommand(ICommand command) => _commands.Add(command);

    public void Execute()
    {
        foreach (var command in _commands)
            command.Execute();
        Console.WriteLine("✅ Transaction completed");
    }

    public void Rollback()
    {
        for (int i = _commands.Count - 1; i >= 0; i--)
            _commands[i].Undo();
        Console.WriteLine("⏮ Transaction rolled back");
    }
}

// Usage
var repo = new UserRepository();
var transaction = new Transaction();

transaction.AddCommand(new CreateUserCommand(repo, new User { Id = 1, Name = "Alice" }));
transaction.AddCommand(new CreateUserCommand(repo, new User { Id = 2, Name = "Bob" }));
transaction.Execute();
transaction.Rollback();  // All users removed
```

### 4. Macro/Batch Commands

```csharp
public interface ICommand
{
    void Execute();
    void Undo();
}

public class MacroCommand : ICommand
{
    private List<ICommand> _commands = new();

    public void Add(ICommand command) => _commands.Add(command);
    public void Remove(ICommand command) => _commands.Remove(command);

    public void Execute()
    {
        Console.WriteLine("▶ Executing macro...");
        foreach (var command in _commands)
            command.Execute();
    }

    public void Undo()
    {
        Console.WriteLine("⏮ Undoing macro...");
        for (int i = _commands.Count - 1; i >= 0; i--)
            _commands[i].Undo();
    }
}

public class Light
{
    public void On() => Console.WriteLine("Light ON");
    public void Off() => Console.WriteLine("Light OFF");
}

public class Door
{
    public void Lock() => Console.WriteLine("Door LOCKED");
    public void Unlock() => Console.WriteLine("Door UNLOCKED");
}

public class TV
{
    public void On() => Console.WriteLine("TV ON");
    public void Off() => Console.WriteLine("TV OFF");
}

public class TurnOnCommand : ICommand
{
    private Light _light;
    public TurnOnCommand(Light light) => _light = light;
    public void Execute() => _light.On();
    public void Undo() => _light.Off();
}

public class LockCommand : ICommand
{
    private Door _door;
    public LockCommand(Door door) => _door = door;
    public void Execute() => _door.Lock();
    public void Undo() => _door.Unlock();
}

public class TurnOnTVCommand : ICommand
{
    private TV _tv;
    public TurnOnTVCommand(TV tv) => _tv = tv;
    public void Execute() => _tv.On();
    public void Undo() => _tv.Off();
}

// Usage - "Goodnight" macro
var goodnight = new MacroCommand();
goodnight.Add(new TurnOnCommand(new Light()));
goodnight.Add(new LockCommand(new Door()));
goodnight.Add(new TurnOnTVCommand(new TV()));

goodnight.Execute();  // Executes all commands
// Output: Light ON, Door LOCKED, TV ON

goodnight.Undo();     // Undoes all in reverse
// Output: TV OFF, Door UNLOCKED, Light OFF
```

### 5. Command Queue

```csharp
public interface ICommand
{
    void Execute();
}

public class Task
{
    public string Name { get; set; }
    public Task(string name) => Name = name;
}

public class TaskExecuteCommand : ICommand
{
    private Task _task;

    public TaskExecuteCommand(Task task) => _task = task;

    public void Execute() => Console.WriteLine($"✓ Executing task: {_task.Name}");
}

public class CommandQueue
{
    private Queue<ICommand> _queue = new();

    public void Enqueue(ICommand command)
    {
        _queue.Enqueue(command);
        Console.WriteLine("📝 Command queued");
    }

    public void ProcessQueue()
    {
        Console.WriteLine("⏳ Processing queue...");
        while (_queue.Count > 0)
        {
            var command = _queue.Dequeue();
            command.Execute();
        }
        Console.WriteLine("✅ Queue processed");
    }

    public int PendingCount => _queue.Count;
}

// Usage
var queue = new CommandQueue();
queue.Enqueue(new TaskExecuteCommand(new Task("Task 1")));
queue.Enqueue(new TaskExecuteCommand(new Task("Task 2")));
queue.Enqueue(new TaskExecuteCommand(new Task("Task 3")));

queue.ProcessQueue();
// Output:
// Processing queue...
// ✓ Executing task: Task 1
// ✓ Executing task: Task 2
// ✓ Executing task: Task 3
// ✅ Queue processed
```

### 6. Async Commands

```csharp
public interface IAsyncCommand
{
    Task ExecuteAsync();
    Task UndoAsync();
}

public class EmailService
{
    public async Task SendAsync(string to, string message)
    {
        await Task.Delay(100);  // Simulate async work
        Console.WriteLine($"📧 Email sent to {to}: {message}");
    }
}

public class SendEmailCommand : IAsyncCommand
{
    private EmailService _service;
    private string _to;
    private string _message;

    public SendEmailCommand(EmailService service, string to, string message)
    {
        _service = service;
        _to = to;
        _message = message;
    }

    public async Task ExecuteAsync()
    {
        await _service.SendAsync(_to, _message);
    }

    public async Task UndoAsync()
    {
        Console.WriteLine("⏮ Email retraction not supported");
        await Task.CompletedTask;
    }
}

public class AsyncCommandExecutor
{
    private Stack<IAsyncCommand> _history = new();

    public async Task ExecuteAsync(IAsyncCommand command)
    {
        await command.ExecuteAsync();
        _history.Push(command);
    }

    public async Task UndoAsync()
    {
        if (_history.Count > 0)
        {
            var command = _history.Pop();
            await command.UndoAsync();
        }
    }
}

// Usage
var service = new EmailService();
var executor = new AsyncCommandExecutor();

await executor.ExecuteAsync(new SendEmailCommand(service, "user@example.com", "Hello"));
await executor.ExecuteAsync(new SendEmailCommand(service, "admin@example.com", "Report"));
await executor.UndoAsync();
```

---

## Command Pattern Structure

```
       Invoker
         |
         | uses
         v
     ICommand
      /  |  \
     /   |   \
Cmd1  Cmd2  Cmd3
   |
   | target
   v
 Receiver
```

---

## Pros and Cons

### Advantages
✅ **Decouples Sender/Receiver** - Invoker doesn't know about receiver  
✅ **Undo/Redo** - Store command history easily  
✅ **Queuing** - Queue commands for later execution  
✅ **Logging** - Log command history  
✅ **Macro Commands** - Composite commands together  
✅ **Async Execution** - Execute commands asynchronously  

### Disadvantages
❌ **More Classes** - One per command  
❌ **Overhead** - Extra objects for simple operations  
❌ **Memory** - Storing command history uses memory  
❌ **Complexity** - May be overkill for simple cases  

---

## When to Use

### ✅ Use Command When:
- Need undo/redo functionality
- Want to queue operations
- Need logging/auditing of operations
- Parameterize objects with operations
- Need to delay command execution
- Build macro/composite commands
- Support transaction rollback

### ❌ Don't Use When:
- Simple operations
- No undo needed
- Tight coupling acceptable
- Performance critical
- Simplicity valued

---

## Real-World Examples

### Text Editors
```csharp
// Undo/Redo support
editor.Execute(new TypeCommand("Hello"));
editor.Execute(new DeleteCommand());
editor.Undo();  // Restores "Hello"
```

### Remote Controls
```csharp
// Execute commands on different devices
remote.SetCommand(new LightCommand());
remote.SetCommand(new TVCommand());
remote.Click();
```

### Job Scheduling
```csharp
// Queue jobs for execution
scheduler.Enqueue(new BackupCommand());
scheduler.Enqueue(new CleanupCommand());
scheduler.ProcessJobs();
```

---

## Interview Questions

**Q: How is Command different from Strategy?**
A: Command encapsulates a request; Strategy encapsulates algorithm choice. Command is "what to do"; Strategy is "how to do it".

**Q: How do you implement Undo/Redo?**
A: Maintain stacks of executed and undone commands. Execute pushes to undo stack; Undo pops and executes undo.

**Q: Can commands be composed?**
A: Yes, MacroCommand can hold multiple commands. Execute all; Undo reverses in order.

**Q: What about asynchronous commands?**
A: Use async/await in Execute/Undo methods. Executor awaits command completion.

---

## Related Patterns

| Pattern | Relation |
|---------|----------|
| **Macro** | Multiple commands composed together |
| **Template Method** | Fixed algorithm steps; Command parameterizes steps |
| **Callback** | Command is callback object |
| **Memento** | Command + Memento = undo with state restoration |
| **Transaction** | Commands in transaction with rollback |

---

## Summary

Command pattern encapsulates requests as objects, enabling undo/redo, queuing, logging, and macro operations. Perfect for editors, remote controls, job schedulers, or any scenario requiring operation parameterization and control.

**Key Takeaway:** Command encapsulates a request as an object, decoupling sender from receiver.
