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
///     Indicates that the descriptor can add a new methods to the object
/// </summary>
/// <typeparam name="TContext">The type of execution context</typeparam>
public interface IDescriptorExtension<TContext> : IDescriptorCollector
{
    /// <summary>
    ///     Registers the extensions for the object
    /// </summary>
    /// <param name="manager">The extension manager, that provides shell for extension registration</param>
    void RegisterExtensions(IExtensionManager<TContext> manager);
}