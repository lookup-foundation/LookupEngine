# Project Structure

LookupEngine separates the public contract from the engine that implements it. Keep code in the project that owns its responsibility, and keep the abstractions layer free of engine dependencies.

## Solution Groups

The solution is defined in `LookupEngine.slnx`.

* **`/source`**. The shipped NuGet packages.
    * `source/LookupEngine.Abstractions`: public interfaces, contracts, and metadata types. **Pure, framework-agnostic C#** with no dependency on the engine. Everything a custom descriptor needs to compile lives here.
    * `source/LookupEngine`: the reflection-based decomposition engine, built-in descriptors, diagnostics, and options. Depends on `LookupEngine.Abstractions`.
* **`/tests`**. Verification projects.
    * `tests/LookupEngine.Tests.Unit`: unit tests (TUnit).
    * `tests/LookupEngine.Tests.Performance`: benchmarks (BenchmarkDotNet, console executable).
* **`/build`**. The ModularPipelines build system (`Build.csproj`) for compile, test, changelog generation, and GitHub publishing.
* **Root level**:
    * Configuration: `Directory.Build.props`, `Directory.Packages.props`, `global.json`, `GitVersion.yml`, `renovate.json`, `.editorconfig`.
    * Documentation: `README.md`, `CHANGELOG.md`, `CONTRIBUTING.md`.
    * Agent guidelines: `AGENTS.md`, `CLAUDE.md`, `.junie/AGENTS.md`, `docs/`.
    * CI/CD: `.github/workflows`.

## Abstractions Layer (`source/LookupEngine.Abstractions`)

* `Configuration/`: the extensibility contracts. `IDescriptorCollector`, `IDescriptorConfigurator`(`<TContext>`), `IDescriptorEnumerator`, `IDescriptorRedirector`(`<TContext>`), the `IMemberConfigurator`(`<TContext>`) builder entry point, and the `MemberResolverBuilder` and `ExtensionBuilder` fluent builders.
* `Decomposition/`: the value model. The `Descriptor` base class, `IVariant`, `IVariantsCollection`(`<T>`), the `Variants` factory, and internal variant containers.
* `Metadata/`: the result model. `DecomposedObject`, `DecomposedMember`, `DecomposedValue`.
* `Enums/`: `MemberAttributes` (flags) and `MemberEvaluationPolicy`.

## Engine Layer (`source/LookupEngine`)

* `Engine/`: `LookupComposer` and the generic `LookupComposer<TContext>`, each split into partial files by responsibility (configuration, evaluation, redirection, diagnostics, and per-member-kind decomposition for properties, fields, methods, events, and enumeration).
* `Descriptors/`: built-in descriptors (`ObjectDescriptor`, `StringDescriptor`, `BooleanDescriptor`, `EnumerableDescriptor`, `ExceptionDescriptor`).
* `Options/`: `DecomposeOptions`(`<TContext>`) and `MethodEvaluationPolicy`.
* `Diagnostic/`: the internal diagnosers (`TimeDiagnoser`, `MemoryDiagnoser`) behind `IEngineDiagnoser`.
* `Exceptions/`: `EngineException` for internal engine errors only.
* `Formaters/`: reflection and modifier formatting helpers.

## Change Placement

* Add a public contract, interface, or metadata type → `LookupEngine.Abstractions`, in the matching subfolder. Never reach back into the engine from here.
* Add support for a new value kind → a descriptor in `LookupEngine/Descriptors`. See [Descriptor System](./descriptors.md).
* Change how members are discovered or evaluated → the relevant `LookupComposer.*` partial in `Engine/`.
* Add a configuration knob → `DecomposeOptions`.
* Add unit coverage → `tests/LookupEngine.Tests.Unit`. Add a performance measurement → `tests/LookupEngine.Tests.Performance`.
