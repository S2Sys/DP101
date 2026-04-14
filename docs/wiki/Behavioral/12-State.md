# State Pattern

## Overview

**Category:** Behavioral Pattern  
**Purpose:** Allow an object to alter its behavior when its internal state changes. The object will appear to change its class.  
**Complexity:** Medium  
**Key Concept:** State-dependent behavior

## Problem

Object behavior depends on state, creating complex conditionals:

```csharp
// Problem: State-dependent behavior with many conditions
public class Order
{
    public string Status { get; set; }

    public void Process()
    {
        if (Status == "New")
        {
            Console.WriteLine("Validating order");
            Status = "Validated";
        }
        else if (Status == "Validated")
        {
            Console.WriteLine("Processing payment");
            Status = "Processing";
        }
        else if (Status == "Processing")
        {
            Console.WriteLine("Shipping order");
            Status = "Shipped";
        }
    }

    public void Cancel()
    {
        if (Status == "New" || Status == "Validated")
            Status = "Cancelled";
        else if (Status == "Shipped")
            throw new InvalidOperationException("Cannot cancel shipped order");
    }
}
// Adding states means modifying this class everywhere!
```

## Solution

Encapsulate state behavior in separate state classes:

```csharp
public interface IOrderState
{
    void Process(Order order);
    void Cancel(Order order);
}

public class NewOrderState : IOrderState
{
    public void Process(Order order)
    {
        Console.WriteLine("Validating order");
        order.State = new ValidatedOrderState();
    }

    public void Cancel(Order order)
    {
        Console.WriteLine("Cancelling new order");
        order.State = new CancelledOrderState();
    }
}

public class Order
{
    public IOrderState State { get; set; }

    public Order() => State = new NewOrderState();

    public void Process() => State.Process(this);
    public void Cancel() => State.Cancel(this);
}

// Adding new state: just create new state class, no changes to Order!
```

## Implementation Approaches

### 1. Traffic Light State Machine

```csharp
public interface ITrafficLightState
{
    void Next(TrafficLight light);
    void Display();
}

public class RedLightState : ITrafficLightState
{
    public void Next(TrafficLight light)
    {
        Console.WriteLine("Red -> Green");
        light.SetState(new GreenLightState());
    }

    public void Display() => Console.WriteLine("🔴 STOP - Red Light");
}

public class GreenLightState : ITrafficLightState
{
    public void Next(TrafficLight light)
    {
        Console.WriteLine("Green -> Yellow");
        light.SetState(new YellowLightState());
    }

    public void Display() => Console.WriteLine("🟢 GO - Green Light");
}

public class YellowLightState : ITrafficLightState
{
    public void Next(TrafficLight light)
    {
        Console.WriteLine("Yellow -> Red");
        light.SetState(new RedLightState());
    }

    public void Display() => Console.WriteLine("🟡 CAUTION - Yellow Light");
}

public class TrafficLight
{
    private ITrafficLightState _state;

    public TrafficLight() => _state = new RedLightState();

    public void SetState(ITrafficLightState state) => _state = state;
    public void Change() => _state.Next(this);
    public void Display() => _state.Display();
}

// Usage
var light = new TrafficLight();
light.Display();  // Red Light
light.Change();   // Changes to Green
light.Display();  // Green Light
```

### 2. TCP Connection States

```csharp
public interface ITCPState
{
    void Open(TCPConnection connection);
    void Close(TCPConnection connection);
    void Send(string data);
}

public class ClosedState : ITCPState
{
    public void Open(TCPConnection connection)
    {
        Console.WriteLine("Opening connection...");
        connection.SetState(new EstablishedState());
    }

    public void Close(TCPConnection connection)
    {
        Console.WriteLine("Already closed");
    }

    public void Send(string data)
    {
        Console.WriteLine("Cannot send: connection closed");
    }
}

public class EstablishedState : ITCPState
{
    public void Open(TCPConnection connection)
    {
        Console.WriteLine("Already open");
    }

    public void Close(TCPConnection connection)
    {
        Console.WriteLine("Closing connection...");
        connection.SetState(new ClosedState());
    }

    public void Send(string data)
    {
        Console.WriteLine($"Sending: {data}");
    }
}

public class ListenState : ITCPState
{
    public void Open(TCPConnection connection)
    {
        Console.WriteLine("Already listening");
    }

    public void Close(TCPConnection connection)
    {
        Console.WriteLine("Stopping listener...");
        connection.SetState(new ClosedState());
    }

    public void Send(string data)
    {
        Console.WriteLine("Cannot send: in listen state");
    }
}

public class TCPConnection
{
    private ITCPState _state;

    public TCPConnection() => _state = new ClosedState();

    public void SetState(ITCPState state) => _state = state;
    public void Open() => _state.Open(this);
    public void Close() => _state.Close(this);
    public void Send(string data) => _state.Send(data);
}

// Usage
var connection = new TCPConnection();
connection.Send("Hello");  // Cannot send: closed
connection.Open();         // Opens connection
connection.Send("Hello");  // Sends: Hello
connection.Close();        // Closes connection
```

### 3. Media Player States

```csharp
public interface IMediaPlayerState
{
    void Play(MediaPlayer player);
    void Pause(MediaPlayer player);
    void Stop(MediaPlayer player);
}

public class PlayingState : IMediaPlayerState
{
    public void Play(MediaPlayer player)
    {
        Console.WriteLine("Already playing");
    }

    public void Pause(MediaPlayer player)
    {
        Console.WriteLine("▶ ➜ ⏸  Pausing...");
        player.SetState(new PausedState());
    }

    public void Stop(MediaPlayer player)
    {
        Console.WriteLine("▶ ➜ ⏹  Stopping...");
        player.SetState(new StoppedState());
    }
}

public class PausedState : IMediaPlayerState
{
    public void Play(MediaPlayer player)
    {
        Console.WriteLine("⏸  ➜ ▶  Resuming...");
        player.SetState(new PlayingState());
    }

    public void Pause(MediaPlayer player)
    {
        Console.WriteLine("Already paused");
    }

    public void Stop(MediaPlayer player)
    {
        Console.WriteLine("⏸  ➜ ⏹  Stopping...");
        player.SetState(new StoppedState());
    }
}

public class StoppedState : IMediaPlayerState
{
    public void Play(MediaPlayer player)
    {
        Console.WriteLine("⏹ ➜ ▶  Playing...");
        player.SetState(new PlayingState());
    }

    public void Pause(MediaPlayer player)
    {
        Console.WriteLine("Cannot pause: not playing");
    }

    public void Stop(MediaPlayer player)
    {
        Console.WriteLine("Already stopped");
    }
}

public class MediaPlayer
{
    private IMediaPlayerState _state;
    public string CurrentTrack { get; set; }

    public MediaPlayer() => _state = new StoppedState();

    public void SetState(IMediaPlayerState state) => _state = state;
    public void Play() => _state.Play(this);
    public void Pause() => _state.Pause(this);
    public void Stop() => _state.Stop(this);
}

// Usage
var player = new MediaPlayer { CurrentTrack = "Song.mp3" };
player.Play();    // Playing
player.Pause();   // Pausing
player.Play();    // Resuming
player.Stop();    // Stopping
```

### 4. Order Processing States

```csharp
public interface IOrderState
{
    void Process(Order order);
    void Cancel(Order order);
    void Ship(Order order);
}

public class NewOrderState : IOrderState
{
    public void Process(Order order)
    {
        Console.WriteLine("📋 New Order -> Validating");
        order.SetState(new ValidatedOrderState());
    }

    public void Cancel(Order order)
    {
        Console.WriteLine("❌ Cancelling new order");
        order.SetState(new CancelledOrderState());
    }

    public void Ship(Order order)
    {
        Console.WriteLine("Cannot ship unvalidated order");
    }
}

public class ValidatedOrderState : IOrderState
{
    public void Process(Order order)
    {
        Console.WriteLine("💳 Validated -> Processing Payment");
        order.SetState(new ProcessingOrderState());
    }

    public void Cancel(Order order)
    {
        Console.WriteLine("❌ Cancelling validated order");
        order.SetState(new CancelledOrderState());
    }

    public void Ship(Order order)
    {
        Console.WriteLine("Cannot ship before payment");
    }
}

public class ProcessingOrderState : IOrderState
{
    public void Process(Order order)
    {
        Console.WriteLine("✅ Payment Processed -> Ready to Ship");
        order.SetState(new ReadyToShipOrderState());
    }

    public void Cancel(Order order)
    {
        Console.WriteLine("❌ Refunding and cancelling");
        order.SetState(new CancelledOrderState());
    }

    public void Ship(Order order)
    {
        Console.WriteLine("Cannot ship during payment processing");
    }
}

public class ReadyToShipOrderState : IOrderState
{
    public void Process(Order order)
    {
        Console.WriteLine("Order already processed");
    }

    public void Cancel(Order order)
    {
        Console.WriteLine("Cannot cancel: too late");
    }

    public void Ship(Order order)
    {
        Console.WriteLine("📦 Shipping order");
        order.SetState(new ShippedOrderState());
    }
}

public class ShippedOrderState : IOrderState
{
    public void Process(Order order) => Console.WriteLine("Order already shipped");
    public void Cancel(Order order) => Console.WriteLine("Cannot cancel shipped order");
    public void Ship(Order order) => Console.WriteLine("Order already shipped");
}

public class CancelledOrderState : IOrderState
{
    public void Process(Order order) => Console.WriteLine("Cannot process cancelled order");
    public void Cancel(Order order) => Console.WriteLine("Already cancelled");
    public void Ship(Order order) => Console.WriteLine("Cannot ship cancelled order");
}

public class Order
{
    public int Id { get; set; }
    private IOrderState _state;

    public Order(int id)
    {
        Id = id;
        _state = new NewOrderState();
    }

    public void SetState(IOrderState state) => _state = state;
    public void Process() => _state.Process(this);
    public void Cancel() => _state.Cancel(this);
    public void Ship() => _state.Ship(this);
}

// Usage
var order = new Order(1);
order.Process();  // New -> Validated
order.Process();  // Validated -> Processing
order.Process();  // Processing -> ReadyToShip
order.Ship();     // Ship order
```

### 5. Document States

```csharp
public interface IDocumentState
{
    void Publish(Document doc);
    void Review(Document doc);
    void Archive(Document doc);
}

public class DraftState : IDocumentState
{
    public void Publish(Document doc)
    {
        Console.WriteLine("📝 Draft -> Under Review");
        doc.SetState(new ReviewState());
    }

    public void Review(Document doc) => Console.WriteLine("Cannot review: not under review");
    public void Archive(Document doc) => Console.WriteLine("Cannot archive: must be published");
}

public class ReviewState : IDocumentState
{
    public void Publish(Document doc)
    {
        Console.WriteLine("✅ Review Complete -> Published");
        doc.SetState(new PublishedState());
    }

    public void Review(Document doc) => Console.WriteLine("Under review");
    public void Archive(Document doc) => Console.WriteLine("Cannot archive: under review");
}

public class PublishedState : IDocumentState
{
    public void Publish(Document doc) => Console.WriteLine("Already published");
    public void Review(Document doc) => Console.WriteLine("Cannot review: already published");

    public void Archive(Document doc)
    {
        Console.WriteLine("📚 Published -> Archived");
        doc.SetState(new ArchivedState());
    }
}

public class ArchivedState : IDocumentState
{
    public void Publish(Document doc) => Console.WriteLine("Cannot publish: archived");
    public void Review(Document doc) => Console.WriteLine("Cannot review: archived");
    public void Archive(Document doc) => Console.WriteLine("Already archived");
}

public class Document
{
    public string Title { get; set; }
    private IDocumentState _state;

    public Document(string title)
    {
        Title = title;
        _state = new DraftState();
    }

    public void SetState(IDocumentState state) => _state = state;
    public void Publish() => _state.Publish(this);
    public void Review() => _state.Review(this);
    public void Archive() => _state.Archive(this);
}
```

---

## State Pattern Structure

```
    Context
      |
      | uses
      v
  IState (Interface)
    /  |  \
   /   |   \
State1 State2 State3
  |
  |-- OnEnter()
  |-- OnExit()
  |-- Handle()
```

---

## Pros and Cons

### Advantages
✅ **Eliminates Conditionals** - No complex if-else chains  
✅ **Encapsulates State Logic** - Each state in separate class  
✅ **Open/Closed Principle** - Easy to add new states  
✅ **Single Responsibility** - Each state handles one behavior  
✅ **Clear State Transitions** - Explicit state changes  

### Disadvantages
❌ **More Classes** - One per state  
❌ **Complexity** - Overhead for simple state machines  
❌ **Over-Design** - May be overkill  
❌ **Circular Dependencies** - Context and states may reference each other  

---

## When to Use

### ✅ Use State When:
- Object behavior depends on state
- Many conditional branches based on state
- State-specific operations
- Complex state transitions
- States change frequently
- Need to isolate state logic

### ❌ Don't Use When:
- Few states
- Simple behavior
- Inheritance sufficient
- Few state changes
- Simplicity valued

---

## Interview Questions

**Q: What's the difference between State and Strategy patterns?**
A: State encapsulates state-dependent behavior; Strategy encapsulates algorithm choice. State transitions are internal; Strategy is chosen by client.

**Q: How do states transition?**
A: Context passes itself to state methods; state decides next state and calls context.SetState(newState).

**Q: Can states be shared between contexts?**
A: Yes, if they don't store context data. Use context parameter to method instead.

**Q: What's the biggest advantage of State pattern?**
A: Eliminates massive if-else chains. Each state is isolated, making code maintainable.

---

## State vs. Strategy Comparison

| Aspect | State | Strategy |
|--------|-------|----------|
| **Purpose** | Encapsulate state-based behavior | Encapsulate algorithm choice |
| **Selection** | Object selects based on state | Client selects strategy |
| **Change When** | Internal state changes | Client changes algorithm |
| **Coupling** | States tied to context | Strategy independent |
| **Transitions** | Explicit state changes | Strategy replacement |

---

## Real-World Examples

- **Order Processing** - New, Validated, Processing, Shipped, Cancelled
- **Traffic Lights** - Red, Yellow, Green
- **TCP Connections** - Closed, Listen, Established
- **Media Players** - Playing, Paused, Stopped
- **Document Workflows** - Draft, Review, Published, Archived
- **Game Characters** - Idle, Walking, Running, Jumping

---

## Summary

State pattern elegantly handles objects with complex state-dependent behavior. Perfect for order processing, state machines, workflows, and any scenario with multiple states affecting behavior. Use when state transitions and state-specific operations are central to the design.

**Key Takeaway:** State encapsulates state-dependent behavior, eliminating complex conditionals.
