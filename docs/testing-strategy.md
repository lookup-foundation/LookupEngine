# Testing Strategy

Tests cover the engine's behavior, meaning decomposition correctness, descriptor extensibility, evaluation policy, error handling, and thread safety, not the .NET reflection APIs themselves.

## Unit Tests

* **Framework:** TUnit, run through the Microsoft.Testing.Platform runner. Assertions use TUnit's assertion library.
* **Location:** `tests/LookupEngine.Tests.Unit`.
* **Coverage focus:** descriptor behavior, the configuration, extension, and redirection paths, member-attribute filtering, method evaluation policy, variant handling, null handling, serialization, options, and thread safety. The existing test files are named by concern, so add new cases to the matching file or create one named for the new concern.
* **Edge cases:** null inputs, empty collections, members that throw, static and private members, boundary values.

## What to Test

* **Engine behavior the library adds:** that a custom descriptor's configured member resolves to the expected value, that redirection follows when enabled, that a throwing member surfaces its exception as the value instead of crashing, and that each `Decompose*` call is isolated across threads.
* **Both API shapes:** the non-generic `LookupComposer` and the context-aware `LookupComposer<TContext>` where behavior differs.

## Performance Tests

* **Framework:** BenchmarkDotNet, as a console executable.
* **Location:** `tests/LookupEngine.Tests.Performance`.
* **Purpose:** measure reflection overhead, allocations, and time for decomposition and for changes to the per-member hot path. See [Performance](./performance.md).
* **Benchmark the alternatives.** When an implementation could be written more than one way, add a benchmark that compares the candidates rather than choosing by intuition.
* **Strategy benchmarks stay self-contained.** A benchmark comparing candidate implementations holds its own clean copies of those candidates and does not reference LookupEngine's implementation types, so the comparison measures the strategy alone and survives the engine's internals changing. End-to-end benchmarks of the public API across versions (such as `DecomposeBenchmark`) are the exception and call the public entry point directly.
* **Baseline marks the current implementation.** Put `[Benchmark(Baseline = true)]` on the candidate that mirrors what the project ships today, not on the simplest or first candidate. Then every `Ratio`/`Alloc Ratio` reads as "how this alternative compares to the shipped code": below `1.00` flags a real optimization opportunity, at or above `1.00` confirms the current choice. When the shipped approach changes, move the baseline with it.

## Running

* Build/test through the ModularPipelines build (`/build`): the `test` argument adds the test module. The local SDK and runner come from `global.json`.
