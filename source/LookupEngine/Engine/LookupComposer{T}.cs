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
using LookupEngine.Options;

// ReSharper disable once CheckNamespace
namespace LookupEngine;

public partial class LookupComposer
{
    /// <summary>
    ///     Decomposes the object and all its members using the caller-supplied execution context, forwarding it to any context-aware descriptors and handlers registered in <paramref name="options"/>.
    ///     Returns a well-defined result with no members for a <see langword="null"/> value.
    /// </summary>
    /// <param name="value">The object to decompose, or <see langword="null"/>.</param>
    /// <param name="options">Controls which members are included, how they are evaluated, and the context passed to descriptors.</param>
    /// <typeparam name="TContext">The type of execution context passed through to context-aware descriptors.</typeparam>
    /// <returns>The decomposed object and its evaluated members.</returns>
    [Pure]
    public static DecomposedObject Decompose<TContext>(object? value, DecomposeOptions<TContext> options)
    {
        if (value is null) return CreateNullableDecomposition();

        return value switch
        {
            Type type => new LookupComposer<TContext>(value, options).DecomposeStatic(type),
            _ => new LookupComposer<TContext>(value, options).DecomposeInstance()
        };
    }

    /// <summary>
    ///     Decomposes the object's identity and type metadata without evaluating any members, forwarding the execution context to any context-aware descriptors.
    ///     Returns a well-defined result for a <see langword="null"/> value.
    /// </summary>
    /// <param name="value">The object to describe, or <see langword="null"/>.</param>
    /// <param name="options">Controls descriptor resolution, optional redirection, and the context passed to descriptors.</param>
    /// <typeparam name="TContext">The type of execution context passed through to context-aware descriptors.</typeparam>
    /// <returns>The decomposed object with an empty <see cref="DecomposedObject.Members"/> list.</returns>
    [Pure]
    public static DecomposedObject DecomposeObject<TContext>(object? value, DecomposeOptions<TContext> options)
    {
        if (value is null) return CreateNullableDecomposition();

        return value switch
        {
            Type type => new LookupComposer<TContext>(value, options).DecomposeStaticObject(type),
            _ => new LookupComposer<TContext>(value, options).DecomposeInstanceObject()
        };
    }

    /// <summary>
    ///     Decomposes and evaluates all members of the object using the caller-supplied execution context, without producing a root object description.
    ///     Returns an empty list for a <see langword="null"/> value.
    /// </summary>
    /// <param name="value">The object whose members to decompose, or <see langword="null"/>.</param>
    /// <param name="options">Controls which members are included, how they are evaluated, and the context passed to descriptors.</param>
    /// <typeparam name="TContext">The type of execution context passed through to context-aware descriptors.</typeparam>
    /// <returns>The evaluated members, or an empty list when <paramref name="value"/> is <see langword="null"/>.</returns>
    [Pure]
    public static List<DecomposedMember> DecomposeMembers<TContext>(object? value, DecomposeOptions<TContext> options)
    {
        if (value is null) return [];

        return value switch
        {
            Type type => new LookupComposer<TContext>(value, options).DecomposeStaticMembers(type),
            _ => new LookupComposer<TContext>(value, options).DecomposeInstanceMembers()
        };
    }
}