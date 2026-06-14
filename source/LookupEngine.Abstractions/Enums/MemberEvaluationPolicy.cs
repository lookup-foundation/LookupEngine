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
///     The evaluation state of a decomposed member.
/// </summary>
public enum MemberEvaluationPolicy
{
    /// <summary>The member was evaluated eagerly during decomposition.</summary>
    Evaluated = 0,

    /// <summary>
    ///     The member was not evaluated during decomposition. Call <see cref="DecomposedMember.Evaluate()"/>
    ///     to trigger evaluation on demand.
    /// </summary>
    Deferred = 1,

    /// <summary>
    ///     The member is permanently disabled. Force evaluation reports the disabled result without invoking it.
    /// </summary>
    Disabled = 2,

    /// <summary>
    ///     The member cannot be evaluated by the engine.
    /// </summary>
    Unsupported = 3
}