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
    ///     Callback of the extension registration
    /// </summary>
    public void Register(string name, Func<TContext, IVariant> extension)
    {
        try
        {
            var result = EvaluateValue(extension);
            if (result.Value is NotSupportedException && !_options.IncludeUnsupported) return;
            
            WriteExtensionMember(result, name);
        }
        catch (Exception exception)
        {
            WriteExtensionMember(exception, name);
        }
    }
}