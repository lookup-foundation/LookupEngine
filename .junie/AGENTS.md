# LookupEngine Agent Instructions

LookupEngine is a high-performance, reflection-based engine that decomposes any runtime object into its members and evaluates their values, tracking time and memory for each operation. It ships as a public NuGet package split into two layers. `source/LookupEngine.Abstractions` holds pure, framework-agnostic C# (interfaces, contracts, metadata), and `source/LookupEngine` holds the reflection engine, built-in descriptors, diagnostics, and options. Custom type handling is added through descriptors, never by changing the engine.

## Non-Negotiables

* Keep the two layers separated. `LookupEngine.Abstractions` stays pure C# with no engine or implementation dependency. Engine logic and built-in descriptors live in `LookupEngine`.
* Never crash during decomposition. Reflection failures are caught and surfaced as values (for example, the thrown exception becomes the member value), never propagated out of `Decompose`.
* Stay thread-safe by design. Each `LookupComposer.Decompose*` call creates an isolated instance. Never introduce shared mutable state between decompositions.
* This is a synchronous library. Do not add `async` or `await`, because reflection (`GetValue`, `Invoke`) is inherently synchronous.
* Extend through descriptors implementing the `Configuration` interfaces, not by special-casing types inside the engine. Descriptors are immutable after construction and use primary constructors.
* Treat the public surface as a contract. Mark public types `[PublicAPI]` and read-only methods `[Pure]`, keep nullable warnings as errors, and never break an existing public API. Deprecate instead.
* Be allocation-conscious on the decomposition hot path. Reach for the most efficient construct that fits (value-type `struct`, `Span`, `[UnsafeAccessor]`, and similar where applicable), pre-size collections, avoid LINQ and boxing where it matters, and dispose every `IEnumerator`.
* When an implementation has more than one viable approach, benchmark the alternatives and let the numbers decide. Strategy benchmarks hold their own clean candidate code and must not reference LookupEngine's implementation.
* Update `README.md`, `CHANGELOG.md`, and XML docs in the same change as any public-surface change. XML docs state clearly what a member does and never describe the implementation, which is free to change.

## Specialized Docs

Before making related changes, read the matching file.

* [Project Structure](../docs/project-structure.md). Solution layout, the Abstractions and engine split, and where each change belongs.
* [Architecture](../docs/architecture.md). Design goals, the decomposition pipeline, the entry-point API, and options.
* [Descriptor System](../docs/descriptors.md). The extensibility model: descriptors, configurators, redirection, variants, and built-in descriptors.
* [Code Style](../docs/code-style.md). C# conventions, naming, file and class structure, data objects, and error handling.
* [Performance](../docs/performance.md). Allocation and reflection guidelines for the hot path.
* [Testing Strategy](../docs/testing-strategy.md). Unit tests (TUnit) and benchmarks (BenchmarkDotNet).
* [Package Management](../docs/package-management.md). Centralized NuGet versions and multi-targeting.
