---
name: lookupengine-performance
description: >
    Keep the LookupEngine reflection engine and its descriptors allocation-light on the per-member hot path, and add a BenchmarkDotNet benchmark to check possible implementations.
    USE FOR: reducing allocations or reflection cost on the decompose path, choosing value types over heap types, pre-sizing collections, avoiding boxing and LINQ on hot paths, using spans, caching reflection metadata, disposing enumerators, reaching for unsafe accessors, and deciding when to benchmark and which candidate is the baseline.
    DO NOT USE FOR: writing a new abstractions or understanding the decompose pipeline structure.
license: MIT
---

# LookupEngine Performance

LookupEngine runs on hot paths such as interactive object inspectors and decomposes an object member by member; allocation and reflection cost compound quickly.
Every decomposition already records the time and bytes each member evaluation cost; a regression is visible in the result itself.
Keep the engine and the descriptors it invokes cheap on the per-member path; reach for the most efficient construct the situation allows, and let a benchmark, not intuition, decide between viable implementations.

## When to use

- Reducing allocations or reflection overhead on the decompose path in `source/LookupEngine`, or in a descriptor invoked per member.
- Deciding whether a change needs a BenchmarkDotNet benchmark and which candidate carries the baseline.

## When not to use

- Writing a new abstractions.
- Understanding the decompose pipeline structure.

## Workflow

### Step 1: Prefer value types for short-lived helpers

A small, immutable helper that would otherwise allocate on the heap should be a `struct`.
The fluent builders `MemberResolverBuilder` and `ExtensionBuilder` are `struct`; keep member configuration free of heap allocation; their value-producing methods are `readonly`.
`MemberRegistration` in the composer is a `readonly struct` for the same reason.
Do not return a `struct` builder through an interface, which boxes it back onto the heap.

### Step 2: Pre-size every collection with a known count

When the element count is known or estimable, construct the collection with that capacity so it never regrows.
The composer pre-sizes the member list, a single-entry registration list starts at capacity one, and the `Variants` factory takes a capacity hint.

```csharp
DecomposedMembers = new List<DecomposedMember>(32);
```

Pass the capacity to `Variants.Values<T>(capacity)` from a descriptor; do not grow from empty.

### Step 3: Avoid boxing and LINQ on the decompose path

Reflection already boxes value-typed results; add no further boxing in engine or descriptor code.
Walk members with a plain `foreach`, not LINQ; LINQ allocates iterators and delegates. The method decompose loop is a plain loop.

```csharp
var members = MemberDeclaringType.GetMethods(bindingFlags);
foreach (var member in members)
{
    if (member.IsSpecialName) continue;
    // ...
}
```

### Step 4: Use spans for slicing and scanning

Slice, trim, and scan with `Span<T>` / `ReadOnlySpan<T>`, not substrings or intermediate arrays.
`ReflexionFormater.FormatMemberName` trims a trailing `&` from a by-ref type name through a span, not by allocating a new string.

```csharp
var name = FormatTypeName(parameterType).AsSpan();
builder.Append(name[^1] == '&' ? name[..^1] : name);
```

For a small, bounded scratch buffer, use `stackalloc` behind a size guard so it stays off the heap.

### Step 5: Cache reflection metadata and touch it once

Compute binding flags once per hierarchy level, not once per member, and build the type hierarchy once with `GetTypeHierarchy` before walking the inheritance chain.
Cache a repeated reflection lookup in an immutable or concurrent structure: the composer caches the per-type interface-map check in a static `ConcurrentDictionary`; `GetInterfaceMap` runs once per descriptor type.

```csharp
private static readonly ConcurrentDictionary<(Type Descriptor, Type Interface), bool> ImplementationOwnerCache = new();
```

Always dispose an `IEnumerator` that is `IDisposable`, in a `try`/`finally`, as both `AddEnumerableItems` and `EnumerableDescriptor.ComputeIsEmpty` do.
For a measured hot path that reads a known, stable non-public member, replace the reflection call the engine makes today (`member.GetValue(Input)` / `member.Invoke(Input, null)` in `LookupComposer.Diagnostic.cs`) with an
`[UnsafeAccessor]` accessor, which skips the reflection lookup and invocation overhead with no extra allocation.

```csharp
[UnsafeAccessor(UnsafeAccessorKind.Method, Name = "ComputeInternal")]
private static extern int ComputeInternal(SampleType target);
```

Reserve `[UnsafeAccessor]` for hot paths proven by a benchmark, and gate it with `#if NET`; it is unavailable on the `net48` target.

### Step 6: Benchmark only a real choice, then verify

Add a benchmark to `tests/LookupEngine.Tests.Benchmarks` only when an implementation has more than one viable approach; compare the candidates and let the numbers decide.
Mark the candidate that mirrors the shipped code `[Benchmark(Baseline = true)]`; every `Ratio` and `Alloc Ratio` then reads against what the project ships today.
A strategy benchmark holds its own clean copies of the candidates and must not reference the engine's implementation types; it stays valid after the engine adopts a winner. `TypeHierarchyBenchmark` and
`ExtensionRegistrationBenchmark` follow this.
An end-to-end benchmark that calls the public entry point, such as `DecomposeBenchmark`, is the one exception that may reference the real API.

```csharp
[Benchmark(Baseline = true)]
public List<Type> ListWithDynamicGrowth()
{
    return GetTypeHierarchy(Type, true);
}
```

Run the benchmark console project in Release and read the allocation and time columns.

```shell
dotnet run -c Release --project tests/LookupEngine.Tests.Benchmarks
```

## Validation

- [ ] Small, immutable, short-lived helpers are `struct`, and no `struct` builder is returned through an interface.
- [ ] Every collection with a known count is pre-sized, including `Variants.Values<T>(capacity)` from descriptors.
- [ ] The decompose path uses plain loops, adds no boxing, and slices with spans where it trims or scans.
- [ ] Binding flags and the type hierarchy are computed once, repeated reflection lookups are cached in an immutable or concurrent structure, and every `IDisposable` enumerator is disposed.
- [ ] `[UnsafeAccessor]` is used only on a benchmark-proven hot path and gated with `#if NET`.
- [ ] A benchmark was added only for a real implementation choice, the shipped candidate carries `[Benchmark(Baseline = true)]`, and strategy benchmarks reference no engine types.

## Common Pitfalls

| Pitfall                                                           | Correct approach                                                                             |
|-------------------------------------------------------------------|----------------------------------------------------------------------------------------------|
| Allocating a class for a short-lived builder or helper            | Use a `readonly struct` with `readonly` methods, as `MemberResolverBuilder` does.            |
| Growing a `List<T>` from empty when the count is known            | Construct it with a capacity; pass the hint to `Variants.Values<T>(capacity)`.               |
| LINQ or extra boxing on the per-member path                       | Use plain `foreach` loops and avoid re-boxing values reflection already boxed.               |
| Recomputing binding flags or the type hierarchy per member        | Compute flags once per level and build the hierarchy once with `GetTypeHierarchy`.           |
| Leaving an `IEnumerator` undisposed after iterating               | Dispose it in `try`/`finally` when it is `IDisposable`.                                      |
| Marking the first or simplest candidate as the benchmark baseline | Put `[Benchmark(Baseline = true)]` on the candidate that mirrors the shipped implementation. |
| Adding a benchmark for a change with only one viable approach     | Benchmark only when candidates genuinely compete; otherwise report time and allocations.     |
