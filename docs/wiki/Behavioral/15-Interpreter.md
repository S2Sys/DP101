# Interpreter Pattern

## Overview

**Category:** Behavioral Pattern  
**Purpose:** Define a representation for a grammar and an interpreter to interpret sentences in the language.  
**Complexity:** High  
**Key Concept:** Language parsing and evaluation

## Problem

Need to interpret custom languages or expressions:

```csharp
// Problem: Hardcoded expression evaluation
public class Calculator
{
    public double Evaluate(string expression)
    {
        // Parse "2 + 3 * 4"
        // Hard to extend for new operators or grammar rules
        // What if we need: "5 > 3 AND 2 < 4"?
    }
}
```

## Solution

Define grammar rules with abstract syntax tree (AST):

```csharp
public interface IExpression
{
    double Interpret();
}

public class NumberExpression : IExpression
{
    private double _value;

    public NumberExpression(double value) => _value = value;

    public double Interpret() => _value;
}

public class AddExpression : IExpression
{
    private IExpression _left, _right;

    public AddExpression(IExpression left, IExpression right)
    {
        _left = left;
        _right = right;
    }

    public double Interpret() => _left.Interpret() + _right.Interpret();
}

// Easy to extend with new grammar rules!
```

## Implementation Approaches

### 1. Mathematical Expression Interpreter

```csharp
public interface IExpression
{
    double Interpret();
}

public class NumberExpression : IExpression
{
    private double _value;

    public NumberExpression(double value) => _value = value;

    public double Interpret() => _value;
}

public class AddExpression : IExpression
{
    private IExpression _left;
    private IExpression _right;

    public AddExpression(IExpression left, IExpression right)
    {
        _left = left;
        _right = right;
    }

    public double Interpret() => _left.Interpret() + _right.Interpret();
}

public class SubtractExpression : IExpression
{
    private IExpression _left;
    private IExpression _right;

    public SubtractExpression(IExpression left, IExpression right)
    {
        _left = left;
        _right = right;
    }

    public double Interpret() => _left.Interpret() - _right.Interpret();
}

public class MultiplyExpression : IExpression
{
    private IExpression _left;
    private IExpression _right;

    public MultiplyExpression(IExpression left, IExpression right)
    {
        _left = left;
        _right = right;
    }

    public double Interpret() => _left.Interpret() * _right.Interpret();
}

public class DivideExpression : IExpression
{
    private IExpression _left;
    private IExpression _right;

    public DivideExpression(IExpression left, IExpression right)
    {
        _left = left;
        _right = right;
    }

    public double Interpret() => _left.Interpret() / _right.Interpret();
}

// Usage - build expression tree manually
var expression = new AddExpression(
    new NumberExpression(2),
    new MultiplyExpression(
        new NumberExpression(3),
        new NumberExpression(4)
    )
);

Console.WriteLine(expression.Interpret());  // 2 + (3 * 4) = 14
```

### 2. Boolean Expression Interpreter

```csharp
public interface IBoolExpression
{
    bool Interpret();
}

public class TrueExpression : IBoolExpression
{
    public bool Interpret() => true;
}

public class FalseExpression : IBoolExpression
{
    public bool Interpret() => false;
}

public class VariableExpression : IBoolExpression
{
    private string _name;
    private Dictionary<string, bool> _context;

    public VariableExpression(string name, Dictionary<string, bool> context)
    {
        _name = name;
        _context = context;
    }

    public bool Interpret() => _context.ContainsKey(_name) && _context[_name];
}

public class AndExpression : IBoolExpression
{
    private IBoolExpression _left;
    private IBoolExpression _right;

    public AndExpression(IBoolExpression left, IBoolExpression right)
    {
        _left = left;
        _right = right;
    }

    public bool Interpret() => _left.Interpret() && _right.Interpret();
}

public class OrExpression : IBoolExpression
{
    private IBoolExpression _left;
    private IBoolExpression _right;

    public OrExpression(IBoolExpression left, IBoolExpression right)
    {
        _left = left;
        _right = right;
    }

    public bool Interpret() => _left.Interpret() || _right.Interpret();
}

public class NotExpression : IBoolExpression
{
    private IBoolExpression _expression;

    public NotExpression(IBoolExpression expression) => _expression = expression;

    public bool Interpret() => !_expression.Interpret();
}

// Usage
var context = new Dictionary<string, bool>
{
    { "A", true },
    { "B", false },
    { "C", true }
};

// (A AND C) OR NOT B
var expression = new OrExpression(
    new AndExpression(
        new VariableExpression("A", context),
        new VariableExpression("C", context)
    ),
    new NotExpression(
        new VariableExpression("B", context)
    )
);

Console.WriteLine(expression.Interpret());  // (true AND true) OR NOT false = true
```

### 3. SQL Query Interpreter

```csharp
public interface IQueryExpression
{
    List<string> Interpret();
}

public class SelectExpression : IQueryExpression
{
    private List<string> _columns;
    private IQueryExpression _from;

    public SelectExpression(List<string> columns, IQueryExpression from)
    {
        _columns = columns;
        _from = from;
    }

    public List<string> Interpret()
    {
        var results = _from.Interpret();
        Console.WriteLine($"SELECT {string.Join(", ", _columns)}");
        return results;
    }
}

public class FromExpression : IQueryExpression
{
    private string _tableName;
    private List<string> _data;

    public FromExpression(string tableName, List<string> data)
    {
        _tableName = tableName;
        _data = data;
    }

    public List<string> Interpret()
    {
        Console.WriteLine($"FROM {_tableName}");
        return _data;
    }
}

public class WhereExpression : IQueryExpression
{
    private IQueryExpression _source;
    private Func<string, bool> _predicate;

    public WhereExpression(IQueryExpression source, Func<string, bool> predicate)
    {
        _source = source;
        _predicate = predicate;
    }

    public List<string> Interpret()
    {
        var results = _source.Interpret();
        var filtered = results.Where(_predicate).ToList();
        Console.WriteLine($"WHERE (filtered to {filtered.Count} rows)");
        return filtered;
    }
}

// Usage
var data = new List<string> { "User1", "User2", "User3" };
var query = new SelectExpression(
    new List<string> { "Name", "Email" },
    new WhereExpression(
        new FromExpression("Users", data),
        row => row.Contains("User") && row != "User2"
    )
);

var results = query.Interpret();
// Output:
// SELECT Name, Email
// FROM Users
// WHERE (filtered to 2 rows)
```

### 4. Configuration Language Interpreter

```csharp
public interface IConfigExpression
{
    Dictionary<string, object> Interpret();
}

public class ConfigProperty : IConfigExpression
{
    private string _key;
    private object _value;

    public ConfigProperty(string key, object value)
    {
        _key = key;
        _value = value;
    }

    public Dictionary<string, object> Interpret() => 
        new Dictionary<string, object> { { _key, _value } };
}

public class ConfigSection : IConfigExpression
{
    private string _sectionName;
    private List<IConfigExpression> _expressions;

    public ConfigSection(string name, List<IConfigExpression> expressions)
    {
        _sectionName = name;
        _expressions = expressions;
    }

    public Dictionary<string, object> Interpret()
    {
        var config = new Dictionary<string, object>();
        var sectionConfig = new Dictionary<string, object>();

        foreach (var expr in _expressions)
        {
            var result = expr.Interpret();
            foreach (var kvp in result)
            {
                sectionConfig[kvp.Key] = kvp.Value;
            }
        }

        config[_sectionName] = sectionConfig;
        return config;
    }
}

// Usage - build configuration tree
var config = new ConfigSection(
    "Database",
    new List<IConfigExpression>
    {
        new ConfigProperty("Host", "localhost"),
        new ConfigProperty("Port", 5432),
        new ConfigProperty("Username", "admin")
    }
);

var result = config.Interpret();
Console.WriteLine("Configuration loaded");
```

### 5. Regular Expression Pattern Interpreter

```csharp
public interface IPatternExpression
{
    bool Matches(string text);
}

public class LiteralExpression : IPatternExpression
{
    private string _literal;

    public LiteralExpression(string literal) => _literal = literal;

    public bool Matches(string text) => text == _literal;
}

public class WildcardExpression : IPatternExpression
{
    public bool Matches(string text) => true;  // Matches any single char
}

public class AndPatternExpression : IPatternExpression
{
    private IPatternExpression _left;
    private IPatternExpression _right;

    public AndPatternExpression(IPatternExpression left, IPatternExpression right)
    {
        _left = left;
        _right = right;
    }

    public bool Matches(string text) => _left.Matches(text) && _right.Matches(text);
}

public class OrPatternExpression : IPatternExpression
{
    private IPatternExpression _left;
    private IPatternExpression _right;

    public OrPatternExpression(IPatternExpression left, IPatternExpression right)
    {
        _left = left;
        _right = right;
    }

    public bool Matches(string text) => _left.Matches(text) || _right.Matches(text);
}

// Usage
var pattern = new OrPatternExpression(
    new LiteralExpression("admin"),
    new LiteralExpression("user")
);

Console.WriteLine(pattern.Matches("admin"));   // true
Console.WriteLine(pattern.Matches("guest"));   // false
```

---

## Interpreter Pattern Structure

```
    Client
      |
      v
   IExpression
    /   |   \
   /    |    \
Term1 Term2 Term3
```

---

## Pros and Cons

### Advantages
✅ **Grammar Representation** - Easy to define grammar rules  
✅ **Simple Grammar** - Clean for simple languages  
✅ **Extensible** - Easy to add new expression types  
✅ **Encapsulation** - Each expression handles its interpretation  
✅ **AST Navigation** - Easy to traverse expression tree  

### Disadvantages
❌ **Complex Grammars** - Becomes unwieldy for complex languages  
❌ **Performance** - Recursive interpretation can be slow  
❌ **Tree Size** - Large trees consume memory  
❌ **Parsing** - Interpreter doesn't handle parsing; need separate parser  
❌ **Maintenance** - Large number of expression classes  

---

## When to Use

### ✅ Use Interpreter When:
- Need to define grammar
- Language is simple
- Performance not critical
- Custom expression language
- Configuration language
- Query language
- Domain-specific language (DSL)

### ❌ Don't Use When:
- Complex grammar needed
- Performance critical
- General-purpose language
- Too many expression types
- Simplicity valued

---

## Real-World Examples

- **SQL parsers** - Query interpretation
- **Expression evaluators** - Math expressions
- **Regular expressions** - Pattern matching
- **Configuration languages** - YAML, XML parsers
- **Template engines** - Template interpretation
- **DSLs** - Domain-specific languages
- **Rule engines** - Business rule evaluation

---

## Interview Questions

**Q: What's the difference between Interpreter and Visitor?**
A: Interpreter defines grammar; Visitor operates on existing structures. Interpreter builds AST; Visitor traverses it.

**Q: How does Interpreter handle parsing?**
A: Interpreter doesn't parse. Need separate parser to build AST; Interpreter evaluates it.

**Q: Can you modify expression behavior at runtime?**
A: Depends on implementation. With visitor pattern, yes. With direct interpretation, need new expression class.

**Q: When is Interpreter too complex?**
A: When grammar is complex or performance matters. Use parser generators instead.

---

## Related Patterns

| Pattern | Relation |
|---------|----------|
| **Visitor** | Both traverse structures; Visitor more flexible |
| **Composite** | Interpreter often uses Composite for AST |
| **Factory** | Factory creates expression nodes |
| **Parser** | Often combined with parser for lexing |

---

## Summary

Interpreter pattern elegantly defines grammar and interprets expressions in that grammar. Perfect for DSLs, query languages, configuration languages, and expression evaluators. Define grammar as expression hierarchy, then interpret with visitor-like traversal.

**Key Takeaway:** Interpreter defines grammar representation and evaluates expressions in that language.
