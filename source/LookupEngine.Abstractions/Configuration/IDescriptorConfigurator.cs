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

namespace LookupEngine.Abstractions.Configuration;

/// <summary>
///     Indicates that the descriptor configures how its members are evaluated
/// </summary>
/// <remarks>
///     A single place to resolve member handlers, override the evaluation policy per member,
///     and register synthetic extension members
/// </remarks>
public interface IDescriptorConfigurator : IDescriptorCollector
{
    /// <summary>
    ///     Configures the members of the described object
    /// </summary>
    /// <param name="manager">The manager that registers member handlers, overrides, and extensions</param>
    void Configure(IMemberManager manager);
}