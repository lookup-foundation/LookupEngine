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

using LookupEngine.Abstractions.Decomposition;

namespace LookupEngine.Descriptors;

/// <summary>
///     Default fallback descriptor for any object type. Derives the display name from <see cref="object.ToString()"/>.
/// </summary>
public sealed class ObjectDescriptor : Descriptor
{
    public ObjectDescriptor(object? value)
    {
        Name = value?.ToString();
    }
}