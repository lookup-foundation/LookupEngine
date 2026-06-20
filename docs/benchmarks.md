# Benchmarks

Benchmarks live in `tests/LookupEngine.Tests.Benchmarks`, a BenchmarkDotNet console executable. They measure reflection overhead, allocations, and time for decomposition and for changes to the per-member hot path.

## When to Benchmark

* Add a benchmark when an implementation has more than one viable approach. Compare the candidates and let the numbers decide, never pick a strategy by intuition.
* A performance-sensitive change reports allocations and time before and after. A change that adds allocations to the per-member path needs justification.

## Strategy Benchmarks

* A strategy benchmark holds its own clean copies of the candidate implementations. It must not reference LookupEngine's implementation types. This isolates the comparison to the strategy itself and keeps the benchmark valid after the engine adopts a winner and its internals change.
* **The baseline marks the shipped implementation.** Put `[Benchmark(Baseline = true)]` on the candidate that mirrors what the project ships today, not on the simplest or first candidate.
* Every `Ratio` and `Alloc Ratio` then reads against the shipped code. Below `1.00` flags a real optimization, at or above `1.00` confirms the current choice. Move the baseline when the shipped approach changes.

## End-to-End Benchmarks

A benchmark that measures the public API across versions, such as `DecomposeBenchmark`, calls the public entry point directly. This is the one exception to the self-contained rule.

## Build and Run

Run the benchmark console project directly in Release: `dotnet run -c Release --project tests/LookupEngine.Tests.Benchmarks`.
