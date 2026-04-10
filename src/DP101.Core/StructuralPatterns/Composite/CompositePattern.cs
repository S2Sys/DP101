namespace DP101.Core.StructuralPatterns.Composite;

/// <summary>
/// COMPOSITE PATTERN
///
/// Intent: Compose objects into tree structures to represent part-whole hierarchies.
/// Allows clients to treat individual objects and compositions uniformly.
///
/// PROS:
/// - Simplifies client code
/// - Works with tree structures naturally
/// - New component types easy to add
/// - Single Responsibility Principle
///
/// CONS:
/// - Overly general design
/// - Type checking becomes difficult
/// - Can hide specific types
/// </summary>

// ========== EXAMPLE 1: FILE SYSTEM ==========

// Component interface
public abstract class FileSystemComponent
{
    public string Name { get; protected set; }

    public abstract void Display(int indentLevel = 0);
    public abstract long GetSize();
}

// Leaf
public class File : FileSystemComponent
{
    public long Size { get; }

    public File(string name, long size)
    {
        Name = name;
        Size = size;
    }

    public override void Display(int indentLevel = 0)
    {
        Console.WriteLine(new string(' ', indentLevel) + $"File: {Name} ({Size} bytes)");
    }

    public override long GetSize() => Size;
}

// Composite
public class Directory : FileSystemComponent
{
    private readonly List<FileSystemComponent> _children = [];

    public Directory(string name)
    {
        Name = name;
    }

    public void Add(FileSystemComponent component)
    {
        _children.Add(component);
    }

    public void Remove(FileSystemComponent component)
    {
        _children.Remove(component);
    }

    public override void Display(int indentLevel = 0)
    {
        Console.WriteLine(new string(' ', indentLevel) + $"Directory: {Name}/");
        foreach (var child in _children)
        {
            child.Display(indentLevel + 2);
        }
    }

    public override long GetSize()
    {
        return _children.Sum(c => c.GetSize());
    }
}

// ========== EXAMPLE 2: ORGANIZATION STRUCTURE ==========

public abstract class OrganizationComponent
{
    public string Name { get; protected set; }

    public abstract void ShowStructure(int indentLevel = 0);
    public abstract int GetEmployeeCount();
}

public class Employee : OrganizationComponent
{
    public string Position { get; }

    public Employee(string name, string position)
    {
        Name = name;
        Position = position;
    }

    public override void ShowStructure(int indentLevel = 0)
    {
        Console.WriteLine(new string(' ', indentLevel) + $"Employee: {Name} ({Position})");
    }

    public override int GetEmployeeCount() => 1;
}

public class Department : OrganizationComponent
{
    private readonly List<OrganizationComponent> _members = [];

    public Department(string name)
    {
        Name = name;
    }

    public void Add(OrganizationComponent member)
    {
        _members.Add(member);
    }

    public void Remove(OrganizationComponent member)
    {
        _members.Remove(member);
    }

    public override void ShowStructure(int indentLevel = 0)
    {
        Console.WriteLine(new string(' ', indentLevel) + $"Department: {Name}");
        foreach (var member in _members)
        {
            member.ShowStructure(indentLevel + 2);
        }
    }

    public override int GetEmployeeCount()
    {
        return _members.Sum(m => m.GetEmployeeCount());
    }
}

// ========== EXAMPLE 3: MENU SYSTEM ==========

public abstract class MenuItem
{
    public string Name { get; protected set; }

    public abstract void Display();
    public abstract void Execute();
}

public class MenuItem_Item : MenuItem
{
    public MenuItem_Item(string name)
    {
        Name = name;
    }

    public override void Display() => Console.WriteLine(Name);
    public override void Execute() => Console.WriteLine($"Executing {Name}");
}

public class MenuItem_Menu : MenuItem
{
    private readonly List<MenuItem> _items = [];

    public MenuItem_Menu(string name)
    {
        Name = name;
    }

    public void Add(MenuItem item) => _items.Add(item);
    public void Remove(MenuItem item) => _items.Remove(item);

    public override void Display()
    {
        Console.WriteLine($"Menu: {Name}");
        foreach (var item in _items)
        {
            Console.Write("  ");
            item.Display();
        }
    }

    public override void Execute()
    {
        Console.WriteLine($"Opening menu: {Name}");
        foreach (var item in _items)
        {
            item.Execute();
        }
    }
}

// ========== EXAMPLE 4: GRAPHICS COMPONENT ==========

public abstract class GraphicsComponent
{
    public string Name { get; protected set; }

    public abstract void Draw();
    public abstract void Move(int dx, int dy);
}

public class Shape : GraphicsComponent
{
    public Shape(string name)
    {
        Name = name;
    }

    public override void Draw() => Console.WriteLine($"Drawing shape: {Name}");
    public override void Move(int dx, int dy) => Console.WriteLine($"Moving {Name} by ({dx}, {dy})");
}

public class Group : GraphicsComponent
{
    private readonly List<GraphicsComponent> _components = [];

    public Group(string name)
    {
        Name = name;
    }

    public void Add(GraphicsComponent component) => _components.Add(component);
    public void Remove(GraphicsComponent component) => _components.Remove(component);

    public override void Draw()
    {
        Console.WriteLine($"Drawing group: {Name}");
        foreach (var component in _components)
        {
            component.Draw();
        }
    }

    public override void Move(int dx, int dy)
    {
        Console.WriteLine($"Moving group: {Name} by ({dx}, {dy})");
        foreach (var component in _components)
        {
            component.Move(dx, dy);
        }
    }
}

// ========== EXAMPLE 5: COMMENT TREE ==========

public class Comment
{
    public string Author { get; set; }
    public string Text { get; set; }
    private readonly List<Comment> _replies = [];

    public Comment(string author, string text)
    {
        Author = author;
        Text = text;
    }

    public void AddReply(Comment reply) => _replies.Add(reply);
    public void RemoveReply(Comment reply) => _replies.Remove(reply);

    public void Display(int indentLevel = 0)
    {
        Console.WriteLine(new string(' ', indentLevel) + $"{Author}: {Text}");
        foreach (var reply in _replies)
        {
            reply.Display(indentLevel + 2);
        }
    }

    public int GetCommentCount()
    {
        return 1 + _replies.Sum(r => r.GetCommentCount());
    }
}

// ========== EXAMPLE 6: TASK HIERARCHY ==========

public abstract class Task
{
    public string Title { get; protected set; }

    public abstract void Display(int indentLevel = 0);
    public abstract void MarkComplete();
    public abstract int GetRemainingTasks();
}

public class SimpleTask : Task
{
    public bool IsComplete { get; private set; }

    public SimpleTask(string title)
    {
        Title = title;
        IsComplete = false;
    }

    public override void Display(int indentLevel = 0)
    {
        var status = IsComplete ? "[✓]" : "[ ]";
        Console.WriteLine(new string(' ', indentLevel) + $"{status} {Title}");
    }

    public override void MarkComplete() => IsComplete = true;
    public override int GetRemainingTasks() => IsComplete ? 0 : 1;
}

public class CompositeTask : Task
{
    private readonly List<Task> _subtasks = [];

    public CompositeTask(string title)
    {
        Title = title;
    }

    public void AddSubtask(Task task) => _subtasks.Add(task);
    public void RemoveSubtask(Task task) => _subtasks.Remove(task);

    public override void Display(int indentLevel = 0)
    {
        Console.WriteLine(new string(' ', indentLevel) + $"Task Group: {Title}");
        foreach (var task in _subtasks)
        {
            task.Display(indentLevel + 2);
        }
    }

    public override void MarkComplete()
    {
        foreach (var task in _subtasks)
        {
            task.MarkComplete();
        }
    }

    public override int GetRemainingTasks()
    {
        return _subtasks.Sum(t => t.GetRemainingTasks());
    }
}
