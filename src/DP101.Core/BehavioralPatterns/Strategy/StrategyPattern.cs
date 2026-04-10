namespace DP101.Core.BehavioralPatterns.Strategy;

/// <summary>
/// STRATEGY PATTERN
///
/// Intent: Define a family of algorithms, encapsulate each, and make them interchangeable.
/// Let the client choose the algorithm.
///
/// PROS:
/// - Encapsulates algorithms
/// - Easy to add new strategies
/// - Eliminates conditional statements
/// - Allows runtime algorithm selection
/// - Adheres to Open/Closed Principle
///
/// CONS:
/// - Increases number of classes
/// - Overhead for simple algorithms
/// - Client must be aware of strategies
/// </summary>

// ========== EXAMPLE 1: PAYMENT STRATEGY ==========

public interface IPaymentStrategy
{
    void Pay(decimal amount);
    string GetPaymentMethod();
}

public class CreditCardPayment : IPaymentStrategy
{
    private string _cardNumber;

    public CreditCardPayment(string cardNumber)
    {
        _cardNumber = cardNumber;
    }

    public void Pay(decimal amount)
    {
        Console.WriteLine($"Paying ${amount} using Credit Card {_cardNumber}");
    }

    public string GetPaymentMethod() => "Credit Card";
}

public class PayPalPayment : IPaymentStrategy
{
    private string _email;

    public PayPalPayment(string email)
    {
        _email = email;
    }

    public void Pay(decimal amount)
    {
        Console.WriteLine($"Paying ${amount} via PayPal to {_email}");
    }

    public string GetPaymentMethod() => "PayPal";
}

public class CryptoCurrencyPayment : IPaymentStrategy
{
    private string _walletAddress;

    public CryptoCurrencyPayment(string walletAddress)
    {
        _walletAddress = walletAddress;
    }

    public void Pay(decimal amount)
    {
        Console.WriteLine($"Paying ${amount} in cryptocurrency to wallet {_walletAddress}");
    }

    public string GetPaymentMethod() => "Cryptocurrency";
}

public class BankTransferPayment : IPaymentStrategy
{
    private string _accountNumber;

    public BankTransferPayment(string accountNumber)
    {
        _accountNumber = accountNumber;
    }

    public void Pay(decimal amount)
    {
        Console.WriteLine($"Paying ${amount} via Bank Transfer to account {_accountNumber}");
    }

    public string GetPaymentMethod() => "Bank Transfer";
}

public class ShoppingCart
{
    private IPaymentStrategy _paymentStrategy;
    private decimal _total;

    public void SetPaymentStrategy(IPaymentStrategy strategy)
    {
        _paymentStrategy = strategy;
    }

    public void AddItem(decimal price)
    {
        _total += price;
    }

    public void Checkout()
    {
        if (_paymentStrategy == null)
            throw new InvalidOperationException("Payment strategy not set");

        Console.WriteLine($"Total amount: ${_total}");
        Console.WriteLine($"Payment method: {_paymentStrategy.GetPaymentMethod()}");
        _paymentStrategy.Pay(_total);
    }
}

// ========== EXAMPLE 2: SORTING STRATEGY ==========

public interface ISortingStrategy
{
    void Sort(int[] array);
    string GetName();
}

public class BubbleSort : ISortingStrategy
{
    public void Sort(int[] array)
    {
        int n = array.Length;
        for (int i = 0; i < n - 1; i++)
        {
            for (int j = 0; j < n - i - 1; j++)
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

    public string GetName() => "Bubble Sort";
}

public class QuickSort : ISortingStrategy
{
    public void Sort(int[] array)
    {
        if (array.Length > 0)
            QuickSortHelper(array, 0, array.Length - 1);
    }

    private void QuickSortHelper(int[] array, int left, int right)
    {
        if (left < right)
        {
            int pi = Partition(array, left, right);
            QuickSortHelper(array, left, pi - 1);
            QuickSortHelper(array, pi + 1, right);
        }
    }

    private int Partition(int[] array, int left, int right)
    {
        int pivot = array[right];
        int i = left - 1;

        for (int j = left; j < right; j++)
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
        array[i + 1] = array[right];
        array[right] = temp2;

        return i + 1;
    }

    public string GetName() => "Quick Sort";
}

public class MergeSort : ISortingStrategy
{
    public void Sort(int[] array)
    {
        if (array.Length > 1)
            MergeSortHelper(array, 0, array.Length - 1);
    }

    private void MergeSortHelper(int[] array, int left, int right)
    {
        if (left < right)
        {
            int mid = left + (right - left) / 2;
            MergeSortHelper(array, left, mid);
            MergeSortHelper(array, mid + 1, right);
            Merge(array, left, mid, right);
        }
    }

    private void Merge(int[] array, int left, int mid, int right)
    {
        int[] temp = new int[right - left + 1];
        int i = left, j = mid + 1, k = 0;

        while (i <= mid && j <= right)
        {
            temp[k++] = array[i] <= array[j] ? array[i++] : array[j++];
        }

        while (i <= mid) temp[k++] = array[i++];
        while (j <= right) temp[k++] = array[j++];

        Array.Copy(temp, 0, array, left, temp.Length);
    }

    public string GetName() => "Merge Sort";
}

public class Sorter
{
    private ISortingStrategy _strategy;

    public void SetStrategy(ISortingStrategy strategy)
    {
        _strategy = strategy;
    }

    public void Sort(int[] array)
    {
        if (_strategy == null)
            throw new InvalidOperationException("Sorting strategy not set");

        Console.WriteLine($"Using {_strategy.GetName()}");
        _strategy.Sort(array);
    }
}

// ========== EXAMPLE 3: COMPRESSION STRATEGY ==========

public interface ICompressionStrategy
{
    string Compress(string data);
    string Decompress(string data);
}

public class GZipCompression : ICompressionStrategy
{
    public string Compress(string data) => $"GZip({data.Substring(0, Math.Min(3, data.Length))})";
    public string Decompress(string data) => data.Replace("GZip(", "").Replace(")", "");
}

public class RAWCompression : ICompressionStrategy
{
    public string Compress(string data) => $"RAW({data})";
    public string Decompress(string data) => data.Replace("RAW(", "").Replace(")", "");
}

public class ZipCompression : ICompressionStrategy
{
    public string Compress(string data) => $"ZIP({data.Substring(0, Math.Min(5, data.Length))}...)";
    public string Decompress(string data) => data.Replace("ZIP(", "").Replace(")", "");
}

public class Archiver
{
    private ICompressionStrategy _strategy;

    public void SetCompressionStrategy(ICompressionStrategy strategy)
    {
        _strategy = strategy;
    }

    public void Compress(string data)
    {
        if (_strategy == null)
            throw new InvalidOperationException("Strategy not set");

        var compressed = _strategy.Compress(data);
        Console.WriteLine($"Compressed: {compressed}");
    }
}

// ========== EXAMPLE 4: FORMATTING STRATEGY ==========

public interface IFormattingStrategy
{
    string Format(string text);
}

public class MarkdownFormatting : IFormattingStrategy
{
    public string Format(string text) => $"# {text}";
}

public class HTMLFormatting : IFormattingStrategy
{
    public string Format(string text) => $"<h1>{text}</h1>";
}

public class XMLFormatting : IFormattingStrategy
{
    public string Format(string text) => $"<title>{text}</title>";
}

public class PlainTextFormatting : IFormattingStrategy
{
    public string Format(string text) => text;
}

public class DocumentFormatter
{
    private IFormattingStrategy _strategy;

    public void SetFormatter(IFormattingStrategy strategy)
    {
        _strategy = strategy;
    }

    public void Format(string content)
    {
        if (_strategy == null)
            throw new InvalidOperationException("Formatter not set");

        Console.WriteLine(_strategy.Format(content));
    }
}
