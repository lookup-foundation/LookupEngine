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

using LookupEngine.Abstractions;

// ReSharper disable once CheckNamespace
namespace LookupEngine;

public partial class LookupComposer
{
    /// <summary>
    ///     Decomposes the object and all its members, evaluating each member's value and recording the time and memory cost of each evaluation.
    ///     Pass a <see cref="Type"/> to decompose static members.
    ///     Any other value decomposes instance members. Returns a well-defined result with no members for a <see langword="null"/> value.
    /// </summary>
    /// <param name="value">The object to decompose, or <see langword="null"/>.</param>
    /// <param name="options">Controls which members are included and how they are evaluated. Defaults to <see cref="DecomposeOptions.Default"/>.</param>
    /// <returns>The decomposed object and its evaluated members.</returns>
    [Pure]
    public static DecomposedObject Decompose(object? value, DecomposeOptions? options = null)
    {
        if (value is null) return CreateNullableDecomposition();

        options ??= DecomposeOptions.Default;
        return value switch
        {
            Type type => new LookupComposer(value, options).DecomposeStatic(type),
            _ => new LookupComposer(value, options).DecomposeInstance()
        };
    }

    /// <summary>
    ///     Decomposes the object's identity and type metadata without evaluating any members.
    ///     Returns a well-defined result for a <see langword="null"/> value.
    /// </summary>
    /// <param name="value">The object to describe, or <see langword="null"/>.</param>
    /// <param name="options">Controls descriptor resolution and optional redirection. Defaults to <see cref="DecomposeOptions.Default"/>.</param>
    /// <returns>The decomposed object with an empty <see cref="DecomposedObject.Members"/> list.</returns>
    [Pure]
    public static DecomposedObject DecomposeObject(object? value, DecomposeOptions? options = null)
    {
        if (value is null) return CreateNullableDecomposition();

        options ??= DecomposeOptions.Default;
        return value switch
        {
            Type type => new LookupComposer(value, options).DecomposeStaticObject(type),
            _ => new LookupComposer(value, options).DecomposeInstanceObject()
        };
    }

    /// <summary>
    ///     Decomposes and evaluates all members of the object without producing a root object description.
    ///     Returns an empty list for a <see langword="null"/> value.
    /// </summary>
    /// <param name="value">The object whose members to decompose, or <see langword="null"/>.</param>
    /// <param name="options">Controls which members are included and how they are evaluated. Defaults to <see cref="DecomposeOptions.Default"/>.</param>
    /// <returns>The evaluated members, or an empty list when <paramref name="value"/> is <see langword="null"/>.</returns>
    [Pure]
    public static List<DecomposedMember> DecomposeMembers(object? value, DecomposeOptions? options = null)
    {
        if (value is null) return [];

        options ??= DecomposeOptions.Default;
        return value switch
        {
            Type type => new LookupComposer(value, options).DecomposeStaticMembers(type),
            _ => new LookupComposer(value, options).DecomposeInstanceMembers()
        };
    }
}