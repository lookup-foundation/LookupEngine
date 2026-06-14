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
///     Fluent builder for configuring how an existing member is resolved and evaluated using caller-supplied context.
///     Obtained from <see cref="IMemberConfigurator{TContext}.Member"/>.
/// </summary>
/// <typeparam name="TContext">The type of execution context available to registered handlers.</typeparam>
[PublicAPI]
public struct MemberResolverBuilder<TContext>
{
    private readonly string _name;
    private readonly Action<string, Func<ParameterInfo[], bool>?, Func<TContext, IVariant>?, MemberEvaluationPolicy?> _callback;
    private Func<ParameterInfo[], bool>? _predicate;

    /// <summary>
    ///     Initializes the builder with the member name and the engine callback.
    /// </summary>
    public MemberResolverBuilder(string name, Action<string, Func<ParameterInfo[], bool>?, Func<TContext, IVariant>?, MemberEvaluationPolicy?> callback)
    {
        _name = name;
        _callback = callback;
    }

    /// <summary>
    ///     Restricts this configuration to the overload whose parameter list satisfies the predicate.
    ///     Without this call, the configuration applies to all overloads.
    /// </summary>
    /// <param name="predicate">Evaluated against the runtime parameter list of each overload.</param>
    public MemberResolverBuilder<TContext> When(Func<ParameterInfo[], bool> predicate)
    {
        _predicate = predicate;
        return this;
    }

    /// <summary>
    ///     Supplies a context-aware handler whose result replaces the reflected value.
    ///     The engine's <see cref="MethodEvaluationPolicy"/> still decides whether to evaluate eagerly or defer.
    /// </summary>
    public readonly void Resolve(Func<TContext, IVariant> handler)
    {
        _callback(_name, _predicate, handler, null);
    }

    /// <summary>
    ///     Supplies a context-aware handler whose result replaces the reflected value.
    ///     The engine's <see cref="MethodEvaluationPolicy"/> still decides whether to evaluate eagerly or defer.
    /// </summary>
    public readonly void Resolve(Func<TContext, object?> handler)
    {
        _callback(_name, _predicate, context => Variants.Value(handler(context)), null);
    }

    /// <summary>
    ///     Forces the member to be deferred regardless of the evaluation policy.
    ///     Force evaluation invokes the provided context-aware handler.
    /// </summary>
    public readonly void Defer(Func<TContext, IVariant> handler)
    {
        _callback(_name, _predicate, handler, MemberEvaluationPolicy.Deferred);
    }

    /// <summary>
    ///     Forces the member to be deferred regardless of the evaluation policy.
    ///     Force evaluation invokes the provided context-aware handler.
    /// </summary>
    public readonly void Defer(Func<TContext, object?> handler)
    {
        _callback(_name, _predicate, context => Variants.Value(handler(context)), MemberEvaluationPolicy.Deferred);
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
    ///     Uses the provided context-aware handler as the value source.
    /// </summary>
    public readonly void Evaluate(Func<TContext, IVariant> handler)
    {
        _callback(_name, _predicate, handler, MemberEvaluationPolicy.Evaluated);
    }

    /// <summary>
    ///     Forces the member to be evaluated eagerly during decomposition regardless of the evaluation policy.
    ///     Uses the provided context-aware handler as the value source.
    /// </summary>
    public readonly void Evaluate(Func<TContext, object?> handler)
    {
        _callback(_name, _predicate, context => Variants.Value(handler(context)), MemberEvaluationPolicy.Evaluated);
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