# Descriptor System

Descriptors are how LookupEngine handles a type without the engine knowing about that type. A descriptor names an object, optionally configures how its members are evaluated, redirects evaluation elsewhere, or exposes itself as a collection. All descriptor contracts live in `source/LookupEngine.Abstractions`, so custom descriptors compile against the abstractions alone.

## Descriptor Base

`Descriptor` (in `Decomposition/Descriptor.cs`) is the abstract base every descriptor derives from. It carries a display `Name` and an optional `Description`. A descriptor selects behavior by implementing the `Configuration` interfaces below. A bare descriptor, like `ObjectDescriptor`, just supplies a name. Set `Name` in the constructor.

```csharp
public sealed class PointDescriptor : Descriptor
{
    public PointDescriptor(Point point)
    {
        Name = $"({point.X}, {point.Y})";
    }
}
```

Descriptors are immutable after construction. Capture the described value with a primary constructor when the descriptor needs it later, as `EnumerableDescriptor` does.

## Configuration Interfaces (`Configuration/`)

* **`IDescriptorCollector`**. A marker indicating the descriptor can retrieve members. The interfaces below extend it.

* **`IDescriptorConfigurator`**. Configures how the described object's members are evaluated. A single `Configure` call resolves member handlers, overrides evaluation policy per member, and registers synthetic extension members through the supplied `IMemberConfigurator`.

  ```csharp
  public void Configure(IMemberConfigurator configuration)
  {
      // Resolve a specific overload of an existing member
      configuration.Member(nameof(MyType.GetItem))
          .When(parameters => parameters.Length == 0)
          .Resolve(() => Variants.Value(ComputeItem()));

      // Register a synthetic member the type does not have
      configuration.Extension("CustomProperty")
          .Register(() => CalculateCustomValue());
  }
  ```

  Use the generic `IDescriptorConfigurator<TContext>`, with `Configure(IMemberConfigurator<TContext>)`, when handlers need the decomposition context.

* **`IDescriptorRedirector`**. Redirects evaluation from this object to another, for example to resolve an id to its entity. Active only when `DecomposeOptions.EnableRedirection` is set.

  ```csharp
  public bool TryRedirect(string target, out object result)
  {
      result = _database.GetById(_id);
      return result is not null;
  }
  ```

  `IDescriptorRedirector<TContext>` adds the context parameter, `TryRedirect(string target, TContext context, out object result)`.

* **`IDescriptorEnumerator`**. Marks the descriptor as a collection of descriptors. Exposes `IsEmpty` and an `Enumerator` property that returns a **fresh, non-advanced** enumerator on each access. Evaluate `IsEmpty` lazily, preferring an `ICollection.Count` fast path, so the source is not enumerated until needed. See `EnumerableDescriptor`.

## The Member Configurator (`IMemberConfigurator`)

`IMemberConfigurator` is the builder handed to `Configure`. It has two entry points.

* `Member(name)` returns a `MemberResolverBuilder` for an **existing** member. Optionally narrow to one overload with `.When(predicate)`, then choose evaluation timing.
    * `Resolve(handler)` supplies the value, evaluated per the engine's policy.
    * `Evaluate(handler?)` always evaluates during decomposition.
    * `Defer(handler?)` always defers, evaluating only on force.
    * `Disable()` never evaluates, and reports the disabled result.
* `Extension(name)` returns an `ExtensionBuilder` for a **synthetic** member.
    * `Register(handler)` supplies its value, evaluated eagerly.
    * `Defer(handler)` defers evaluation, running the handler only on force.
    * `Disable()` and `NotSupported()` register it without a value.
    * `AsStatic()` marks it static, visible with `IncludeStaticMembers`.
    * `Map(apiName)` ties it to a real API member via `nameof` for cross-version compile-time tracking.

Handlers come in two shapes everywhere. `Func<IVariant>` gives full control over value and description, and `Func<object?>` wraps the value in a variant for you.

## Variants (`Variants` factory)

A member can resolve to one value or several. The `Variants` factory builds `IVariant`s.

* `Variants.Value(value)` and `Variants.Value(value, description)` create a single evaluated value.
* `Variants.Values<T>(capacity)` creates a typed `IVariantsCollection<T>` to accumulate multiple variants. Pass the capacity for allocation efficiency.
* `Variants.Empty<T>()` creates an empty result, used when a member has no solutions.

## Type Resolution

`DecomposeOptions.TypeResolver` maps a value, and an optional declared type, to a `Descriptor`. The built-in resolver dispatches by type to a `BooleanDescriptor`, `StringDescriptor`, `EnumerableDescriptor`, or `ExceptionDescriptor`, falling back to `ObjectDescriptor`. Override the resolver to plug in custom descriptors for your own types.

## Method Evaluation Policy

`MethodEvaluationPolicy`, on `DecomposeOptions.EvaluationPolicy`, decides which methods auto-evaluate during decomposition versus defer behind an evaluation handle. The `EvaluatedFilter` predicate receives the method and the type currently being decomposed, and returns whether to evaluate it eagerly. The `None` and `All` presets cover "defer everything" and "evaluate everything except methods returning `void`". The defaults live in the class.

The policy governs **methods only**. Properties and synthetic extensions are evaluated eagerly by default and are never deferred by the policy. Defer them explicitly with a per-member `Defer` instead.

## Built-in Descriptors (`source/LookupEngine/Descriptors`)

`ObjectDescriptor` is the default fallback and names via `ToString()`. `StringDescriptor` and `BooleanDescriptor` cover those primitives. `ExceptionDescriptor` names from the innermost exception message. `EnumerableDescriptor` implements both `IDescriptorEnumerator` and `IDescriptorConfigurator`, with a lazy `IsEmpty`. Read these as the reference implementations before writing a new descriptor.

## Guidelines

* **Immutability.** Descriptors are immutable after construction. Use a primary constructor when the descriptor holds the value for later, as `EnumerableDescriptor` does. A descriptor that only derives a `Name` needs no captured state, as `ObjectDescriptor` and `StringDescriptor` show.
* **Context.** Use the `<TContext>` interface variants only when a handler genuinely needs execution context.
* **No circular redirects.** Avoid redirect chains that loop (A to B to A), because there is no built-in cycle guard.
* **Speed.** Descriptor code runs for every evaluated member, so keep it allocation-light and fast. See [Performance](./performance.md).
