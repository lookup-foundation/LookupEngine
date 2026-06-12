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

using LookupEngine.Abstractions.Decomposition;
using LookupEngine.Abstractions.Enums;

namespace LookupEngine.Abstractions.Configuration;

/// <summary>
///     A builder for configuring and registering a context-aware extension member
/// </summary>
/// <typeparam name="TContext">The type of execution context</typeparam>
[PublicAPI]
public struct ExtensionBuilder<TContext>
{
    private readonly string _name;
    private readonly Action<string, MemberAttributes, Func<TContext, IVariant>> _registerCallback;
    private readonly Action<string, MemberAttributes, MemberEvaluationPolicy> _registerResultCallback;
    private MemberAttributes _attributes = MemberAttributes.Extension;

    /// <summary>
    ///     Creates a new context-aware extension builder
    /// </summary>
    public ExtensionBuilder(
        string name,
        Action<string, MemberAttributes, Func<TContext, IVariant>> registerCallback,
        Action<string, MemberAttributes, MemberEvaluationPolicy> registerResultCallback)
    {
        _name = name;
        _registerCallback = registerCallback;
        _registerResultCallback = registerResultCallback;
    }

    /// <summary>
    ///     Maps the extension to a specific API member name for cross-version compilation tracking
    /// </summary>
    /// <param name="apiName">The API member name, typically provided via nameof()</param>
    public ExtensionBuilder<TContext> Map(string apiName)
    {
        return this;
    }

    /// <summary>
    ///     Marks the extension as static, visible only when IncludeStaticMembers is enabled
    /// </summary>
    public ExtensionBuilder<TContext> AsStatic()
    {
        _attributes |= MemberAttributes.Static;
        return this;
    }

    /// <summary>
    ///     Registers the extension with the specified context-aware evaluation handler
    /// </summary>
    /// <param name="handler">The function that evaluates the extension value using the context</param>
    public readonly void Register(Func<TContext, IVariant> handler)
    {
        _registerCallback(_name, _attributes, handler);
    }

    /// <summary>
    ///     Registers the extension with the specified context-aware evaluation handler
    /// </summary>
    /// <param name="handler">The function that evaluates the extension value using the context</param>
    public readonly void Register(Func<TContext, object?> handler)
    {
        _registerCallback(_name, _attributes, context => Variants.Value(handler(context)));
    }

    /// <summary>
    ///     Marks the extension as not supported, visible only when IncludeUnsupported is enabled
    /// </summary>
    public readonly void AsNotSupported()
    {
        _registerResultCallback(_name, _attributes, MemberEvaluationPolicy.Unsupported);
    }

    /// <summary>
    ///     Marks the extension as disabled, visible only when IncludeUnsupported is enabled
    /// </summary>
    public readonly void AsDisabled()
    {
        _registerResultCallback(_name, _attributes, MemberEvaluationPolicy.Disabled);
    }
}