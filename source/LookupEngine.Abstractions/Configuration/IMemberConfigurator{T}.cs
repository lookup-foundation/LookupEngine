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
///     Configures context-aware member handlers, evaluation overrides, and extensions for a descriptor
/// </summary>
/// <typeparam name="TContext">The type of execution context</typeparam>
public interface IMemberConfigurator<TContext>
{
    /// <summary>
    ///     Configures an existing member of the described type by name
    /// </summary>
    /// <param name="name">The member name; affects all overloads unless narrowed with <c>When</c></param>
    MemberResolverBuilder<TContext> Member(string name);

    /// <summary>
    ///     Defines a synthetic member that the described type does not have
    /// </summary>
    /// <param name="name">The extension member name</param>
    ExtensionBuilder<TContext> Extension(string name);
}