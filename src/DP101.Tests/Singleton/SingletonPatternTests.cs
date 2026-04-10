namespace DP101.Tests.Singleton;

using DP101.Core.CreationalPatterns.Singleton;
using Xunit;

public class SingletonPatternTests
{
    #region Basic Singleton Tests

    [Fact]
    public void BasicSingleton_ReturnsOnlyOneInstance()
    {
        var instance1 = BasicSingleton.Instance;
        var instance2 = BasicSingleton.Instance;

        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void BasicSingleton_GetInfo_ReturnsCorrectMessage()
    {
        var instance = BasicSingleton.Instance;
        Assert.Equal("Basic Singleton Instance", instance.GetInfo());
    }

    #endregion

    #region Thread-Safe Lock Singleton Tests

    [Fact]
    public void ThreadSafeLockingSingleton_ReturnsOnlyOneInstance()
    {
        var instance1 = ThreadSafeLockingSingleton.Instance;
        var instance2 = ThreadSafeLockingSingleton.Instance;

        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void ThreadSafeLockingSingleton_IsThreadSafe()
    {
        var instances = new List<ThreadSafeLockingSingleton>();
        var threads = new List<Thread>();

        for (int i = 0; i < 10; i++)
        {
            var thread = new Thread(() =>
            {
                instances.Add(ThreadSafeLockingSingleton.Instance);
            });
            threads.Add(thread);
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        // All instances should be the same
        var first = instances[0];
        Assert.All(instances, instance => Assert.Same(first, instance));
    }

    #endregion

    #region Double-Checked Locking Singleton Tests

    [Fact]
    public void DoubleCheckedLockingSingleton_ReturnsOnlyOneInstance()
    {
        var instance1 = DoubleCheckedLockingSingleton.Instance;
        var instance2 = DoubleCheckedLockingSingleton.Instance;

        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void DoubleCheckedLockingSingleton_IsThreadSafe()
    {
        var instances = new List<DoubleCheckedLockingSingleton>();
        var threads = new List<Thread>();

        for (int i = 0; i < 10; i++)
        {
            var thread = new Thread(() =>
            {
                instances.Add(DoubleCheckedLockingSingleton.Instance);
            });
            threads.Add(thread);
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        // All instances should be the same
        var first = instances[0];
        Assert.All(instances, instance => Assert.Same(first, instance));
    }

    #endregion

    #region Lazy Singleton Tests

    [Fact]
    public void LazySingleton_ReturnsOnlyOneInstance()
    {
        var instance1 = LazySingleton.Instance;
        var instance2 = LazySingleton.Instance;

        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void LazySingleton_IsThreadSafe()
    {
        var instances = new List<LazySingleton>();
        var threads = new List<Thread>();

        for (int i = 0; i < 10; i++)
        {
            var thread = new Thread(() =>
            {
                instances.Add(LazySingleton.Instance);
            });
            threads.Add(thread);
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        // All instances should be the same
        var first = instances[0];
        Assert.All(instances, instance => Assert.Same(first, instance));
    }

    [Fact]
    public void LazySingleton_GetInfo_ReturnsCorrectMessage()
    {
        var instance = LazySingleton.Instance;
        Assert.Equal("Lazy<T> Singleton Instance", instance.GetInfo());
    }

    #endregion

    #region Bill Pugh Singleton Tests

    [Fact]
    public void BillPughSingleton_ReturnsOnlyOneInstance()
    {
        var instance1 = BillPughSingleton.Instance;
        var instance2 = BillPughSingleton.Instance;

        Assert.Same(instance1, instance2);
    }

    [Fact]
    public void BillPughSingleton_IsThreadSafe()
    {
        var instances = new List<BillPughSingleton>();
        var threads = new List<Thread>();

        for (int i = 0; i < 10; i++)
        {
            var thread = new Thread(() =>
            {
                instances.Add(BillPughSingleton.Instance);
            });
            threads.Add(thread);
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        // All instances should be the same
        var first = instances[0];
        Assert.All(instances, instance => Assert.Same(first, instance));
    }

    #endregion

    #region Use Case Tests

    [Fact]
    public void Logger_Singleton_ReturnsOnlyOneInstance()
    {
        // Clear any existing logs
        Logger.Instance.ClearLogs();

        Logger.Instance.Log("Test message 1");
        var logsCount1 = Logger.Instance.GetAllLogs().Count;

        Logger.Instance.Log("Test message 2");
        var logsCount2 = Logger.Instance.GetAllLogs().Count;

        Assert.Equal(1, logsCount1);
        Assert.Equal(2, logsCount2);
    }

    [Fact]
    public void DatabaseConnection_Singleton_ManagesSingleInstance()
    {
        var db = DatabaseConnection.Instance;
        Assert.False(db.IsConnected);

        db.Connect();
        Assert.True(db.IsConnected);

        db.Disconnect();
        Assert.False(db.IsConnected);
    }

    [Fact]
    public void ConfigurationManager_Singleton_ReturnsOnlyOneInstance()
    {
        var config = ConfigurationManager.Instance;

        var appName = config.GetSetting("AppName");
        Assert.Equal("DP101 Application", appName);

        config.SetSetting("CustomKey", "CustomValue");
        var customValue = config.GetSetting("CustomKey");
        Assert.Equal("CustomValue", customValue);
    }

    [Fact]
    public void ApplicationState_Singleton_IsThreadSafe()
    {
        var state = ApplicationState.Instance;
        state.ClearState();

        var tasks = new List<Task>();
        for (int i = 0; i < 10; i++)
        {
            var key = i;
            tasks.Add(Task.Run(() =>
            {
                state.SetState($"Key{key}", key);
            }));
        }

        Task.WaitAll(tasks.ToArray());

        Assert.Equal(10, state.GetStateCount());
    }

    [Fact]
    public void CacheManager_Singleton_StoresAndRetrievesValues()
    {
        var cache = CacheManager.Instance;
        cache.Clear();

        cache.Set("testKey", "testValue");
        var retrieved = cache.Get<string>("testKey");

        Assert.Equal("testValue", retrieved);
    }

    [Fact]
    public void CacheManager_Singleton_HandleExpiration()
    {
        var cache = CacheManager.Instance;
        cache.Clear();

        cache.Set("expireKey", "testValue", TimeSpan.FromMilliseconds(100));
        Assert.True(cache.Exists("expireKey"));

        System.Threading.Thread.Sleep(150);
        Assert.False(cache.Exists("expireKey"));
    }

    #endregion
}
