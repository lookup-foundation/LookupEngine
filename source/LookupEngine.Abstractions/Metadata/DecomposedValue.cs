using System.Diagnostics;
using LookupEngine.Abstractions.Decomposition;
using System.Text.Json.Serialization;

// ReSharper disable once CheckNamespace
namespace LookupEngine.Abstractions;

/// <summary>
///     The evaluated value of a decomposed member, including its type metadata and optional descriptor.
/// </summary>
[PublicAPI]
[DebuggerDisplay("Name = {Name}, Value = {RawValue}")]
public sealed class DecomposedValue
{
    /// <summary>
    ///     The raw object produced by member evaluation.
    /// </summary>
    [JsonIgnore] public object? RawValue { get; init; }

    /// <summary>
    ///     The display name of this value, derived from its <see cref="Descriptor"/> or type name.
    ///     Empty string when no value was produced (deferred, disabled, or unsupported members).
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    ///     The short type name of this value (e.g. <c>List&lt;String&gt;</c>).
    /// </summary>
    public required string TypeName { get; init; }

    /// <summary>
    ///     The fully qualified type name of this value (e.g. <c>System.Collections.Generic.List&lt;String&gt;</c>).
    /// </summary>
    public required string TypeFullName { get; init; }

    /// <summary>
    ///     An optional description provided by the descriptor for this value, or <see langword="null"/> when not supplied.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    ///     The descriptor that named and described this value.
    /// </summary>
    [JsonIgnore] public Descriptor? Descriptor { get; init; }
}