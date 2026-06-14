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
///     Context-aware builder passed to <see cref="IDescriptorConfigurator{TContext}.Configure"/> to configure member handlers, evaluation policy overrides, and synthetic extension members for a descriptor.
/// </summary>
/// <typeparam name="TContext">The type of execution context available to registered handlers.</typeparam>
public interface IMemberConfigurator<TContext>
{
    /// <summary>
    ///     Returns a builder for configuring an existing member of the described type.
    /// </summary>
    /// <param name="name">
    ///     The member name. Affects all overloads unless narrowed with <see cref="MemberResolverBuilder{TContext}.When"/>.
    /// </param>
    MemberResolverBuilder<TContext> Member(string name);

    /// <summary>
    ///     Returns a builder for registering a synthetic member that the described type does not declare.
    /// </summary>
    /// <param name="name">The display name for the extension member.</param>
    ExtensionBuilder<TContext> Extension(string name);
}