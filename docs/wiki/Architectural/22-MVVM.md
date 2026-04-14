# MVVM (Model-View-ViewModel) Architecture

## Overview

**Category:** Architectural Pattern  
**Purpose:** Separate UI from business logic with automatic data binding, enabling test-driven development and rich interactive UIs.  
**Complexity:** Medium-High  
**Use Case:** WPF, UWP, Xamarin, Angular, Vue.js applications

## Problem

Traditional MVC Controller becomes complex when:
- UI updates frequently and bidirectionally
- Many interdependent UI controls
- Lots of state management logic
- View needs to respond to property changes
- Testing UI logic becomes difficult

```csharp
// BAD: Complex controller with UI state management
public class UserController
{
    private UserView _view;
    
    public void LoadUser(int id)
    {
        var user = _service.GetUser(id);
        _view.NameLabel.Text = user.Name;
        _view.EmailLabel.Text = user.Email;
        _view.PhoneLabel.Text = user.Phone;
        _view.SaveButton.Enabled = false;
    }
    
    public void OnNameChanged(string newName)
    {
        _currentUser.Name = newName;
        _view.SaveButton.Enabled = true;  // UI logic!
        _view.ValidationLabel.Text = ValidateName(newName);
    }
}
// UI state logic mixed with business logic!
```

## Solution

Use ViewModel to handle UI state, with automatic data binding:

```csharp
// MODEL: Business logic
public class User
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}

// VIEWMODEL: UI logic and state
public class UserViewModel : INotifyPropertyChanged
{
    private User _user;
    private bool _isSaveEnabled;

    public string Name
    {
        get => _user.Name;
        set
        {
            _user.Name = value;
            OnPropertyChanged(nameof(Name));
            IsSaveEnabled = true;
            ValidationMessage = ValidateName(value);
        }
    }

    public bool IsSaveEnabled
    {
        get => _isSaveEnabled;
        set
        {
            _isSaveEnabled = value;
            OnPropertyChanged(nameof(IsSaveEnabled));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    private void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

// VIEW: Just binds to ViewModel
<TextBlock Text="Name:" />
<TextBox Text="{Binding Name, Mode=TwoWay}" />
<Button IsEnabled="{Binding IsSaveEnabled}">Save</Button>
<TextBlock Text="{Binding ValidationMessage}" />
```

## Implementation Approaches

### 1. WPF MVVM Application

```csharp
// MODEL
public class Customer
{
    public int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
}

public class CustomerService
{
    public Customer GetCustomer(int id)
    {
        return new Customer { Id = id, FirstName = "John", LastName = "Doe", Email = "john@example.com" };
    }

    public void SaveCustomer(Customer customer)
    {
        Console.WriteLine($"Saving {customer.FirstName} {customer.LastName}");
    }
}

// VIEWMODEL
public class CustomerViewModel : INotifyPropertyChanged
{
    private readonly CustomerService _service;
    private Customer _customer;
    private string _statusMessage;
    private bool _isSaving;
    private RelayCommand _saveCommand;

    public string FirstName
    {
        get => _customer.FirstName;
        set
        {
            if (_customer.FirstName != value)
            {
                _customer.FirstName = value;
                OnPropertyChanged(nameof(FirstName));
                _saveCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public string LastName
    {
        get => _customer.LastName;
        set
        {
            if (_customer.LastName != value)
            {
                _customer.LastName = value;
                OnPropertyChanged(nameof(LastName));
            }
        }
    }

    public string Email
    {
        get => _customer.Email;
        set
        {
            if (_customer.Email != value)
            {
                _customer.Email = value;
                OnPropertyChanged(nameof(Email));
            }
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set
        {
            _statusMessage = value;
            OnPropertyChanged(nameof(StatusMessage));
        }
    }

    public RelayCommand SaveCommand => _saveCommand;

    public CustomerViewModel()
    {
        _service = new CustomerService();
        _customer = new Customer();
        _saveCommand = new RelayCommand(Save, CanSave);
    }

    public void LoadCustomer(int id)
    {
        _customer = _service.GetCustomer(id);
        OnPropertyChanged(nameof(FirstName));
        OnPropertyChanged(nameof(LastName));
        OnPropertyChanged(nameof(Email));
    }

    private void Save()
    {
        _isSaving = true;
        _service.SaveCustomer(_customer);
        StatusMessage = "✅ Customer saved successfully";
        _isSaving = false;
    }

    private bool CanSave() => !_isSaving && !string.IsNullOrEmpty(FirstName);

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

// RELAY COMMAND: Simplified command implementation
public class RelayCommand : ICommand
{
    private readonly Action _execute;
    private readonly Func<bool> _canExecute;

    public RelayCommand(Action execute, Func<bool> canExecute = null)
    {
        _execute = execute;
        _canExecute = canExecute;
    }

    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter) => _canExecute?.Invoke() ?? true;
    public void Execute(object parameter) => _execute?.Invoke();
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}

// VIEW: XAML (WPF)
<Window x:Class="MvvmApp.CustomerView"
        xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
    <StackPanel Padding="20">
        <TextBlock Text="Customer Details" FontSize="18" FontWeight="Bold"/>
        
        <Label Content="First Name:"/>
        <TextBox Text="{Binding FirstName, Mode=TwoWay}"/>
        
        <Label Content="Last Name:"/>
        <TextBox Text="{Binding LastName, Mode=TwoWay}"/>
        
        <Label Content="Email:"/>
        <TextBox Text="{Binding Email, Mode=TwoWay}"/>
        
        <Button Content="Save" Command="{Binding SaveCommand}" Margin="0,20,0,0"/>
        
        <TextBlock Text="{Binding StatusMessage}" Foreground="Green" Margin="0,10,0,0"/>
    </StackPanel>
</Window>

// CODE-BEHIND: Minimal logic
public partial class CustomerView : Window
{
    public CustomerView()
    {
        InitializeComponent();
        var viewModel = new CustomerViewModel();
        viewModel.LoadCustomer(1);
        this.DataContext = viewModel;
    }
}
```

### 2. Angular MVVM Application

```typescript
// MODEL
export interface Product {
  id: number;
  name: string;
  price: number;
  quantity: number;
}

export class ProductService {
  getProducts(): Observable<Product[]> {
    return of([
      { id: 1, name: 'Laptop', price: 999, quantity: 5 },
      { id: 2, name: 'Mouse', price: 25, quantity: 50 }
    ]);
  }

  saveProduct(product: Product): Observable<any> {
    console.log('Saving:', product);
    return of({ success: true });
  }
}

// VIEWMODEL
@Component({
  selector: 'app-product-list',
  template: `
    <div class="products">
      <h2>Products</h2>
      
      <div *ngFor="let product of products$ | async">
        <input [(ngModel)]="product.name" placeholder="Name"/>
        <input [(ngModel)]="product.price" placeholder="Price"/>
        <button (click)="saveProduct(product)">Save</button>
        <span>{{ statusMessage }}</span>
      </div>
    </div>
  `
})
export class ProductListComponent implements OnInit {
  products$: Observable<Product[]>;
  statusMessage = '';

  constructor(private productService: ProductService) {
    this.products$ = this.productService.getProducts();
  }

  ngOnInit() {
  }

  saveProduct(product: Product) {
    this.productService.saveProduct(product).subscribe(
      () => this.statusMessage = '✅ Saved',
      error => this.statusMessage = '❌ Error'
    );
  }
}
```

### 3. Xamarin MVVM

```csharp
// MODEL
public class Book
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Author { get; set; }
    public decimal Price { get; set; }
}

// VIEWMODEL
public class BookViewModel : INotifyPropertyChanged
{
    private Book _book;
    private List<Book> _books;
    private ICommand _saveCommand;
    private ICommand _deleteCommand;

    public string Title
    {
        get => _book.Title;
        set
        {
            if (_book.Title != value)
            {
                _book.Title = value;
                OnPropertyChanged(nameof(Title));
            }
        }
    }

    public List<Book> Books
    {
        get => _books;
        set
        {
            _books = value;
            OnPropertyChanged(nameof(Books));
        }
    }

    public ICommand SaveCommand => _saveCommand ??= 
        new Command(() => SaveBook());

    public ICommand DeleteCommand => _deleteCommand ??= 
        new Command<Book>((book) => DeleteBook(book));

    private void SaveBook()
    {
        // Save logic
        Console.WriteLine($"Saving: {_book.Title}");
    }

    private void DeleteBook(Book book)
    {
        _books.Remove(book);
        OnPropertyChanged(nameof(Books));
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}

// VIEW: XAML
<ContentPage xmlns="http://xamarin.com/schemas/2014/forms">
    <StackLayout Padding="20">
        <Entry Text="{Binding Title, Mode=TwoWay}" Placeholder="Title"/>
        
        <CollectionView ItemsSource="{Binding Books}">
            <CollectionView.ItemTemplate>
                <DataTemplate>
                    <StackLayout Padding="10">
                        <Label Text="{Binding Title, StringFormat='Title: {0}'}"/>
                        <Label Text="{Binding Author, StringFormat='Author: {0}'}"/>
                        <Button Text="Delete" Command="{Binding Source={RelativeSource AncestorType={x:Type local:BookViewModel}}, Path=DeleteCommand}" CommandParameter="{Binding .}"/>
                    </StackLayout>
                </DataTemplate>
            </CollectionView.ItemTemplate>
        </CollectionView>
        
        <Button Text="Save" Command="{Binding SaveCommand}"/>
    </StackLayout>
</ContentPage>
```

---

## MVVM Flow Diagram

```
┌──────────────────┐
│   USER INTERACTS │
│   WITH VIEW      │
└────────┬─────────┘
         │ User input / Events
         ▼
    ┌─────────────────────────────────────┐
    │ VIEW (XAML/HTML)                    │
    │ - Display data bound to ViewModel   │
    │ - Handle user events                │
    │ - No business logic                 │
    └────────────┬────────────────────────┘
                 │ Two-way Data Binding
    ┌────────────▼────────────────────────┐
    │ VIEWMODEL                           │
    │ - Handle UI state                   │
    │ - Commands (Save, Delete, etc)      │
    │ - INotifyPropertyChanged            │
    │ - Call business logic               │
    └────────────┬────────────────────────┘
                 │ Service calls
    ┌────────────▼────────────────────────┐
    │ MODEL                               │
    │ - Domain objects                    │
    │ - Business logic                    │
    │ - Data access                       │
    └─────────────────────────────────────┘
```

---

## MVVM Key Features

- **Two-Way Data Binding** - Automatic sync between View and ViewModel
- **INotifyPropertyChanged** - Notify View of property changes
- **Commands** - Handle button clicks without code-behind
- **No Code-Behind** - View has minimal logic (XAML only)
- **Testable** - ViewModel tested independently of View

---

## Pros and Cons

### Advantages
✅ **Testability** - ViewModel logic tested without UI  
✅ **Data Binding** - Automatic UI updates  
✅ **Reusability** - ViewModel works with different Views  
✅ **Maintainability** - Clean separation of concerns  
✅ **Designer Friendly** - Designers work on XAML while devs work on ViewModel  

### Disadvantages
❌ **Learning Curve** - Data binding and INotifyPropertyChanged  
❌ **Complexity** - More layers than MVC  
❌ **Performance** - Excessive binding can be slow  
❌ **Debugging** - Bindings failures hard to debug  
❌ **Boilerplate** - Property change notifications repetitive  

---

## When to Use MVVM

### ✅ Use MVVM When:
- Building WPF, UWP, Xamarin applications
- Rich desktop/mobile UI with many interactions
- Two-way data binding needed
- Want UI separated from business logic
- Building Angular/Vue/React SPAs
- Testing UI logic without rendering

### ❌ Don't Use When:
- Simple web forms
- Server-rendered pages
- Minimal UI interactions
- No frameworks supporting data binding

---

## Interview Questions

**Q: What's INotifyPropertyChanged and why needed?**
A: Interface that notifies View when property changes. Without it, View won't update when ViewModel properties change.

**Q: How is MVVM different from MVC?**
A: MVC: Controller orchestrates. MVVM: ViewModel handles UI state with data binding. MVVM for rich clients, MVC for web.

**Q: What are Commands in MVVM?**
A: Objects that encapsulate actions. Allow buttons/events to call ViewModel methods without code-behind. Implement ICommand.

**Q: Can ViewModel reference View?**
A: No, that violates MVVM. ViewModel should be UI-agnostic. If needed, use Messenger pattern to decouple.

---

## Real-World Examples

- **WPF Applications** - Microsoft desktop
- **UWP Apps** - Universal Windows Platform
- **Xamarin Forms** - Mobile apps
- **Angular** - SPA framework
- **Vue.js** - JavaScript framework
- **React** - JavaScript framework (with Redux)

---

## Summary

MVVM separates UI from business logic through automatic data binding and ViewModel state management. Perfect for rich interactive applications with complex UI state. Enables UI developers and business logic developers to work independently. Trade-off: more layers, binding complexity.

**Key Takeaway:** MVVM uses data binding and ViewModels to separate UI state from business logic.
