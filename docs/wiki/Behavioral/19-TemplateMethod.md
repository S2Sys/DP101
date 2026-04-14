# Template Method Pattern

## Overview

**Category:** Behavioral Pattern  
**Purpose:** Define the skeleton of an algorithm in an operation, deferring some steps to subclasses.  
**Also Called:** Method Template  
**Complexity:** Low

## Problem

Multiple classes have similar algorithms with small variations:

```csharp
// Problem: Duplicate algorithm logic
public class CsvReporter
{
    public void GenerateReport()
    {
        OpenFile();
        WriteHeader();
        WriteData();  // Different format
        CloseFile();
    }
}

public class HtmlReporter
{
    public void GenerateReport()
    {
        OpenFile();
        WriteHeader();  // Different format
        WriteData();    // Different format
        CloseFile();
    }
}

// Algorithm skeleton duplicated in multiple classes!
```

## Solution

Define algorithm skeleton in base class, let subclasses override steps:

```csharp
public abstract class Reporter
{
    // Template method - defines algorithm skeleton
    public void GenerateReport()
    {
        OpenFile();
        WriteHeader();
        WriteData();   // Subclass implements
        CloseFile();
    }

    protected abstract void WriteHeader();
    protected abstract void WriteData();

    protected void OpenFile() => Console.WriteLine("Opening file...");
    protected void CloseFile() => Console.WriteLine("Closing file...");
}

// Subclasses only implement varying parts
public class CsvReporter : Reporter
{
    protected override void WriteHeader() => Console.WriteLine("CSV Header");
    protected override void WriteData() => Console.WriteLine("CSV Data");
}
```

## Implementation Approaches

### 1. Beverage Preparation Template

```csharp
public abstract class Beverage
{
    // Template method
    public final void Prepare()
    {
        BoilWater();
        Brew();
        PourInCup();
        AddCondiments();
    }

    protected void BoilWater() => Console.WriteLine("🔥 Boiling water");
    protected void PourInCup() => Console.WriteLine("☕ Pouring in cup");

    protected abstract void Brew();
    protected abstract void AddCondiments();
}

public class Coffee : Beverage
{
    protected override void Brew()
    {
        Console.WriteLine("☕ Brewing coffee");
    }

    protected override void AddCondiments()
    {
        Console.WriteLine("➕ Adding milk and sugar");
    }
}

public class Tea : Beverage
{
    protected override void Brew()
    {
        Console.WriteLine("🫖 Steeping tea");
    }

    protected override void AddCondiments()
    {
        Console.WriteLine("➕ Adding lemon");
    }
}

// Usage - same algorithm, different implementations
var coffee = new Coffee();
coffee.Prepare();
// Output:
// Boiling water
// Brewing coffee
// Pouring in cup
// Adding milk and sugar

var tea = new Tea();
tea.Prepare();
// Output:
// Boiling water
// Steeping tea
// Pouring in cup
// Adding lemon
```

### 2. Data Processing Template

```csharp
public abstract class DataProcessor
{
    // Template method
    public void Process(string data)
    {
        var loaded = LoadData(data);
        var validated = Validate(loaded);
        var transformed = Transform(validated);
        Save(transformed);
    }

    protected abstract string LoadData(string input);
    protected abstract bool Validate(string data);
    protected abstract string Transform(string data);
    protected abstract void Save(string data);
}

public class CsvProcessor : DataProcessor
{
    protected override string LoadData(string input)
    {
        Console.WriteLine("📂 Loading CSV file");
        return input;
    }

    protected override bool Validate(string data)
    {
        Console.WriteLine("✓ Validating CSV format");
        return true;
    }

    protected override string Transform(string data)
    {
        Console.WriteLine("🔄 Parsing CSV to objects");
        return data.ToUpper();
    }

    protected override void Save(string data)
    {
        Console.WriteLine("💾 Saving to database");
    }
}

public class JsonProcessor : DataProcessor
{
    protected override string LoadData(string input)
    {
        Console.WriteLine("📂 Loading JSON file");
        return input;
    }

    protected override bool Validate(string data)
    {
        Console.WriteLine("✓ Validating JSON structure");
        return true;
    }

    protected override string Transform(string data)
    {
        Console.WriteLine("🔄 Parsing JSON to objects");
        return data;
    }

    protected override void Save(string data)
    {
        Console.WriteLine("💾 Saving to data store");
    }
}

// Usage
var csvProcessor = new CsvProcessor();
csvProcessor.Process("data.csv");

var jsonProcessor = new JsonProcessor();
jsonProcessor.Process("data.json");
```

### 3. Report Generation Template

```csharp
public abstract class Report
{
    // Template method defines report structure
    public void Generate()
    {
        Console.WriteLine("📄 Generating Report...");
        AddHeader();
        AddContent();
        AddFooter();
        Console.WriteLine("✅ Report complete\n");
    }

    protected abstract void AddHeader();
    protected abstract void AddContent();
    protected abstract void AddFooter();
}

public class PdfReport : Report
{
    protected override void AddHeader()
    {
        Console.WriteLine("📋 PDF Header: Document Title");
    }

    protected override void AddContent()
    {
        Console.WriteLine("📝 PDF Content: Formatted text with styles");
    }

    protected override void AddFooter()
    {
        Console.WriteLine("📄 PDF Footer: Page numbers");
    }
}

public class HtmlReport : Report
{
    protected override void AddHeader()
    {
        Console.WriteLine("📋 HTML Header: <h1>Title</h1>");
    }

    protected override void AddContent()
    {
        Console.WriteLine("📝 HTML Content: <p>Paragraphs and links</p>");
    }

    protected override void AddFooter()
    {
        Console.WriteLine("📄 HTML Footer: <footer>Copyright</footer>");
    }
}

public class ExcelReport : Report
{
    protected override void AddHeader()
    {
        Console.WriteLine("📋 Excel Header: Row with title");
    }

    protected override void AddContent()
    {
        Console.WriteLine("📝 Excel Content: Data in cells");
    }

    protected override void AddFooter()
    {
        Console.WriteLine("📄 Excel Footer: Summary row");
    }
}

// Usage
var pdfReport = new PdfReport();
pdfReport.Generate();

var htmlReport = new HtmlReport();
htmlReport.Generate();
```

### 4. Authentication Template

```csharp
public abstract class AuthenticationStrategy
{
    // Template method
    public bool Authenticate(string username, string password)
    {
        if (!ValidateInput(username, password))
        {
            Console.WriteLine("❌ Invalid input");
            return false;
        }

        if (!UserExists(username))
        {
            Console.WriteLine("❌ User not found");
            return false;
        }

        if (!ValidatePassword(username, password))
        {
            Console.WriteLine("❌ Invalid password");
            return false;
        }

        LogAuthSuccess(username);
        return true;
    }

    protected virtual bool ValidateInput(string username, string password)
    {
        return !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password);
    }

    protected abstract bool UserExists(string username);
    protected abstract bool ValidatePassword(string username, string password);

    protected virtual void LogAuthSuccess(string username)
    {
        Console.WriteLine($"✅ {username} authenticated successfully");
    }
}

public class DatabaseAuthentication : AuthenticationStrategy
{
    protected override bool UserExists(string username)
    {
        Console.WriteLine($"🔍 Checking database for user: {username}");
        return true;  // Simulate found
    }

    protected override bool ValidatePassword(string username, string password)
    {
        Console.WriteLine($"🔐 Validating password against database hash");
        return password == "admin123";
    }
}

public class LdapAuthentication : AuthenticationStrategy
{
    protected override bool UserExists(string username)
    {
        Console.WriteLine($"🔍 Checking LDAP for user: {username}");
        return true;
    }

    protected override bool ValidatePassword(string username, string password)
    {
        Console.WriteLine($"🔐 Validating against LDAP server");
        return password == "ldap_pass";
    }
}

public class ApiKeyAuthentication : AuthenticationStrategy
{
    protected override bool ValidateInput(string username, string password)
    {
        return password.Length == 32;  // API key length
    }

    protected override bool UserExists(string username)
    {
        Console.WriteLine($"🔍 Checking API key store");
        return true;
    }

    protected override bool ValidatePassword(string username, string password)
    {
        Console.WriteLine($"🔐 Validating API key");
        return password == "12345678901234567890123456789012";
    }
}

// Usage
var dbAuth = new DatabaseAuthentication();
dbAuth.Authenticate("alice", "admin123");

var ldapAuth = new LdapAuthentication();
ldapAuth.Authenticate("bob", "ldap_pass");

var apiAuth = new ApiKeyAuthentication();
apiAuth.Authenticate("service", "12345678901234567890123456789012");
```

### 5. Game Character Action Template

```csharp
public abstract class GameCharacter
{
    // Template method for action sequence
    public void PerformAction(string action)
    {
        Prepare();
        ExecuteAction(action);
        React();
    }

    protected void Prepare() => Console.WriteLine("🎮 Preparing to act...");
    protected void React() => Console.WriteLine("✨ Action complete!\n");

    protected abstract void ExecuteAction(string action);
}

public class Warrior : GameCharacter
{
    protected override void ExecuteAction(string action)
    {
        if (action == "Attack")
        {
            Console.WriteLine("⚔️ Warrior swings sword (high damage)");
        }
        else if (action == "Defend")
        {
            Console.WriteLine("🛡️ Warrior raises shield");
        }
    }
}

public class Mage : GameCharacter
{
    protected override void ExecuteAction(string action)
    {
        if (action == "Attack")
        {
            Console.WriteLine("🔥 Mage casts fireball");
        }
        else if (action == "Defend")
        {
            Console.WriteLine("🛡️ Mage creates shield spell");
        }
    }
}

public class Rogue : GameCharacter
{
    protected override void ExecuteAction(string action)
    {
        if (action == "Attack")
        {
            Console.WriteLine("🗡️ Rogue backstabs (high crit)");
        }
        else if (action == "Defend")
        {
            Console.WriteLine("💨 Rogue evasion dodge");
        }
    }
}

// Usage
var warrior = new Warrior();
warrior.PerformAction("Attack");
warrior.PerformAction("Defend");

var mage = new Mage();
mage.PerformAction("Attack");
mage.PerformAction("Defend");
```

---

## Template Method vs. Strategy

```csharp
// Template Method: Base class defines algorithm
public abstract class BaseAlgorithm
{
    public void Execute()  // Concrete template method
    {
        Step1();
        Step2();
        Step3();
    }
    protected abstract void Step1();
    protected abstract void Step2();
    protected abstract void Step3();
}

// Strategy: Client selects algorithm
public interface IAlgorithm
{
    void Execute();
}
public class AlgorithmA : IAlgorithm { }
public class AlgorithmB : IAlgorithm { }

// Template Method: Inheritance
var obj = new ConcreteAlgorithm();
obj.Execute();

// Strategy: Composition
IAlgorithm algo = new AlgorithmA();
algo.Execute();
```

---

## Pros and Cons

### Advantages
✅ **Code Reuse** - Common algorithm structure in base class  
✅ **DRY Principle** - Don't Repeat Yourself  
✅ **Control** - Base class controls algorithm flow  
✅ **Consistency** - All subclasses follow same steps  
✅ **Easy to Extend** - Subclass only implements varying parts  

### Disadvantages
❌ **Inheritance Required** - Must use inheritance  
❌ **Hollywood Principle** - "Don't call us, we'll call you"  
❌ **Flexibility Limited** - Can't change algorithm structure  
❌ **Over-Design** - Overkill for simple cases  

---

## When to Use

### ✅ Use Template Method When:
- Multiple classes with similar algorithms
- Common algorithm structure with varying steps
- Want to control algorithm flow
- DRY principle important
- Algorithm steps have common sequence
- Inheritance hierarchy makes sense

### ❌ Don't Use When:
- Different algorithm structures needed
- Composition preferred over inheritance
- Few variations
- Simple logic sufficient
- Flexibility over structure needed

---

## Interview Questions

**Q: What's the difference between Template Method and Strategy?**
A: Template Method uses inheritance; Strategy uses composition. Template Method for similar algorithms; Strategy for algorithm selection.

**Q: Can you use hooks in Template Method?**
A: Yes, optional override points. Make some protected methods empty (hooks) so subclasses can optionally override.

**Q: How to handle shared data?**
A: Store in base class protected fields. Subclasses access shared state through base class methods.

**Q: What if subclass needs different algorithm flow?**
A: Then don't use Template Method. Use Strategy pattern for flexible algorithm selection.

---

## Hook Methods

Provide empty default implementations for optional overrides:

```csharp
public abstract class DataProcessor
{
    public void Process()
    {
        BeforeProcess();  // Hook
        LoadData();
        Transform();
        AfterProcess();   // Hook
    }

    protected virtual void BeforeProcess() { }  // Optional hook
    protected virtual void AfterProcess() { }   // Optional hook

    protected abstract void LoadData();
    protected abstract void Transform();
}
```

---

## Related Patterns

| Pattern | Relation |
|---------|----------|
| **Strategy** | Both encapsulate algorithms; Template Method uses inheritance |
| **Factory** | Often used together for object creation |
| **Hook Methods** | Optional extension points in template |

---

## Summary

Template Method pattern defines algorithm skeleton in base class, letting subclasses override specific steps. Perfect for shared algorithm structure with varying implementations, report generation, data processing, authentication, and any scenario with common algorithm flow. Use hooks for optional customization.

**Key Takeaway:** Template Method defines algorithm structure, subclasses customize steps.
