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
///     The evaluation state of a decomposed member value
/// </summary>
public enum MemberEvaluationPolicy
{
    /// <summary>
    ///     The member value was evaluated during decomposition
    /// </summary>
    Evaluated = 0,

    /// <summary>
    ///     The member evaluation was deferred and is available for force evaluation
    /// </summary>
    Deferred = 1,

    /// <summary>
    ///     The member evaluation is disabled, force evaluation reports the disabled result
    /// </summary>
    Disabled = 2,

    /// <summary>
    ///     The member cannot be evaluated by the engine
    /// </summary>
    Unsupported = 3
}