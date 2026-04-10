namespace DP101.Core.BehavioralPatterns.State;

/// <summary>
/// STATE PATTERN
///
/// Intent: Allow an object to alter its behavior when its internal state changes.
/// The object will appear to change its class.
///
/// PROS:
/// - Encapsulates state-specific behavior
/// - Eliminates conditional statements
/// - Single Responsibility Principle
/// - Makes state transitions explicit
///
/// CONS:
/// - Increases number of classes
/// - Can be overkill for simple state machines
/// - Potential performance overhead
/// </summary>

// ========== EXAMPLE 1: TRAFFIC LIGHT ==========

public interface ITrafficLightState
{
    void Next(TrafficLight context);
    string GetColor();
    void Display();
}

public class RedLightState : ITrafficLightState
{
    public void Next(TrafficLight context)
    {
        context.SetState(new GreenLightState());
    }

    public string GetColor() => "Red";

    public void Display()
    {
        Console.WriteLine("🔴 Red Light - STOP");
    }
}

public class YellowLightState : ITrafficLightState
{
    public void Next(TrafficLight context)
    {
        context.SetState(new RedLightState());
    }

    public string GetColor() => "Yellow";

    public void Display()
    {
        Console.WriteLine("🟡 Yellow Light - CAUTION");
    }
}

public class GreenLightState : ITrafficLightState
{
    public void Next(TrafficLight context)
    {
        context.SetState(new YellowLightState());
    }

    public string GetColor() => "Green";

    public void Display()
    {
        Console.WriteLine("🟢 Green Light - GO");
    }
}

public class TrafficLight
{
    private ITrafficLightState _currentState;

    public TrafficLight()
    {
        _currentState = new RedLightState();
    }

    public void SetState(ITrafficLightState state)
    {
        _currentState = state;
    }

    public void Change()
    {
        _currentState.Next(this);
    }

    public void Display()
    {
        _currentState.Display();
    }
}

// ========== EXAMPLE 2: TCP CONNECTION ==========

public interface ITCPConnectionState
{
    void Open(TCPConnection context);
    void Close(TCPConnection context);
    void Send(TCPConnection context, string data);
    string GetStateName();
}

public class ClosedState : ITCPConnectionState
{
    public void Open(TCPConnection context)
    {
        Console.WriteLine("Opening connection...");
        context.SetState(new EstablishedState());
    }

    public void Close(TCPConnection context)
    {
        Console.WriteLine("Already closed");
    }

    public void Send(TCPConnection context, string data)
    {
        Console.WriteLine("Cannot send: Connection not established");
    }

    public string GetStateName() => "Closed";
}

public class ListeningState : ITCPConnectionState
{
    public void Open(TCPConnection context)
    {
        Console.WriteLine("Already listening");
    }

    public void Close(TCPConnection context)
    {
        context.SetState(new ClosedState());
    }

    public void Send(TCPConnection context, string data)
    {
        Console.WriteLine("Cannot send: Not connected");
    }

    public string GetStateName() => "Listening";
}

public class EstablishedState : ITCPConnectionState
{
    public void Open(TCPConnection context)
    {
        Console.WriteLine("Already established");
    }

    public void Close(TCPConnection context)
    {
        Console.WriteLine("Closing connection...");
        context.SetState(new ClosedState());
    }

    public void Send(TCPConnection context, string data)
    {
        Console.WriteLine($"Sending: {data}");
    }

    public string GetStateName() => "Established";
}

public class TCPConnection
{
    private ITCPConnectionState _state;

    public TCPConnection()
    {
        _state = new ClosedState();
    }

    public void SetState(ITCPConnectionState state)
    {
        _state = state;
    }

    public void Open() => _state.Open(this);
    public void Close() => _state.Close(this);
    public void Send(string data) => _state.Send(this, data);
    public string GetState() => _state.GetStateName();
}

// ========== EXAMPLE 3: MEDIA PLAYER ==========

public interface IMediaPlayerState
{
    void Play(MediaPlayer player);
    void Pause(MediaPlayer player);
    void Stop(MediaPlayer player);
    string GetState();
}

public class StoppedState : IMediaPlayerState
{
    public void Play(MediaPlayer player)
    {
        Console.WriteLine("▶️  Playing...");
        player.SetState(new PlayingState());
    }

    public void Pause(MediaPlayer player)
    {
        Console.WriteLine("Cannot pause: Not playing");
    }

    public void Stop(MediaPlayer player)
    {
        Console.WriteLine("Already stopped");
    }

    public string GetState() => "Stopped";
}

public class PlayingState : IMediaPlayerState
{
    public void Play(MediaPlayer player)
    {
        Console.WriteLine("Already playing");
    }

    public void Pause(MediaPlayer player)
    {
        Console.WriteLine("⏸️  Paused");
        player.SetState(new PausedState());
    }

    public void Stop(MediaPlayer player)
    {
        Console.WriteLine("⏹️  Stopped");
        player.SetState(new StoppedState());
    }

    public string GetState() => "Playing";
}

public class PausedState : IMediaPlayerState
{
    public void Play(MediaPlayer player)
    {
        Console.WriteLine("▶️  Resuming...");
        player.SetState(new PlayingState());
    }

    public void Pause(MediaPlayer player)
    {
        Console.WriteLine("Already paused");
    }

    public void Stop(MediaPlayer player)
    {
        Console.WriteLine("⏹️  Stopped");
        player.SetState(new StoppedState());
    }

    public string GetState() => "Paused";
}

public class MediaPlayer
{
    private IMediaPlayerState _state;

    public MediaPlayer()
    {
        _state = new StoppedState();
    }

    public void SetState(IMediaPlayerState state)
    {
        _state = state;
    }

    public void Play() => _state.Play(this);
    public void Pause() => _state.Pause(this);
    public void Stop() => _state.Stop(this);
    public string GetCurrentState() => _state.GetState();
}

// ========== EXAMPLE 4: ORDER STATE MACHINE ==========

public interface IOrderState
{
    void Process(Order order);
    void Cancel(Order order);
    void Ship(Order order);
    string GetState();
}

public class NewOrderState : IOrderState
{
    public void Process(Order order)
    {
        Console.WriteLine("Processing order...");
        order.SetState(new ProcessingState());
    }

    public void Cancel(Order order)
    {
        Console.WriteLine("Canceling new order...");
        order.SetState(new CancelledState());
    }

    public void Ship(Order order)
    {
        Console.WriteLine("Cannot ship: Order not processed");
    }

    public string GetState() => "New";
}

public class ProcessingState : IOrderState
{
    public void Process(Order order)
    {
        Console.WriteLine("Already processing");
    }

    public void Cancel(Order order)
    {
        Console.WriteLine("Canceling order...");
        order.SetState(new CancelledState());
    }

    public void Ship(Order order)
    {
        Console.WriteLine("Shipping order...");
        order.SetState(new ShippedState());
    }

    public string GetState() => "Processing";
}

public class ShippedState : IOrderState
{
    public void Process(Order order)
    {
        Console.WriteLine("Already shipped");
    }

    public void Cancel(Order order)
    {
        Console.WriteLine("Cannot cancel: Already shipped");
    }

    public void Ship(Order order)
    {
        Console.WriteLine("Already shipped");
    }

    public string GetState() => "Shipped";
}

public class DeliveredState : IOrderState
{
    public void Process(Order order)
    {
        Console.WriteLine("Already delivered");
    }

    public void Cancel(Order order)
    {
        Console.WriteLine("Cannot cancel: Already delivered");
    }

    public void Ship(Order order)
    {
        Console.WriteLine("Already delivered");
    }

    public string GetState() => "Delivered";
}

public class CancelledState : IOrderState
{
    public void Process(Order order)
    {
        Console.WriteLine("Cannot process: Order cancelled");
    }

    public void Cancel(Order order)
    {
        Console.WriteLine("Already cancelled");
    }

    public void Ship(Order order)
    {
        Console.WriteLine("Cannot ship: Order cancelled");
    }

    public string GetState() => "Cancelled";
}

public class Order
{
    private IOrderState _state;
    public string OrderId { get; }

    public Order(string orderId)
    {
        OrderId = orderId;
        _state = new NewOrderState();
    }

    public void SetState(IOrderState state)
    {
        _state = state;
    }

    public void Process() => _state.Process(this);
    public void Cancel() => _state.Cancel(this);
    public void Ship() => _state.Ship(this);
    public string GetCurrentState() => _state.GetState();
}

// ========== EXAMPLE 5: DOCUMENT STATE ==========

public interface IDocumentState
{
    void Publish(Document doc);
    void Review(Document doc);
    void Reject(Document doc);
    string GetStatus();
}

public class DraftState : IDocumentState
{
    public void Publish(Document doc)
    {
        Console.WriteLine("Publishing draft...");
        doc.SetState(new PublishedState());
    }

    public void Review(Document doc)
    {
        Console.WriteLine("Sending for review...");
        doc.SetState(new ReviewState());
    }

    public void Reject(Document doc)
    {
        Console.WriteLine("Cannot reject draft");
    }

    public string GetStatus() => "Draft";
}

public class ReviewState : IDocumentState
{
    public void Publish(Document doc)
    {
        Console.WriteLine("Cannot publish: Under review");
    }

    public void Review(Document doc)
    {
        Console.WriteLine("Already under review");
    }

    public void Reject(Document doc)
    {
        Console.WriteLine("Rejected. Returning to draft...");
        doc.SetState(new DraftState());
    }

    public string GetStatus() => "Review";
}

public class PublishedState : IDocumentState
{
    public void Publish(Document doc)
    {
        Console.WriteLine("Already published");
    }

    public void Review(Document doc)
    {
        Console.WriteLine("Cannot review: Already published");
    }

    public void Reject(Document doc)
    {
        Console.WriteLine("Cannot reject: Already published");
    }

    public string GetStatus() => "Published";
}

public class Document
{
    private IDocumentState _state;
    public string Title { get; }

    public Document(string title)
    {
        Title = title;
        _state = new DraftState();
    }

    public void SetState(IDocumentState state)
    {
        _state = state;
    }

    public void Publish() => _state.Publish(this);
    public void Review() => _state.Review(this);
    public void Reject() => _state.Reject(this);
    public string GetStatus() => _state.GetStatus();
}
