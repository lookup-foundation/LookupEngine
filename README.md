# LookupEngine

A high-performance .NET library for runtime object analysis that provides deep inspection of object members through reflection, with built-in performance metrics and configurable evaluation strategies.

## Features

- Runtime inspection of public, private and static fields, properties and methods of any object.
- Built-in computation time and memory allocation tracking for each evaluated member.
- Extensible type descriptor system with custom resolvers and value converters.
- Context-aware member resolution for enhanced metadata and value evaluation.
- Support for multiple value variants based on method overloads and parameters.
- Deferred method evaluation with namespace and return type policies, per-member descriptor overrides, and on-demand force evaluation.
- Safe execution model with configurable error handling and member access control.

## How to use

Basic Example:

```C#
var data = Colors.Red;
var decomposition = LookupComposer.Decompose(data);
```

Static object:

```C#
var data = typeof(Colors);
var decomposition = LookupComposer.Decompose(data);
```

Using context to provide additional metadata to the engine:

```C#
// Any object can be used as a context. 
// It is used by descriptors that require context to resolve members, or add in-context extensions.
var data = Colors.Red;
var options = new DecomposeOptions<ExecutionContext>
{
    Context = new ExecutionContext
    {
        Version = "1.0",
        Runtime = "CoreCLR",
        Description = "LookupEngine Context"
    }
};

var decomposition = LookupComposer.Decompose(data, options);
```

Custom options:

```C#
var data = Colors.Red;
var options = new DecomposeOptions
{
    IncludeRoot = false,
    IncludeFields = false,
    IncludeEvents = true,
    IncludeUnsupported = false,
    IncludePrivateMembers = false,
    IncludeStaticMembers = true,
    EnableExtensions = true,
    EnableRedirection = true,
    EvaluationPolicy = MethodEvaluationPolicy.All,
    TypeResolver = (obj, type) =>
    {
        return obj switch
        {
            bool value when type is null || type == typeof(bool) => new BooleanDescriptor(value),
            string value when type is null || type == typeof(string) => new StringDescriptor(value),
            IEnumerable value => new EnumerableDescriptor(value),
            Exception value when type is null || type == typeof(Exception) => new ExceptionDescriptor(value),
            _ => new ObjectDescriptor(obj)
        };
    }
};

var decomposition = LookupComposer.Decompose(data, options);
```

Decomposition output:

```json
{
    "Name": "#FFFF0000",
    "TypeName": "Color",
    "TypeFullName": "System.Windows.Media.Color",
    "Members": [
        {
            "Name": "R",
            "DeclaringTypeName": "Color",
            "DeclaringTypeFullName": "System.Windows.Media.Color",
            "ComputationTime": 0.0008,
            "AllocatedBytes": 192,
            "MemberAttributes": 8,
            "Value": {
                "Name": "255",
                "TypeName": "Byte",
                "TypeFullName": "System.Byte"
            }
        },
        {
            "Name": "G",
            "DeclaringTypeName": "Color",
            "DeclaringTypeFullName": "System.Windows.Media.Color",
            "ComputationTime": 0.0004,
            "AllocatedBytes": 192,
            "MemberAttributes": 8,
            "Value": {
                "Name": "0",
                "TypeName": "Byte",
                "TypeFullName": "System.Byte"
            }
        },
        {
            "Name": "B",
            "DeclaringTypeName": "Color",
            "DeclaringTypeFullName": "System.Windows.Media.Color",
            "ComputationTime": 0.0005,
            "AllocatedBytes": 192,
            "MemberAttributes": 8,
            "Value": {
                "Name": "0",
                "TypeName": "Byte",
                "TypeFullName": "System.Byte"
            }
        }
    ]
}
```

## Descriptors

Descriptors describe exactly how the engine should handle types, parametric methods, and provide additional metadata for the object.

To register a descriptor, it is required to set the `TypeResolver` property of `DecomposeOptions`, that is responsible for mapping a descriptor to a type.

```C#
var options = new DecomposeOptions
{
    TypeResolver = (obj, type) =>
    {
        return obj switch
        {
            bool value => new BooleanDescriptor(value),
            string value => new StringDescriptor(value),
            _ => new ObjectDescriptor(obj)
        };
    }
};
```

Describing an object is implemented with interfaces.

### IDescriptorConfigurator

A single place to configure how the engine handles a type: resolve member handlers, override the evaluation policy per member, and register synthetic extension members. 
`configuration.Member(name)` configures an existing member; `configuration.Extension(name)` adds a member the type does not have.

```c#
public sealed class ElementDescriptor(Element element) : Descriptor, IDescriptorConfigurator
{
    public void Configure(IMemberConfigurator configuration)
    {
        // existing members
        configuration.Member(nameof(Element.IsHidden)).Resolve(() => element.IsHidden(Context.ActiveView));
        configuration.Member(nameof(Element.CanBeHidden)).Defer(() => element.CanBeHidden(Context.ActiveView));
        configuration.Member(nameof(Element.Delete)).Disable();

        // synthetic members
        configuration.Extension("HEX").Register(() => ColorRepresentationUtils.ColorToHex(element.Color));
    }
}
```

For existing members:

`Resolve` - handler, evaluated per the global evaluation policy.
`Defer` - force the policy to defer evaluation.
`Evaluate` - force the policy to auto member evaluation.
`Disable` - force the policy to disable evaluation.

For synthetic members:

`Register` - handler, evaluated eagerly.
`Defer` - defer evaluation. The handler runs only on force evaluation.
`Disable` - register the member as disabled.
`NotSupported` - register the member as unsupported.
`AsStatic` - mark the member static. It appears only when `IncludeStaticMembers` is enabled.
`Map` - tie the member to a real API name via `nameof` for compile-time tracking across API versions.

Handlers may return a plain value, which is wrapped automatically, or an `IVariant` from the `Variants` class when you need an evaluation-context description or multiple values:

```c#
public sealed class ElementDescriptor(Element element) : Descriptor, IDescriptorConfigurator
{
    public void Configure(IMemberConfigurator configuration)
    {
        configuration.Member(nameof(Element.Name)).Resolve(() => element.Name); // plain value
        configuration.Member(nameof(Element.IsHidden)).Resolve(() => Variants.Value(element.IsHidden(Context.ActiveView), "Active view")); // value with description

        configuration.Member(nameof(Element.GetBoundingBox)).Resolve(() => Variants.Values<BoundingBoxXYZ>(2) // multiple values
            .Add(element.get_BoundingBox(null), "Model")
            .Add(element.get_BoundingBox(Context.ActiveView), "Active view")
            .Consume());
    }
}
```

`When` can filter members by its runtime parameters:

```c#
public sealed class EntityDescriptor(Entity entity) : Descriptor, IDescriptorConfigurator
{
    public void Configure(IMemberConfigurator configuration)
    {
        configuration.Member(nameof(Entity.Get))
            .When(parameters => parameters is [{ParameterType: var type}] && type == typeof(string))
            .Resolve(() => entity.Get<string>(/* ... */));
    }
}
```

If you need an evaluation context, use the generic interface version.
The context is passed to the engine as an option and is single for all descriptors. The generic and non-generic versions can live in the same class:

```C#
public sealed class ReferenceDescriptor(Reference reference) : Descriptor, IDescriptorConfigurator<Document>
{
    public void Configure(IMemberConfigurator<Document> configuration)
    {
        configuration.Member(nameof(Reference.ConvertToStableRepresentation))
            .Resolve(document => reference.ConvertToStableRepresentation(document));
    }
}
```

### IDescriptorRedirector

Redirects the evaluation of the current object to another object.
As a result, you will get a new evaluated value instead of the original one.
For example, you can get the object itself instead of its ID in the output:

```c#
public sealed class ElementIdDescriptor(long elementId) : Descriptor, IDescriptorRedirector
{
    public bool TryRedirect(string target, out object result)
    {
        if (elementId < 0) return false;

        result = Database.GetElementById(elementId);
        return true;
    }
}
```

If you need an evaluation context for redirection, use the generic interface version.
Context is passed to the engine as an option and is single for all descriptors:

```c#
public sealed class ElementIdDescriptor(ElementId elementId) : Descriptor, IDescriptorRedirector<Document>
{
    public bool TryRedirect(string target, Document context, out object result)
    {
        if (elementId == ElementId.InvalidElementId) return false;

        result = elementId.ToElement(context);
        return true;
    }
}
```

### IDescriptorCollector

Serves as a marker that the object is maintainable, and available for internal component analysis. Advantage of being used as a marker in UI applications. Does not have any effect for CLI applications.

```c#
public sealed class ApplicationDescriptor : Descriptor, IDescriptorCollector
{
    public ApplicationDescriptor(Application application)
    {
        Name = application.VersionName;
    }
}
```

## Evaluation policy

Invoking methods during decomposition can cause side effects, so the engine defers them by default.
Deferred methods are included in the decomposition without being invoked and can be evaluated later on demand.

To evaluate methods automatically, set the `EvaluationPolicy` property of `DecomposeOptions`, that controls which methods are allowed to be invoked:

```C#
// Defer all methods (default)
var options = new DecomposeOptions
{
    EvaluationPolicy = MethodEvaluationPolicy.None
};

// Evaluate all methods except those returning void
var options = new DecomposeOptions
{
    EvaluationPolicy = MethodEvaluationPolicy.All
};

// Evaluate methods selected by a custom filter
var options = new DecomposeOptions
{
    EvaluationPolicy = new MethodEvaluationPolicy
    {
        EvaluatedFilter = (member, declaringType) =>
        {
            if (declaringType.Namespace is null) return false;
            if (declaringType.Namespace.StartsWith("System", StringComparison.Ordinal)) return true;
            return false;
        }
};
```

`EvaluatedFilter` receives the method and the type currently being decomposed, and returns whether to evaluate the method eagerly.
The `All` policy evaluates everything except methods returning `void`, so side-effect-only methods are never auto-invoked.

### Member evaluation policy overrides

Descriptors override the evaluation policy for specific members through `IDescriptorConfigurator`, in both directions and for methods and properties alike:

```C#
public sealed class DocumentDescriptor(Document document) : Descriptor, IDescriptorConfigurator
{
    public void Configure(IMemberConfigurator configuration)
    {
        configuration.Member(nameof(Document.GetTypeOfStorage)).Evaluate(); // evaluate during decomposition, even when the policy defers
        configuration.Member(nameof(Document.EnumerateUserDefinedParameters)).Defer(); // never evaluate automatically, force evaluation runs it
        configuration.Member(nameof(Document.Close)).Disable(); // never evaluate, force evaluation reports the disabled result
    }
}
```

### Force evaluation

A deferred member is marked with the `EvaluationPolicy` property, and its value contains the method return type instead of the evaluation result.
To evaluate the member on demand, for example by a user action in the UI, call `Evaluate()`.
The value and performance metrics are updated in place:

```C#
var data = Colors.Red;
var decomposition = LookupComposer.Decompose(data);

foreach (var member in decomposition.Members)
{
    if (member.EvaluationPolicy == MemberEvaluationPolicy.Deferred)
    {
        member.Evaluate();
    }
}
```

Force evaluation runs the same pipeline as the decomposition: the resolver handler or the method invocation, with redirection, type resolution and metrics.