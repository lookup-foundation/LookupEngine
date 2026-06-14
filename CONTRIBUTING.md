## Table of contents

<!-- TOC -->
  * [Contributing to LookupEngine](#contributing-to-lookupengine)
  * [Fork, Clone, Branch and Create your PR](#fork-clone-branch-and-create-your-pr)
  * [Development Guidelines](#development-guidelines)
    * [Core Principles](#core-principles)
    * [Code Quality Standards](#code-quality-standards)
  * [Building](#building)
    * [Compiling Source Code](#compiling-source-code)
  * [Solution structure](#solution-structure)
  * [Code Style & Conventions](#code-style--conventions)
    * [Naming Conventions](#naming-conventions)
    * [File Organization](#file-organization)
    * [Error Handling Pattern](#error-handling-pattern)
  * [Testing Requirements](#testing-requirements)
    * [Unit Test Guidelines](#unit-test-guidelines)
    * [Example Test Structure](#example-test-structure)
<!-- TOC -->

## Contributing to LookupEngine

Thank you for considering contributing to LookupEngine! This document provides guidelines and instructions for contributing to the project.

## Fork, Clone, Branch and Create your PR

1. Fork the repo if you haven't already.
2. Clone your fork locally.
3. Create & push a feature branch.
4. Create a [Draft Pull Request (PR)](https://github.blog/2019-02-14-introducing-draft-pull-requests/).
5. Work on your changes.

## Development Guidelines

### Core Principles

- **Performance First:** LookupEngine is a high-performance reflection library. Every change should consider performance implications.
- **Thread-Safety:** Maintain the thread-safe design. Each `Decompose()` call must create an isolated instance.
- **Backward Compatibility:** Public API changes require careful consideration and major version bumps.
- **Zero Dependencies:** Keep `LookupEngine.Abstractions` dependency-free (except JetBrains.Annotations for dev).

### Code Quality Standards

When adding new classes/methods/changing existing code:

1. **Testing:**
   - Write unit tests for new functionality
   - Run all tests locally before submitting PR
   - Add performance benchmarks for critical paths

2. **Documentation:**
   - Add comprehensive XML documentation comments
   - Include usage examples for public APIs
   - Update `Readme.md` for new features

3. **Code Style:**
   - Use primary constructors (C# 12)
   - File-scoped namespaces
   - No abbreviations in naming (`repository`, not `repo`)

4. **Review Process:**
   - Keep commits atomic with clear messages
   - Address code review feedback promptly
   - Squash commits before merging if requested

## Building

### Compiling Source Code

**Using JetBrains Rider (Recommended):**

1. Open JetBrains Rider
2. Open the solution file `LookupEngine.slnx`
3. Wait for solution to restore packages
4. Select `Debug` configuration from the dropdown
5. Build: `Build -> Build Solution` (or `Ctrl+Shift+B`)
6. Run tests: `Tools -> Run Unit Tests` (or `Ctrl+U, Ctrl+R`)

## Solution structure

```
LookupEngine/
├── source/                              # Core library code
│   ├── LookupEngine/                    # Main implementation
│   │   ├── Engine/                      # Core decomposition engine
│   │   │   ├── LookupComposer.cs        # Public API
│   │   │   ├── LookupComposer.Decomposition.cs
│   │   │   ├── LookupComposer.Decomposition.Methods.cs
│   │   │   ├── LookupComposer.Decomposition.Properties.cs
│   │   │   ├── LookupComposer.Decomposition.Fields.cs
│   │   │   └── LookupComposer.Features.*.cs
│   │   ├── Descriptors/                 # Built-in type descriptors
│   │   ├── Diagnostic/                  # Performance tracking
│   │   └── Options/                     # Configuration
│   └── LookupEngine.Abstractions/       # Public contracts
│       ├── Configuration/               # Descriptor interfaces
│       ├── Decomposition/               # Core abstractions
│       ├── Metadata/                    # Output models
│       └── Enums/                       # Public enums
├── tests/
│   ├── LookupEngine.Tests.Unit/         # Unit tests
│   └── LookupEngine.Tests.Performance/  # BenchmarkDotNet benchmarks
├── build/                               # Nuke build automation
├── .github/workflows/                   # CI/CD pipelines
├── .junie/                              # AI development guidelines
└── .run/                                # JetBrains Rider run configs
```

## Code Style & Conventions

### Naming Conventions

```csharp
// ✅ Good - Descriptive, no abbreviations
public sealed class ElementDescriptor(Element element) : Descriptor, IDescriptorResolver
{
    public Func<IVariant>? Resolve(string target, ParameterInfo[] parameters)
    {
        return target switch
        {
            nameof(Element.GetBoundingBox) => ResolveBoundingBox,
            _ => null
        };
    }

    private IVariant ResolveBoundingBox()
    {
        return Variants.Value(element.GetBoundingBox());
    }
}

// ❌ Bad - Abbreviations, unclear naming
public class ElDesc(Element el) : Descriptor
{
    public Func<IVariant>? Resolve(string tgt, ParameterInfo[] parms)
    {
        return tgt switch
        {
            "GetBBox" => ResolveBBox,
            _ => null
        };
    }
}
```

### File Organization

**Partial Classes:** Use for logical separation of concerns
```
LookupComposer.cs                        // Public API
LookupComposer.Decomposition.cs          // Core decomposition logic
LookupComposer.Decomposition.Methods.cs  // Method-specific logic
LookupComposer.Diagnostic.cs             // Performance tracking
```

### Error Handling Pattern

```csharp
// ✅ Good - Graceful degradation
private void DecomposeProperties(BindingFlags bindingFlags)
{
    var members = MemberDeclaringType.GetProperties(bindingFlags);
    foreach (var member in members)
    {
        object? value;
        try
        {
            value = EvaluateValue(member);
        }
        catch (TargetInvocationException exception)
        {
            value = exception.InnerException; // Unwrap
        }
        catch (Exception exception)
        {
            value = exception; // Exception becomes the value
        }

        WriteDecompositionMember(value, member);
    }
}

// ❌ Bad - Crashes on error
private void DecomposeProperties(BindingFlags bindingFlags)
{
    foreach (var member in MemberDeclaringType.GetProperties(bindingFlags))
    {
        var value = member.GetValue(_input); // Unhandled exceptions!
        WriteDecompositionMember(value, member);
    }
}
```

## Testing Requirements

### Unit Test Guidelines

1. **Test naming:** `MethodName_Scenario_ExpectedBehavior`
   ```csharp
   [Fact]
   public void Decompose_NullValue_ReturnsNullableDecomposition()
   {
       var result = LookupComposer.Decompose(null);
       Assert.Equal(nameof(Object), result.TypeName);
   }
   ```

2. **Coverage requirements:**
   - All public APIs must have tests
   - Edge cases: null values, empty collections, exceptions
   - Descriptor interfaces: resolver, extension, redirector

3. **Performance tests:**
   - Add benchmarks for new decomposition paths
   - Measure memory allocations
   - Compare against baseline

### Example Test Structure

```csharp
public class DecompositionTests
{
    [Fact]
    public void Decompose_SimpleObject_ReturnsMembers()
    {
        // Arrange
        var testObject = new { Name = "Test", Value = 42 };

        // Act
        var result = LookupComposer.Decompose(testObject);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Members.Count);
    }
}
```