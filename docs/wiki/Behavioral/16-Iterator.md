# Iterator Pattern

## Overview

**Category:** Behavioral Pattern  
**Purpose:** Provide a way to access elements of an aggregate object sequentially without exposing its underlying representation.  
**Also Called:** Cursor  
**Complexity:** Low  
**Key Concept:** Sequential access to collection elements

## Problem

Need to traverse collections without exposing internal structure:

```csharp
// Problem: Exposing internal structure
public class List
{
    private int[] _items;

    // Client must know internal array structure
    public int[] GetItems() => _items;  // Exposes internal!
}

// Usage
var list = new List();
var items = list.GetItems();
for (int i = 0; i < items.Length; i++)
{
    // Direct access exposes implementation
}
```

## Solution

Provide iterator interface for sequential access:

```csharp
public interface IIterator
{
    bool HasNext();
    object Next();
    void Remove();
}

public class List
{
    private int[] _items;

    public IIterator CreateIterator()
    {
        return new ListIterator(this);
    }
}

// Usage - don't know or care about internal structure
var iterator = list.CreateIterator();
while (iterator.HasNext())
{
    var item = iterator.Next();
}
```

## Implementation Approaches

### 1. Simple List Iterator

```csharp
public interface IIterator<T>
{
    bool HasNext();
    T Next();
    void Reset();
}

public interface IIterable<T>
{
    IIterator<T> CreateIterator();
}

public class SimpleList<T> : IIterable<T>
{
    private List<T> _items = new();

    public void Add(T item) => _items.Add(item);

    public IIterator<T> CreateIterator() => new SimpleListIterator(this);

    private class SimpleListIterator : IIterator<T>
    {
        private SimpleList<T> _list;
        private int _index = 0;

        public SimpleListIterator(SimpleList<T> list) => _list = list;

        public bool HasNext() => _index < _list._items.Count;

        public T Next()
        {
            if (!HasNext())
                throw new InvalidOperationException("No more elements");
            return _list._items[_index++];
        }

        public void Reset() => _index = 0;
    }
}

// Usage
var list = new SimpleList<string>();
list.Add("Alice");
list.Add("Bob");
list.Add("Charlie");

var iterator = list.CreateIterator();
while (iterator.HasNext())
{
    Console.WriteLine($"👤 {iterator.Next()}");
}

iterator.Reset();  // Can iterate again
while (iterator.HasNext())
{
    Console.WriteLine($"Name: {iterator.Next()}");
}
```

### 2. Two-Way Iterator

```csharp
public interface IBiIterator<T>
{
    bool HasNext();
    bool HasPrevious();
    T Next();
    T Previous();
    void Reset();
}

public class BiIterableList<T>
{
    private List<T> _items = new();

    public void Add(T item) => _items.Add(item);

    public IBiIterator<T> CreateIterator() => new BiListIterator(this);

    private class BiListIterator : IBiIterator<T>
    {
        private BiIterableList<T> _list;
        private int _index = 0;

        public BiListIterator(BiIterableList<T> list) => _list = list;

        public bool HasNext() => _index < _list._items.Count;
        public bool HasPrevious() => _index > 0;

        public T Next()
        {
            if (!HasNext())
                throw new InvalidOperationException("No more elements");
            return _list._items[_index++];
        }

        public T Previous()
        {
            if (!HasPrevious())
                throw new InvalidOperationException("No previous elements");
            return _list._items[--_index];
        }

        public void Reset() => _index = 0;
    }
}

// Usage
var list = new BiIterableList<int>();
list.Add(1);
list.Add(2);
list.Add(3);

var iterator = list.CreateIterator();
Console.WriteLine(iterator.Next());      // 1
Console.WriteLine(iterator.Next());      // 2
Console.WriteLine(iterator.Previous());  // 1
Console.WriteLine(iterator.Next());      // 2
```

### 3. Tree Iterator

```csharp
public interface ITreeIterator<T>
{
    bool HasNext();
    T Next();
}

public class TreeNode<T>
{
    public T Value { get; set; }
    public TreeNode<T> Left { get; set; }
    public TreeNode<T> Right { get; set; }

    public TreeNode(T value) => Value = value;

    public ITreeIterator<T> CreateInOrderIterator() => 
        new InOrderIterator(this);

    public ITreeIterator<T> CreatePreOrderIterator() => 
        new PreOrderIterator(this);

    public ITreeIterator<T> CreateLevelOrderIterator() => 
        new LevelOrderIterator(this);

    // In-order: Left, Root, Right
    private class InOrderIterator : ITreeIterator<T>
    {
        private Stack<TreeNode<T>> _stack = new();
        private TreeNode<T> _current;

        public InOrderIterator(TreeNode<T> root) => _current = root;

        public bool HasNext()
        {
            return _current != null || _stack.Count > 0;
        }

        public T Next()
        {
            while (_current != null)
            {
                _stack.Push(_current);
                _current = _current.Left;
            }

            if (_stack.Count == 0)
                throw new InvalidOperationException("No more elements");

            _current = _stack.Pop();
            var result = _current.Value;
            _current = _current.Right;
            return result;
        }
    }

    // Pre-order: Root, Left, Right
    private class PreOrderIterator : ITreeIterator<T>
    {
        private Stack<TreeNode<T>> _stack = new();

        public PreOrderIterator(TreeNode<T> root)
        {
            if (root != null)
                _stack.Push(root);
        }

        public bool HasNext() => _stack.Count > 0;

        public T Next()
        {
            if (!HasNext())
                throw new InvalidOperationException("No more elements");

            var node = _stack.Pop();
            if (node.Right != null)
                _stack.Push(node.Right);
            if (node.Left != null)
                _stack.Push(node.Left);
            return node.Value;
        }
    }

    // Level-order: BFS
    private class LevelOrderIterator : ITreeIterator<T>
    {
        private Queue<TreeNode<T>> _queue = new();

        public LevelOrderIterator(TreeNode<T> root)
        {
            if (root != null)
                _queue.Enqueue(root);
        }

        public bool HasNext() => _queue.Count > 0;

        public T Next()
        {
            if (!HasNext())
                throw new InvalidOperationException("No more elements");

            var node = _queue.Dequeue();
            if (node.Left != null)
                _queue.Enqueue(node.Left);
            if (node.Right != null)
                _queue.Enqueue(node.Right);
            return node.Value;
        }
    }
}

// Usage
var root = new TreeNode<int>(1)
{
    Left = new TreeNode<int>(2)
    {
        Left = new TreeNode<int>(4),
        Right = new TreeNode<int>(5)
    },
    Right = new TreeNode<int>(3)
};

Console.WriteLine("In-order:");
var inOrder = root.CreateInOrderIterator();
while (inOrder.HasNext())
    Console.WriteLine(inOrder.Next());  // 4, 2, 5, 1, 3

Console.WriteLine("Pre-order:");
var preOrder = root.CreatePreOrderIterator();
while (preOrder.HasNext())
    Console.WriteLine(preOrder.Next());  // 1, 2, 4, 5, 3

Console.WriteLine("Level-order:");
var levelOrder = root.CreateLevelOrderIterator();
while (levelOrder.HasNext())
    Console.WriteLine(levelOrder.Next());  // 1, 2, 3, 4, 5
```

### 4. Filter Iterator

```csharp
public interface IFilterIterator<T>
{
    bool HasNext();
    T Next();
}

public class FilterableList<T>
{
    private List<T> _items = new();

    public void Add(T item) => _items.Add(item);

    public IFilterIterator<T> CreateFilterIterator(Func<T, bool> predicate) =>
        new FilterIterator(_items, predicate);

    private class FilterIterator : IFilterIterator<T>
    {
        private List<T> _items;
        private Func<T, bool> _predicate;
        private int _index = 0;

        public FilterIterator(List<T> items, Func<T, bool> predicate)
        {
            _items = items;
            _predicate = predicate;
        }

        public bool HasNext()
        {
            while (_index < _items.Count)
            {
                if (_predicate(_items[_index]))
                    return true;
                _index++;
            }
            return false;
        }

        public T Next()
        {
            if (!HasNext())
                throw new InvalidOperationException("No more elements");
            return _items[_index++];
        }
    }
}

// Usage
var list = new FilterableList<int>();
list.Add(1);
list.Add(2);
list.Add(3);
list.Add(4);
list.Add(5);

var evenIterator = list.CreateFilterIterator(x => x % 2 == 0);
while (evenIterator.HasNext())
{
    Console.WriteLine($"Even: {evenIterator.Next()}");  // 2, 4
}

var oddIterator = list.CreateFilterIterator(x => x % 2 != 0);
while (oddIterator.HasNext())
{
    Console.WriteLine($"Odd: {oddIterator.Next()}");   // 1, 3, 5
}
```

### 5. Reverse Iterator

```csharp
public interface IReverseIterator<T>
{
    bool HasNext();
    T Next();
}

public class ReverseIterableList<T>
{
    private List<T> _items = new();

    public void Add(T item) => _items.Add(item);

    public IReverseIterator<T> CreateReverseIterator() => 
        new ReverseIterator(this);

    private class ReverseIterator : IReverseIterator<T>
    {
        private ReverseIterableList<T> _list;
        private int _index;

        public ReverseIterator(ReverseIterableList<T> list)
        {
            _list = list;
            _index = _list._items.Count - 1;
        }

        public bool HasNext() => _index >= 0;

        public T Next()
        {
            if (!HasNext())
                throw new InvalidOperationException("No more elements");
            return _list._items[_index--];
        }
    }
}

// Usage
var list = new ReverseIterableList<string>();
list.Add("First");
list.Add("Second");
list.Add("Third");

var iterator = list.CreateReverseIterator();
while (iterator.HasNext())
{
    Console.WriteLine(iterator.Next());  // Third, Second, First
}
```

---

## Iterator Pattern Structure

```
    Client
      |
      v
  IIterable
      |
      | createIterator()
      v
  IIterator
  - HasNext()
  - Next()
  - Reset()
```

---

## Pros and Cons

### Advantages
✅ **Encapsulation** - Hides internal collection structure  
✅ **Multiple Iterators** - Different iteration strategies  
✅ **Uniform Interface** - Same interface for different collections  
✅ **Separation of Concerns** - Iteration logic separate from collection  
✅ **Easy to Change** - Can change iteration without affecting collection  

### Disadvantages
❌ **Complexity** - Extra classes for iterators  
❌ **Performance** - Slight overhead vs. direct access  
❌ **State Management** - Iterator maintains state  
❌ **Not Needed** - C# IEnumerable/IEnumerator built-in  

---

## When to Use

### ✅ Use Iterator When:
- Need to traverse collection without exposing structure
- Multiple iteration strategies needed
- Collection structure might change
- Want uniform access interface
- Legacy collections need iteration interface

### ❌ Don't Use When:
- C# IEnumerable sufficient (usually!)
- Simple collection with direct access
- Performance critical
- Simplicity valued

---

## Real-World Examples

- **C# IEnumerator/IEnumerable** - Built-in iterator pattern
- **Java Iterator** - Collections framework
- **Database cursors** - Iterate over query results
- **File system** - Directory traversal
- **Graph traversal** - BFS/DFS iterators
- **Tree traversal** - In-order, Pre-order, Level-order

---

## Interview Questions

**Q: What's the difference between Iterator and Enumerable?**
A: Enumerable creates iterators; Iterator traverses collection. Enumerable is factory; Iterator is worker.

**Q: Can multiple iterators exist simultaneously?**
A: Yes, each iterator maintains own position. Each client can iterate independently.

**Q: What about modifications during iteration?**
A: Depends on implementation. Usually throw exception if collection modified during iteration (fail-fast).

**Q: Why not just use foreach/LINQ?**
A: You should in C#! Iterator pattern is more relevant in languages without built-in support.

---

## Related Patterns

| Pattern | Relation |
|---------|----------|
| **Composite** | Iterator often used with Composite structures |
| **Factory** | Iterable creates iterator instances |
| **Strategy** | Different iteration strategies |

---

## .NET Built-in

In C#, Iterator pattern is built-in:

```csharp
// Instead of custom iterator:
public interface IIterator<T>
{
    bool HasNext();
    T Next();
}

// Use built-in:
public interface IEnumerable<T>
{
    IEnumerator<T> GetEnumerator();
}

public interface IEnumerator<T> : IDisposable
{
    T Current { get; }
    bool MoveNext();
}

// Usage with foreach (syntactic sugar for Iterator pattern):
foreach (var item in collection)
{
    Console.WriteLine(item);
}
```

---

## Summary

Iterator pattern provides uniform sequential access to collection elements without exposing internal structure. Perfect for custom collections, tree/graph traversal, and scenarios with multiple iteration strategies. In C#, prefer built-in IEnumerable/IEnumerator and foreach loops.

**Key Takeaway:** Iterator provides sequential access without exposing collection structure.
