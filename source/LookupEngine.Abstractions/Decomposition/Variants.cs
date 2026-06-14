using LookupEngine.Abstractions.Decomposition.Containers;

namespace LookupEngine.Abstractions.Decomposition;

/// <summary>
///     Factory for creating <see cref="IVariant"/> instances to return from descriptor handlers.
/// </summary>
public static class Variants
{
    /// <summary>
    ///     Creates a variant that holds a single evaluated value.
    /// </summary>
    /// <param name="value">The evaluated value.</param>
    [Pure]
    public static IVariant Value(object? value)
    {
        return new Variant(value);
    }

    /// <summary>
    ///     Creates a variant that holds a single evaluated value with a description.
    /// </summary>
    /// <param name="value">The evaluated value.</param>
    /// <param name="description">A description of the context in which this value was resolved.</param>
    [Pure]
    public static IVariant Value(object? value, string description)
    {
        return new Variant(value, description);
    }

    /// <summary>
    ///     Creates a typed, mutable collection for multiple variant values.
    ///     Populate with <see cref="IVariantsCollection{T}.Add(T?)"/>, then return it from the handler.
    /// </summary>
    /// <typeparam name="T">The element type of each value.</typeparam>
    /// <param name="capacity">The expected number of values. Pre-sizing avoids reallocations on the hot path.</param>
    [Pure]
    public static IVariantsCollection<T> Values<T>(int capacity)
    {
        return new Variants<T>(capacity);
    }

    /// <summary>
    ///     Creates an empty variant for a member that produced no result.
    /// </summary>
    [Pure]
    public static IVariant Empty<T>()
    {
        return new Variants<T>(0);
    }
}