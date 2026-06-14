# Performance

LookupEngine runs on hot paths and decomposes objects member-by-member, so allocation and reflection cost compound quickly. Every decomposition already records its own time and memory (see [Architecture](./architecture.md)). Keep the engine and descriptors cheap enough that those numbers stay low.

Reach for the most efficient construct the situation allows. The guidelines below name the common ones, but the rule is general: when a cheaper primitive fits, use it.

## Memory Allocation

* **Prefer value types** for small, short-lived helpers that would otherwise allocate. The fluent builders (`MemberResolverBuilder`, `ExtensionBuilder`) are `struct` precisely so configuration costs no heap allocation. Use a `struct` (or `readonly struct`) when the type is small, immutable, and does not need reference identity.
* **Pre-size collections** when the count is known or estimable. The member list and variant collections take a capacity hint, so pass it (`Variants.Values<T>(capacity)`) rather than growing from empty.
* **Avoid boxing.** Reflection naturally boxes value types, so do not add further boxing in descriptor or evaluation code.
* **Avoid LINQ on the decomposition path.** Prefer plain loops where iterator allocation or delegate overhead matters.
* **Lazy work.** Compute expensive descriptor state only when asked. For example, `EnumerableDescriptor.IsEmpty` caches and uses an `ICollection.Count` fast path before falling back to enumeration.

## Spans and Slicing

* **Prefer `Span<T>` and `ReadOnlySpan<T>`** for slicing, parsing, and scanning instead of substrings, intermediate arrays, or `List<T>` copies. String and pattern work (as in `MethodEvaluationPolicy`) is a natural fit.
* **Use `stackalloc`** for small, bounded scratch buffers to keep them off the heap, guarded by a size check before falling back to a rented or heap buffer.

## Reflection

* **Cache binding flags** and reuse them across the decomposition instead of recomputing per member.
* **Build the type hierarchy once** and walk it for the inheritance chain rather than re-querying.
* **Always dispose `IEnumerator`** when it is `IDisposable`, using `try/finally` (see `EnumerableDescriptor.ComputeIsEmpty`). Each `Enumerator` access must return a fresh, non-advanced enumerator.
* **Consider `[UnsafeAccessor]`** for hot-path access to non-public members instead of classic reflection (`GetValue`/`Invoke`), where the target is known and stable. It avoids the reflection lookup and invocation overhead with no extra allocation. Reserve it for measured hot paths and validate the win with a benchmark.

## Validating Changes

* Any performance-sensitive change gets a benchmark in `tests/LookupEngine.Tests.Performance` (BenchmarkDotNet). See [Testing Strategy](./testing-strategy.md).
* Compare allocations and time before and after. A change that adds allocations to the per-member path needs justification.
* **When an implementation has more than one viable approach, always benchmark the alternatives and let the numbers decide.** Do not pick a strategy by intuition. The existing strategy benchmarks (for example, wildcard matching, sorting, type-name formatting) follow this.
* **Keep strategy benchmarks self-contained.** A benchmark that compares candidate implementations holds its own clean copies of those candidates and must not reference LookupEngine's implementation types. This isolates the comparison to the strategy itself and keeps the benchmark valid after the engine adopts a winner and its internals change. End-to-end benchmarks that measure the public API across versions (such as `DecomposeBenchmark`) are the exception and call the public entry point directly.
