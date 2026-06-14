# 2.0.0

## Breaking changes

- Replaced the extension registration API with a unified member configuration API. `IExtensionManager` and `IDescriptorResolver` are removed in favor of `IDescriptorConfigurator`, which exposes a single `Configure(IMemberConfigurator configuration)` entry point.
- Member handlers, evaluation overrides, and synthetic extensions are now declared through the fluent `MemberResolverBuilder` (`configuration.Member(name)`) and `ExtensionBuilder` (`configuration.Extension(name)`) builders instead of the old `Register`/`Resolve` methods.

## Engine

- Added force evaluation support: deferred members carry an `Evaluator` handle and can be evaluated on demand via `DecomposedMember.Evaluate()`.
- Added `MethodEvaluationPolicy` to control which methods are evaluated automatically during decomposition, with namespace wildcard matching and return-type exclusions (`None` and `All` presets, configurable via `DecomposeOptions.EvaluationPolicy`).
- Added `MemberEvaluationPolicy` (`Evaluated`, `Deferred`, `Disabled`, `Unsupported`) to expose the evaluation state of each decomposed member.
- Per-member configuration now supports forcing evaluation, deferring, disabling, marking as unsupported, and narrowing to a specific overload with `When`.
- Engine skips writing unsupported extension members based on configuration.

## Fixes

- Fixed concurrent access to the extension ownership cache.
- Fixed negative evaluation metrics reported for throwing members.
- Fixed full type name formatting for types without a namespace.
- Fixed unbounded redirection loops.
- Fixed description leakage between members through shared descriptors.
- Fixed inconsistent depth of enumerated collection items.
- Fixed repeated enumeration of collection sources.

## Performance

- Added object decomposition benchmarks.
- Added extension registration benchmarks for the new configuration API.

## Testing

- Added evaluation override, method evaluation, redirection, and enumerable test suites.
- Added regression tests for the fixed engine defects.
- Extended serialization round-trip checks and added more target frameworks to the test project.

## Dependencies

- Upgraded `ModularPipelines` to 3.2.8.
- Upgraded `Polyfill` to 10.9.0.
- Upgraded `TUnit` to 1.54.0.
- Updated the .NET SDK to 10.0.300 and replaced compiler directives with Polyfill sources.
- Added `renovate.json` for automated dependency updates.

# 1.1.0

## Global changes

- Migrated from `Nuke` to `ModularPipelines` build system.
- Support for .NET 10.
- Removed legacy `.sln` file in favor of `.slnx`.

## Engine

- Added support for serialization of decomposition results.
- Added exception handling for fields evaluation.
- Improved enumerator handling with proper disposal.
- Improved `DeclaringTypeFullName` formatting for types in global namespace.
- Replaced directives with `#if NET` for better cross-framework support.

## Performance

- Added new performance benchmarks for object decomposition.
- Refactored existing benchmarks for better organization and naming consistency.
- Optimized memory allocations in core engine paths.

## Testing

- Added ~100 new tests for the engine.

# 1.0.0

Initial release. Enjoy!