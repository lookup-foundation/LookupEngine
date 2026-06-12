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
using LookupEngine.Abstractions;

// ReSharper disable once CheckNamespace
namespace LookupEngine;

public partial class LookupComposer
{
    /// <summary>
    ///     Decompose the object instance and its structure
    /// </summary>
    [Pure]
    private DecomposedObject DecomposeInstance()
    {
        _decomposedObject = DecomposeInstanceObject();
        var objectType = Input.GetType();
        var members = DecomposeInstanceMembers(objectType);
        _decomposedObject.Members.AddRange(members);

        return _decomposedObject;
    }

    /// <summary>
    ///     Decompose the object instance without a structure
    /// </summary>
    [Pure]
    private DecomposedObject DecomposeInstanceObject()
    {
        Input = RedirectValue(Input, out var instanceDescriptor, out var instanceDescription);

        var objectType = Input.GetType();
        return CreateInstanceDecomposition(Input, objectType, instanceDescriptor, instanceDescription);
    }

    /// <summary>
    ///     Decompose the static object and its structure
    /// </summary>
    [Pure]
    private DecomposedObject DecomposeStatic(Type type)
    {
        _decomposedObject = DecomposeStaticObject(type);
        var members = DecomposeStaticMembers(type);
        _decomposedObject.Members.AddRange(members);

        return _decomposedObject;
    }

    /// <summary>
    ///     Decompose the static object without a structure
    /// </summary>
    [Pure]
    private DecomposedObject DecomposeStaticObject(Type type)
    {
        var staticDescriptor = _options.TypeResolver.Invoke(null, type);
        return CreateStaticDecomposition(type, staticDescriptor);
    }

    /// <summary>
    ///     Decompose the object instance structure
    /// </summary>
    [Pure]
    private List<DecomposedMember> DecomposeInstanceMembers()
    {
        return DecomposeInstanceMembers(Input.GetType());
    }

    /// <summary>
    ///     Decompose the object instance structure
    /// </summary>
    [Pure]
    private List<DecomposedMember> DecomposeInstanceMembers(Type objectType)
    {
        DecomposedMembers = new List<DecomposedMember>(32);

        var objectTypeHierarchy = GetTypeHierarchy(objectType);
        for (var i = objectTypeHierarchy.Count - 1; i >= 0; i--)
        {
            MemberDeclaringType = objectTypeHierarchy[i];
            MemberDeclaringDescriptor = _options.TypeResolver.Invoke(Input, MemberDeclaringType);

            var flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            if (_options.IncludeStaticMembers) flags |= BindingFlags.Static;
            if (_options.IncludePrivateMembers) flags |= BindingFlags.NonPublic;

            ConfigureMembers();
            DecomposeFields(flags);
            DecomposeProperties(flags);
            DecomposeMethods(flags);
            DecomposeEvents(flags);
            FlushExtensions();

            _depth--;
        }

        MemberDeclaringType = objectType;
        AddEnumerableItems();

        return DecomposedMembers;
    }

    /// <summary>
    ///     Decompose the static object structure
    /// </summary>
    [Pure]
    private List<DecomposedMember> DecomposeStaticMembers(Type objectType)
    {
        DecomposedMembers = new List<DecomposedMember>(32);

        var flags = BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly;
        if (_options.IncludePrivateMembers) flags |= BindingFlags.NonPublic;

        var objectTypeHierarchy = GetTypeHierarchy(objectType);
        for (var i = objectTypeHierarchy.Count - 1; i >= 0; i--)
        {
            MemberDeclaringType = objectTypeHierarchy[i];
            MemberDeclaringDescriptor = _options.TypeResolver.Invoke(null, MemberDeclaringType);

            ConfigureMembers();
            DecomposeFields(flags);
            DecomposeProperties(flags);
            DecomposeMethods(flags);
            FlushExtensions();

            _depth--;
        }

        return DecomposedMembers;
    }

    /// <summary>
    ///     Get the type hierarchy of the input type
    /// </summary>
    [Pure]
    private List<Type> GetTypeHierarchy(Type inputType)
    {
        var types = new List<Type>();
        while (inputType.BaseType is not null)
        {
            types.Add(inputType);
            inputType = inputType.BaseType;
        }

        if (_options.IncludeRoot) types.Add(inputType);

        return types;
    }
}