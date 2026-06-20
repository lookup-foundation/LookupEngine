using System.Reflection;

// ReSharper disable once CheckNamespace
namespace LookupEngine;

/// <summary>
///     Determines which methods are evaluated automatically during decomposition.
///     Methods for which <see cref="EvaluatedFilter"/> returns <see langword="false"/> are deferred and included with an evaluation handle.
/// </summary>
[PublicAPI]
public sealed class MethodEvaluationPolicy
{
    /// <summary>
    ///     Decides whether a method is evaluated automatically during decomposition.
    ///     Receives the method and the type currently being decomposed, and returns <see langword="true"/> to evaluate it eagerly or <see langword="false"/> to defer it.
    ///     Defaults to deferring every method.
    /// </summary>
    public Func<MethodInfo, Type, bool> EvaluatedFilter { get; init; } = static (_, _) => false;

    /// <summary>
    ///     A policy that defers all methods. No methods are evaluated eagerly.
    /// </summary>
    public static MethodEvaluationPolicy None { get; } = new();

    /// <summary>
    ///     A policy that evaluates all methods automatically, except those returning <see langword="void"/>.
    /// </summary>
    public static MethodEvaluationPolicy All { get; } = new()
    {
        EvaluatedFilter = static (member, _) => member.ReturnType != typeof(void)
    };

    /// <summary>
    ///     Check if the method is allowed to be evaluated during decomposition.
    /// </summary>
    internal bool IsEvaluationAllowed(MethodInfo member, Type declaringType)
    {
        return EvaluatedFilter.Invoke(member, declaringType);
    }
}