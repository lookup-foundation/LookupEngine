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
///     Allows a descriptor to configure member evaluation using caller-supplied context.
///     Implement this to resolve member handlers, override evaluation policies per member, and register synthetic extension members.
///     Use this generic variant when handlers need data from the decomposition's execution context.
/// </summary>
/// <typeparam name="TContext">The type of execution context passed through from the decomposition options.</typeparam>
public interface IDescriptorConfigurator<TContext> : IDescriptorCollector
{
    /// <summary>
    ///     Called by the engine once per decomposition to apply context-aware member configuration.
    /// </summary>
    /// <param name="configuration">Builder for registering context-aware member handlers, policy overrides, and extensions.</param>
    void Configure(IMemberConfigurator<TContext> configuration);
}