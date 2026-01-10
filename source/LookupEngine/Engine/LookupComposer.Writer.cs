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

using System.Collections;
using System.Reflection;
using LookupEngine.Abstractions;
using LookupEngine.Abstractions.Decomposition;
using LookupEngine.Abstractions.Enums;
using LookupEngine.Formaters;

// ReSharper disable once CheckNamespace
namespace LookupEngine;

public partial class LookupComposer
{
    /// <summary>
    ///     Write a nullable object to the decomposition result
    /// </summary>
    private protected static DecomposedObject CreateNullableDecomposition()
    {
        return new DecomposedObject
        {
            Name = $"{nameof(System)}.{nameof(Object)}",
            RawValue = null,
            TypeName = nameof(Object),
            TypeFullName = $"{nameof(System)}.{nameof(Object)}"
        };
    }

    /// <summary>
    ///     Write an object metadata to the decomposition result
    /// </summary>
    private static DecomposedObject CreateInstanceDecomposition(object instance, Type type, Descriptor descriptor)
    {
        var formatTypeName = ReflexionFormater.FormatTypeName(type);
        var hasUnknownName = descriptor.Name is null ||
                             type.Namespace is null ||
                             descriptor.Name!.StartsWith(type.Namespace, StringComparison.OrdinalIgnoreCase);

        return new DecomposedObject
        {
            Name = hasUnknownName ? formatTypeName : descriptor.Name!,
            Description = descriptor.Description,
            RawValue = instance,
            TypeName = formatTypeName,
            TypeFullName = $"{type.Namespace}.{formatTypeName}",
            Descriptor = descriptor
        };
    }

    private static DecomposedObject CreateStaticDecomposition(Type type, Descriptor descriptor)
    {
        var formatTypeName = ReflexionFormater.FormatTypeName(type);
        var hasUnknownName = descriptor.Name is null ||
                             type.Namespace is null ||
                             descriptor.Name!.StartsWith(type.Namespace, StringComparison.OrdinalIgnoreCase);

        return new DecomposedObject
        {
            Name = hasUnknownName ? formatTypeName : descriptor.Name!,
            Description = descriptor.Description,
            RawValue = type,
            TypeName = formatTypeName,
            TypeFullName = $"{type.Namespace}.{formatTypeName}",
            Descriptor = descriptor
        };
    }

    private void WriteEnumerableMember(object? value, int index)
    {
        var member = new DecomposedMember
        {
            Depth = _depth,
            Value = CreateValue(nameof(IEnumerable), value),
            Name = $"{ReflexionFormater.FormatTypeName(MemberDeclaringType).Replace("[]", string.Empty)}[{index}]",
            MemberAttributes = MemberAttributes.Property,
            DeclaringTypeName = nameof(IEnumerable),
            DeclaringTypeFullName = $"{nameof(System)}.{nameof(System.Collections)}.{nameof(IEnumerable)}",
        };

        DecomposedMembers.Add(member);
    }

    private protected void WriteExtensionMember(object? value, string name)
    {
        var formatTypeName = ReflexionFormater.FormatTypeName(MemberDeclaringType);

        var member = new DecomposedMember
        {
            Depth = _depth,
            Name = name,
            Value = CreateValue(name, value),
            DeclaringTypeName = formatTypeName,
            DeclaringTypeFullName = MemberDeclaringType.Namespace != null
                ? $"{MemberDeclaringType.Namespace}.{formatTypeName}"
                : formatTypeName,
            MemberAttributes = MemberAttributes.Extension,
            ComputationTime = TimeDiagnoser.GetElapsed().TotalMilliseconds,
            AllocatedBytes = MemoryDiagnoser.GetAllocatedBytes()
        };

        DecomposedMembers.Add(member);
    }

    private void WriteDecompositionMember(object? value, MemberInfo memberInfo)
    {
        var formatTypeName = ReflexionFormater.FormatTypeName(MemberDeclaringType);

        var member = new DecomposedMember
        {
            Depth = _depth,
            Value = CreateValue(memberInfo.Name, value),
            Name = memberInfo.Name,
            DeclaringTypeName = formatTypeName,
            DeclaringTypeFullName = MemberDeclaringType.Namespace != null
                ? $"{MemberDeclaringType.Namespace}.{formatTypeName}"
                : formatTypeName,
            MemberAttributes = ModifiersFormater.FormatAttributes(memberInfo),
            ComputationTime = TimeDiagnoser.GetElapsed().TotalMilliseconds,
            AllocatedBytes = MemoryDiagnoser.GetAllocatedBytes()
        };

        DecomposedMembers.Add(member);
    }

    private void WriteDecompositionMember(object? value, MemberInfo memberInfo, ParameterInfo[] parameters)
    {
        var formatTypeName = ReflexionFormater.FormatTypeName(MemberDeclaringType);

        var member = new DecomposedMember
        {
            Depth = _depth,
            Value = CreateValue(memberInfo.Name, value),
            Name = ReflexionFormater.FormatMemberName(memberInfo, parameters),
            DeclaringTypeName = formatTypeName,
            DeclaringTypeFullName = MemberDeclaringType.Namespace != null
                ? $"{MemberDeclaringType.Namespace}.{formatTypeName}"
                : formatTypeName,
            MemberAttributes = ModifiersFormater.FormatAttributes(memberInfo),
            ComputationTime = TimeDiagnoser.GetElapsed().TotalMilliseconds,
            AllocatedBytes = MemoryDiagnoser.GetAllocatedBytes()
        };

        DecomposedMembers.Add(member);
    }

    private DecomposedValue CreateNullableValue()
    {
        return new DecomposedValue
        {
            RawValue = null,
            Name = string.Empty,
            TypeName = nameof(Object),
            TypeFullName = $"{nameof(System)}.{nameof(Object)}"
        };
    }

    private DecomposedValue CreateValue(string targetMember, object? value)
    {
        if (value is null) return CreateNullableValue();
        if (value is IVariant {Value: null}) return CreateNullableValue();

        value = RedirectValue(value, targetMember, out var valueDescriptor);

        var valueType = value.GetType();
        var formatTypeName = ReflexionFormater.FormatTypeName(valueType);
        var hasUnknownName = valueDescriptor.Name is null ||
                             valueType.Namespace is null ||
                             valueDescriptor.Name.StartsWith(valueType.Namespace, StringComparison.OrdinalIgnoreCase);

        return new DecomposedValue
        {
            RawValue = value,
            Name = hasUnknownName ? formatTypeName : valueDescriptor.Name!,
            Description = valueDescriptor.Description,
            TypeName = formatTypeName,
            TypeFullName = $"{valueType.Namespace}.{formatTypeName}",
            Descriptor = valueDescriptor
        };
    }
}