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
///     A builder for configuring how an existing member is resolved and evaluated using an execution context
/// </summary>
/// <typeparam name="TContext">The type of execution context</typeparam>
[PublicAPI]
public struct MemberResolverBuilder<TContext>
{
    private readonly string _name;
    private readonly Action<string, Func<ParameterInfo[], bool>?, Func<TContext, IVariant>?, MemberEvaluationPolicy?> _callback;
    private Func<ParameterInfo[], bool>? _predicate;

    /// <summary>
    ///     Creates a new context-aware member resolver builder
    /// </summary>
    public MemberResolverBuilder(string name, Action<string, Func<ParameterInfo[], bool>?, Func<TContext, IVariant>?, MemberEvaluationPolicy?> callback)
    {
        _name = name;
        _callback = callback;
    }

    /// <summary>
    ///     Restricts the configuration to the overload whose parameters match the predicate
    /// </summary>
    /// <param name="predicate">The predicate evaluated against the member runtime parameters</param>
    public MemberResolverBuilder<TContext> When(Func<ParameterInfo[], bool> predicate)
    {
        _predicate = predicate;
        return this;
    }

    /// <summary>
    ///     Resolves the member with the specified handler, evaluated according to the engine evaluation policy
    /// </summary>
    public readonly void Resolve(Func<TContext, IVariant> handler)
    {
        _callback(_name, _predicate, handler, null);
    }

    /// <summary>
    ///     Resolves the member with the specified handler, evaluated according to the engine evaluation policy
    /// </summary>
    public readonly void Resolve(Func<TContext, object?> handler)
    {
        _callback(_name, _predicate, context => Variants.Value(handler(context)), null);
    }

    /// <summary>
    ///     Defers the member regardless of the evaluation policy; force evaluation invokes the handler
    /// </summary>
    public readonly void Defer(Func<TContext, IVariant> handler)
    {
        _callback(_name, _predicate, handler, MemberEvaluationPolicy.Deferred);
    }

    /// <summary>
    ///     Defers the member regardless of the evaluation policy; force evaluation invokes the handler
    /// </summary>
    public readonly void Defer(Func<TContext, object?> handler)
    {
        _callback(_name, _predicate, context => Variants.Value(handler(context)), MemberEvaluationPolicy.Deferred);
    }

    /// <summary>
    ///     Defers the member regardless of the evaluation policy; force evaluation invokes the member directly
    /// </summary>
    public readonly void Defer()
    {
        _callback(_name, _predicate, null, MemberEvaluationPolicy.Deferred);
    }

    /// <summary>
    ///     Evaluates the member during decomposition regardless of the evaluation policy
    /// </summary>
    public readonly void Evaluate(Func<TContext, IVariant> handler)
    {
        _callback(_name, _predicate, handler, MemberEvaluationPolicy.Evaluated);
    }

    /// <summary>
    ///     Evaluates the member during decomposition regardless of the evaluation policy
    /// </summary>
    public readonly void Evaluate(Func<TContext, object?> handler)
    {
        _callback(_name, _predicate, context => Variants.Value(handler(context)), MemberEvaluationPolicy.Evaluated);
    }

    /// <summary>
    ///     Evaluates the member during decomposition regardless of the evaluation policy, invoking the member directly
    /// </summary>
    public readonly void Evaluate()
    {
        _callback(_name, _predicate, null, MemberEvaluationPolicy.Evaluated);
    }

    /// <summary>
    ///     Disables the member; it is never evaluated and force evaluation reports the disabled result
    /// </summary>
    public readonly void Disable()
    {
        _callback(_name, _predicate, null, MemberEvaluationPolicy.Disabled);
    }
}