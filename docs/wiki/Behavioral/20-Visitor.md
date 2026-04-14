# Visitor Pattern

## Overview

**Category:** Behavioral Pattern  
**Purpose:** Represent an operation to be performed on elements of an object structure. Visitor lets you define a new operation without changing the classes of the elements on which it operates.  
**Also Called:** Operation on Elements  
**Complexity:** High

## Problem

Adding new operations to object structures requires modifying element classes:

```csharp
// Problem: Adding operations requires changing element classes
public abstract class Element
{
    public abstract void Render();      // Rendering operation
    public abstract void Export();      // Export operation
    public abstract void Validate();    // Validation operation
    // Adding more operations requires changing all subclasses!
}

// Shape classes polluted with operations that don't belong
public class Circle : Element
{
    public override void Render() { }
    public override void Export() { }
    public override void Validate() { }
}
```

## Solution

Use visitor to define operations separate from elements:

```csharp
public interface IElement
{
    void Accept(IVisitor visitor);
}

public interface IVisitor
{
    void Visit(Circle circle);
    void Visit(Square square);
}

// Elements stay clean, only accept visitors
public class Circle : IElement
{
    public void Accept(IVisitor visitor) => visitor.Visit(this);
}

// Operations defined in visitors, not in elements
public class RenderVisitor : IVisitor
{
    public void Visit(Circle circle) => Console.WriteLine("Rendering circle");
    public void Visit(Square square) => Console.WriteLine("Rendering square");
}
```

## Implementation Approaches

### 1. Shape Visitor

```csharp
public interface IShape
{
    void Accept(IShapeVisitor visitor);
    double GetArea();
}

public interface IShapeVisitor
{
    void Visit(Circle circle);
    void Visit(Rectangle rectangle);
    void Visit(Triangle triangle);
}

public class Circle : IShape
{
    public double Radius { get; set; }

    public Circle(double radius) => Radius = radius;

    public void Accept(IShapeVisitor visitor) => visitor.Visit(this);

    public double GetArea() => Math.PI * Radius * Radius;
}

public class Rectangle : IShape
{
    public double Width { get; set; }
    public double Height { get; set; }

    public Rectangle(double width, double height)
    {
        Width = width;
        Height = height;
    }

    public void Accept(IShapeVisitor visitor) => visitor.Visit(this);

    public double GetArea() => Width * Height;
}

public class Triangle : IShape
{
    public double Base { get; set; }
    public double Height { get; set; }

    public Triangle(double baseLength, double height)
    {
        Base = baseLength;
        Height = height;
    }

    public void Accept(IShapeVisitor visitor) => visitor.Visit(this);

    public double GetArea() => 0.5 * Base * Height;
}

// Visitor 1: Calculate area
public class AreaCalculatorVisitor : IShapeVisitor
{
    public double TotalArea { get; private set; }

    public void Visit(Circle circle)
    {
        Console.WriteLine($"📐 Calculating circle area: {circle.GetArea():F2}");
        TotalArea += circle.GetArea();
    }

    public void Visit(Rectangle rectangle)
    {
        Console.WriteLine($"📐 Calculating rectangle area: {rectangle.GetArea():F2}");
        TotalArea += rectangle.GetArea();
    }

    public void Visit(Triangle triangle)
    {
        Console.WriteLine($"📐 Calculating triangle area: {triangle.GetArea():F2}");
        TotalArea += triangle.GetArea();
    }
}

// Visitor 2: Calculate perimeter
public class PerimeterCalculatorVisitor : IShapeVisitor
{
    public double TotalPerimeter { get; private set; }

    public void Visit(Circle circle)
    {
        var perimeter = 2 * Math.PI * circle.Radius;
        Console.WriteLine($"📏 Circle perimeter: {perimeter:F2}");
        TotalPerimeter += perimeter;
    }

    public void Visit(Rectangle rectangle)
    {
        var perimeter = 2 * (rectangle.Width + rectangle.Height);
        Console.WriteLine($"📏 Rectangle perimeter: {perimeter:F2}");
        TotalPerimeter += perimeter;
    }

    public void Visit(Triangle triangle)
    {
        var perimeter = triangle.Base + (2 * Math.Sqrt((triangle.Height * triangle.Height) + 
                        (triangle.Base / 2) * (triangle.Base / 2)));
        Console.WriteLine($"📏 Triangle perimeter: {perimeter:F2}");
        TotalPerimeter += perimeter;
    }
}

// Usage
var shapes = new List<IShape>
{
    new Circle(5),
    new Rectangle(4, 6),
    new Triangle(3, 4)
};

var areaVisitor = new AreaCalculatorVisitor();
foreach (var shape in shapes)
    shape.Accept(areaVisitor);
Console.WriteLine($"✅ Total area: {areaVisitor.TotalArea:F2}\n");

var perimeterVisitor = new PerimeterCalculatorVisitor();
foreach (var shape in shapes)
    shape.Accept(perimeterVisitor);
Console.WriteLine($"✅ Total perimeter: {perimeterVisitor.TotalPerimeter:F2}");
```

### 2. Document Element Visitor

```csharp
public interface IDocumentElement
{
    void Accept(IDocumentVisitor visitor);
}

public interface IDocumentVisitor
{
    void Visit(TextElement text);
    void Visit(ImageElement image);
    void Visit(CodeElement code);
    void Visit(HeadingElement heading);
}

public class TextElement : IDocumentElement
{
    public string Content { get; set; }
    public TextElement(string content) => Content = content;
    public void Accept(IDocumentVisitor visitor) => visitor.Visit(this);
}

public class ImageElement : IDocumentElement
{
    public string ImagePath { get; set; }
    public ImageElement(string path) => ImagePath = path;
    public void Accept(IDocumentVisitor visitor) => visitor.Visit(this);
}

public class CodeElement : IDocumentElement
{
    public string Code { get; set; }
    public string Language { get; set; }
    public CodeElement(string code, string lang) { Code = code; Language = lang; }
    public void Accept(IDocumentVisitor visitor) => visitor.Visit(this);
}

public class HeadingElement : IDocumentElement
{
    public string Title { get; set; }
    public int Level { get; set; }
    public HeadingElement(string title, int level) { Title = title; Level = level; }
    public void Accept(IDocumentVisitor visitor) => visitor.Visit(this);
}

// Visitor 1: HTML export
public class HtmlExportVisitor : IDocumentVisitor
{
    private List<string> _html = new();

    public void Visit(TextElement text) => _html.Add($"<p>{text.Content}</p>");
    public void Visit(ImageElement image) => _html.Add($"<img src='{image.ImagePath}'/>");
    public void Visit(CodeElement code) => 
        _html.Add($"<pre><code class='{code.Language}'>{code.Code}</code></pre>");
    public void Visit(HeadingElement heading) => 
        _html.Add($"<h{heading.Level}>{heading.Title}</h{heading.Level}>");

    public void ExportTo(string filename)
    {
        Console.WriteLine($"📝 Exporting to HTML: {filename}");
        foreach (var line in _html)
            Console.WriteLine(line);
    }
}

// Visitor 2: Markdown export
public class MarkdownExportVisitor : IDocumentVisitor
{
    private List<string> _markdown = new();

    public void Visit(TextElement text) => _markdown.Add(text.Content);
    public void Visit(ImageElement image) => _markdown.Add($"![image]({image.ImagePath})");
    public void Visit(CodeElement code) => 
        _markdown.Add($"```{code.Language}\n{code.Code}\n```");
    public void Visit(HeadingElement heading) => 
        _markdown.Add($"{'#' repeated heading.Level} {heading.Title}");

    public void ExportTo(string filename)
    {
        Console.WriteLine($"📝 Exporting to Markdown: {filename}");
        foreach (var line in _markdown)
            Console.WriteLine(line);
    }
}

// Usage
var document = new List<IDocumentElement>
{
    new HeadingElement("My Document", 1),
    new TextElement("This is a paragraph."),
    new CodeElement("var x = 42;", "csharp"),
    new ImageElement("image.png")
};

var htmlVisitor = new HtmlExportVisitor();
foreach (var element in document)
    element.Accept(htmlVisitor);
htmlVisitor.ExportTo("output.html");
```

### 3. File System Visitor

```csharp
public interface IFileSystemElement
{
    void Accept(IFileSystemVisitor visitor);
}

public interface IFileSystemVisitor
{
    void Visit(File file);
    void Visit(Directory directory);
}

public class File : IFileSystemElement
{
    public string Name { get; set; }
    public long Size { get; set; }

    public File(string name, long size)
    {
        Name = name;
        Size = size;
    }

    public void Accept(IFileSystemVisitor visitor) => visitor.Visit(this);
}

public class Directory : IFileSystemElement
{
    public string Name { get; set; }
    public List<IFileSystemElement> Contents { get; set; }

    public Directory(string name)
    {
        Name = name;
        Contents = new List<IFileSystemElement>();
    }

    public void Accept(IFileSystemVisitor visitor) => visitor.Visit(this);
}

// Visitor 1: Calculate total size
public class SizeCalculatorVisitor : IFileSystemVisitor
{
    public long TotalSize { get; private set; }

    public void Visit(File file)
    {
        Console.WriteLine($"📄 {file.Name}: {file.Size} bytes");
        TotalSize += file.Size;
    }

    public void Visit(Directory directory)
    {
        Console.WriteLine($"📁 {directory.Name}/");
        foreach (var element in directory.Contents)
        {
            element.Accept(this);
        }
    }
}

// Visitor 2: List all files
public class FileListVisitor : IFileSystemVisitor
{
    private int _indent = 0;

    public void Visit(File file)
    {
        Console.WriteLine(new string(' ', _indent) + $"📄 {file.Name}");
    }

    public void Visit(Directory directory)
    {
        Console.WriteLine(new string(' ', _indent) + $"📁 {directory.Name}/");
        _indent += 2;
        foreach (var element in directory.Contents)
        {
            element.Accept(this);
        }
        _indent -= 2;
    }
}

// Usage
var root = new Directory("root");
root.Contents.Add(new File("file1.txt", 1024));
root.Contents.Add(new File("file2.txt", 2048));

var subdir = new Directory("subdir");
subdir.Contents.Add(new File("file3.txt", 512));
root.Contents.Add(subdir);

var sizeVisitor = new SizeCalculatorVisitor();
root.Accept(sizeVisitor);
Console.WriteLine($"Total size: {sizeVisitor.TotalSize} bytes\n");

var listVisitor = new FileListVisitor();
root.Accept(listVisitor);
```

### 4. Report Generator Visitor

```csharp
public interface IReportElement
{
    void Accept(IReportVisitor visitor);
}

public interface IReportVisitor
{
    void Visit(TextSection section);
    void Visit(DataTable table);
    void Visit(Chart chart);
}

public class TextSection : IReportElement
{
    public string Content { get; set; }
    public TextSection(string content) => Content = content;
    public void Accept(IReportVisitor visitor) => visitor.Visit(this);
}

public class DataTable : IReportElement
{
    public string[][] Data { get; set; }
    public DataTable(string[][] data) => Data = data;
    public void Accept(IReportVisitor visitor) => visitor.Visit(this);
}

public class Chart : IReportElement
{
    public string ChartType { get; set; }
    public Chart(string type) => ChartType = type;
    public void Accept(IReportVisitor visitor) => visitor.Visit(this);
}

// Visitor 1: PDF report
public class PdfReportVisitor : IReportVisitor
{
    public void Visit(TextSection section)
    {
        Console.WriteLine($"📄 PDF Text: {section.Content}");
    }

    public void Visit(DataTable table)
    {
        Console.WriteLine($"📊 PDF Table with {table.Data.Length} rows");
    }

    public void Visit(Chart chart)
    {
        Console.WriteLine($"📈 PDF Chart: {chart.ChartType}");
    }
}

// Visitor 2: Excel report
public class ExcelReportVisitor : IReportVisitor
{
    public void Visit(TextSection section)
    {
        Console.WriteLine($"📊 Excel Cell: {section.Content}");
    }

    public void Visit(DataTable table)
    {
        Console.WriteLine($"📊 Excel Sheet with {table.Data.Length} rows");
    }

    public void Visit(Chart chart)
    {
        Console.WriteLine($"📈 Excel Embedded Chart: {chart.ChartType}");
    }
}

// Usage
var report = new List<IReportElement>
{
    new TextSection("Sales Report Q1"),
    new DataTable(new[] { new[] { "Product", "Sales" }, new[] { "A", "100" } }),
    new Chart("BarChart")
};

var pdfVisitor = new PdfReportVisitor();
foreach (var element in report)
    element.Accept(pdfVisitor);

Console.WriteLine();

var excelVisitor = new ExcelReportVisitor();
foreach (var element in report)
    element.Accept(excelVisitor);
```

---

## Visitor Pattern Structure

```
       IElement              IVisitor
       /      \              /    \
    ConcreteA ConcreteB  VisitorX VisitorY
        |        |
        +---Accept(Visitor)
            Calls: Visitor.Visit(this)
```

---

## Pros and Cons

### Advantages
✅ **Separation of Concerns** - Operations separate from structures  
✅ **Open/Closed Principle** - Easy to add new operations  
✅ **Multiple Operations** - Same structure supports many visitors  
✅ **Complex Operations** - Visitor can access all element properties  
✅ **No Type Checking** - Polymorphism handles types automatically  

### Disadvantages
❌ **Complex** - One of the most complex patterns  
❌ **Double Dispatch** - Hard to understand initially  
❌ **Adding Elements** - New element types require changing all visitors  
❌ **Encapsulation** - Elements must expose enough data to visitors  
❌ **Overkill** - Overhead for simple operations  

---

## When to Use

### ✅ Use Visitor When:
- Many distinct operations on complex object structures
- Structure is stable, operations change frequently
- Need to perform several unrelated operations
- Operations depend on concrete types
- Want to avoid cluttering element classes
- Need to add operations without modifying elements

### ❌ Don't Use When:
- Structure changes frequently
- Few operations needed
- Simple methods sufficient
- Simplicity valued over flexibility
- Elements must be independent

---

## Interview Questions

**Q: What's "double dispatch" in Visitor pattern?**
A: First dispatch: calling Accept() on element type. Second dispatch: calling Visit() on visitor type. Polymorphism works twice.

**Q: How does Visitor differ from Composite?**
A: Composite builds tree structure; Visitor operates on it. Composite is about structure; Visitor is about operations.

**Q: What if you need to add a new element type?**
A: You must update all visitor interfaces and implementations. This is a weakness of Visitor pattern.

**Q: Can you combine Visitor with Iterator?**
A: Yes, iterate over elements and apply visitor to each. Visitor handles operations; Iterator handles traversal.

---

## Related Patterns

| Pattern | Relation |
|---------|----------|
| **Composite** | Visitor often used with Composite for tree operations |
| **Iterator** | Iterator traverses; Visitor operates on elements |
| **Strategy** | Both encapsulate operations; Visitor for structures |
| **Double Dispatch** | Core mechanism enabling Visitor pattern |

---

## Summary

Visitor pattern separates operations from object structures through polymorphism. Perfect for complex structures needing multiple operations: ASTs, file systems, document objects, game entities. Operations defined in visitors, structures in elements. Trade-off: hard to add element types, easy to add operations.

**Key Takeaway:** Visitor separates operations from structures through double dispatch.
