// Copyright (c) Lookup Foundation and Contributors
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted,
// provided that the above copyright notice appears in all copies and
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting
// documentation.
//
// THIS PROGRAM IS PROVIDED "AS IS" AND WITH ALL FAULTS.
// NO IMPLIED WARRANTY OF MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE IS PROVIDED.
// THERE IS NO GUARANTEE THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.

using System.Reflection;
using LookupEngine.Abstractions.Decomposition;
using LookupEngine.Abstractions.Enums;

namespace LookupEngine.Abstractions.Configuration;

/// <summary>
///     Builder for configuring how an existing member is resolved and evaluated.
/// </summary>
[PublicAPI]
public struct MemberResolverBuilder
{
    private readonly string _name;
    private readonly Action<string, Func<ParameterInfo[], bool>?, Func<object?>?, MemberEvaluationPolicy?> _callback;
    private Func<ParameterInfo[], bool>? _predicate;

    /// <summary>
    ///     Initializes the builder with the member name and the engine callback.
    /// </summary>
    public MemberResolverBuilder(string name, Action<string, Func<ParameterInfo[], bool>?, Func<object?>?, MemberEvaluationPolicy?> callback)
    {
        _name = name;
        _callback = callback;
    }

    /// <summary>
    ///     Restricts this configuration to the overload whose parameter list satisfies the predicate.
    ///     Without this call, the configuration applies to all overloads.
    /// </summary>
    /// <param name="predicate">Evaluated against the runtime parameter list of each overload.</param>
    public MemberResolverBuilder When(Func<ParameterInfo[], bool> predicate)
    {
        _predicate = predicate;
        return this;
    }

    /// <summary>
    ///     Supplies a custom handler whose result replaces the reflected value.
    ///     The engine's <see cref="MethodEvaluationPolicy"/> still decides whether to evaluate eagerly or defer.
    /// </summary>
    public readonly void Resolve(Func<IVariant> handler)
    {
        _callback(_name, _predicate, handler, null);
    }

    /// <summary>
    ///     Supplies a custom handler whose result replaces the reflected value.
    ///     The engine's <see cref="MethodEvaluationPolicy"/> still decides whether to evaluate eagerly or defer.
    /// </summary>
    public readonly void Resolve(Func<object?> handler)
    {
        _callback(_name, _predicate, handler, null);
    }

    /// <summary>
    ///     Forces the member to be deferred regardless of the evaluation policy.
    ///     Force evaluation invokes the provided handler.
    /// </summary>
    public readonly void Defer(Func<IVariant> handler)
    {
        _callback(_name, _predicate, handler, MemberEvaluationPolicy.Deferred);
    }

    /// <summary>
    ///     Forces the member to be deferred regardless of the evaluation policy.
    ///     Force evaluation invokes the provided handler.
    /// </summary>
    public readonly void Defer(Func<object?> handler)
    {
        _callback(_name, _predicate, handler, MemberEvaluationPolicy.Deferred);
    }

    /// <summary>
    ///     Forces the member to be deferred regardless of the evaluation policy.
    ///     Force evaluation invokes the member directly via reflection.
    /// </summary>
    public readonly void Defer()
    {
        _callback(_name, _predicate, null, MemberEvaluationPolicy.Deferred);
    }

    /// <summary>
    ///     Forces the member to be evaluated eagerly during decomposition regardless of the evaluation policy.
    ///     Uses the provided handler as the value source.
    /// </summary>
    public readonly void Evaluate(Func<IVariant> handler)
    {
        _callback(_name, _predicate, handler, MemberEvaluationPolicy.Evaluated);
    }

    /// <summary>
    ///     Forces the member to be evaluated eagerly during decomposition regardless of the evaluation policy.
    ///     Uses the provided handler as the value source.
    /// </summary>
    public readonly void Evaluate(Func<object?> handler)
    {
        _callback(_name, _predicate, handler, MemberEvaluationPolicy.Evaluated);
    }

    /// <summary>
    ///     Forces the member to be evaluated eagerly during decomposition regardless of the evaluation policy.
    ///     Invokes the member directly via reflection.
    /// </summary>
    public readonly void Evaluate()
    {
        _callback(_name, _predicate, null, MemberEvaluationPolicy.Evaluated);
    }

    /// <summary>
    ///     Permanently disables the member. It is never evaluated, and force evaluation reports the disabled result.
    /// </summary>
    public readonly void Disable()
    {
        _callback(_name, _predicate, null, MemberEvaluationPolicy.Disabled);
    }
}