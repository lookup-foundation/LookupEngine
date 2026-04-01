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

using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using LookupEngine.Abstractions.Configuration;
using LookupEngine.Abstractions.Decomposition;
using LookupEngine.Abstractions.Enums;

//ReSharper disable once CheckNamespace
namespace LookupEngine;

[SuppressMessage("ReSharper", "SuspiciousTypeConversion.Global")]
public partial class LookupComposer : IExtensionManager
{
    private static readonly Dictionary<(Type Descriptor, Type Interface), bool> ExtensionOwnerCache = [];

    /// <summary>
    ///     Add extension members to the decomposition
    /// </summary>
    private protected virtual void ExecuteExtensions()
    {
        if (!_options.EnableExtensions) return;
        if (MemberDeclaringDescriptor is not IDescriptorExtension extension) return;
        if (!DeclaresOwnExtensions(extension, typeof(IDescriptorExtension))) return;

        extension.RegisterExtensions(this);
    }

    /// <summary>
    ///     Defines an extension with the specified name and returns a builder for configuration
    /// </summary>
    public ExtensionBuilder Define(string name)
    {
        return new ExtensionBuilder(name, RegisterExtension, RegisterExtensionResult, TryRegisterExtension);
    }

    /// <summary>
    ///     Registers the extension with evaluation
    /// </summary>
    private protected void RegisterExtension(string name, MemberAttributes attributes, Func<IVariant> handler)
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
    ///     Registers the extension without implementation
    /// </summary>
    private protected void RegisterExtensionResult(string name, MemberAttributes attributes, bool isDisabled)
    {
        if (!_options.IncludeUnsupported) return;
        if ((attributes & MemberAttributes.Static) != 0 && !_options.IncludeStaticMembers) return;

        Exception exception = isDisabled
            ? new InvalidOperationException("Member execution disabled")
            : new NotSupportedException("Unsupported method overload");

        WriteExtensionMember(exception, name, attributes);
    }

    /// <summary>
    ///     Registers the extension and returns whether the evaluated result is truthy
    /// </summary>
    private protected bool TryRegisterExtension(string name, MemberAttributes attributes, Func<IVariant> handler)
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

    /// <summary>
    ///     Checks whether the descriptor type declares its own RegisterExtensions implementation for the given interface
    /// </summary>
    private protected static bool DeclaresOwnExtensions(IDescriptorCollector descriptor, Type interfaceType)
    {
        var type = descriptor.GetType();
        var key = (type, interfaceType);

#if NET8_0_OR_GREATER
        ref var owns = ref CollectionsMarshal.GetValueRefOrAddDefault(ExtensionOwnerCache, key, out var exists);
        if (!exists)
        {
            var map = type.GetInterfaceMap(interfaceType);
            owns = map.TargetMethods[0].DeclaringType == type;
        }

        return owns;
#else
        if (ExtensionOwnerCache.TryGetValue(key, out var owns))
        {
            return owns;
        }

        var map = type.GetInterfaceMap(interfaceType);
        owns = map.TargetMethods[0].DeclaringType == type;
        ExtensionOwnerCache[key] = owns;

        return owns;
#endif
    }
}