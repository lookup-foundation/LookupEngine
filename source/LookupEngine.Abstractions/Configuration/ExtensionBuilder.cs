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
///     Builder for configuring and registering a synthetic extension member.
/// </summary>
[PublicAPI]
public struct ExtensionBuilder
{
    private readonly string _name;
    private readonly Action<string, MemberAttributes, Func<object?>, MemberEvaluationPolicy?> _registerCallback;
    private readonly Action<string, MemberAttributes, MemberEvaluationPolicy> _registerResultCallback;
    private MemberAttributes _attributes = MemberAttributes.Extension;

    /// <summary>
    ///     Initializes the builder with the extension name and the engine registration callbacks.
    /// </summary>
    public ExtensionBuilder(
        string name,
        Action<string, MemberAttributes, Func<object?>, MemberEvaluationPolicy?> registerCallback,
        Action<string, MemberAttributes, MemberEvaluationPolicy> registerResultCallback)
    {
        _name = name;
        _registerCallback = registerCallback;
        _registerResultCallback = registerResultCallback;
    }

    /// <summary>
    ///     Associates this extension with an existing API member name for compile-time tracking across API versions.
    /// </summary>
    /// <param name="apiName">The real API member name, typically supplied via <c>nameof()</c>.</param>
    /// <remarks>Has no effect at runtime.</remarks>
    public ExtensionBuilder Map(string apiName)
    {
        // The apiName parameter ensures compile-time validation of API member existence.
        // It is intentionally unused at runtime
        return this;
    }

    /// <summary>
    ///     Marks the extension as static. It appears in results only when <c>DecomposeOptions.IncludeStaticMembers</c> is enabled.
    /// </summary>
    public ExtensionBuilder AsStatic()
    {
        _attributes |= MemberAttributes.Static;
        return this;
    }

    /// <summary>
    ///     Registers the extension with the evaluation handler that produces its value, evaluated eagerly during decomposition.
    /// </summary>
    /// <param name="handler">Returns the resolved value for this extension.</param>
    /// <remarks>Extensions are not governed by the engine evaluation policy; they evaluate eagerly unless deferred with <see cref="Defer(Func{IVariant})"/>.</remarks>
    public readonly void Register(Func<IVariant> handler)
    {
        _registerCallback(_name, _attributes, handler, null);
    }

    /// <summary>
    ///     Registers the extension with the evaluation handler that produces its value, evaluated eagerly during decomposition.
    /// </summary>
    /// <param name="handler">Returns the resolved value for this extension.</param>
    /// <remarks>Extensions are not governed by the engine evaluation policy; they evaluate eagerly unless deferred with <see cref="Defer(Func{object})"/>.</remarks>
    public readonly void Register(Func<object?> handler)
    {
        _registerCallback(_name, _attributes, handler, null);
    }

    /// <summary>
    ///     Registers the extension as deferred. The handler is invoked only on force evaluation.
    /// </summary>
    /// <param name="handler">Returns the resolved value for this extension.</param>
    public readonly void Defer(Func<IVariant> handler)
    {
        _registerCallback(_name, _attributes, handler, MemberEvaluationPolicy.Deferred);
    }

    /// <summary>
    ///     Registers the extension as deferred. The handler is invoked only on force evaluation.
    /// </summary>
    /// <param name="handler">Returns the resolved value for this extension.</param>
    public readonly void Defer(Func<object?> handler)
    {
        _registerCallback(_name, _attributes, handler, MemberEvaluationPolicy.Deferred);
    }

    /// <summary>
    ///     Registers the extension as deferred. The void action is invoked for its side effect only on force evaluation.
    /// </summary>
    /// <param name="handler">Performs the action for this extension; the extension resolves to no value.</param>
    public readonly void Defer(Action handler)
    {
        _registerCallback(_name, _attributes, () =>
        {
            handler.Invoke();
            return null;
        }, MemberEvaluationPolicy.Deferred);
    }

    /// <summary>
    ///     Registers the extension as unsupported. It appears in results only when <c>DecomposeOptions.IncludeUnsupported</c> is enabled.
    /// </summary>
    public readonly void NotSupported()
    {
        _registerResultCallback(_name, _attributes, MemberEvaluationPolicy.Unsupported);
    }

    /// <summary>
    ///     Registers the extension as disabled. It appears in results only when <c>DecomposeOptions.IncludeUnsupported</c> is enabled.
    /// </summary>
    public readonly void Disable()
    {
        _registerResultCallback(_name, _attributes, MemberEvaluationPolicy.Disabled);
    }
}