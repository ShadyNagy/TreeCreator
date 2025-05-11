# TreeCreator

[![NuGet](https://img.shields.io/nuget/v/TreeCreator.svg)](https://www.nuget.org/packages/TreeCreator/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)

A flexible and powerful .NET library for generating directory tree representations with advanced filtering capabilities. Similar to the classic `tree` command-line utility but with more configuration options and a fluent API.

## Features

- Generate ASCII/text tree representations of directory structures
- Exclude specific directories or file extensions
- Include only specific directories or file extensions
- Fluent API for easy configuration
- Customizable output format
- Support for modern .NET applications

## Installation

### Package Manager

```
Install-Package TreeCreator
```

### .NET CLI

```
dotnet add package TreeCreator
```

### PackageReference

```xml
<PackageReference Include="TreeCreator" Version="1.0.3" />
```

## Quick Start

```csharp
using TreeCreator;

// Create a new tree generator
var treeCreator = TreeCreatorFactory.Create();

// Generate a tree for the specified directory
var result = treeCreator.Generate("C:/Projects/MyProject");

// Display the tree
Console.WriteLine(result.ToString());
```

## Usage Examples

### Basic Usage

```csharp
using TreeCreator;

// Create a new tree generator
var treeCreator = TreeCreatorFactory.Create();

// Generate a tree for the specified directory
var result = treeCreator.Generate("C:/Projects/MyProject");

// Display the tree
Console.WriteLine(result.ToString());
```

### Excluding Directories

```csharp
var treeCreator = TreeCreatorFactory.Create()
    .ExcludeDirectories("bin", "obj", "node_modules", ".git");

var result = treeCreator.Generate("C:/Projects/MyProject");
```

### Excluding File Extensions

```csharp
var treeCreator = TreeCreatorFactory.Create()
    .ExcludeExtensions(".dll", ".exe", ".pdb");

var result = treeCreator.Generate("C:/Projects/MyProject");
```

### Including Only Specific Directories

```csharp
var treeCreator = TreeCreatorFactory.Create()
    .IncludeOnlyDirectories("src", "tests", "docs");

var result = treeCreator.Generate("C:/Projects/MyProject");
```

### Including Only Specific File Extensions

```csharp
var treeCreator = TreeCreatorFactory.Create()
    .IncludeOnlyExtensions(".cs", ".csproj", ".md");

var result = treeCreator.Generate("C:/Projects/MyProject");
```

### Combining Multiple Filters

```csharp
var treeCreator = TreeCreatorFactory.Create()
    .ExcludeDirectories("bin", "obj")
    .IncludeOnlyExtensions(".cs", ".csproj");

var result = treeCreator.Generate("C:/Projects/MyProject");
```

### Working with the Result

```csharp
var treeCreator = TreeCreatorFactory.Create();
var result = treeCreator.Generate("C:/Projects/MyProject");

// Get the string representation
string treeString = result.ToString();

// Access individual lines
foreach (var line in result.Lines)
{
    Console.WriteLine($"Line: {line}");
}

// Save to a file
File.WriteAllText("tree-output.txt", result.ToString());
```

## API Reference

### TreeCreatorFactory

Static factory for creating instances of `ITreeCreator`.

```csharp
public static class TreeCreatorFactory
{
    public static ITreeCreator Create();
}
```

### ITreeCreator

Interface defining the core functionality of the tree generator.

```csharp
public interface ITreeCreator
{
    ITreeCreator ExcludeDirectories(params string[] directoryNames);
    ITreeCreator ExcludeExtensions(params string[] extensions);
    ITreeCreator IncludeOnlyDirectories(params string[] directoryNames);
    ITreeCreator IncludeOnlyExtensions(params string[] extensions);
    TreeResult Generate(string? rootPath);
}
```

### TreeResult

Represents the result of a directory tree generation operation.

```csharp
public class TreeResult
{
    public TreeResult(string rootPath);
    public void AppendLine(string line);
    public IReadOnlyList<string> Lines { get; }
    public override string ToString();
}
```

### TreeCreatorOptions

Configuration options for the tree generator.

```csharp
public class TreeCreatorOptions
{
    public HashSet<string> ExcludedDirectories { get; }
    public HashSet<string> ExcludedExtensions { get; }
    public HashSet<string> IncludedDirectories { get; }
    public HashSet<string> IncludedExtensions { get; }
}
```

## Example Output

```
C:/Projects/MyProject/
├── src/
│   ├── TreeCreator/
│   │   ├── DefaultTreeCreator.cs
│   │   ├── Interfaces/
│   │   │   └── ITreeCreator.cs
│   │   ├── Models/
│   │   │   ├── TreeCreatorOptions.cs
│   │   │   └── TreeResult.cs
│   │   └── TreeCreatorFactory.cs
│   └── TreeCreator.csproj
├── tests/
│   ├── TreeCreator.Tests/
│   │   ├── DefaultTreeCreatorTests.cs
│   │   └── TreeResultTests.cs
│   └── TreeCreator.Tests.csproj
└── README.md
```

## Requirements

- .NET 8.0 or higher

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## Acknowledgments

- Inspired by the classic Unix/Linux `tree` command
- Developed with love for the .NET community
