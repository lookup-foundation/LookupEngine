using System.Diagnostics;
using System.Text.Json.Serialization;
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
    ///     The member attributes
    /// </summary>
    public MemberAttributes MemberAttributes { get; init; }

    /// <summary>
    ///     Time of evaluation the member value
    /// </summary>
    public double ComputationTime { get; set; }

    /// <summary>
    ///     Allocating memory for member evaluation
    /// </summary>
    public long AllocatedBytes { get; set; }

    /// <summary>
    ///     The evaluation policy override
    /// </summary>
    public MemberEvaluationPolicy EvaluationPolicy { get; set; }

    /// <summary>
    ///     Evaluated member value metadata
    /// </summary>
    public required DecomposedValue Value { get; set; }

    /// <summary>
    ///     Engine-provided handle that evaluates the deferred member.
    ///     Null for evaluated members and after deserialization
    /// </summary>
    [JsonIgnore]
    public Action<DecomposedMember>? Evaluator { get; set; }

    /// <summary>
    ///     Force evaluation of the deferred member through the engine, updating the value and metrics in place
    /// </summary>
    /// <exception cref="InvalidOperationException">The member is not deferred or was deserialized</exception>
    public void Evaluate()
    {
        if (Evaluator is null) throw new InvalidOperationException("The member is not deferred or was deserialized");

        Evaluator.Invoke(this);
        Evaluator = null;
    }
}