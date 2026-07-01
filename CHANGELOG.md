# 2.0.6

- `DecomposedMember.Evaluate()` is now repeatable: it no longer clears `Evaluator` after invoking, so the value can be refreshed on demand. Evaluated members now carry an `Evaluator`, and `Evaluator != null` means the member is evaluable.
- `Evaluate()` on a `Disabled` member now throws `InvalidOperationException` instead of reporting a synthetic disabled result; disabled and unsupported members have a `null` `Evaluator`.

# 2.0.5

## Fixes

- Methods returning `void` now report a `void` value type instead of `Object`.
- Write-only properties now honor `DecomposeOptions.IncludeUnsupported` and are skipped when unsupported members are excluded.

# 2.0.4

## Breaking changes

- Replaced `MethodEvaluationPolicy.IncludedNamespaces` and `ExcludedReturnTypes` with a single `EvaluatedFilter` predicate of type `Func<MethodInfo, Type, bool>` that decides per method whether to evaluate it eagerly.

# 2.0.3

- Added `ExtensionBuilder.Defer(Action)` and `ExtensionBuilder<TContext>.Defer(Action<TContext>)` overloads for deferring void extensions. The extension resolves to no value on force evaluation.

# 2.0.2

- Added deferred evaluation for synthetic extensions: `ExtensionBuilder.Defer(handler)` registers an extension whose value is computed only on force evaluation, mirroring the member builder.

## Breaking changes

- Renamed `ExtensionBuilder.AsDisabled()` to `Disable()` and `ExtensionBuilder.AsNotSupported()` to `NotSupported()`, aligning the extension terminal verbs with the member builder.

# 2.0.1

- Object-returning extension and member handlers no longer allocate a wrapping closure or a variant on each evaluation. The builder constructor callbacks now carry `Func<object?>` instead of `Func<IVariant>`; variant-returning handlers are unchanged.

# 2.0.0

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

## Breaking changes

- Replaced the extension registration API with a unified member configuration API. `IExtensionManager` and `IDescriptorResolver` are removed in favor of `IDescriptorConfigurator`, which exposes a single `Configure(IMemberConfigurator configuration)` entry point.
- Member handlers, evaluation overrides, and synthetic extensions are now declared through the fluent `MemberResolverBuilder` (`configuration.Member(name)`) and `ExtensionBuilder` (`configuration.Extension(name)`) builders instead of the old `Register`/`Resolve` methods.

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