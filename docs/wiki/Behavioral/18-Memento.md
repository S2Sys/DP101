# Memento Pattern

## Overview

**Category:** Behavioral Pattern  
**Purpose:** Capture an object's internal state without violating encapsulation, and allow restoration of that state later.  
**Also Called:** Token, Snapshot  
**Complexity:** Medium

## Problem

Need to save and restore object state without exposing internal structure:

```csharp
// Problem: State exposure for saving/restoring
public class TextEditor
{
    private string _content;

    // Exposing internal state!
    public string GetState() => _content;
    public void SetState(string state) => _content = state;
}

// Client must know internal structure to save/restore
```

## Solution

Create memento to capture state, caretaker to manage mementos:

```csharp
// Memento encapsulates state
public class TextEditorMemento
{
    public string Content { get; private set; }

    public TextEditorMemento(string content) => Content = content;
}

// Originator creates and restores from memento
public class TextEditor
{
    private string _content;

    public void SetContent(string content) => _content = content;

    public TextEditorMemento CreateMemento() => 
        new TextEditorMemento(_content);

    public void RestoreFromMemento(TextEditorMemento memento) =>
        _content = memento.Content;
}

// Caretaker manages memento history
public class History
{
    private Stack<TextEditorMemento> _history = new();

    public void SaveState(TextEditor editor) =>
        _history.Push(editor.CreateMemento());

    public void Undo(TextEditor editor)
    {
        if (_history.Count > 0)
            editor.RestoreFromMemento(_history.Pop());
    }
}
```

## Implementation Approaches

### 1. Text Editor with Undo

```csharp
public class EditorMemento
{
    public string Content { get; private set; }
    public DateTime SaveTime { get; private set; }

    public EditorMemento(string content)
    {
        Content = content;
        SaveTime = DateTime.Now;
    }
}

public class TextEditorWithHistory
{
    private string _content = "";

    public void Type(string text)
    {
        _content += text;
        Console.WriteLine($"📝 Content: {_content}");
    }

    public EditorMemento CreateMemento()
    {
        Console.WriteLine($"💾 Saving state: '{_content}'");
        return new EditorMemento(_content);
    }

    public void RestoreFromMemento(EditorMemento memento)
    {
        _content = memento.Content;
        Console.WriteLine($"⏮ Restored to: '{_content}'");
    }

    public string GetContent() => _content;
}

public class EditorHistory
{
    private Stack<EditorMemento> _undoStack = new();
    private Stack<EditorMemento> _redoStack = new();

    public void SaveState(TextEditorWithHistory editor)
    {
        _undoStack.Push(editor.CreateMemento());
        _redoStack.Clear();  // Clear redo on new edit
    }

    public void Undo(TextEditorWithHistory editor)
    {
        if (_undoStack.Count > 0)
        {
            var memento = _undoStack.Pop();
            _redoStack.Push(new EditorMemento(editor.GetContent()));
            editor.RestoreFromMemento(memento);
        }
    }

    public void Redo(TextEditorWithHistory editor)
    {
        if (_redoStack.Count > 0)
        {
            var memento = _redoStack.Pop();
            _undoStack.Push(new EditorMemento(editor.GetContent()));
            editor.RestoreFromMemento(memento);
        }
    }
}

// Usage
var editor = new TextEditorWithHistory();
var history = new EditorHistory();

editor.Type("Hello");
history.SaveState(editor);

editor.Type(" World");
history.SaveState(editor);

editor.Type("!");
history.SaveState(editor);

Console.WriteLine($"Content: {editor.GetContent()}");  // Hello World!

history.Undo(editor);
Console.WriteLine($"Content: {editor.GetContent()}");  // Hello World

history.Undo(editor);
Console.WriteLine($"Content: {editor.GetContent()}");  // Hello

history.Redo(editor);
Console.WriteLine($"Content: {editor.GetContent()}");  // Hello World
```

### 2. Game State Memento

```csharp
public class GameState
{
    public int Level { get; set; }
    public int Score { get; set; }
    public string Position { get; set; }
    public List<string> Inventory { get; set; }

    public GameState(int level, int score, string position, List<string> inventory)
    {
        Level = level;
        Score = score;
        Position = position;
        Inventory = new List<string>(inventory);
    }
}

public class Game
{
    public int Level { get; set; }
    public int Score { get; set; }
    public string PlayerPosition { get; set; }
    public List<string> Inventory { get; set; }

    public Game()
    {
        Level = 1;
        Score = 0;
        PlayerPosition = "Start";
        Inventory = new List<string>();
    }

    public GameState CreateSavePoint()
    {
        Console.WriteLine($"💾 Game saved at Level {Level}");
        return new GameState(Level, Score, PlayerPosition, Inventory);
    }

    public void RestoreSavePoint(GameState state)
    {
        Level = state.Level;
        Score = state.Score;
        PlayerPosition = state.Position;
        Inventory = new List<string>(state.Inventory);
        Console.WriteLine($"⏮ Game restored to Level {Level}");
    }

    public void Play()
    {
        Level++;
        Score += 100;
        PlayerPosition = "Dungeon";
        Inventory.Add("Sword");
        Console.WriteLine($"🎮 Playing... Level: {Level}, Score: {Score}");
    }

    public void DisplayStatus()
    {
        Console.WriteLine($"📊 Level: {Level}, Score: {Score}, Position: {PlayerPosition}");
        Console.WriteLine($"🎒 Inventory: {string.Join(", ", Inventory)}");
    }
}

public class GameSaveManager
{
    private Dictionary<string, GameState> _saves = new();

    public void SaveGame(string slotName, Game game)
    {
        _saves[slotName] = game.CreateSavePoint();
        Console.WriteLine($"✅ Saved to slot: {slotName}");
    }

    public void LoadGame(string slotName, Game game)
    {
        if (_saves.ContainsKey(slotName))
        {
            game.RestoreSavePoint(_saves[slotName]);
            Console.WriteLine($"✅ Loaded from slot: {slotName}");
        }
        else
        {
            Console.WriteLine($"❌ Slot not found: {slotName}");
        }
    }

    public void ListSaves()
    {
        Console.WriteLine("💾 Saved games:");
        foreach (var slot in _saves.Keys)
            Console.WriteLine($"  - {slot}");
    }
}

// Usage
var game = new Game();
var saveManager = new GameSaveManager();

game.Play();
game.DisplayStatus();
saveManager.SaveGame("Slot1", game);

game.Play();
game.DisplayStatus();
saveManager.SaveGame("Slot2", game);

saveManager.LoadGame("Slot1", game);
game.DisplayStatus();
```

### 3. Database Transaction Memento

```csharp
public class DatabaseMemento
{
    public Dictionary<string, string> Data { get; private set; }

    public DatabaseMemento(Dictionary<string, string> data)
    {
        Data = new Dictionary<string, string>(data);
    }
}

public class Database
{
    private Dictionary<string, string> _data = new();

    public void Set(string key, string value)
    {
        _data[key] = value;
        Console.WriteLine($"📝 Set {key} = {value}");
    }

    public string Get(string key) => _data.ContainsKey(key) ? _data[key] : "Not found";

    public DatabaseMemento CreateSnapshot()
    {
        Console.WriteLine("💾 Creating database snapshot");
        return new DatabaseMemento(_data);
    }

    public void RestoreSnapshot(DatabaseMemento memento)
    {
        _data = new Dictionary<string, string>(memento.Data);
        Console.WriteLine("⏮ Database restored from snapshot");
    }

    public void DisplayData()
    {
        Console.WriteLine("📊 Database contents:");
        foreach (var kvp in _data)
            Console.WriteLine($"  {kvp.Key} = {kvp.Value}");
    }
}

public class Transaction
{
    private Stack<DatabaseMemento> _snapshots = new();
    private Database _database;

    public Transaction(Database database) => _database = database;

    public void Begin()
    {
        _snapshots.Push(_database.CreateSnapshot());
        Console.WriteLine("🔄 Transaction started");
    }

    public void Commit()
    {
        Console.WriteLine("✅ Transaction committed");
    }

    public void Rollback()
    {
        if (_snapshots.Count > 0)
        {
            var snapshot = _snapshots.Pop();
            _database.RestoreSnapshot(snapshot);
            Console.WriteLine("⏮ Transaction rolled back");
        }
    }
}

// Usage
var db = new Database();
var transaction = new Transaction(db);

transaction.Begin();
db.Set("Name", "Alice");
db.Set("Age", "30");
transaction.Commit();

transaction.Begin();
db.Set("Name", "Bob");
db.DisplayData();
transaction.Rollback();

db.DisplayData();  // Back to Alice, 30
```

### 4. Configuration Memento

```csharp
public class ConfigMemento
{
    public string Version { get; private set; }
    public DateTime SaveTime { get; private set; }
    public Dictionary<string, object> Settings { get; private set; }

    public ConfigMemento(string version, Dictionary<string, object> settings)
    {
        Version = version;
        SaveTime = DateTime.Now;
        Settings = new Dictionary<string, object>(settings);
    }
}

public class ApplicationConfiguration
{
    public string Version { get; set; }
    public Dictionary<string, object> Settings { get; set; }

    public ApplicationConfiguration()
    {
        Version = "1.0";
        Settings = new Dictionary<string, object>();
    }

    public void UpdateSetting(string key, object value)
    {
        Settings[key] = value;
        Console.WriteLine($"⚙️ Updated {key} = {value}");
    }

    public ConfigMemento CreateSnapshot()
    {
        Console.WriteLine($"💾 Saving config version {Version}");
        return new ConfigMemento(Version, Settings);
    }

    public void RestoreSnapshot(ConfigMemento memento)
    {
        Version = memento.Version;
        Settings = new Dictionary<string, object>(memento.Settings);
        Console.WriteLine($"⏮ Restored config version {Version}");
    }

    public void DisplaySettings()
    {
        Console.WriteLine($"⚙️ Configuration v{Version}:");
        foreach (var kvp in Settings)
            Console.WriteLine($"  {kvp.Key} = {kvp.Value}");
    }
}

public class ConfigurationHistory
{
    private List<ConfigMemento> _history = new();

    public void Save(ApplicationConfiguration config)
    {
        _history.Add(config.CreateSnapshot());
    }

    public void RestoreVersion(int index, ApplicationConfiguration config)
    {
        if (index >= 0 && index < _history.Count)
        {
            config.RestoreSnapshot(_history[index]);
        }
    }

    public void ListVersions()
    {
        Console.WriteLine("📜 Configuration history:");
        for (int i = 0; i < _history.Count; i++)
        {
            var memento = _history[i];
            Console.WriteLine($"  v{i}: {memento.Version} - {memento.SaveTime}");
        }
    }
}

// Usage
var config = new ApplicationConfiguration();
var history = new ConfigurationHistory();

config.UpdateSetting("Theme", "Dark");
config.UpdateSetting("FontSize", 12);
history.Save(config);

config.UpdateSetting("Theme", "Light");
config.UpdateSetting("FontSize", 14);
history.Save(config);

config.DisplaySettings();

history.RestoreVersion(0, config);
config.DisplaySettings();
```

### 5. Drawing Canvas Memento

```csharp
public class CanvasMemento
{
    public List<Shape> Shapes { get; private set; }

    public CanvasMemento(List<Shape> shapes)
    {
        Shapes = new List<Shape>(shapes);
    }
}

public class Shape
{
    public string Name { get; set; }
    public string Color { get; set; }

    public Shape(string name, string color)
    {
        Name = name;
        Color = color;
    }

    public override string ToString() => $"{Name} ({Color})";
}

public class DrawingCanvas
{
    private List<Shape> _shapes = new();

    public void AddShape(Shape shape)
    {
        _shapes.Add(shape);
        Console.WriteLine($"🎨 Added {shape}");
    }

    public void RemoveShape(Shape shape)
    {
        _shapes.Remove(shape);
        Console.WriteLine($"🎨 Removed {shape}");
    }

    public CanvasMemento CreateSnapshot()
    {
        Console.WriteLine("💾 Saving canvas");
        return new CanvasMemento(_shapes);
    }

    public void RestoreSnapshot(CanvasMemento memento)
    {
        _shapes = new List<Shape>(memento.Shapes);
        Console.WriteLine("⏮ Canvas restored");
    }

    public void DisplayShapes()
    {
        Console.WriteLine("🖼️ Canvas contains:");
        foreach (var shape in _shapes)
            Console.WriteLine($"  - {shape}");
    }
}

public class DrawingHistory
{
    private Stack<CanvasMemento> _undoStack = new();
    private Stack<CanvasMemento> _redoStack = new();

    public void SaveState(DrawingCanvas canvas)
    {
        _undoStack.Push(canvas.CreateSnapshot());
        _redoStack.Clear();
    }

    public void Undo(DrawingCanvas canvas)
    {
        if (_undoStack.Count > 0)
        {
            _redoStack.Push(canvas.CreateSnapshot());
            canvas.RestoreSnapshot(_undoStack.Pop());
        }
    }

    public void Redo(DrawingCanvas canvas)
    {
        if (_redoStack.Count > 0)
        {
            _undoStack.Push(canvas.CreateSnapshot());
            canvas.RestoreSnapshot(_redoStack.Pop());
        }
    }
}

// Usage
var canvas = new DrawingCanvas();
var history = new DrawingHistory();

canvas.AddShape(new Shape("Circle", "Red"));
history.SaveState(canvas);

canvas.AddShape(new Shape("Square", "Blue"));
history.SaveState(canvas);

canvas.DisplayShapes();

history.Undo(canvas);
canvas.DisplayShapes();
```

---

## Memento Pattern Structure

```
    Originator              Caretaker           Memento
  (TextEditor)        (TextEditorHistory)   (EditorMemento)
  - state              - mementos stack      - state
  - CreateMemento()    - SaveState()         (encapsulated)
  - RestoreMemento()   - Undo()
                       - Redo()
```

---

## Pros and Cons

### Advantages
✅ **Encapsulation** - State exposed only through memento  
✅ **Undo/Redo** - Easy to implement history  
✅ **State Preservation** - Capture state at any point  
✅ **Simplicity** - Clean separation of concerns  
✅ **Non-Invasive** - Doesn't require special methods in originator  

### Disadvantages
❌ **Memory Usage** - Storing multiple states uses memory  
❌ **Performance** - Creating mementos can be slow  
❌ **Serialization** - Complex objects hard to serialize  
❌ **Maintenance** - Must maintain memento if originator changes  

---

## When to Use

### ✅ Use Memento When:
- Need undo/redo functionality
- Must save and restore state
- State snapshots needed
- Don't want to expose internal state
- Transaction rollback required
- Game save/load functionality

### ❌ Don't Use When:
- Stateless objects
- State very large
- Performance critical
- Simple setters sufficient
- Simplicity valued

---

## Interview Questions

**Q: How is Memento different from Command pattern?**
A: Memento captures object state; Command encapsulates request. Memento for state restoration; Command for request execution.

**Q: What about large state objects?**
A: Use references instead of copies, or implement custom serialization. Consider Proxy pattern for lazy restoration.

**Q: Can mementos be public?**
A: Yes, but usually private (package-private in Java). Protect memento from external modification.

**Q: How to handle complex objects in memento?**
A: Deep copy state, or use weak references. Consider Prototype pattern for complex cloning.

---

## Related Patterns

| Pattern | Relation |
|---------|----------|
| **Command** | Both support undo; Command for actions, Memento for state |
| **Prototype** | Both copy state; Prototype for cloning, Memento for history |
| **Caretaker** | Often used with Memento for history management |

---

## Summary

Memento pattern captures and restores object state without exposing internal structure. Perfect for undo/redo, game saves, transaction rollbacks, and configuration versioning. Originator creates memento, caretaker manages history, memento encapsulates state.

**Key Takeaway:** Memento captures state for later restoration without exposing internals.
