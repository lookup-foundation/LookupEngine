using System.Collections;
using System.Diagnostics.CodeAnalysis;
using LookupEngine.Abstractions.Configuration;
using LookupEngine.Abstractions.Decomposition;

namespace LookupEngine.Descriptors;

[SuppressMessage("ReSharper", "NotDisposedResourceIsReturnedByProperty")]
public sealed class EnumerableDescriptor(IEnumerable value) : Descriptor, IDescriptorEnumerator, IDescriptorConfigurator
{
    private bool? _isEmpty;

    /// <summary>
    ///     A new enumerator of the described collection. Each access creates a fresh, non-advanced enumerator
    /// </summary>
    public IEnumerator Enumerator => value.GetEnumerator();

    /// <summary>
    ///     Indicates that the described collection is empty. Evaluated lazily to avoid enumerating the source until requested
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

    public void Configure(IMemberManager manager)
    {
        manager.Member(nameof(IEnumerable.GetEnumerator)).Resolve(Variants.Empty<IEnumerator>);
    }
}