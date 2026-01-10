using System.Diagnostics;
using JetBrains.Annotations;
using LookupEngine.Abstractions.Decomposition;
#if NET
using System.Text.Json.Serialization;
#endif

// ReSharper disable once CheckNamespace
namespace LookupEngine.Abstractions;

/// <summary>
///     Represents a decomposed object
/// </summary>
[PublicAPI]
[DebuggerDisplay("Name = {Name} Value = {RawValue}")]
public sealed class DecomposedObject
{
    /// <summary>
    ///     The raw, non-evaluated value
    /// </summary>
#if NET
    [JsonIgnore]
#endif
    public required object? RawValue { get; init; }

    /// <summary>
    ///     The name of the object
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    ///     Object type name
    /// </summary>
    public required string TypeName { get; init; }

    /// <summary>
    ///     Object type full name
    /// </summary>
    public required string TypeFullName { get; init; }

    /// <summary>
    ///     The description of the evaluation context
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    ///     Descriptor for object description
    /// </summary>
#if NET
    [JsonIgnore]
#endif
    public Descriptor? Descriptor { get; init; }

    /// <summary>
    ///     The collection of object members
    /// </summary>
    public List<DecomposedMember> Members { get; } = [];
}