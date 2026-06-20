# Testing

Tests cover the behavior the library adds: decomposition correctness, descriptor extensibility, evaluation policy, error handling, and thread safety. They do not test the .NET reflection APIs themselves. Every change ships with tests.

## Framework

* **TUnit**, run through the Microsoft.Testing.Platform runner, in `tests/LookupEngine.Tests.Unit`.
* Assertions use the TUnit API: `await Assert.That(actual).IsEqualTo(expected)`. Group related assertions in a `using (Assert.Multiple())` block.

## Structure

* One `sealed class` per concern, named `<Concern>Tests`. Add a case to the matching file, or create one named for the new concern.
* Test methods follow `<Method>_<Condition>_<Expected>`, carry `[Test]`, and return `async Task`. Split the body into blocks marked with `// Arrange`, `// Act`, and `// Assert` comments.
* Declare test-only types as `file sealed class` helpers at the end of the file. Do not share fixtures across files.

## What to Test

* **Engine behavior the library adds:** a custom descriptor's configured member resolves to the expected value, redirection follows when enabled, a throwing member surfaces its exception as the value instead of a crash, and each `Decompose*` call is isolated across threads.
* **Both API shapes:** the non-generic `LookupComposer` and the context-aware `LookupComposer<TContext>` where behavior differs.
* **Edge cases:** null inputs, empty collections, members that throw, static and private members, boundary values.

## Build and Test

TUnit runs on the Microsoft.Testing.Platform, so `dotnet test` runs the suite directly.

* `dotnet test` runs the unit tests.
* `dotnet test -c Release` runs them in Release.
