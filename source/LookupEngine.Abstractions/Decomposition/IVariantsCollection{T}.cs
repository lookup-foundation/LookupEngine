namespace LookupEngine.Abstractions.Decomposition;

/// <summary>
///     A typed, mutable accumulator for multiple <see cref="IVariant"/> values produced by a single member.
/// </summary>
/// <typeparam name="T">The element type of each added value.</typeparam>
public interface IVariantsCollection<in T> : IVariantsCollection
{
    /// <summary>
    ///     Adds a value to the collection and returns the collection for chaining.
    ///     Null values and empty collections are silently ignored.
    /// </summary>
    /// <param name="result">The value to add.</param>
    IVariantsCollection<T> Add(T? result);

    /// <summary>
    ///     Adds a value with a description to the collection and returns the collection for chaining.
    ///     Null values and empty collections are silently ignored.
    /// </summary>
    /// <param name="result">The value to add.</param>
    /// <param name="description">A description of the context in which this value was resolved.</param>
    IVariantsCollection<T> Add(T? result, string description);
}