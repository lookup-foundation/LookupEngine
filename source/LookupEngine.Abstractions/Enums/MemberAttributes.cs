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

namespace LookupEngine.Abstractions.Enums;

/// <summary>
///     Describes the kind and visibility modifiers of a decomposed member.
/// </summary>
[Flags]
public enum MemberAttributes
{
    /// <summary>A non-public member.</summary>
    Private = 0b1,

    /// <summary>A static member.</summary>
    Static = 0b10,

    /// <summary>A field member.</summary>
    Field = 0b100,

    /// <summary>A property member.</summary>
    Property = 0b1000,

    /// <summary>A method member.</summary>
    Method = 0b10000,

    /// <summary>A synthetic member registered through <see cref="IMemberConfigurator"/>.</summary>
    Extension = 0b100000,

    /// <summary>An event member.</summary>
    Event = 0b1000000
}