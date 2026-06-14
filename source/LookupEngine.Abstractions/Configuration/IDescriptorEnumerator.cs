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

namespace LookupEngine.Abstractions.Configuration;

/// <summary>
///     Allows a descriptor to expose its object as an ordered sequence of decomposable items.
///     The engine iterates this sequence and adds each element as a member of the decomposed result.
/// </summary>
public interface IDescriptorEnumerator : IDescriptorCollector
{
    /// <summary>
    ///     <see langword="true"/> when the described collection contains no elements.
    /// </summary>
    bool IsEmpty { get; }

    /// <summary>
    ///     Returns a fresh, non-advanced enumerator over the described collection.
    ///     Each access must return a new enumerator positioned before the first element.
    /// </summary>
    IEnumerator Enumerator { get; }
}