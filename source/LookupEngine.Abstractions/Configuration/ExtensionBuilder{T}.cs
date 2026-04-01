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

using JetBrains.Annotations;
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
    private readonly Action<string, MemberAttributes, bool> _registerResultCallback;
    private readonly Func<string, MemberAttributes, Func<TContext, IVariant>, bool> _tryRegisterCallback;
    private MemberAttributes _attributes = MemberAttributes.Extension;

    /// <summary>
    ///     Creates a new context-aware extension builder
    /// </summary>
    public ExtensionBuilder(
        string name,
        Action<string, MemberAttributes, Func<TContext, IVariant>> registerCallback,
        Action<string, MemberAttributes, bool> registerResultCallback,
        Func<string, MemberAttributes, Func<TContext, IVariant>, bool> tryRegisterCallback)
    {
        _name = name;
        _registerCallback = registerCallback;
        _registerResultCallback = registerResultCallback;
        _tryRegisterCallback = tryRegisterCallback;
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
    public void Register(Func<TContext, IVariant> handler)
    {
        _registerCallback(_name, _attributes, handler);
    }

    /// <summary>
    ///     Registers the extension and returns whether the evaluated result is truthy
    /// </summary>
    /// <param name="handler">The function that evaluates the extension value using the context</param>
    /// <returns>True if the evaluated value is boolean true; false otherwise or on exception</returns>
    public bool TryRegister(Func<TContext, IVariant> handler)
    {
        return _tryRegisterCallback(_name, _attributes, handler);
    }

    /// <summary>
    ///     Marks the extension as not supported, visible only when IncludeUnsupported is enabled
    /// </summary>
    public void AsNotSupported()
    {
        _registerResultCallback(_name, _attributes, false);
    }

    /// <summary>
    ///     Marks the extension as disabled, visible only when IncludeUnsupported is enabled
    /// </summary>
    public void AsDisabled()
    {
        _registerResultCallback(_name, _attributes, true);
    }
}
