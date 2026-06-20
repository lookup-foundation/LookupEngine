# LookupEngine Agent Instructions

LookupEngine decomposes any runtime object into its members through reflection and evaluates each value. It records the time and memory every operation costs. It ships as a public NuGet package in two layers: `source/LookupEngine.Abstractions` holds pure, framework-agnostic contracts, and `source/LookupEngine` holds the reflection engine, built-in descriptors, and options.

## Non-Negotiables

* **Two-layer separation.** `LookupEngine.Abstractions` stays pure C# with no engine dependency. The engine and built-in descriptors live in `LookupEngine`.
* **Never crash during decomposition.** A reflection failure is captured as the member's value, never propagated out of `Decompose`.
* **Thread-safe by design.** Each `LookupComposer.Decompose*` call runs on an isolated instance. Never add shared mutable state.
* **Synchronous only.** Do not add `async` or `await`. Reflection is synchronous.
* **Extend through descriptors.** Add support for a type with a descriptor that implements the `Configuration` interfaces, never with a special case inside the engine.
* **The public surface is a contract.** Mark public types `[PublicAPI]` and side-effect-free methods `[Pure]`. Never break an existing public API, deprecate it instead.
* **Performance is a core requirement.** The engine runs on hot paths. Read [Performance](./docs/performance.md) before hot-path or descriptor work.
* **Tests ship with every change.** Add a benchmark only when more than one viable implementation exists. See [Testing](./docs/testing.md) and [Benchmarks](./docs/benchmarks.md).
* **Verify unfamiliar APIs.** When unsure of an API's behavior or signature, confirm it before use. Search the web for the official docs. To read a referenced library's source, query GitHub with `gh` (`gh api`, `gh search code`). If `gh` is unavailable, search the web or ask. Never inspect compiled DLLs or XML extracted from NuGet packages.
* **Keep docs in sync.** A public-surface change updates `README.md`, `CHANGELOG.md`, and the XML docs in the same commit. See [Documentation](./docs/documentation.md).

## Build

The build is a ModularPipelines project. Run `dotnet run -c Release` from the `build` directory to compile.

## Specialized Docs

Read the matching file before related work.

* [Project Structure](./docs/project-structure.md). Solution layout, the two-layer split, and where each change belongs.
* [Architecture](./docs/architecture.md). Design goals, the decomposition pipeline, the entry-point API, options, and the error-handling contract.
* [Descriptor System](./docs/descriptors.md). The extensibility model: descriptors, configurators, redirection, variants, and built-in descriptors.
* [Code Style](./docs/code-style.md). C# conventions, naming, attributes, language features, and data objects.
* [Performance](./docs/performance.md). Allocation and reflection on the hot path.
* [Testing](./docs/testing.md). Unit tests with TUnit.
* [Benchmarks](./docs/benchmarks.md). When and how to benchmark with BenchmarkDotNet.
* [Documentation](./docs/documentation.md). XML docs, README, CHANGELOG, and wiki.
* [Package Management](./docs/package-management.md). Centralized versions, multi-targeting, and dependencies.
