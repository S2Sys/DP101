# Composite Pattern

## Overview

**Category:** Structural Pattern  
**Purpose:** Compose objects into tree structures to represent part-whole hierarchies. Allows clients to treat individual objects and compositions uniformly.  
**Complexity:** Medium  
**Key Concept:** Part-whole hierarchies and recursive structures

## Problem

Need to build tree structures where:
- Individual items and collections are used identically
- Want unified interface for both leaves and nodes
- Building hierarchies without special cases
- Example: File system (files and directories), UI components

```csharp
// Problem: Different handling for files vs. folders
public class File { }
public class Folder 
{ 
    public List<File> Files { get; set; }  // Only files!
    public List<Folder> Folders { get; set; }  // Only folders!
}
// Can't treat them uniformly
```

## Solution

Define common interface that both leaves and compositions implement:

```csharp
public abstract class FileSystemComponent
{
    public abstract void Display();
    public abstract long GetSize();
}

public class File : FileSystemComponent
{
    public override void Display() => Console.WriteLine($"File: {Name}");
    public override long GetSize() => Size;
}

public class Directory : FileSystemComponent
{
    private List<FileSystemComponent> _children = new();

    public void Add(FileSystemComponent component) => _children.Add(component);
    public void Remove(FileSystemComponent component) => _children.Remove(component);

    public override void Display()
    {
        Console.WriteLine($"Directory: {Name}");
        foreach (var child in _children)
            child.Display();  // Recursive!
    }

    public override long GetSize() => _children.Sum(c => c.GetSize());
}

// Now treat uniformly!
var root = new Directory("root");
var file = new File("data.txt");
root.Add(file);  // Add file
root.Add(new Directory("subfolder"));  // Add directory
root.Display();  // Works for both!
```

## Implementation Approaches

### 1. File System

```csharp
public abstract class FileSystemComponent
{
    public string Name { get; protected set; }
    public abstract void Display(int indent = 0);
    public abstract long GetSize();
}

public class File : FileSystemComponent
{
    public long Size { get; set; }

    public File(string name, long size)
    {
        Name = name;
        Size = size;
    }

    public override void Display(int indent = 0)
    {
        Console.WriteLine(new string(' ', indent) + $"📄 {Name} ({Size} bytes)");
    }

    public override long GetSize() => Size;
}

public class Directory : FileSystemComponent
{
    private List<FileSystemComponent> _children = new();

    public Directory(string name) => Name = name;

    public void Add(FileSystemComponent component) => _children.Add(component);

    public override void Display(int indent = 0)
    {
        Console.WriteLine(new string(' ', indent) + $"📁 {Name}/");
        foreach (var child in _children)
            child.Display(indent + 2);
    }

    public override long GetSize() => _children.Sum(c => c.GetSize());
}

// Usage
var root = new Directory("root");
var docs = new Directory("Documents");
docs.Add(new File("resume.pdf", 512));
docs.Add(new File("cover.doc", 256));
root.Add(docs);
root.Display();
root.GetSize();  // Total size of all files
```

### 2. Organization Hierarchy

```csharp
public abstract class OrganizationComponent
{
    public string Name { get; protected set; }
    public abstract void ShowStructure(int indent = 0);
    public abstract int GetEmployeeCount();
}

public class Employee : OrganizationComponent
{
    public string Position { get; set; }

    public Employee(string name, string position)
    {
        Name = name;
        Position = position;
    }

    public override void ShowStructure(int indent = 0)
    {
        Console.WriteLine(new string(' ', indent) + $"👤 {Name} ({Position})");
    }

    public override int GetEmployeeCount() => 1;
}

public class Department : OrganizationComponent
{
    private List<OrganizationComponent> _members = new();

    public Department(string name) => Name = name;

    public void Add(OrganizationComponent member) => _members.Add(member);

    public override void ShowStructure(int indent = 0)
    {
        Console.WriteLine(new string(' ', indent) + $"🏢 {Name}");
        foreach (var member in _members)
            member.ShowStructure(indent + 2);
    }

    public override int GetEmployeeCount() => _members.Sum(m => m.GetEmployeeCount());
}
```

### 3. UI Components

```csharp
public abstract class GraphicsComponent
{
    public string Name { get; protected set; }
    public abstract void Draw();
    public abstract void Move(int dx, int dy);
}

public class Shape : GraphicsComponent
{
    public Shape(string name) => Name = name;

    public override void Draw() => Console.WriteLine($"Drawing {Name}");
    public override void Move(int dx, int dy) => 
        Console.WriteLine($"Moving {Name} by ({dx}, {dy})");
}

public class Group : GraphicsComponent
{
    private List<GraphicsComponent> _components = new();

    public Group(string name) => Name = name;

    public void Add(GraphicsComponent component) => _components.Add(component);

    public override void Draw()
    {
        foreach (var component in _components)
            component.Draw();
    }

    public override void Move(int dx, int dy)
    {
        foreach (var component in _components)
            component.Move(dx, dy);
    }
}
```

### 4. Task Hierarchy

```csharp
public abstract class Task
{
    public string Title { get; protected set; }
    public abstract void Display(int indent = 0);
    public abstract void MarkComplete();
    public abstract int GetRemainingTasks();
}

public class SimpleTask : Task
{
    public bool IsComplete { get; private set; }

    public SimpleTask(string title) => Title = title;

    public override void Display(int indent = 0)
    {
        var status = IsComplete ? "[✓]" : "[ ]";
        Console.WriteLine(new string(' ', indent) + $"{status} {Title}");
    }

    public override void MarkComplete() => IsComplete = true;
    public override int GetRemainingTasks() => IsComplete ? 0 : 1;
}

public class CompositeTask : Task
{
    private List<Task> _subtasks = new();

    public CompositeTask(string title) => Title = title;

    public void AddSubtask(Task task) => _subtasks.Add(task);

    public override void Display(int indent = 0)
    {
        Console.WriteLine(new string(' ', indent) + $"📋 {Title}");
        foreach (var task in _subtasks)
            task.Display(indent + 2);
    }

    public override void MarkComplete()
    {
        foreach (var task in _subtasks)
            task.MarkComplete();
    }

    public override int GetRemainingTasks() => _subtasks.Sum(t => t.GetRemainingTasks());
}
```

---

## Composite Pattern Structure

```
       Component (Abstract)
            /\
           /  \
          /    \
      Leaf    Composite
       (File)  (Directory)
              Contains List<Component>
              Can contain Leaf or Composite
```

---

## Pros and Cons

### Advantages
✅ **Uniform Interface** - Treat leaves and composites same way  
✅ **Recursive Structures** - Naturally handle tree hierarchies  
✅ **Open/Closed** - Easy to add new component types  
✅ **Flexible** - Build complex hierarchies easily  
✅ **Simplifies Client Code** - No special cases for leaves/composites  

### Disadvantages
❌ **Type Confusion** - Hard to distinguish leaves from composites  
❌ **Constraint Violations** - Leaf components might accept children  
❌ **Over-Design** - Overkill for simple hierarchies  
❌ **Memory** - Extra objects for leaf wrappers  

---

## When to Use

### ✅ Use Composite When:
- Building part-whole hierarchies
- Want to treat individuals and collections uniformly
- Tree structures with uniform operations
- Build from simple primitives
- Recursive composition needed

### ❌ Don't Use When:
- Simple list of items
- Different operations for leaves/composites
- Type distinction critical
- Performance sensitive
- Simpler design sufficient

---

## Real-World Examples

- **File Systems** - Files and directories
- **GUI Frameworks** - Components and containers
- **HTML/DOM** - Elements and containers
- **Organization Charts** - Employees and departments
- **Menus** - Menu items and submenus
- **Documents** - Paragraphs, sections, chapters

---

## Interview Questions

**Q: What's the key benefit of Composite pattern?**
A: Treating individual objects and compositions uniformly through common interface.

**Q: How does Composite handle recursive structures?**
A: Components contain other components; display/operations recurse through tree.

**Q: Can a leaf component contain children?**
A: Depends on design. Pure Composite allows all components to have children.

---

## Summary

Composite pattern elegantly handles tree hierarchies by treating leaves and compositions uniformly. Perfect for any recursive structure (file systems, UI components, task lists). Simplifies code by eliminating type checks.

**Key Takeaway:** Composite represents part-whole hierarchies with unified interface.
