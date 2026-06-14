using System.Collections;
using LookupEngine.Abstractions.Decomposition;
using LookupEngine.Descriptors;

// ReSharper disable once CheckNamespace
namespace LookupEngine;

/// <summary>
///     Controls which members are included in a decomposition and how they are evaluated.
///     All opt-in flags default to <see langword="false"/>; only the features you enable are active.
/// </summary>
[PublicAPI]
public class DecomposeOptions
{
    /// <summary>
    ///     When <see langword="true"/>, includes <see cref="object"/> itself at the top of the type hierarchy.
    ///     When <see langword="false"/> (default), the hierarchy stops at the first non-<see cref="object"/> base type.
    /// </summary>
    public bool IncludeRoot { get; set; }

    /// <summary>
    ///     When <see langword="true"/>, includes field members in the decomposition result.
    /// </summary>
    public bool IncludeFields { get; set; }

    /// <summary>
    ///     When <see langword="true"/>, includes event members in the decomposition result.
    /// </summary>
    public bool IncludeEvents { get; set; }

    /// <summary>
    ///     When <see langword="true"/>, includes members with
    ///     <see cref="MemberEvaluationPolicy.Unsupported"/> or <see cref="MemberEvaluationPolicy.Disabled"/> status.
    /// </summary>
    public bool IncludeUnsupported { get; set; }

    /// <summary>
    ///     When <see langword="true"/>, includes non-public members in the decomposition result.
    /// </summary>
    public bool IncludePrivateMembers { get; set; }

    /// <summary>
    ///     When <see langword="true"/>, includes static members in the decomposition result.
    /// </summary>
    /// <remarks>
    ///     Applies to instance decomposition and registered extensions.
    ///     Decomposing a <see cref="Type"/> always includes static members regardless of this flag,
    ///     because static members are the entire content of a type decomposition.
    /// </remarks>
    public bool IncludeStaticMembers { get; set; }

    /// <summary>
    ///     When <see langword="true"/>, calls <see cref="IDescriptorConfigurator.Configure"/> on the descriptor
    ///     for each type level, enabling synthetic extension members and member configuration overrides.
    /// </summary>
    public bool EnableExtensions { get; set; }

    /// <summary>
    ///     When <see langword="true"/>, calls <see cref="IDescriptorRedirector.TryRedirect"/> on the descriptor
    ///     for each evaluated value, allowing substitution with a different object.
    /// </summary>
    public bool EnableRedirection { get; set; }

    /// <summary>
    ///     Determines which methods are evaluated eagerly during decomposition.
    ///     Methods that do not match the policy are deferred and included with an evaluation handle.
    ///     Defaults to <see cref="MethodEvaluationPolicy.None"/>, which defers all methods.
    /// </summary>
    public MethodEvaluationPolicy EvaluationPolicy { get; set; } = MethodEvaluationPolicy.None;

    /// <summary>
    ///     Maps an object value and its declared type to the <see cref="Descriptor"/> that names and describes it.
    ///     Override this to plug in custom descriptors for your own types.
    ///     Defaults to the built-in resolver that covers common system types and falls back to
    ///     <see cref="ObjectDescriptor"/> for everything else.
    /// </summary>
    public Func<object?, Type?, Descriptor> TypeResolver
    {
        get
        {
            return field ??= DefaultResolveMap;
        }
        set;
    }

    /// <summary>
    ///     Returns a <see cref="DecomposeOptions"/> instance with all options at their defaults.
    /// </summary>
    public static DecomposeOptions Default => new();

    private static Descriptor DefaultResolveMap(object? obj, Type? type)
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
}