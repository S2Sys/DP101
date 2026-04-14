# Chain of Responsibility Pattern

## Overview

**Category:** Behavioral Pattern  
**Purpose:** Avoid coupling the sender of a request to its receiver by giving more than one object a chance to handle the request. Chain the receiving objects and pass the request along the chain until an object handles it.  
**Also Called:** CoR  
**Complexity:** Medium

## Problem

Need to handle requests by multiple handlers in sequence:

```csharp
// Problem: Tightly coupled request handling
public class RequestProcessor
{
    public void ProcessRequest(Request request)
    {
        if (request.Type == "Email")
        {
            // Email handling code
        }
        else if (request.Type == "SMS")
        {
            // SMS handling code
        }
        else if (request.Type == "Push")
        {
            // Push notification handling code
        }
        // Adding new handler requires modifying this class!
    }
}
```

## Solution

Create chain of handlers, each handling specific request type:

```csharp
public abstract class RequestHandler
{
    protected RequestHandler _nextHandler;

    public void SetNext(RequestHandler handler) => _nextHandler = handler;

    public virtual void Handle(Request request)
    {
        if (CanHandle(request))
        {
            ProcessRequest(request);
        }
        else if (_nextHandler != null)
        {
            _nextHandler.Handle(request);
        }
    }

    protected abstract bool CanHandle(Request request);
    protected abstract void ProcessRequest(Request request);
}

// Adding new handler: just create new handler class, no changes!
```

## Implementation Approaches

### 1. Logging Handler Chain

```csharp
public abstract class Logger
{
    protected Logger _nextLogger;
    protected LogLevel _level;

    public void SetNextLogger(Logger logger) => _nextLogger = logger;

    public void LogMessage(LogLevel level, string message)
    {
        if (level >= _level)
        {
            Write(message);
        }

        _nextLogger?.LogMessage(level, message);
    }

    protected abstract void Write(string message);
}

public enum LogLevel
{
    Info = 1,
    Debug = 2,
    Error = 3
}

public class ConsoleLogger : Logger
{
    public ConsoleLogger(LogLevel level)
    {
        _level = level;
    }

    protected override void Write(string message)
    {
        Console.WriteLine($"[Console] {message}");
    }
}

public class FileLogger : Logger
{
    public FileLogger(LogLevel level)
    {
        _level = level;
    }

    protected override void Write(string message)
    {
        Console.WriteLine($"[File] Writing to file: {message}");
    }
}

public class EmailLogger : Logger
{
    public EmailLogger(LogLevel level)
    {
        _level = level;
    }

    protected override void Write(string message)
    {
        Console.WriteLine($"[Email] Sending alert: {message}");
    }
}

// Usage - chain handlers
var consoleLogger = new ConsoleLogger(LogLevel.Info);
var fileLogger = new FileLogger(LogLevel.Debug);
var emailLogger = new EmailLogger(LogLevel.Error);

consoleLogger.SetNextLogger(fileLogger);
fileLogger.SetNextLogger(emailLogger);

consoleLogger.LogMessage(LogLevel.Info, "Information");     // Console only
consoleLogger.LogMessage(LogLevel.Debug, "Debug details");  // Console + File
consoleLogger.LogMessage(LogLevel.Error, "Error occurred"); // Console + File + Email
```

### 2. HTTP Request Handler Pipeline

```csharp
public class HttpRequest
{
    public string Path { get; set; }
    public string Method { get; set; }
    public Dictionary<string, string> Headers { get; set; } = new();
    public string Body { get; set; }
}

public class HttpResponse
{
    public int StatusCode { get; set; }
    public string Body { get; set; }
}

public abstract class HttpHandler
{
    protected HttpHandler _nextHandler;

    public void SetNext(HttpHandler handler) => _nextHandler = handler;

    public virtual HttpResponse Handle(HttpRequest request)
    {
        if (CanHandle(request))
            return ProcessRequest(request);

        return _nextHandler?.Handle(request) ?? new HttpResponse { StatusCode = 404, Body = "Not Found" };
    }

    protected abstract bool CanHandle(HttpRequest request);
    protected abstract HttpResponse ProcessRequest(HttpRequest request);
}

public class AuthenticationHandler : HttpHandler
{
    protected override bool CanHandle(HttpRequest request)
    {
        return !request.Headers.ContainsKey("Authorization");
    }

    protected override HttpResponse ProcessRequest(HttpRequest request)
    {
        Console.WriteLine("🔐 Authenticating request...");
        request.Headers["Authenticated"] = "true";
        return _nextHandler?.Handle(request) ?? new HttpResponse { StatusCode = 200 };
    }
}

public class AuthorizationHandler : HttpHandler
{
    protected override bool CanHandle(HttpRequest request)
    {
        return request.Headers.ContainsKey("Authenticated");
    }

    protected override HttpResponse ProcessRequest(HttpRequest request)
    {
        Console.WriteLine("✅ Authorizing request...");
        return _nextHandler?.Handle(request) ?? new HttpResponse { StatusCode = 200 };
    }
}

public class LoggingHandler : HttpHandler
{
    protected override bool CanHandle(HttpRequest request)
    {
        return true;  // Always log
    }

    protected override HttpResponse ProcessRequest(HttpRequest request)
    {
        Console.WriteLine($"📝 Logging {request.Method} {request.Path}");
        var response = _nextHandler?.Handle(request) ?? new HttpResponse { StatusCode = 200 };
        Console.WriteLine($"📝 Response: {response.StatusCode}");
        return response;
    }
}

public class ApiHandler : HttpHandler
{
    protected override bool CanHandle(HttpRequest request)
    {
        return request.Path.StartsWith("/api/");
    }

    protected override HttpResponse ProcessRequest(HttpRequest request)
    {
        Console.WriteLine($"🌐 Processing API call: {request.Path}");
        return new HttpResponse { StatusCode = 200, Body = "API Response" };
    }
}

// Usage - chain handlers
var loggingHandler = new LoggingHandler();
var authHandler = new AuthenticationHandler();
var authzHandler = new AuthorizationHandler();
var apiHandler = new ApiHandler();

loggingHandler.SetNext(authHandler);
authHandler.SetNext(authzHandler);
authzHandler.SetNext(apiHandler);

var request = new HttpRequest { Path = "/api/users", Method = "GET" };
var response = loggingHandler.Handle(request);
```

### 3. Approval Chain

```csharp
public class ExpenseRequest
{
    public decimal Amount { get; set; }
    public string Description { get; set; }
    public bool IsApproved { get; set; }
}

public abstract class Approver
{
    protected Approver _nextApprover;
    protected decimal _approvalLimit;

    public void SetNext(Approver approver) => _nextApprover = approver;

    public void ProcessRequest(ExpenseRequest request)
    {
        if (CanApprove(request))
        {
            Approve(request);
        }
        else if (_nextApprover != null)
        {
            _nextApprover.ProcessRequest(request);
        }
        else
        {
            Console.WriteLine("❌ Request denied - no approver");
        }
    }

    protected abstract bool CanApprove(ExpenseRequest request);
    protected abstract void Approve(ExpenseRequest request);
}

public class Manager : Approver
{
    public Manager()
    {
        _approvalLimit = 1000;
    }

    protected override bool CanApprove(ExpenseRequest request)
    {
        return request.Amount <= _approvalLimit;
    }

    protected override void Approve(ExpenseRequest request)
    {
        Console.WriteLine($"✅ Manager approved: ${request.Amount}");
        request.IsApproved = true;
    }
}

public class Director : Approver
{
    public Director()
    {
        _approvalLimit = 10000;
    }

    protected override bool CanApprove(ExpenseRequest request)
    {
        return request.Amount <= _approvalLimit;
    }

    protected override void Approve(ExpenseRequest request)
    {
        Console.WriteLine($"✅ Director approved: ${request.Amount}");
        request.IsApproved = true;
    }
}

public class CFO : Approver
{
    public CFO()
    {
        _approvalLimit = decimal.MaxValue;
    }

    protected override bool CanApprove(ExpenseRequest request)
    {
        return request.Amount <= _approvalLimit;
    }

    protected override void Approve(ExpenseRequest request)
    {
        Console.WriteLine($"✅ CFO approved: ${request.Amount}");
        request.IsApproved = true;
    }
}

// Usage
var manager = new Manager();
var director = new Director();
var cfo = new CFO();

manager.SetNext(director);
director.SetNext(cfo);

var smallRequest = new ExpenseRequest { Amount = 500, Description = "Office supplies" };
var largeRequest = new ExpenseRequest { Amount = 50000, Description = "Equipment" };

manager.ProcessRequest(smallRequest);    // Manager approves
manager.ProcessRequest(largeRequest);    // Goes up to CFO
```

### 4. Event Handler Chain

```csharp
public class Event
{
    public string Type { get; set; }
    public string Data { get; set; }
    public bool Handled { get; set; }
}

public abstract class EventHandler
{
    protected EventHandler _nextHandler;

    public void SetNext(EventHandler handler) => _nextHandler = handler;

    public void Handle(Event @event)
    {
        if (CanHandle(@event))
        {
            ProcessEvent(@event);
            @event.Handled = true;
        }

        if (!@event.Handled && _nextHandler != null)
        {
            _nextHandler.Handle(@event);
        }
    }

    protected abstract bool CanHandle(Event @event);
    protected abstract void ProcessEvent(Event @event);
}

public class ClickHandler : EventHandler
{
    protected override bool CanHandle(Event @event) => @event.Type == "Click";

    protected override void ProcessEvent(Event @event)
    {
        Console.WriteLine($"🖱️ Click handled: {@event.Data}");
    }
}

public class KeyDownHandler : EventHandler
{
    protected override bool CanHandle(Event @event) => @event.Type == "KeyDown";

    protected override void ProcessEvent(Event @event)
    {
        Console.WriteLine($"⌨️ Key pressed: {@event.Data}");
    }
}

public class MouseMoveHandler : EventHandler
{
    protected override bool CanHandle(Event @event) => @event.Type == "MouseMove";

    protected override void ProcessEvent(Event @event)
    {
        Console.WriteLine($"🖱️ Mouse moved: {@event.Data}");
    }
}

public class DefaultHandler : EventHandler
{
    protected override bool CanHandle(Event @event) => true;  // Catch-all

    protected override void ProcessEvent(Event @event)
    {
        Console.WriteLine($"⚠️ Unhandled event: {@event.Type}");
    }
}

// Usage
var clickHandler = new ClickHandler();
var keyHandler = new KeyDownHandler();
var moveHandler = new MouseMoveHandler();
var defaultHandler = new DefaultHandler();

clickHandler.SetNext(keyHandler);
keyHandler.SetNext(moveHandler);
moveHandler.SetNext(defaultHandler);

clickHandler.Handle(new Event { Type = "Click", Data = "Button" });
clickHandler.Handle(new Event { Type = "KeyDown", Data = "Enter" });
clickHandler.Handle(new Event { Type = "Unknown", Data = "Some data" });
```

### 5. Support Ticket Handler

```csharp
public class SupportTicket
{
    public int Priority { get; set; }  // 1-5
    public string Issue { get; set; }
    public string Resolution { get; set; }
}

public abstract class SupportHandler
{
    protected SupportHandler _nextHandler;

    public void SetNext(SupportHandler handler) => _nextHandler = handler;

    public void HandleTicket(SupportTicket ticket)
    {
        if (CanHandle(ticket))
        {
            Resolve(ticket);
        }
        else if (_nextHandler != null)
        {
            _nextHandler.HandleTicket(ticket);
        }
    }

    protected abstract bool CanHandle(SupportTicket ticket);
    protected abstract void Resolve(SupportTicket ticket);
}

public class Level1Support : SupportHandler
{
    protected override bool CanHandle(SupportTicket ticket) => ticket.Priority <= 2;

    protected override void Resolve(SupportTicket ticket)
    {
        Console.WriteLine($"🙋 Level 1: Resolved {ticket.Issue}");
        ticket.Resolution = "Level 1 resolution";
    }
}

public class Level2Support : SupportHandler
{
    protected override bool CanHandle(SupportTicket ticket) => ticket.Priority <= 4;

    protected override void Resolve(SupportTicket ticket)
    {
        Console.WriteLine($"👨‍💼 Level 2: Resolved {ticket.Issue}");
        ticket.Resolution = "Level 2 resolution";
    }
}

public class Level3Support : SupportHandler
{
    protected override bool CanHandle(SupportTicket ticket) => ticket.Priority <= 5;

    protected override void Resolve(SupportTicket ticket)
    {
        Console.WriteLine($"👨‍💻 Level 3: Resolved {ticket.Issue}");
        ticket.Resolution = "Level 3 resolution";
    }
}

// Usage
var level1 = new Level1Support();
var level2 = new Level2Support();
var level3 = new Level3Support();

level1.SetNext(level2);
level2.SetNext(level3);

level1.HandleTicket(new SupportTicket { Priority = 1, Issue = "Login issue" });
level1.HandleTicket(new SupportTicket { Priority = 3, Issue = "Database problem" });
level1.HandleTicket(new SupportTicket { Priority = 5, Issue = "System architecture" });
```

---

## Chain of Responsibility Structure

```
      Client
        |
        v
    Handler1
        |
        v
    Handler2
        |
        v
    Handler3
```

---

## Pros and Cons

### Advantages
✅ **Decouples Sender and Receiver** - Sender doesn't know who handles request  
✅ **Single Responsibility** - Each handler handles one responsibility  
✅ **Open/Closed Principle** - Easy to add new handlers  
✅ **Flexible Chain** - Can modify chain at runtime  
✅ **Request Distribution** - Automatic routing based on handler logic  

### Disadvantages
❌ **No Guarantee of Handling** - Request might not be handled  
❌ **Debugging** - Hard to trace which handler processes request  
❌ **Performance** - Request traverses multiple handlers  
❌ **Order Dependent** - Handler order affects outcome  

---

## When to Use

### ✅ Use Chain of Responsibility When:
- Multiple objects might handle a request
- Handler isn't known in advance
- Set of handlers should be dynamic
- Want to decouple sender from receiver
- Logging/authorization/validation chains
- Event handling systems

### ❌ Don't Use When:
- Handler is known in advance
- Single handler always handles request
- Simple conditional logic sufficient
- Performance critical
- Simplicity valued

---

## Real-World Examples

- **Logging frameworks** - Log levels with multiple handlers
- **HTTP middleware** - Request pipeline processing
- **Exception handling** - Try-catch-finally chains
- **UI event handling** - Event bubbling/capturing
- **Support tickets** - Multi-level escalation
- **Approval workflows** - Manager → Director → CFO
- **Help desk systems** - Level-based support routing

---

## Interview Questions

**Q: How is Chain of Responsibility different from Observer?**
A: CoR passes request along chain until handled; Observer notifies all observers. CoR is sequential; Observer is broadcast.

**Q: What if no handler handles the request?**
A: Request goes unhandled. Should have default handler at end of chain to catch unhandled requests.

**Q: Can you modify the chain at runtime?**
A: Yes, you can insert/remove handlers dynamically by setting SetNext() at runtime.

**Q: What's the performance impact?**
A: Request might traverse many handlers. Add early termination if possible or organize by probability.

---

## Related Patterns

| Pattern | Relation |
|---------|----------|
| **Decorator** | Both wrap objects; Decorator adds behavior, CoR passes to next |
| **Command** | Can execute commands in chain |
| **Mediator** | Centralizes communication; CoR distributes |
| **Observer** | Observer notifies all; CoR passes to one handler |

---

## Summary

Chain of Responsibility elegantly distributes requests along a chain of handlers. Perfect for logging, authorization, support tickets, approval workflows, and any scenario where multiple handlers might process requests. Decouples sender from receiver while allowing dynamic chain modification.

**Key Takeaway:** Chain of Responsibility passes requests along a chain until an object handles it.
