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

namespace LookupEngine.Abstractions.Decomposition;

/// <summary>
///     A single resolved value produced by a member evaluation handler.
/// </summary>
public interface IVariant
{
    /// <summary>
    ///     The resolved value, or <see langword="null"/> if the evaluation produced no result.
    /// </summary>
    object? Value { get; }

    /// <summary>
    ///     An optional description of the context in which this value was resolved.
    /// </summary>
    string? Description { get; }
}