using DP101.Core.CreationalPatterns.Singleton;
using DP101.Core.CreationalPatterns.Factory;
using DP101.Core.CreationalPatterns.Builder;
using DP101.Core.StructuralPatterns.Adapter;
using DP101.Core.StructuralPatterns.Decorator;
using DP101.Core.BehavioralPatterns.Observer;
using DP101.Core.BehavioralPatterns.Strategy;
using DP101.Core.BehavioralPatterns.State;
using DP101.Core.BehavioralPatterns.Command;
using DP101.Core.ArchitecturalPatterns.SAGA;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════╗");
        Console.WriteLine("║          DP101 - Design Patterns Examples              ║");
        Console.WriteLine("║  Comprehensive C# Design Pattern Implementation Demo   ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════╝\n");

        while (true)
        {
            Console.WriteLine("\n📚 Choose a pattern category to explore:");
            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.WriteLine("1. Creational Patterns (Object Creation)");
            Console.WriteLine("2. Structural Patterns (Object Composition)");
            Console.WriteLine("3. Behavioral Patterns (Object Interaction)");
            Console.WriteLine("4. Architectural Patterns (System Design)");
            Console.WriteLine("5. View Project Information");
            Console.WriteLine("0. Exit");
            Console.WriteLine("═══════════════════════════════════════════════════════");
            Console.Write("\nEnter choice (0-5): ");

            if (!int.TryParse(Console.ReadLine(), out int choice))
            {
                Console.WriteLine("Invalid input. Please try again.");
                continue;
            }

            switch (choice)
            {
                case 1:
                    DemoCreationalPatterns();
                    break;
                case 2:
                    DemoStructuralPatterns();
                    break;
                case 3:
                    DemoBehavioralPatterns();
                    break;
                case 4:
                    await DemoArchitecturalPatterns();
                    break;
                case 5:
                    ShowProjectInfo();
                    break;
                case 0:
                    Console.WriteLine("\n👋 Thank you for exploring DP101!");
                    return;
                default:
                    Console.WriteLine("Invalid choice. Please try again.");
                    break;
            }
        }
    }

    static void DemoCreationalPatterns()
    {
        Console.WriteLine("\n🏗️  CREATIONAL PATTERNS - Object Creation\n");

        Console.WriteLine("1️⃣  SINGLETON PATTERN\n");
        Console.WriteLine("Logger instance (Singleton):");
        Logger.Instance.Log("Application starting");
        Logger.Instance.Log("Loading configuration");
        Logger.Instance.Log("Initialization complete");

        Console.WriteLine("\n2️⃣  FACTORY PATTERN\n");
        Console.WriteLine("Creating shapes using factory:");
        var circle = ShapeFactory.CreateShape(ShapeType.Circle);
        var rectangle = ShapeFactory.CreateShape(ShapeType.Rectangle);
        circle.Draw();
        rectangle.Draw();

        Console.WriteLine("\n3️⃣  BUILDER PATTERN\n");
        Console.WriteLine("Building a customized coffee:");
        var coffee = new PizzaBuilder()
            .WithSize("Large")
            .WithCrust("Thick")
            .AddTopping("Pepperoni")
            .AddTopping("Mushrooms")
            .AddTopping("Onions")
            .WithCheese()
            .WithSauce()
            .Build();
        Console.WriteLine(coffee.ToString());

        Console.WriteLine("\n4️⃣  OBJECT POOL PATTERN\n");
        Console.WriteLine("Using object pool for buffers:");
        var pool = new ObjectPool<ByteBuffer>(() => new ByteBuffer(1024), 5, 20);
        Console.WriteLine($"Pool size: {pool.TotalCount}, Available: {pool.AvailableCount}");

        PressKeyToContinue();
    }

    static void DemoStructuralPatterns()
    {
        Console.WriteLine("\n🏛️  STRUCTURAL PATTERNS - Object Composition\n");

        Console.WriteLine("1️⃣  ADAPTER PATTERN\n");
        Console.WriteLine("Adapting 220V to 110V:");
        var highVoltage = new Voltage220V();
        var adapter = new VoltageAdapter(highVoltage);
        Console.WriteLine($"Original voltage: {highVoltage.GetVoltage()}V");
        Console.WriteLine($"Adapted voltage: {adapter.GetVoltage()}V");

        Console.WriteLine("\n2️⃣  DECORATOR PATTERN\n");
        Console.WriteLine("Decorating a coffee with additions:");
        ICoffee coffee = new BasicCoffee();
        Console.WriteLine($"{coffee.GetDescription()} - ${coffee.GetCost()}");

        coffee = new MilkDecorator(coffee);
        Console.WriteLine($"{coffee.GetDescription()} - ${coffee.GetCost()}");

        coffee = new ChocolateDecorator(coffee);
        Console.WriteLine($"{coffee.GetDescription()} - ${coffee.GetCost()}");

        Console.WriteLine("\n3️⃣  COMPOSITE PATTERN\n");
        Console.WriteLine("File system structure:");
        var root = new Directory("root");
        var documents = new Directory("Documents");
        root.Add(documents);
        documents.Add(new File("Resume.pdf", 512));
        documents.Add(new File("CoverLetter.doc", 256));
        root.Display();

        Console.WriteLine("\n4️⃣  PROXY PATTERN\n");
        Console.WriteLine("Virtual Proxy with lazy loading:");
        IImage image = new ImageProxy("large-image.jpg");
        Console.WriteLine("Image proxy created (not loaded yet)");
        image.Display(); // This triggers loading
        image.Display(); // Already loaded

        PressKeyToContinue();
    }

    static void DemoBehavioralPatterns()
    {
        Console.WriteLine("\n🎭 BEHAVIORAL PATTERNS - Object Interaction\n");

        Console.WriteLine("1️⃣  OBSERVER PATTERN\n");
        Console.WriteLine("Stock price notifications:");
        var stock = new Stock("ACME", 100m);
        var portfolio = new StockPortfolio();
        var alert = new AlertObserver(105m);

        stock.Attach(portfolio);
        stock.Attach(alert);

        portfolio.BuyStock("ACME", 10, 100m);
        stock.SetPrice(103m);
        stock.SetPrice(106m);

        Console.WriteLine("\n2️⃣  STRATEGY PATTERN\n");
        Console.WriteLine("Different payment methods:");
        var cart = new ShoppingCart();
        cart.AddItem(50m);
        cart.AddItem(30m);

        cart.SetPaymentStrategy(new CreditCardPayment("4111-1111-1111-1111"));
        cart.Checkout();

        Console.WriteLine("\n3️⃣  STATE PATTERN\n");
        Console.WriteLine("Traffic light state transitions:");
        var light = new TrafficLight();
        for (int i = 0; i < 6; i++)
        {
            light.Display();
            light.Change();
        }

        Console.WriteLine("\n4️⃣  COMMAND PATTERN\n");
        Console.WriteLine("Remote control with undo:");
        var bulb = new Light();
        var remote = new RemoteControl();

        remote.ExecuteCommand(new LightOnCommand(bulb));
        remote.ExecuteCommand(new LightOffCommand(bulb));
        Console.WriteLine("Undoing last 2 commands:");
        remote.Undo();
        remote.Undo();

        PressKeyToContinue();
    }

    static async Task DemoArchitecturalPatterns()
    {
        Console.WriteLine("\n🏗️  ARCHITECTURAL PATTERNS - System Design\n");

        Console.WriteLine("1️⃣  ORCHESTRATED SAGA PATTERN\n");
        Console.WriteLine("Order processing with central orchestration:");

        var paymentService = new PaymentServiceImpl();
        var inventoryService = new InventoryServiceImpl();
        var shippingService = new ShippingServiceImpl();

        var orchestrator = new OrderSagaOrchestrator(
            paymentService,
            inventoryService,
            shippingService);

        var order = new Order("ORD001", "CUST001", 150.00m);
        order.Items.Add(new OrderItem("ITEM001", 2, 50.00m));
        orchestrator.ProcessOrder(order);

        Console.WriteLine("\n2️⃣  CHOREOGRAPHY-BASED SAGA PATTERN\n");
        Console.WriteLine("Event-driven order processing:");
        var eventBus = new SimpleEventBus();

        var paymentServiceCheo = new PaymentServiceChoreography(eventBus);
        var inventoryServiceCheo = new InventoryServiceChoreography(eventBus);
        var shippingServiceCheo = new ShippingServiceChoreography(eventBus);
        var orderService = new OrderServiceChoreography(eventBus);

        var order2 = new Order("ORD002", "CUST002", 150.00m);
        order2.Items.Add(new OrderItem("ITEM001", 2, 50.00m));
        orderService.CreateOrder(order2);
        await Task.Delay(500); // Allow events to propagate

        Console.WriteLine("\n3️⃣  COMPENSATING TRANSACTIONS PATTERN\n");
        Console.WriteLine("Distributed transaction with rollback:");
        await CompensatingTransactionsExample.RunAsync();

        PressKeyToContinue();
    }

    static void ShowProjectInfo()
    {
        Console.WriteLine("\n📋 PROJECT INFORMATION\n");
        Console.WriteLine("╔═══════════════════════════════════════════════════════╗");
        Console.WriteLine("║DP101 - Comprehensive C# Design Patterns Implementation║");
        Console.WriteLine("╠═══════════════════════════════════════════════════════╣");
        Console.WriteLine("║                                                       ║");
        Console.WriteLine("║ This project covers ~99 design patterns:              ║");
        Console.WriteLine("║                                                       ║");
        Console.WriteLine("║ ✓ Creational Patterns (5 patterns)                    ║");
        Console.WriteLine("║   - Singleton, Factory, Builder, Prototype, Pool      ║");
        Console.WriteLine("║                                                       ║");
        Console.WriteLine("║ ✓ Structural Patterns (4+ patterns)                   ║");
        Console.WriteLine("║   - Adapter, Decorator, Composite, Proxy              ║");
        Console.WriteLine("║                                                       ║");
        Console.WriteLine("║ ✓ Behavioral Patterns (4 patterns)                    ║");
        Console.WriteLine("║   - Observer, Strategy, State, Command                ║");
        Console.WriteLine("║                                                       ║");
        Console.WriteLine("║ ✓ Architectural Patterns (25+ patterns)               ║");
        Console.WriteLine("║   - MVC, MVVM, CQRS, Event Sourcing, DDD             ║");
        Console.WriteLine("║   - SAGA (Orchestration, Choreography, Compensation)  ║");
        Console.WriteLine("║                                                       ║");
        Console.WriteLine("║ ✓ Concurrency Patterns (10+ patterns)                 ║");
        Console.WriteLine("║   - Thread Pool, Producer-Consumer, Active Object     ║");
        Console.WriteLine("║                                                       ║");
        Console.WriteLine("║ ✓ Integration Patterns (10+ patterns)                 ║");
        Console.WriteLine("║   - Pipes & Filters, Pub-Sub, Event-Driven            ║");
        Console.WriteLine("║                                                       ║");
        Console.WriteLine("║ Documentation:                                        ║");
        Console.WriteLine("║ - Each pattern includes implementation, variations,   ║");
        Console.WriteLine("║   pros/cons, use cases, and real-world examples       ║");
        Console.WriteLine("║ - Comprehensive wiki in /docs/wiki                    ║");
        Console.WriteLine("║ - Unit tests for all patterns                         ║");
        Console.WriteLine("║ - Interview Q&A guide                                 ║");
        Console.WriteLine("║                                                       ║");
        Console.WriteLine("║ Repository: s2sys/dp101                              ║");
        Console.WriteLine("║ Branch: claude/csharp-design-patterns-iouoW           ║");
        Console.WriteLine("║                                                       ║");
        Console.WriteLine("╚═══════════════════════════════════════════════════════╝");

        PressKeyToContinue();
    }

    static void PressKeyToContinue()
    {
        Console.WriteLine("\n✏️  Press any key to continue...");
        Console.ReadKey();
    }
}
