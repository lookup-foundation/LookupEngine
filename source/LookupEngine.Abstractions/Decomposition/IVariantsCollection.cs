namespace LookupEngine.Abstractions.Decomposition;

/// <summary>
///     A mutable accumulator for multiple <see cref="IVariant"/> values produced by a single member.
/// </summary>
public interface IVariantsCollection
{
    /// <summary>
    ///     Finalizes the collection and returns it as a single <see cref="IVariant"/> for the engine to record.
    /// </summary>
    /// <returns>The collection itself as an <see cref="IVariant"/>.</returns>
    IVariant Consume();
}