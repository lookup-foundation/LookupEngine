using JetBrains.Annotations;
using LookupEngine.Abstractions.Decomposition.Containers;

namespace LookupEngine.Abstractions.Decomposition;

/// <summary>
///     A factory for <see cref="IVariant"/>.
/// </summary>
public static class Variants
{
    /// <summary>
    ///     Create a single evaluated member value variant
    /// </summary>
    [Pure]
    public static IVariant Value(object? value)
    {
        return new Variant(value);
    }

    /// <summary>
    ///     Create a single evaluated member value variant
    /// </summary>
    /// <param name="value">The evaluated value</param>
    /// <param name="description">The description of the evaluation context</param>
    [Pure]
    public static IVariant Value(object? value, string description)
    {
        return new Variant(value, description);
    }

    /// <summary>
    ///     Create an evaluated member value variants collection
    /// </summary>
    /// <param name="capacity">The initial variants capacity. Required for atomic performance optimizations</param>
    [Pure]
    public static IVariantsCollection<T> Values<T>(int capacity)
    {
        return new Variants<T>(capacity);
    }

    /// <summary>
    ///     Creates an empty variant collection
    /// </summary>
    /// <returns>An empty variant collection</returns>
    /// <remarks>An empty collection is returned when there are no solutions for a member</remarks>
    [Pure]
    public static IVariant Empty<T>()
    {
        return new Variants<T>(0);
    }

    /// <summary>
    ///     A variant that disables the member evaluation
    /// </summary>
    [Pure]
    public static IVariant Disabled()
    {
        return new Variant(new InvalidOperationException("Member execution disabled"));
    }
}