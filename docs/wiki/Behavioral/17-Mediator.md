# Mediator Pattern

## Overview

**Category:** Behavioral Pattern  
**Purpose:** Define an object that encapsulates how a set of objects interact. Promote loose coupling by keeping objects from referring to each other explicitly.  
**Also Called:** Controller, Intermediary  
**Complexity:** Medium

## Problem

Objects communicate directly, creating tight coupling:

```csharp
// Problem: Direct coupling between objects
public class Colleague1
{
    private Colleague2 _colleague2;
    private Colleague3 _colleague3;

    public void DoSomething()
    {
        _colleague2.React();
        _colleague3.Update();  // Direct dependencies!
    }
}

// Adding colleagues or changing interactions requires modifying all classes
```

## Solution

Use mediator to coordinate interactions:

```csharp
public interface IMediator
{
    void SendMessage(string message, Colleague colleague);
}

public abstract class Colleague
{
    protected IMediator _mediator;

    public Colleague(IMediator mediator) => _mediator = mediator;

    public virtual void SendMessage(string message) =>
        _mediator.SendMessage(message, this);

    public abstract void ReceiveMessage(string message);
}

// Colleagues communicate through mediator, not directly!
```

## Implementation Approaches

### 1. Chat Room Mediator

```csharp
public interface IChatRoomMediator
{
    void ShowMessage(User sender, string message);
    void RegisterUser(User user);
}

public class ChatRoom : IChatRoomMediator
{
    private List<User> _users = new();

    public void RegisterUser(User user)
    {
        _users.Add(user);
        Console.WriteLine($"👤 {user.Name} joined the chat");
    }

    public void ShowMessage(User sender, string message)
    {
        var timestamp = DateTime.Now.ToString("HH:mm:ss");
        Console.WriteLine($"[{timestamp}] {sender.Name}: {message}");

        foreach (var user in _users)
        {
            if (user != sender)
            {
                user.ReceiveMessage(sender.Name, message);
            }
        }
    }
}

public class User
{
    private IChatRoomMediator _chatRoom;
    public string Name { get; set; }

    public User(string name, IChatRoomMediator chatRoom)
    {
        Name = name;
        _chatRoom = chatRoom;
        _chatRoom.RegisterUser(this);
    }

    public void SendMessage(string message)
    {
        Console.WriteLine($"💬 {Name} sends: {message}");
        _chatRoom.ShowMessage(this, message);
    }

    public void ReceiveMessage(string senderName, string message)
    {
        Console.WriteLine($"📨 {Name} received from {senderName}: {message}");
    }
}

// Usage
var chatRoom = new ChatRoom();
var alice = new User("Alice", chatRoom);
var bob = new User("Bob", chatRoom);
var charlie = new User("Charlie", chatRoom);

alice.SendMessage("Hello everyone!");
bob.SendMessage("Hi Alice!");
charlie.SendMessage("Hey!");
```

### 2. Air Traffic Control Mediator

```csharp
public interface IAirTrafficControl
{
    void RegisterAirplane(Airplane airplane);
    void RequestLanding(Airplane airplane);
    void RequestTakeoff(Airplane airplane);
    void NotifyAirplanes(string message);
}

public class AirTrafficControl : IAirTrafficControl
{
    private List<Airplane> _airplanes = new();
    private Queue<Airplane> _landingQueue = new();
    private bool _runwayAvailable = true;

    public void RegisterAirplane(Airplane airplane)
    {
        _airplanes.Add(airplane);
        Console.WriteLine($"✈️ {airplane.CallSign} registered");
    }

    public void RequestLanding(Airplane airplane)
    {
        _landingQueue.Enqueue(airplane);
        Console.WriteLine($"📡 {airplane.CallSign} requests landing");

        if (_runwayAvailable)
        {
            ProcessLanding();
        }
    }

    public void RequestTakeoff(Airplane airplane)
    {
        if (_runwayAvailable)
        {
            _runwayAvailable = false;
            Console.WriteLine($"✈️ {airplane.CallSign} cleared for takeoff");
            airplane.Takeoff();
            _runwayAvailable = true;

            if (_landingQueue.Count > 0)
            {
                ProcessLanding();
            }
        }
        else
        {
            Console.WriteLine($"⏳ {airplane.CallSign} waiting for takeoff");
        }
    }

    public void NotifyAirplanes(string message)
    {
        foreach (var airplane in _airplanes)
        {
            airplane.ReceiveNotification(message);
        }
    }

    private void ProcessLanding()
    {
        if (_landingQueue.Count > 0)
        {
            var airplane = _landingQueue.Dequeue();
            _runwayAvailable = false;
            Console.WriteLine($"✈️ {airplane.CallSign} cleared for landing");
            airplane.Land();
            _runwayAvailable = true;

            if (_landingQueue.Count > 0)
            {
                ProcessLanding();
            }
        }
    }
}

public class Airplane
{
    private IAirTrafficControl _atc;
    public string CallSign { get; set; }

    public Airplane(string callSign, IAirTrafficControl atc)
    {
        CallSign = callSign;
        _atc = atc;
        _atc.RegisterAirplane(this);
    }

    public void RequestLanding() => _atc.RequestLanding(this);
    public void RequestTakeoff() => _atc.RequestTakeoff(this);
    public void Land() => Console.WriteLine($"🛬 {CallSign} landed");
    public void Takeoff() => Console.WriteLine($"🛫 {CallSign} takeoff");
    public void ReceiveNotification(string message) =>
        Console.WriteLine($"📢 {CallSign} received: {message}");
}

// Usage
var atc = new AirTrafficControl();
var plane1 = new Airplane("AA100", atc);
var plane2 = new Airplane("UA200", atc);

plane1.RequestLanding();
plane2.RequestTakeoff();
atc.NotifyAirplanes("Weather: Clear skies");
```

### 3. Dialog Box Mediator

```csharp
public interface IDialogMediator
{
    void SendMessage(string message, UIComponent component);
    void RegisterComponent(UIComponent component);
}

public class DialogBox : IDialogMediator
{
    private Dictionary<string, UIComponent> _components = new();

    public void RegisterComponent(UIComponent component)
    {
        _components[component.Name] = component;
    }

    public void SendMessage(string message, UIComponent sender)
    {
        Console.WriteLine($"📨 {sender.Name} sent: {message}");

        // Mediator handles the interaction logic
        if (sender.Name == "OKButton")
        {
            if (_components.ContainsKey("TextField"))
            {
                var textField = _components["TextField"];
                textField.ReceiveMessage($"Text was: {message}");
            }
        }
        else if (sender.Name == "CancelButton")
        {
            Console.WriteLine("❌ Dialog cancelled");
        }
    }
}

public abstract class UIComponent
{
    protected IDialogMediator _mediator;
    public string Name { get; set; }

    public UIComponent(string name, IDialogMediator mediator)
    {
        Name = name;
        _mediator = mediator;
        _mediator.RegisterComponent(this);
    }

    public abstract void ReceiveMessage(string message);
}

public class Button : UIComponent
{
    public Button(string name, IDialogMediator mediator) : base(name, mediator) { }

    public void Click(string data)
    {
        Console.WriteLine($"🔘 {Name} clicked");
        _mediator.SendMessage(data, this);
    }

    public override void ReceiveMessage(string message) =>
        Console.WriteLine($"🔘 {Name} received: {message}");
}

public class TextField : UIComponent
{
    public string Value { get; set; }

    public TextField(string name, IDialogMediator mediator) : base(name, mediator) { }

    public override void ReceiveMessage(string message) =>
        Console.WriteLine($"📝 {Name} received: {message}");
}

// Usage
var dialog = new DialogBox();
var okButton = new Button("OKButton", dialog);
var cancelButton = new Button("CancelButton", dialog);
var textField = new TextField("TextField", dialog);

okButton.Click("UserInput123");
cancelButton.Click("");
```

### 4. Team Mediator

```csharp
public interface ITeamMediator
{
    void SendRequest(TeamMember sender, string request);
    void RegisterMember(TeamMember member);
}

public class TeamLead : ITeamMediator
{
    private Dictionary<string, TeamMember> _members = new();

    public void RegisterMember(TeamMember member)
    {
        _members[member.Name] = member;
        Console.WriteLine($"👤 {member.Name} joined team");
    }

    public void SendRequest(TeamMember sender, string request)
    {
        Console.WriteLine($"📧 {sender.Name} requests: {request}");

        // Team lead coordinates responses
        if (request.Contains("Code Review"))
        {
            foreach (var member in _members.Values)
            {
                if (member.Name != sender.Name && member.Role == "Developer")
                {
                    member.ReceiveTask($"Review {sender.Name}'s code");
                }
            }
        }
        else if (request.Contains("Design"))
        {
            if (_members.ContainsKey("Designer"))
            {
                _members["Designer"].ReceiveTask(request);
            }
        }
    }
}

public class TeamMember
{
    private ITeamMediator _teamLead;
    public string Name { get; set; }
    public string Role { get; set; }

    public TeamMember(string name, string role, ITeamMediator teamLead)
    {
        Name = name;
        Role = role;
        _teamLead = teamLead;
        _teamLead.RegisterMember(this);
    }

    public void MakeRequest(string request) =>
        _teamLead.SendRequest(this, request);

    public void ReceiveTask(string task) =>
        Console.WriteLine($"👤 {Name} received task: {task}");
}

// Usage
var teamLead = new TeamLead();
var alice = new TeamMember("Alice", "Developer", teamLead);
var bob = new TeamMember("Bob", "Developer", teamLead);
var designer = new TeamMember("Designer", "Designer", teamLead);

alice.MakeRequest("Code Review");
bob.MakeRequest("Design needed");
```

### 5. Chat Room with Roles

```csharp
public interface IChatMediator
{
    void SendMessage(string message, ChatUser sender);
    void RegisterUser(ChatUser user);
    void RemoveUser(ChatUser user);
}

public class ModerationChatRoom : IChatMediator
{
    private Dictionary<string, ChatUser> _users = new();
    private Queue<string> _messages = new();

    public void RegisterUser(ChatUser user)
    {
        _users[user.Name] = user;
        Console.WriteLine($"✅ {user.Name} ({user.Role}) joined");
    }

    public void RemoveUser(ChatUser user)
    {
        _users.Remove(user.Name);
        Console.WriteLine($"❌ {user.Name} left");
    }

    public void SendMessage(string message, ChatUser sender)
    {
        if (sender.Role == "Admin")
        {
            BroadcastMessage(sender.Name, message);
        }
        else if (sender.Role == "Moderator")
        {
            if (!ContainsBadWords(message))
            {
                BroadcastMessage(sender.Name, message);
            }
            else
            {
                Console.WriteLine($"⚠️ Message from {sender.Name} blocked (inappropriate)");
            }
        }
        else
        {
            _messages.Enqueue($"{sender.Name}: {message}");
            Console.WriteLine($"📝 Message queued for moderation");
        }
    }

    private void BroadcastMessage(string senderName, string message)
    {
        Console.WriteLine($"📢 {senderName}: {message}");
        foreach (var user in _users.Values)
        {
            if (user.Name != senderName)
            {
                user.ReceiveMessage(senderName, message);
            }
        }
    }

    private bool ContainsBadWords(string message) =>
        message.ToLower().Contains("badword");
}

public class ChatUser
{
    private IChatMediator _chatRoom;
    public string Name { get; set; }
    public string Role { get; set; }

    public ChatUser(string name, string role, IChatMediator chatRoom)
    {
        Name = name;
        Role = role;
        _chatRoom = chatRoom;
        _chatRoom.RegisterUser(this);
    }

    public void SendMessage(string message) =>
        _chatRoom.SendMessage(message, this);

    public void ReceiveMessage(string senderName, string message) =>
        Console.WriteLine($"📨 {Name} from {senderName}: {message}");
}

// Usage
var chatRoom = new ModerationChatRoom();
var admin = new ChatUser("Admin", "Admin", chatRoom);
var moderator = new ChatUser("Mod", "Moderator", chatRoom);
var user1 = new ChatUser("User1", "User", chatRoom);

admin.SendMessage("Welcome everyone!");
moderator.SendMessage("Follow the rules");
user1.SendMessage("Hello");
```

---

## Mediator Pattern Structure

```
       Colleague1  Colleague2  Colleague3
            \         |         /
             \        |        /
              \       |       /
               \      |      /
                \     |     /
                 \    |    /
                  \   |   /
                   Mediator
```

---

## Pros and Cons

### Advantages
✅ **Reduced Coupling** - Colleagues don't reference each other  
✅ **Centralized Control** - Interaction logic in one place  
✅ **Simplified Communication** - Colleagues talk to mediator only  
✅ **Easy to Modify** - Change interactions by modifying mediator  
✅ **Reusable** - Colleagues can be reused with different mediators  

### Disadvantages
❌ **Mediator Complexity** - Can become complex with many colleagues  
❌ **Single Point of Failure** - Mediator issues affect all colleagues  
❌ **God Object** - Mediator can become too large  
❌ **Over-Design** - May be overkill for simple interactions  

---

## When to Use

### ✅ Use Mediator When:
- Multiple objects communicate with each other
- Communication is complex
- Reusability is important
- Want to decouple objects
- Interaction logic needs to be centralized
- Colleagues should be independent

### ❌ Don't Use When:
- Simple one-to-one communication
- Few objects involved
- Direct coupling acceptable
- Performance critical
- Simplicity valued

---

## Interview Questions

**Q: What's the difference between Mediator and Observer?**
A: Mediator centralizes communication; Observer broadcasts to all. Mediator is one-to-many orchestration; Observer is many-to-many notification.

**Q: Can mediator become too complex?**
A: Yes, if too many interactions. Break into multiple mediators or use simpler patterns.

**Q: How does Mediator differ from Facade?**
A: Facade simplifies external interface; Mediator decouples internal objects. Facade is external simplification; Mediator is internal coordination.

**Q: What if one colleague needs to know about others?**
A: That defeats Mediator's purpose. All communication through mediator.

---

## Related Patterns

| Pattern | Relation |
|---------|----------|
| **Observer** | Both decouple; Observer broadcasts, Mediator orchestrates |
| **Facade** | Both simplify; Facade external, Mediator internal |
| **Command** | Can use commands in mediated communication |

---

## Summary

Mediator pattern centralizes object interactions through a mediator object, reducing coupling and complexity. Perfect for chat rooms, air traffic control, dialog boxes, team coordination, and any scenario with complex multi-object interactions. Mediator becomes the coordinator, while colleagues remain independent.

**Key Takeaway:** Mediator encapsulates how objects interact, promoting loose coupling.
