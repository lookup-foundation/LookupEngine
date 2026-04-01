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

using LookupEngine.Abstractions.Configuration;
using LookupEngine.Abstractions.Decomposition;
using LookupEngine.Abstractions.Enums;

//ReSharper disable once CheckNamespace
namespace LookupEngine;

public partial class LookupComposer<TContext> : IExtensionManager<TContext>
{
    /// <summary>
    ///     Add in-context extension members to the decomposition
    /// </summary>
    private protected override void ExecuteExtensions()
    {
        if (!_options.EnableExtensions) return;

        if (MemberDeclaringDescriptor is IDescriptorExtension extension && DeclaresOwnExtensions(extension, typeof(IDescriptorExtension)))
        {
            extension.RegisterExtensions(this);
        }

        if (MemberDeclaringDescriptor is IDescriptorExtension<TContext> contextExtension && DeclaresOwnExtensions(contextExtension, typeof(IDescriptorExtension<TContext>)))
        {
            contextExtension.RegisterExtensions(this);
        }
    }

    /// <summary>
    ///     Defines a context-aware extension with the specified name and returns a builder for configuration
    /// </summary>
    public new ExtensionBuilder Define(string name)
    {
        return new ExtensionBuilder(name, RegisterExtension, RegisterExtensionResult, TryRegisterExtension);
    }

    /// <summary>
    ///     Defines a context-aware extension with the specified name and returns a builder for configuration
    /// </summary>
    ExtensionBuilder<TContext> IExtensionManager<TContext>.Define(string name)
    {
        return new ExtensionBuilder<TContext>(name, RegisterContextExtension, RegisterExtensionResult, TryRegisterContextExtension);
    }

    /// <summary>
    ///     Registers the context-aware extension with evaluation
    /// </summary>
    private void RegisterContextExtension(string name, MemberAttributes attributes, Func<TContext, IVariant> handler)
    {
        if ((attributes & MemberAttributes.Static) != 0 && !_options.IncludeStaticMembers) return;

        try
        {
            var result = EvaluateValue(handler);
            WriteExtensionMember(result, name, attributes);
        }
        catch (Exception exception)
        {
            WriteExtensionMember(exception, name, attributes);
        }
    }

    /// <summary>
    ///     Registers the context-aware extension and returns whether the evaluated result is truthy
    /// </summary>
    private bool TryRegisterContextExtension(string name, MemberAttributes attributes, Func<TContext, IVariant> handler)
    {
        if ((attributes & MemberAttributes.Static) != 0 && !_options.IncludeStaticMembers) return false;

        try
        {
            var result = EvaluateValue(handler);
            WriteExtensionMember(result, name, attributes);
            return result.Value is true;
        }
        catch (Exception exception)
        {
            WriteExtensionMember(exception, name, attributes);
            return false;
        }
    }
}