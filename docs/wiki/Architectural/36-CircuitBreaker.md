# Circuit Breaker Pattern

## Overview

**Category:** Architectural/Resilience Pattern  
**Purpose:** Prevent cascading failures by failing fast and allowing systems to recover.  
**Complexity:** Medium  
**Use Case:** Microservices, external API calls, distributed systems, resilience

## Problem

Unreliable external services cause cascading failures:
- Client waits for timeout on failed service
- Resources exhausted waiting for slow responses
- Cascades to other services
- System cannot recover

```csharp
// BAD: No resilience - cascading failures
public class PaymentService
{
    private readonly HttpClient _httpClient;

    public async Task<bool> ChargeCard(string cardToken, decimal amount)
    {
        try
        {
            // If payment gateway is down, waits full timeout (30 seconds)
            var response = await _httpClient.PostAsync(
                "https://api.payment-gateway.com/charge",
                new StringContent($"{{\"token\":\"{cardToken}\",\"amount\":{amount}}}"));

            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException)
        {
            // Every request fails and blocks thread
            throw;
        }
    }
}

// Problems:
// - Payment gateway unavailable → all requests timeout
// - Thread pool exhausted waiting for responses
// - Order service can't return response
// - User sees request timeout, no graceful degradation
// - System doesn't know when to retry
// - Cascading failures through entire system
```

## Solution

Detect failures and fail fast, automatically recovering:

```csharp
// GOOD: Circuit Breaker Pattern

public enum CircuitState
{
    Closed,      // Normal operation
    Open,        // Failing, reject requests
    HalfOpen     // Testing if service recovered
}

public class CircuitBreakerPolicy
{
    private CircuitState _state = CircuitState.Closed;
    private int _failureCount = 0;
    private int _successCount = 0;
    private DateTime _lastFailureTime;
    private readonly int _failureThreshold;
    private readonly int _successThreshold;
    private readonly TimeSpan _timeout;

    public CircuitBreakerPolicy(int failureThreshold = 5, int successThreshold = 2, TimeSpan? timeout = null)
    {
        _failureThreshold = failureThreshold;
        _successThreshold = successThreshold;
        _timeout = timeout ?? TimeSpan.FromSeconds(30);
    }

    public bool IsOpen => _state == CircuitState.Open;
    public CircuitState State => _state;

    public void RecordSuccess()
    {
        _failureCount = 0;

        if (_state == CircuitState.HalfOpen)
        {
            _successCount++;
            if (_successCount >= _successThreshold)
            {
                _state = CircuitState.Closed;
                _successCount = 0;
                Console.WriteLine("Circuit closed - service recovered");
            }
        }
    }

    public void RecordFailure()
    {
        _failureCount++;
        _lastFailureTime = DateTime.Now;

        if (_failureCount >= _failureThreshold)
        {
            _state = CircuitState.Open;
            Console.WriteLine("Circuit opened - too many failures");
        }
    }

    public void AttemptReset()
    {
        if (_state == CircuitState.Open && DateTime.Now - _lastFailureTime > _timeout)
        {
            _state = CircuitState.HalfOpen;
            _successCount = 0;
            Console.WriteLine("Circuit half-open - testing recovery");
        }
    }
}

public class ResilientPaymentService
{
    private readonly HttpClient _httpClient;
    private readonly CircuitBreakerPolicy _circuitBreaker;

    public ResilientPaymentService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _circuitBreaker = new CircuitBreakerPolicy(failureThreshold: 5, timeout: TimeSpan.FromSeconds(30));
    }

    public async Task<PaymentResult> ChargeCard(string cardToken, decimal amount)
    {
        _circuitBreaker.AttemptReset();

        // OPEN circuit - fail fast without calling service
        if (_circuitBreaker.IsOpen)
        {
            return new PaymentResult 
            { 
                Success = false, 
                Message = "Payment service temporarily unavailable" 
            };
        }

        try
        {
            // SHORT TIMEOUT - don't wait indefinitely
            using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
            {
                var response = await _httpClient.PostAsync(
                    "https://api.payment-gateway.com/charge",
                    new StringContent($"{{\"token\":\"{cardToken}\",\"amount\":{amount}}}"),
                    cts.Token);

                if (response.IsSuccessStatusCode)
                {
                    _circuitBreaker.RecordSuccess();
                    return new PaymentResult { Success = true, Message = "Payment processed" };
                }
                else
                {
                    _circuitBreaker.RecordFailure();
                    return new PaymentResult { Success = false, Message = "Payment failed" };
                }
            }
        }
        catch (OperationCanceledException)
        {
            _circuitBreaker.RecordFailure();
            return new PaymentResult { Success = false, Message = "Payment service timeout" };
        }
        catch (HttpRequestException ex)
        {
            _circuitBreaker.RecordFailure();
            return new PaymentResult { Success = false, Message = $"Payment service error: {ex.Message}" };
        }
    }
}

public class PaymentResult
{
    public bool Success { get; set; }
    public string Message { get; set; }
}

// Usage
var paymentService = new ResilientPaymentService(new HttpClient());

// Service is working
for (int i = 0; i < 3; i++)
{
    var result = await paymentService.ChargeCard("token123", 99.99m);
    Console.WriteLine($"Payment {i+1}: {result.Message}");
}

// Service starts failing
for (int i = 0; i < 10; i++)
{
    var result = await paymentService.ChargeCard("token123", 99.99m);
    Console.WriteLine($"Payment {i+4}: {result.Message}");
    // After 5 failures, circuit opens and returns immediately
}

// Service recovers - circuit enters half-open state
Thread.Sleep(31000);  // Wait for timeout
var recoveredResult = await paymentService.ChargeCard("token123", 99.99m);
Console.WriteLine($"Recovery attempt: {recoveredResult.Message}");
```

## Implementation Approaches

### 1. Retry Pattern with Circuit Breaker

```csharp
// Retry with exponential backoff + Circuit Breaker
public class RetryWithCircuitBreaker
{
    private readonly CircuitBreakerPolicy _circuitBreaker;
    private readonly int _maxRetries;
    private readonly TimeSpan _initialDelay;

    public RetryWithCircuitBreaker(int maxRetries = 3, TimeSpan? initialDelay = null)
    {
        _circuitBreaker = new CircuitBreakerPolicy();
        _maxRetries = maxRetries;
        _initialDelay = initialDelay ?? TimeSpan.FromMilliseconds(100);
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        _circuitBreaker.AttemptReset();

        if (_circuitBreaker.IsOpen)
            throw new ServiceUnavailableException("Circuit breaker is open");

        TimeSpan delay = _initialDelay;

        for (int attempt = 0; attempt <= _maxRetries; attempt++)
        {
            try
            {
                var result = await operation();
                _circuitBreaker.RecordSuccess();
                return result;
            }
            catch (Exception ex) when (attempt < _maxRetries)
            {
                await Task.Delay(delay);
                delay = TimeSpan.FromMilliseconds(delay.TotalMilliseconds * 2);  // Exponential backoff
            }
            catch (Exception)
            {
                _circuitBreaker.RecordFailure();
                throw;
            }
        }

        throw new InvalidOperationException("Unexpected error");
    }
}

// Usage
var resilientClient = new RetryWithCircuitBreaker(maxRetries: 3);

try
{
    var data = await resilientClient.ExecuteAsync(async () =>
    {
        var response = await httpClient.GetAsync("https://api.example.com/data");
        return await response.Content.ReadAsStringAsync();
    });
}
catch (ServiceUnavailableException)
{
    Console.WriteLine("Service unavailable - circuit is open");
}
```

### 2. Bulkhead Pattern Integration

```csharp
// Isolate resources with Circuit Breaker
public class BulkheadCircuitBreaker
{
    private readonly SemaphoreSlim _semaphore;
    private readonly CircuitBreakerPolicy _circuitBreaker;
    private readonly int _maxConcurrent;

    public BulkheadCircuitBreaker(int maxConcurrent = 10)
    {
        _maxConcurrent = maxConcurrent;
        _semaphore = new SemaphoreSlim(maxConcurrent, maxConcurrent);
        _circuitBreaker = new CircuitBreakerPolicy();
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        _circuitBreaker.AttemptReset();

        if (_circuitBreaker.IsOpen)
            throw new ServiceUnavailableException("Circuit breaker is open");

        // Acquire semaphore - limits concurrent requests
        if (!await _semaphore.WaitAsync(TimeSpan.FromSeconds(1)))
            throw new InvalidOperationException("Bulkhead limit exceeded");

        try
        {
            var result = await operation();
            _circuitBreaker.RecordSuccess();
            return result;
        }
        catch (Exception)
        {
            _circuitBreaker.RecordFailure();
            throw;
        }
        finally
        {
            _semaphore.Release();
        }
    }
}
```

### 3. Fallback Implementation

```csharp
// Circuit Breaker with Fallback
public class CircuitBreakerWithFallback
{
    private readonly CircuitBreakerPolicy _circuitBreaker;
    private readonly Func<Task<T>> _fallback;

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> operation)
    {
        _circuitBreaker.AttemptReset();

        try
        {
            if (_circuitBreaker.IsOpen)
            {
                Console.WriteLine("Circuit open - using fallback");
                return await _fallback();
            }

            var result = await operation();
            _circuitBreaker.RecordSuccess();
            return result;
        }
        catch (Exception)
        {
            _circuitBreaker.RecordFailure();
            Console.WriteLine("Operation failed - using fallback");
            return await _fallback();
        }
    }
}

// Usage with cache fallback
public class UserServiceWithFallback
{
    private readonly IUserRepository _repository;
    private readonly ICache _cache;
    private readonly CircuitBreakerWithFallback _circuitBreaker;

    public async Task<User> GetUser(int id)
    {
        return await _circuitBreaker.ExecuteAsync(
            operation: () => _repository.GetUserAsync(id),
            fallback: () => _cache.GetUserAsync(id)  // Use cache if service down
        );
    }
}
```

## Benefits of Circuit Breaker

✅ **Fast Failure** - Fail fast without waiting  
✅ **Resource Protection** - Prevents resource exhaustion  
✅ **Cascading Failure Prevention** - Stops failure cascade  
✅ **Automatic Recovery** - Automatically tests recovery  
✅ **System Stability** - System stays responsive  
✅ **Monitoring** - Clear state visibility  

## Drawbacks

❌ **Complexity** - Additional state to manage  
❌ **False Positives** - Temporary glitches cause opening  
❌ **Configuration** - Requires tuning thresholds  
❌ **Monitoring** - Need monitoring for circuit state  
❌ **Latency** - Still has initial failures before opening  

## Interview Questions

**Q: What is Circuit Breaker pattern?**
A: Pattern that prevents cascading failures by "short-circuiting" requests to failing services. Tracks failures and stops requests when threshold is exceeded.

**Q: What are the three circuit breaker states?**
A: Closed (normal), Open (failing, reject requests), Half-Open (testing recovery).

**Q: How does Circuit Breaker differ from Retry?**
A: Retry immediately reattempts failed requests. Circuit Breaker stops requests after threshold exceeded, waiting before retrying.

**Q: What's the relationship between Circuit Breaker and Timeout?**
A: Timeout prevents indefinite waiting. Circuit Breaker prevents cascading failures. Both work together for resilience.

**Q: When should circuit open?**
A: After a threshold of failures (e.g., 5 failures in 30 seconds) or when error rate exceeds threshold (e.g., 50%).

**Q: How does half-open state work?**
A: After timeout, circuit enters half-open. Next request is attempted. If successful, circuit closes. If fails, circuit reopens.

## When to Use Circuit Breaker

### ✅ Use When:
- Calling external services
- Microservices architecture
- Need resilience to failures
- Want to prevent cascading failures
- Need automatic recovery

### ❌ Don't Use When:
- Internal in-process calls
- Synchronous operations with fallbacks
- No network calls involved

## Polly Library (C# Implementation)

```csharp
// Using Polly - popular C# resilience library
var circuitBreakerPolicy = Policy
    .Handle<HttpRequestException>()
    .Or<OperationCanceledException>()
    .CircuitBreaker(
        handledEventsAllowedBeforeBreaking: 5,
        durationOfBreak: TimeSpan.FromSeconds(30),
        onBreak: (exception, timespan) =>
        {
            Console.WriteLine($"Circuit broken for {timespan.TotalSeconds}s");
        },
        onReset: () =>
        {
            Console.WriteLine("Circuit reset");
        }
    );

var response = await circuitBreakerPolicy.ExecuteAsync(
    () => httpClient.GetAsync("https://api.example.com/data")
);
```

## Summary

Circuit Breaker pattern prevents cascading failures by stopping requests to failing services and automatically recovering when they stabilize. Essential for microservices and distributed systems. Works in combination with Retry, Timeout, and Fallback patterns to create resilient systems.

**Key Takeaway:** Circuit Breaker fails fast and prevents cascading failures. Essential for production microservices systems.

---

**Related Patterns:**
- Retry Pattern - Automatically retry failed requests
- Timeout Pattern - Prevent indefinite waiting
- Bulkhead Pattern - Isolate resources
- Fallback Pattern - Provide alternative when primary fails
