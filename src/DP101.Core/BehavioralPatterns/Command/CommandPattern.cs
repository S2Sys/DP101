namespace DP101.Core.BehavioralPatterns.Command;

/// <summary>
/// COMMAND PATTERN
///
/// Intent: Encapsulate a request as an object, allowing you to parametrize clients
/// with different requests, queue requests, and support undoable operations.
///
/// Participants:
/// - Command: Interface for executing operations
/// - ConcreteCommand: Implements command interface
/// - Receiver: Performs actual work
/// - Invoker: Executes commands
/// - Client: Creates commands
///
/// PROS:
/// - Decouples sender from receiver
/// - Commands can be queued/scheduled
/// - Supports undo/redo
/// - Can create command sequences
/// - Easy to add new commands
///
/// CONS:
/// - Can result in many command classes
/// - Memory overhead for each command
/// - Extra layers of indirection
/// </summary>

// ========== EXAMPLE 1: LIGHT COMMAND ==========

// Receiver
public class Light
{
    public void On()
    {
        Console.WriteLine("Light is ON");
    }

    public void Off()
    {
        Console.WriteLine("Light is OFF");
    }
}

// Command interface
public interface ICommand
{
    void Execute();
    void Undo();
}

// Concrete commands
public class LightOnCommand : ICommand
{
    private Light _light;

    public LightOnCommand(Light light)
    {
        _light = light;
    }

    public void Execute()
    {
        _light.On();
    }

    public void Undo()
    {
        _light.Off();
    }
}

public class LightOffCommand : ICommand
{
    private Light _light;

    public LightOffCommand(Light light)
    {
        _light = light;
    }

    public void Execute()
    {
        _light.Off();
    }

    public void Undo()
    {
        _light.On();
    }
}

// Invoker
public class RemoteControl
{
    private Stack<ICommand> _commandHistory = [];

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

    public void UndoAll()
    {
        while (_commandHistory.Count > 0)
        {
            Undo();
        }
    }
}

// ========== EXAMPLE 2: DOCUMENT COMMANDS ==========

public class TextDocument
{
    private List<string> _content = [];

    public void AddText(string text)
    {
        _content.Add(text);
        Console.WriteLine($"Added: {text}");
    }

    public void RemoveText()
    {
        if (_content.Count > 0)
        {
            var removed = _content[_content.Count - 1];
            _content.RemoveAt(_content.Count - 1);
            Console.WriteLine($"Removed: {removed}");
        }
    }

    public void DisplayContent()
    {
        Console.WriteLine("Content: " + string.Join(", ", _content));
    }

    public List<string> GetContent() => new List<string>(_content);
}

public class AddTextCommand : ICommand
{
    private TextDocument _document;
    private string _text;

    public AddTextCommand(TextDocument document, string text)
    {
        _document = document;
        _text = text;
    }

    public void Execute()
    {
        _document.AddText(_text);
    }

    public void Undo()
    {
        _document.RemoveText();
    }
}

public class DocumentEditor
{
    private Stack<ICommand> _undoStack = [];
    private Stack<ICommand> _redoStack = [];

    public void ExecuteCommand(ICommand command)
    {
        command.Execute();
        _undoStack.Push(command);
        _redoStack.Clear(); // Clear redo stack when new command executed
    }

    public void Undo()
    {
        if (_undoStack.Count > 0)
        {
            var command = _undoStack.Pop();
            command.Undo();
            _redoStack.Push(command);
        }
    }

    public void Redo()
    {
        if (_redoStack.Count > 0)
        {
            var command = _redoStack.Pop();
            command.Execute();
            _undoStack.Push(command);
        }
    }
}

// ========== EXAMPLE 3: DATABASE COMMANDS ==========

public class Database
{
    private Dictionary<string, string> _data = [];

    public void Insert(string key, string value)
    {
        _data[key] = value;
        Console.WriteLine($"Inserted: {key} = {value}");
    }

    public void Delete(string key)
    {
        if (_data.ContainsKey(key))
        {
            _data.Remove(key);
            Console.WriteLine($"Deleted: {key}");
        }
    }

    public string Get(string key)
    {
        return _data.ContainsKey(key) ? _data[key] : "Not found";
    }

    public void DisplayAll()
    {
        foreach (var item in _data)
        {
            Console.WriteLine($"{item.Key} = {item.Value}");
        }
    }
}

public class DatabaseInsertCommand : ICommand
{
    private Database _database;
    private string _key;
    private string _value;
    private string? _oldValue;

    public DatabaseInsertCommand(Database database, string key, string value)
    {
        _database = database;
        _key = key;
        _value = value;
    }

    public void Execute()
    {
        _oldValue = _database.Get(_key);
        _database.Insert(_key, _value);
    }

    public void Undo()
    {
        _database.Delete(_key);
        if (_oldValue != null && _oldValue != "Not found")
        {
            _database.Insert(_key, _oldValue);
        }
    }
}

public class DatabaseDeleteCommand : ICommand
{
    private Database _database;
    private string _key;
    private string? _savedValue;

    public DatabaseDeleteCommand(Database database, string key)
    {
        _database = database;
        _key = key;
    }

    public void Execute()
    {
        _savedValue = _database.Get(_key);
        _database.Delete(_key);
    }

    public void Undo()
    {
        if (_savedValue != null && _savedValue != "Not found")
        {
            _database.Insert(_key, _savedValue);
        }
    }
}

// ========== EXAMPLE 4: BATCH COMMANDS ==========

public class MacroCommand : ICommand
{
    private List<ICommand> _commands = [];

    public void AddCommand(ICommand command)
    {
        _commands.Add(command);
    }

    public void Execute()
    {
        foreach (var command in _commands)
        {
            command.Execute();
        }
    }

    public void Undo()
    {
        for (int i = _commands.Count - 1; i >= 0; i--)
        {
            _commands[i].Undo();
        }
    }
}

// ========== EXAMPLE 5: SCHEDULED COMMANDS ==========

public class CommandQueue
{
    private Queue<ICommand> _queue = [];

    public void EnqueueCommand(ICommand command)
    {
        _queue.Enqueue(command);
        Console.WriteLine("Command queued");
    }

    public void ProcessAll()
    {
        while (_queue.Count > 0)
        {
            var command = _queue.Dequeue();
            command.Execute();
        }
    }

    public void ProcessNext()
    {
        if (_queue.Count > 0)
        {
            var command = _queue.Dequeue();
            command.Execute();
        }
    }

    public int GetQueueSize() => _queue.Count;
}

// ========== EXAMPLE 6: ASYNC COMMAND ==========

public interface IAsyncCommand
{
    Task ExecuteAsync();
    Task UndoAsync();
}

public class AsyncTextCommand : IAsyncCommand
{
    private TextDocument _document;
    private string _text;

    public AsyncTextCommand(TextDocument document, string text)
    {
        _document = document;
        _text = text;
    }

    public async Task ExecuteAsync()
    {
        await Task.Delay(100); // Simulate async work
        _document.AddText(_text);
    }

    public async Task UndoAsync()
    {
        await Task.Delay(100); // Simulate async work
        _document.RemoveText();
    }
}

public class AsyncCommandExecutor
{
    private Stack<IAsyncCommand> _history = [];

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
