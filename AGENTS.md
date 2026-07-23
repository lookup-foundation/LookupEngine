# LookupEngine

LookupEngine decomposes any runtime object into its members through reflection, evaluates each value, and records the time and memory every evaluation costs.
It ships in two layers: `source/LookupEngine.Abstractions` holds the pure contracts, and `source/LookupEngine` holds the reflection engine, built-in descriptors, options, and diagnostics.
The engine runs on hot paths such as interactive object inspectors; it stays fast and never crashes on the reflection edge cases real objects produce.

## Non-negotiables

* Never crash during decomposition. A reflection failure is captured as the member's value and never propagates out of `Decompose`; unwrap `TargetInvocationException` to its inner exception first.
* Isolate every call. Each `LookupComposer.Decompose*` call runs on a fresh internal instance; concurrent calls are safe without external synchronization. Never add shared mutable state.
* Keep the abstractions layer pure. `LookupEngine.Abstractions` stays framework-agnostic with no dependency on the engine. Everything a custom descriptor needs to compile lives there.
* Synchronous only. Reflection is synchronous; add no `async` or `await`.
* Extend through descriptors. Teach the engine about a type with a descriptor that implements the configuration interfaces, never by special-casing a type inside the engine.
* Never break the public surface. Deprecate a renamed member with `[Obsolete]`, name the replacement, and keep the member functional.
* Every type compiles under every target framework.
* A change ships with a test that exercises it.
* Confirm an unfamiliar API before use through official docs or `gh` (`gh api`, `gh search code`).
* A public-surface change updates `README.md`, `CHANGELOG.md`, and the XML docs in the same commit.

## Architecture

* `LookupComposer` exposes the whole public engine API with `Decompose` (object and members), `DecomposeObject` (object alone), and `DecomposeMembers` (members alone) entry point
* Each call constructs a fresh composer through its `private protected` constructor. That instance is the unit of isolation; all working state lives on it. Any shared cache is immutable or a `ConcurrentDictionary`.
* Decomposition builds the type hierarchy once and walks it base-to-derived. At each level it resolves the declaring descriptor through `DecomposeOptions.TypeResolver`, computes the binding flags once, collects fields, properties, methods, and events in order, and flushes registered extensions; enumerable items are added last. Per member it applies any descriptor configuration, evaluates now or defers per the member's `MemberEvaluationPolicy` and the options' `MethodEvaluationPolicy`, and records the elapsed time and allocated bytes onto the `DecomposedMember`.
* Evaluation runs inside a guarded `try`/`finally` measured by the internal `TimeDiagnoser` and `MemoryDiagnoser` behind `IEngineDiagnoser`. `EngineException` covers internal engine faults only; it never appears on the value-capture path.

## Repository map

* `source/LookupEngine.Abstractions/` — the pure contract layer. `Configuration/` holds the descriptor and member-configurator interfaces and the fluent builders, `Decomposition/` holds the Descriptor abstraction and the `Variants` factory, `Metadata/` holds the result model, and `Enums/` holds the member flags and evaluation policy.
* `source/LookupEngine/` — the engine. `Engine/` holds `LookupComposer` and the generic `LookupComposer<TContext>`, split into partials by responsibility; `Descriptors/` holds the built-in descriptors; `Options/` holds LookupComposer's `DecomposeOptions` and `MethodEvaluationPolicy`; `Diagnostic/` holds the time and memory diagnosers; `Formaters/` holds the display formatters; `Exceptions/` holds engine exceptions.
* `tests/LookupEngine.Tests.Unit/` — the TUnit suite.
* `tests/LookupEngine.Tests.Benchmarks/` — the BenchmarkDotNet console executable.
* `build/` — the ModularPipelines build.
* Root — `Directory.Build.props`, `Directory.Packages.props`, `global.json`, the `LookupEngine.slnx` solution, `README.md`, `CHANGELOG.md`, `CONTRIBUTING.md`, CI workflows.

## Build and verify

* Compile: `dotnet build -c Release`.
* Test: `dotnet test`.
* Run the benchmarks: `dotnet run -c Release --project tests/LookupEngine.Tests.Benchmarks`.
