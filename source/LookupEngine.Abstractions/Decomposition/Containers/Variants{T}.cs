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

namespace LookupEngine.Abstractions.Decomposition.Containers;

/// <summary>
///     Represents a collection of variants
/// </summary>
/// <typeparam name="T">The variant type</typeparam>
/// <param name="capacity">The initial variants capacity. Required for atomic performance optimizations</param>
[PublicAPI]
internal sealed class Variants<T>(int capacity) : IVariant, IVariantsCollection<T>, IReadOnlyCollection<Variant>
{
    private readonly List<Variant> _items = new(capacity);

    /// <summary>
    ///     Gets the number of variants
    /// </summary>
    public int Count => _items.Count;

    /// <summary>
    ///     The value of the stored variants
    /// </summary>
    public object Value => _items.Count == 1 ? _items[0].Value! : this;

    /// <summary>
    ///     The description of the evaluation context
    /// </summary>
    public string? Description => _items.Count == 1 ? _items[0].Description : null;

    /// <summary>
    ///     Adds a new variant
    /// </summary>
    /// <param name="result">The evaluated value</param>
    /// <returns>The variant collection with a new value</returns>
    public IVariantsCollection<T> Add(T? result)
    {
        if (result is null) return this;
        if (result is ICollection {Count: 0}) return this;

        _items.Add(new Variant(result));

        return this;
    }

    /// <summary>
    ///     Adds a new variant with description
    /// </summary>
    /// <param name="result">The evaluated value</param>
    /// <param name="description">The description of the evaluation context</param>
    /// <returns>The variant collection with a new value</returns>
    public IVariantsCollection<T> Add(T? result, string description)
    {
        if (result is null) return this;
        if (result is ICollection {Count: 0}) return this;

        _items.Add(new Variant(result, description));

        return this;
    }

    /// <summary>
    ///     Consume variants and evaluate values
    /// </summary>
    /// <returns>The evaluated variant</returns>
    public IVariant Consume()
    {
        return this;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return _items.GetEnumerator();
    }

    IEnumerator<Variant> IEnumerable<Variant>.GetEnumerator()
    {
        return _items.GetEnumerator();
    }
}