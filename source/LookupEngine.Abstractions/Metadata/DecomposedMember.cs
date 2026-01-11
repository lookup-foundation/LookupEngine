using System.Diagnostics;
using JetBrains.Annotations;
using LookupEngine.Abstractions.Enums;

// ReSharper disable once CheckNamespace
namespace LookupEngine.Abstractions;

/// <summary>
///     Represents a decomposed object member
/// </summary>
[PublicAPI]
[DebuggerDisplay("Name = {Name}, Value = {Value.Name}")]
public sealed class DecomposedMember
{
    /// <summary>
    ///     Depth of object hierarchy
    /// </summary>
    public required int Depth { get; init; }

    /// <summary>
    ///     The member name
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    ///     Object type name that the member belongs to
    /// </summary>
    public required string DeclaringTypeName { get; init; }

    /// <summary>
    ///     Object type full name that the member belongs to
    /// </summary>
    public required string DeclaringTypeFullName { get; init; }

    /// <summary>
    ///     Time of evaluation the member value
    /// </summary>
    public double ComputationTime { get; init; }

    /// <summary>
    ///     Allocating memory for member evaluation
    /// </summary>
    public long AllocatedBytes { get; init; }

    /// <summary>
    ///     The member attributes
    /// </summary>
    public MemberAttributes MemberAttributes { get; init; }

    /// <summary>
    ///     Evaluated member value metadata
    /// </summary>
    public required DecomposedValue Value { get; init; }
}