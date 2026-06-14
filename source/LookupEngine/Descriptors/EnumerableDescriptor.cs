using System.Collections;
using System.Diagnostics.CodeAnalysis;
using LookupEngine.Abstractions.Configuration;
using LookupEngine.Abstractions.Decomposition;

namespace LookupEngine.Descriptors;

/// <summary>
///     Descriptor for any <see cref="IEnumerable"/> value. Exposes its elements as indexed members and suppresses the <c>GetEnumerator</c> method from the decomposition result.
/// </summary>
[SuppressMessage("ReSharper", "NotDisposedResourceIsReturnedByProperty")]
public sealed class EnumerableDescriptor(IEnumerable value) : Descriptor, IDescriptorEnumerator, IDescriptorConfigurator
{
    private bool? _isEmpty;

    /// <summary>
    ///     Returns a fresh, non-advanced enumerator over the described collection.
    /// </summary>
    public IEnumerator Enumerator => value.GetEnumerator();

    /// <summary>
    ///     <see langword="true"/> when the described collection contains no elements. Evaluated lazily.
    /// </summary>
    public bool IsEmpty => _isEmpty ??= ComputeIsEmpty();

    private bool ComputeIsEmpty()
    {
        //Checking types to reduce memory allocation when creating an iterator and increase performance
        if (value is ICollection collection) return collection.Count == 0;

        var enumerator = value.GetEnumerator();
        try
        {
            return !enumerator.MoveNext();
        }
        finally
        {
            if (enumerator is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }

    public void Configure(IMemberConfigurator configuration)
    {
        configuration.Member(nameof(IEnumerable.GetEnumerator)).Resolve(Variants.Empty<IEnumerator>);
    }
}