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
using LookupEngine.Abstractions.Decomposition;
using LookupEngine.Exceptions;

// ReSharper disable once CheckNamespace
namespace LookupEngine;

/// <summary>
///     Static entry point for decomposing any runtime object into a structured, evaluated representation of its members, values, and diagnostic metrics.
///     Pass a <see cref="Type"/> to decompose the static surface of a type.
///     Pass any other value to decompose an instance.
///     A <see langword="null"/> value always produces a well-defined result with no members.
/// </summary>
/// <remarks>
///     Each decomposition call is fully isolated: it creates a dedicated internal instance so concurrent calls are safe without any external synchronization.
///     Decomposition never throws — any reflection failure is captured and surfaced as the member's value.
/// </remarks>
[PublicAPI]
public partial class LookupComposer
{
    private readonly DecomposeOptions _options;

    private int _depth;
    private protected object Input;
    private DecomposedObject? _decomposedObject;

    /// <summary>
    ///     Initialize a new composer instance
    /// </summary>
    private protected LookupComposer(object value, DecomposeOptions options)
    {
        Input = value;
        _options = options;
    }

    /// <summary>
    ///     Decomposed members of the input object
    /// </summary>
    internal List<DecomposedMember> DecomposedMembers
    {
        get
        {
            if (field is null)
            {
                EngineException.ThrowIfEngineNotInitialized(nameof(DecomposedMembers));
            }

            return field;
        }
        set;
    }

    /// <summary>
    ///     The object type for the current hierarchy depth
    /// </summary>
    internal Type MemberDeclaringType
    {
        get
        {
            if (field is null)
            {
                EngineException.ThrowIfEngineNotInitialized(nameof(MemberDeclaringType));
            }

            return field;
        }
        set;
    }

    /// <summary>
    ///     The object descriptor for the current hierarchy depth
    /// </summary>
    internal Descriptor MemberDeclaringDescriptor
    {
        get
        {
            if (field is null)
            {
                EngineException.ThrowIfEngineNotInitialized(nameof(MemberDeclaringDescriptor));
            }

            return field;
        }
        set;
    }
}