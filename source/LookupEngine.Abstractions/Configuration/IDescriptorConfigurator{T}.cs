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
///     Indicates that the descriptor configures how its members are evaluated using an execution context
/// </summary>
/// <typeparam name="TContext">The type of execution context</typeparam>
public interface IDescriptorConfigurator<TContext> : IDescriptorCollector
{
    /// <summary>
    ///     Configures the members of the described object
    /// </summary>
    /// <param name="manager">The manager that registers context-aware member handlers, overrides, and extensions</param>
    void Configure(IMemberManager<TContext> manager);
}