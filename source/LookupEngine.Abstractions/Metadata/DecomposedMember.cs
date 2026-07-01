using System.Diagnostics;
using System.Text.Json.Serialization;
using LookupEngine.Abstractions.Enums;

// ReSharper disable once CheckNamespace
namespace LookupEngine.Abstractions;

/// <summary>
///     Represents a single evaluated member of a decomposed object.
/// </summary>
[PublicAPI]
[DebuggerDisplay("Name = {Name}, Value = {Value.Name}")]
public sealed class DecomposedMember
{
    /// <summary>
    ///     Inheritance depth at which this member was declared, where 0 is the most derived type.
    /// </summary>
    public required int Depth { get; init; }

    /// <summary>
    ///     The member name. Methods are formatted with their parameter types (e.g. <c>GetValue (Int32)</c>).
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    ///     The short name of the type that declares this member.
    /// </summary>
    public required string DeclaringTypeName { get; init; }

    /// <summary>
    ///     The fully qualified name of the type that declares this member.
    /// </summary>
    public required string DeclaringTypeFullName { get; init; }

    /// <summary>
    ///     Flags describing the kind and visibility of this member.
    /// </summary>
    public MemberAttributes MemberAttributes { get; init; }

    /// <summary>
    ///     Time elapsed while evaluating this member, in milliseconds.
    ///     Zero for deferred, disabled, and unsupported members.
    /// </summary>
    public double ComputationTime { get; set; }

    /// <summary>
    ///     Bytes allocated on the managed heap while evaluating this member.
    ///     Zero for deferred, disabled, and unsupported members.
    /// </summary>
    public long AllocatedBytes { get; set; }

    /// <summary>
    ///     The evaluation state of this member.
    /// </summary>
    public MemberEvaluationPolicy EvaluationPolicy { get; set; }

    /// <summary>
    ///     The evaluated value of this member.
    /// </summary>
    public required DecomposedValue Value { get; set; }

    /// <summary>
    ///     Engine-provided handle that evaluates this member on demand.
    ///     Present for deferred and evaluated members; <see langword="null"/> for disabled and unsupported members.
    /// </summary>
    [JsonIgnore]
    public Action<DecomposedMember>? Evaluator { get; set; }

    /// <summary>
    ///     Triggers evaluation of this member through the engine, updating <see cref="Value"/>, <see cref="ComputationTime"/>, and <see cref="AllocatedBytes"/> in place.
    ///     May be called to refresh the value.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    ///     The member cannot be evaluated (it is disabled or unsupported).
    /// </exception>
    public void Evaluate()
    {
        if (Evaluator is null) throw new InvalidOperationException("The member cannot be evaluated");

        Evaluator.Invoke(this);
    }
}