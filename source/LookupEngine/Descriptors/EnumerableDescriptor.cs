using System.Collections;
using System.Reflection;
using LookupEngine.Abstractions.Configuration;
using LookupEngine.Abstractions.Decomposition;

namespace LookupEngine.Descriptors;

public sealed class EnumerableDescriptor : Descriptor, IDescriptorEnumerator, IDescriptorResolver
{
    private readonly IEnumerable _value;
    private bool? _isEmpty;

    public EnumerableDescriptor(IEnumerable value)
    {
        _value = value;
    }

    /// <summary>
    ///     A new enumerator of the described collection. Each access creates a fresh, non-advanced enumerator
    /// </summary>
    public IEnumerator Enumerator => _value.GetEnumerator();

    /// <summary>
    ///     Indicates that the described collection is empty. Evaluated lazily to avoid enumerating the source until requested
    /// </summary>
    public bool IsEmpty => _isEmpty ??= ComputeIsEmpty();

    private bool ComputeIsEmpty()
    {
        //Checking types to reduce memory allocation when creating an iterator and increase performance
        if (_value is ICollection collection) return collection.Count == 0;

        var enumerator = _value.GetEnumerator();
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

    public Func<IVariant>? Resolve(string target, ParameterInfo[] parameters)
    {
        return target switch
        {
            nameof(IEnumerable.GetEnumerator) => ResolveGetEnumerator,
            _ => null
        };

        IVariant ResolveGetEnumerator()
        {
            return Variants.Empty<IEnumerator>();
        }
    }
}