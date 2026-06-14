# Strict C# Production Style

All code must meet production-quality standards. "It works" is not enough. Code must be clean, readable, and self-explanatory. The full formatting ruleset is enforced by `.editorconfig`, so match it rather than restating exact values here.

## General Principles

* **SOLID and DRY.** Single responsibility per type. Extract shared logic rather than duplicating it.
* **Explicit over implicit.** Code should be self-explanatory. Avoid hidden behavior and magic defaults.
* **Modern C#.** `LangVersion` is `latest`, set in `Directory.Build.props`. Reach for the newest language features when they make code clearer or cheaper, including primary constructors, collection expressions, pattern matching, and the `field` keyword for property backing (as `DecomposeOptions.TypeResolver` does). Do not hand-roll what a current language feature expresses directly.
* **Nullable safety.** Nullable reference types are enabled solution-wide. Treat nullability warnings as defects.
* **JetBrains annotations.** `JetBrains.Annotations` is a global using. Mark public types `[PublicAPI]` and read-only methods `[Pure]`.

## Naming

* **Clarity is king.** Names must be descriptive and never abbreviated: `repository` not `repo`, `configuration` not `config`, `context` not `ctx`, `element` not `e`.
* **No single-letter variables** except in very short loops or lambdas.
* **`Async` suffix** on any method returning `Task` or `Task<T>`. Note that LookupEngine is synchronous by design (see below), so these should be rare.
* Interfaces are `IName` and generic type parameters are `TName`, enforced by `.editorconfig`.

## File & Class Structure

* **File-scoped namespaces** (`namespace LookupEngine;`). When a file's folder differs from its namespace, keep the namespace flat and add `// ReSharper disable once CheckNamespace` above it, as the engine partials do.
* **Member order:** private fields, then primary or other constructors, then public properties, then public methods, then private methods.
* **Sealed by default** for data models and descriptors unless inheritance is intended.

## Data Objects

* **Sealed classes** for the metadata models (`DecomposedObject`, `DecomposedMember`, `DecomposedValue`).
* **`required`** for mandatory initialization, and **`{ get; init; }`** for immutable properties.
* Use `record` sparingly, only for genuine value-based equality.

## Synchronous by Design

LookupEngine is a synchronous library. Reflection (`GetValue`, `Invoke`) is inherently synchronous. Do **not** introduce `async` or `await` without a proven, measured benefit.

## Error Handling

* **Graceful degradation.** Decomposition must never crash. In the member-evaluation paths, catch and convert exceptions to values, unwrapping `TargetInvocationException` to its inner exception.

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
* **Custom exceptions.** Prefer a dedicated, semantic exception type over throwing a bare `Exception` for a distinct engine error condition. Use `EngineException` for internal engine errors, and add a new exception type when an error category warrants its own catch. Custom exceptions are for internal errors only, never part of the value-capture path above.
* **Validate at the boundary.** Validate inputs at the public API surface (`LookupComposer.Decompose*`). The engine already tolerates malformed members downstream.

## XML Documentation

* Document every public type, method, and property with a `<summary>` that states clearly and directly what it does. Document parameters with `<param>` and return values with `<returns>`.
* For a non-trivial member, add a short `<remarks>` with the detail a caller needs (constraints, edge cases, what an empty or null result means). Keep it brief.
* **Never document the implementation or its internals.** Describe the observable behavior and contract, not how it is achieved. Implementation changes routinely, and docs that describe behavior do not need to change with it. For example, document that a result is computed lazily if a caller must know, but not which collection type or algorithm produces it.
* If a doc comment references another type or member, link it with `<see cref="..."/>` so renames stay tracked.
