# Architecture & Design Principles

LookupEngine takes an arbitrary runtime object and produces a structured, evaluated description of it. It captures every field, property, method, event, and synthetic extension, with the value, type, and time and memory cost of each. It is built to run on hot paths, such as interactive object inspectors, without crashing on the messy reflection edge cases real objects produce.

## Core Design Goals

* **Performance.** Minimize allocations and track time and memory per operation. See [Performance](./performance.md).
* **Extensibility.** A descriptor-based system supplies custom type handling without touching the engine. See [Descriptor System](./descriptors.md).
* **Safety.** Graceful degradation, where a failing member yields a value rather than a crash.
* **Thread-safety.** A stateless static API with per-call instance isolation.

## Entry-Point API

`LookupComposer` is the static entry point. Three operations, each taking the object and optional `DecomposeOptions` that default to `DecomposeOptions.Default`.

```csharp
DecomposedObject       Decompose(object? value, DecomposeOptions? options = null); // object and its members
DecomposedObject       DecomposeObject(object? value, DecomposeOptions? options = null); // object only, no members
List<DecomposedMember> DecomposeMembers(object? value, DecomposeOptions? options = null); // members only
```

A `Type` argument decomposes the type's static surface, any other value decomposes the instance. A `null` value returns a well-defined nullable decomposition, and an empty list for `DecomposeMembers`.

When decomposition needs caller-supplied context, passed through to context-aware descriptors, use the generic overloads on `LookupComposer<TContext>`. These take a `DecomposeOptions<TContext>` carrying a required `Context`.

```csharp
DecomposedObject Decompose<TContext>(object? value, DecomposeOptions<TContext> options);
```

## Decomposition Pipeline

Each call constructs a fresh `LookupComposer`, the unit of isolation, and walks the object's type hierarchy, collecting members per kind. For each member it:

1. Resolves a `Descriptor` for the declaring value, built-in or custom, via `DecomposeOptions.TypeResolver`.
2. Applies any descriptor configuration, meaning resolved handlers, evaluation overrides, and synthetic extensions.
3. Decides whether to evaluate now or defer, per the member's `MemberEvaluationPolicy` and the options' `MethodEvaluationPolicy`.
4. Evaluates inside guarded reflection. A thrown exception is captured as the member's value, per Error Handling below.
5. Records the time and allocated bytes the evaluation cost.

The output model lives in `Metadata/`. A `DecomposedObject` holds the root descriptor and a list of `DecomposedMember`s. Each member carries its `MemberAttributes`, depth, metrics, evaluation policy, and a lazy evaluator. Each value is a `DecomposedValue`.

## Options

`DecomposeOptions` is the single configuration surface, and all opt-in flags default off. It controls what is included (root, fields, events, private members, static members, unsupported members), which extensibility features are active (`EnableExtensions`, `EnableRedirection`), the `MethodEvaluationPolicy` that decides which methods auto-evaluate versus defer, and the `TypeResolver` map from a value to its `Descriptor`. See [Descriptor System](./descriptors.md) for the resolver and evaluation policy details.

## Design Rules

* The engine never special-cases concrete user types. Per-type behavior comes from descriptors only.
* Diagnostics are internal (`IEngineDiagnoser`). The cross-framework time source and the per-thread allocation counter are wrapped behind `TimeDiagnoser` and `MemoryDiagnoser`.

## Error Handling

The decomposition path never throws. No reflection failure escapes `Decompose`.

* **Capture failures as values.** In a member-evaluation path, catch the exception and store it as the member's value. Unwrap `TargetInvocationException` to its inner exception first.

  ```csharp
  catch (TargetInvocationException exception)
  {
      value = exception.InnerException;
  }
  catch (Exception exception)
  {
      value = exception;
  }
  ```
* **Custom exceptions are for internal errors only.** Prefer a dedicated, semantic exception type over a bare `Exception` for a distinct engine error. Use `EngineException` for an internal engine error, and add a new type when an error category warrants its own catch. These never appear on the value-capture path above.
* **Validate at the boundary.** Validate inputs at the public API surface (`LookupComposer.Decompose*`). The engine tolerates malformed members downstream.
