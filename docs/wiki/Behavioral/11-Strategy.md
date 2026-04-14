# Strategy Pattern

## Overview

**Category:** Behavioral Pattern  
**Purpose:** Define a family of algorithms, encapsulate each one, and make them interchangeable.  
**Also Called:** Policy  
**Complexity:** Medium

## Problem

Need to switch between algorithms at runtime, but hardcoding them creates inflexible code:

```csharp
// Problem: Multiple algorithms hardcoded with conditions
public class PaymentProcessor
{
    public void ProcessPayment(decimal amount, string method)
    {
        if (method == "Credit")
        {
            // Credit card logic here
        }
        else if (method == "PayPal")
        {
            // PayPal logic here
        }
        else if (method == "Crypto")
        {
            // Crypto logic here
        }
        // Adding new method requires changing this class!
    }
}
```

## Solution

Encapsulate algorithms in separate classes with common interface:

```csharp
public interface IPaymentStrategy
{
    void Pay(decimal amount);
}

public class CreditCardStrategy : IPaymentStrategy
{
    public void Pay(decimal amount) => Console.WriteLine($"Paid ${amount} via Credit Card");
}

public class PaymentProcessor
{
    private IPaymentStrategy _strategy;

    public void SetPaymentMethod(IPaymentStrategy strategy) => _strategy = strategy;
    public void ProcessPayment(decimal amount) => _strategy.Pay(amount);
}

// Usage - add new strategy without changing PaymentProcessor!
var processor = new PaymentProcessor();
processor.SetPaymentMethod(new CreditCardStrategy());
processor.ProcessPayment(100m);
```

## Implementation Approaches

### 1. Payment Strategy

```csharp
public interface IPaymentStrategy
{
    void Pay(decimal amount);
}

public class CreditCardStrategy : IPaymentStrategy
{
    private string _cardNumber;

    public CreditCardStrategy(string cardNumber) => _cardNumber = cardNumber;

    public void Pay(decimal amount)
    {
        Console.WriteLine($"Processing ${amount} with Credit Card: {_cardNumber}");
        Console.WriteLine("✓ Payment authorized");
    }
}

public class PayPalStrategy : IPaymentStrategy
{
    private string _email;

    public PayPalStrategy(string email) => _email = email;

    public void Pay(decimal amount)
    {
        Console.WriteLine($"Processing ${amount} via PayPal: {_email}");
        Console.WriteLine("✓ Payment sent");
    }
}

public class CryptoStrategy : IPaymentStrategy
{
    private string _walletAddress;

    public CryptoStrategy(string walletAddress) => _walletAddress = walletAddress;

    public void Pay(decimal amount)
    {
        Console.WriteLine($"Processing {amount} BTC to {_walletAddress}");
        Console.WriteLine("✓ Transaction confirmed");
    }
}

public class PaymentProcessor
{
    private IPaymentStrategy _strategy;

    public void SetPaymentMethod(IPaymentStrategy strategy) => _strategy = strategy;

    public void ProcessPayment(decimal amount)
    {
        if (_strategy == null)
            throw new InvalidOperationException("Payment method not set");

        _strategy.Pay(amount);
    }
}

// Usage
var processor = new PaymentProcessor();
processor.SetPaymentMethod(new CreditCardStrategy("1234-5678-9012-3456"));
processor.ProcessPayment(100m);

processor.SetPaymentMethod(new PayPalStrategy("user@example.com"));
processor.ProcessPayment(50m);
```

### 2. Sorting Strategy

```csharp
public interface ISortingStrategy
{
    void Sort(int[] array);
}

public class BubbleSortStrategy : ISortingStrategy
{
    public void Sort(int[] array)
    {
        Console.WriteLine("Sorting with Bubble Sort");
        for (int i = 0; i < array.Length; i++)
        {
            for (int j = 0; j < array.Length - 1 - i; j++)
            {
                if (array[j] > array[j + 1])
                {
                    int temp = array[j];
                    array[j] = array[j + 1];
                    array[j + 1] = temp;
                }
            }
        }
    }
}

public class QuickSortStrategy : ISortingStrategy
{
    public void Sort(int[] array)
    {
        Console.WriteLine("Sorting with Quick Sort");
        QuickSort(array, 0, array.Length - 1);
    }

    private void QuickSort(int[] array, int low, int high)
    {
        if (low < high)
        {
            int pivot = Partition(array, low, high);
            QuickSort(array, low, pivot - 1);
            QuickSort(array, pivot + 1, high);
        }
    }

    private int Partition(int[] array, int low, int high)
    {
        int pivot = array[high];
        int i = low - 1;
        for (int j = low; j < high; j++)
        {
            if (array[j] < pivot)
            {
                i++;
                int temp = array[i];
                array[i] = array[j];
                array[j] = temp;
            }
        }
        int temp2 = array[i + 1];
        array[i + 1] = array[high];
        array[high] = temp2;
        return i + 1;
    }
}

public class MergeSortStrategy : ISortingStrategy
{
    public void Sort(int[] array)
    {
        Console.WriteLine("Sorting with Merge Sort");
        MergeSort(array, 0, array.Length - 1);
    }

    private void MergeSort(int[] array, int left, int right)
    {
        if (left < right)
        {
            int mid = (left + right) / 2;
            MergeSort(array, left, mid);
            MergeSort(array, mid + 1, right);
            Merge(array, left, mid, right);
        }
    }

    private void Merge(int[] array, int left, int mid, int right)
    {
        int leftSize = mid - left + 1;
        int rightSize = right - mid;
        int[] leftArray = new int[leftSize];
        int[] rightArray = new int[rightSize];

        Array.Copy(array, left, leftArray, 0, leftSize);
        Array.Copy(array, mid + 1, rightArray, 0, rightSize);

        int i = 0, j = 0, k = left;
        while (i < leftSize && j < rightSize)
        {
            if (leftArray[i] <= rightArray[j])
                array[k++] = leftArray[i++];
            else
                array[k++] = rightArray[j++];
        }

        while (i < leftSize)
            array[k++] = leftArray[i++];

        while (j < rightSize)
            array[k++] = rightArray[j++];
    }
}

public class DataSorter
{
    private ISortingStrategy _strategy;

    public void SetSortingStrategy(ISortingStrategy strategy) => _strategy = strategy;

    public void Sort(int[] array)
    {
        _strategy.Sort(array);
    }
}

// Usage
var sorter = new DataSorter();
int[] data = { 64, 34, 25, 12, 22, 11, 90 };

sorter.SetSortingStrategy(new BubbleSortStrategy());
sorter.Sort(data);

sorter.SetSortingStrategy(new QuickSortStrategy());
sorter.Sort((int[])data.Clone());
```

### 3. Compression Strategy

```csharp
public interface ICompressionStrategy
{
    string Compress(string data);
    string Decompress(string compressed);
}

public class GZipCompressionStrategy : ICompressionStrategy
{
    public string Compress(string data)
    {
        Console.WriteLine("Compressing with GZIP");
        return $"[GZIP]{data}";
    }

    public string Decompress(string compressed)
    {
        return compressed.Replace("[GZIP]", "");
    }
}

public class RarCompressionStrategy : ICompressionStrategy
{
    public string Compress(string data)
    {
        Console.WriteLine("Compressing with RAR");
        return $"[RAR]{data}";
    }

    public string Decompress(string compressed)
    {
        return compressed.Replace("[RAR]", "");
    }
}

public class ZipCompressionStrategy : ICompressionStrategy
{
    public string Compress(string data)
    {
        Console.WriteLine("Compressing with ZIP");
        return $"[ZIP]{data}";
    }

    public string Decompress(string compressed)
    {
        return compressed.Replace("[ZIP]", "");
    }
}

public class FileCompressor
{
    private ICompressionStrategy _strategy;

    public void SetCompressionStrategy(ICompressionStrategy strategy) => _strategy = strategy;

    public string CompressFile(string data) => _strategy.Compress(data);
    public string DecompressFile(string data) => _strategy.Decompress(data);
}

// Usage
var compressor = new FileCompressor();
string fileData = "This is sensitive data that needs compression";

compressor.SetCompressionStrategy(new GZipCompressionStrategy());
var compressed = compressor.CompressFile(fileData);

compressor.SetCompressionStrategy(new ZipCompressionStrategy());
var recompressed = compressor.CompressFile(fileData);
```

### 4. Formatting Strategy

```csharp
public interface IFormattingStrategy
{
    string Format(string data);
}

public class UpperCaseFormattingStrategy : IFormattingStrategy
{
    public string Format(string data) => data.ToUpper();
}

public class LowerCaseFormattingStrategy : IFormattingStrategy
{
    public string Format(string data) => data.ToLower();
}

public class TitleCaseFormattingStrategy : IFormattingStrategy
{
    public string Format(string data)
    {
        var info = System.Globalization.CultureInfo.CurrentCulture.TextInfo;
        return info.ToTitleCase(data);
    }
}

public class ReverseFormattingStrategy : IFormattingStrategy
{
    public string Format(string data)
    {
        var chars = data.ToCharArray();
        Array.Reverse(chars);
        return new string(chars);
    }
}

public class TextFormatter
{
    private IFormattingStrategy _strategy;

    public void SetFormattingStrategy(IFormattingStrategy strategy) => _strategy = strategy;

    public string Format(string text) => _strategy.Format(text);
}

// Usage
var formatter = new TextFormatter();

formatter.SetFormattingStrategy(new UpperCaseFormattingStrategy());
Console.WriteLine(formatter.Format("hello world"));  // HELLO WORLD

formatter.SetFormattingStrategy(new ReverseFormattingStrategy());
Console.WriteLine(formatter.Format("hello"));  // olleh
```

---

## Strategy Pattern Structure

```
    Context
      |
      | uses
      v
  IStrategy (Interface)
    /    \
   /      \
Strategy1 Strategy2 Strategy3
```

---

## Pros and Cons

### Advantages
✅ **Algorithm Selection at Runtime** - Change algorithms dynamically  
✅ **Open/Closed Principle** - Easy to add new strategies  
✅ **Eliminates Conditionals** - No if-else chains  
✅ **Single Responsibility** - Each strategy handles one algorithm  
✅ **Code Reusability** - Strategies can be reused in different contexts  

### Disadvantages
❌ **More Classes** - One class per strategy  
❌ **Overhead** - Unnecessary for simple cases  
❌ **Client Complexity** - Client must choose strategy  
❌ **Data Access** - Strategies may need context data  

---

## Real-World Examples

### Payment Processing
```csharp
paymentProcessor.SetPaymentMethod(new CreditCardStrategy(card));
paymentProcessor.ProcessPayment(100m);
```

### Sorting Algorithms
```csharp
var sorter = new DataSorter();
sorter.SetSortingStrategy(new QuickSortStrategy());  // Fast for large data
sorter.Sort(largeArray);
```

### Data Compression
```csharp
var compressor = new FileCompressor();
compressor.SetCompressionStrategy(new GZipCompressionStrategy());
var compressed = compressor.CompressFile(data);
```

### Logging Levels
```csharp
logger.SetStrategy(new DebugStrategy());  // Verbose logging
logger.SetStrategy(new ProductionStrategy());  // Minimal logging
```

---

## When to Use

### ✅ Use Strategy When:
- Multiple algorithms for same task
- Algorithm selection at runtime
- Avoid complex conditional logic
- Want to encapsulate algorithms
- Need to switch behaviors dynamically
- Same family of algorithms

### ❌ Don't Use When:
- Only one algorithm
- Algorithm never changes
- Simple method calls sufficient
- Unnecessary complexity
- Few algorithm variants

---

## Interview Questions

**Q: What's the difference between Strategy and State patterns?**
A: Strategy lets client choose algorithm; State encapsulates state-dependent behavior. Strategy is chosen by client; State is chosen by object itself based on state.

**Q: When would you use Strategy instead of if-else?**
A: When you have multiple algorithms that vary independently and may change. Strategy is cleaner for complex conditions.

**Q: Can strategies share state?**
A: Better if they don't. If they need shared state, pass context object to strategies.

**Q: How is Strategy different from Decorator?**
A: Decorator adds behavior to object; Strategy encapsulates algorithm choice. Decorator chains; Strategy replaces algorithm.

---

## Strategy vs. State

| Aspect | Strategy | State |
|--------|----------|-------|
| **Purpose** | Select algorithm | Encapsulate behavior based on state |
| **Selection** | Client chooses | Object chooses based on state |
| **Change When** | Algorithm changes | Object state changes |
| **Coupling** | Strategy independent of client | State tied to object |
| **Flexibility** | Easy to add strategies | State transitions defined |

---

## Summary

Strategy pattern elegantly handles multiple algorithms by encapsulating them in separate classes. Perfect for payment processors, sorting, compression, formatting, or any scenario with interchangeable algorithms. Use when algorithm selection happens at runtime and many variants exist.

**Key Takeaway:** Strategy lets you select algorithm at runtime without modifying client code.
