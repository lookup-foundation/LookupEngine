using LookupEngine.Abstractions.Decomposition;

namespace LookupEngine.Descriptors;

/// <summary>
///     Descriptor for <see cref="string"/> values. Uses the string value itself as the display name.
/// </summary>
public sealed class StringDescriptor : Descriptor
{
    public StringDescriptor(string text)
    {
        Name = text;
    }
}