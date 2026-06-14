using System.Diagnostics;
using LookupEngine.Abstractions.Decomposition;
using System.Text.Json.Serialization;

// ReSharper disable once CheckNamespace
namespace LookupEngine.Abstractions;

/// <summary>
///     The root result of a decomposition: the described object together with its evaluated members.
/// </summary>
[PublicAPI]
[DebuggerDisplay("Name = {Name}, Value = {RawValue}")]
public sealed class DecomposedObject
{
    /// <summary>
    ///     The original object passed to the decomposition call, before any descriptor redirections.
    /// </summary>
    [JsonIgnore] public object? RawValue { get; init; }

    /// <summary>
    ///     The display name of the object, derived from its <see cref="Descriptor"/> or type name.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    ///     The short type name of the object, with generic arguments formatted (e.g. <c>List&lt;String&gt;</c>).
    /// </summary>
    public required string TypeName { get; init; }

    /// <summary>
    ///     The fully qualified type name, including the namespace (e.g. <c>System.Collections.Generic.List&lt;String&gt;</c>).
    /// </summary>
    public required string TypeFullName { get; init; }

    /// <summary>
    ///     An optional description provided by the descriptor, or <see langword="null"/> when not supplied.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    ///     The descriptor that named and described this object.
    /// </summary>
    [JsonIgnore] public Descriptor? Descriptor { get; init; }

    /// <summary>
    ///     The evaluated members of this object. Empty when the object was decomposed without members.
    /// </summary>
    public List<DecomposedMember> Members { get; init; } = [];
}