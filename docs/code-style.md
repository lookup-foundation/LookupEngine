# Code Style

Production C# only. The full formatting ruleset lives in `.editorconfig`. Match it, do not restate its values here.

## General Principles

* **SOLID and DRY.** One responsibility per type. Extract shared logic rather than duplicate it.
* **Explicit over implicit.** Code is self-explanatory. Avoid hidden behavior and magic defaults.
* **Nullable safety.** Nullable reference types are enabled solution-wide. Treat every nullability warning as a defect. Use `null!` only for a field that is always initialized before use.

## Comments

Public types and members carry XML doc comments, see [Documentation](./documentation.md). Inside the code, comments are the exception.

* Names and structure carry the meaning. Default to no comment.
* Add one only when the reason cannot be read from the code and a reader could break the code without it, such as a non-obvious invariant or why a line that looks removable must stay.
* A comment explains why, never what. Do not restate the code.

## Modern C#

`LangVersion` is `latest`. Reach for the newest feature that expresses the intent directly, and do not hand-roll what the language already provides.

* Primary constructors when a type captures state.
* Collection expressions for literals and spans.
* Pattern matching and switch expressions over branching chains.
* The `field` keyword for property backing.
* Range and index operators for slicing.
* Null-coalescing assignment (`??=`) for lazy initialization.
* Expression-bodied members for simple accessors.
* File-scoped namespaces, and `file`-scoped types if needed.

## Attributes

Decorate members with every JetBrains and .NET attribute that carries meaning, so analyzers, the debugger, and callers read the full contract. `JetBrains.Annotations` is a global using. Placement is enforced by `.editorconfig`.

* `[PublicAPI]` on a public type that is part of the shipped surface.
* `[Pure]` on a method that is free of side effects.

## Naming

* **Clarity first.** Names are descriptive and never abbreviated: `repository` not `repo`, `configuration` not `config`, `context` not `ctx`, `element` not `e`.
* No single-letter variables except in a short loop or lambda.
* `Async` suffix on any method that returns `Task` or `Task<T>`.
* Interfaces are `IName`, generic type parameters are `TName`, both enforced by `.editorconfig`.

## File and Class Structure

* **File-scoped namespaces.** When a file's folder differs from its namespace, keep the namespace flat and add `// ReSharper disable once CheckNamespace` above it, as the engine partials do.
* **Member order:** private fields, constructors, public properties, public methods, private methods.
* **Sealed by default** for data models and descriptors unless inheritance is intended.

## Data Objects

* **Sealed classes** for the metadata models (`DecomposedObject`, `DecomposedMember`, `DecomposedValue`).
* `required` for mandatory initialization, `{ get; init; }` for immutable properties.
* `record` only for genuine value-based equality.

## Synchronous by Design

LookupEngine is synchronous because reflection (`GetValue`, `Invoke`) is synchronous. The `Async` suffix rule therefore rarely applies. Add `async` or `await` only with a measured benefit.
