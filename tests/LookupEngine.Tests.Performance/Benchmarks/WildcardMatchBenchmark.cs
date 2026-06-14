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

using BenchmarkDotNet.Attributes;

namespace LookupEngine.Tests.Performance.Benchmarks;

/// <summary>
///     Compares wildcard matching strategies for a single namespace pattern where <c>*</c> matches zero or more characters.
///     Matching is ordinal and case-sensitive, mirroring <c>MethodEvaluationPolicy</c>.
/// </summary>
public class WildcardMatchBenchmark
{
    private static readonly (string Input, string Pattern)[] Cases =
    [
        ("System.Collections.Generic", "System.*"),
        ("System.Collections.Generic", "System.Collections.*"),
        ("System.Collections.Generic", "*.Generic"),
        ("System.Collections.Generic", "*Collections*"),
        ("System.Collections.Generic", "System.*.Generic"),
        ("System.Collections.Generic", "Autodesk.Revit.*"),
        ("System.Collections.Generic", "System.Collections.Generic")
    ];

    [Params(0, 1, 2, 3, 4, 5, 6)] public int PatternIndex { get; set; }

    private string _input = null!;
    private string _pattern = null!;
    private WildcardImpl.CompiledPattern _compiled;

    [GlobalSetup]
    public void Setup()
    {
        (_input, _pattern) = Cases[PatternIndex];

        // Precompilation performed once when the policy is created.
        _compiled = new WildcardImpl.CompiledPattern(_pattern);
    }

    [Benchmark]
    public bool TwoPointer()
    {
        return WildcardImpl.MatchesWildcard(_input, _pattern);
    }

    [Benchmark]
    public bool SegmentSplit()
    {
        return WildcardImpl.MatchesWildcardSegments(_input, _pattern);
    }

    [Benchmark(Baseline = true)]
    public bool Precompiled()
    {
        return _compiled.Matches(_input);
    }
}

/// <summary>
///     Mirrors the real <c>MethodEvaluationPolicy.IsEvaluationAllowed</c>: a single namespace is matched against an
///     array of patterns with short-circuiting. "Raw" strategies re-parse every pattern on each call, while the
///     precompiled strategy parses the array once and only runs the match.
/// </summary>
public class WildcardMatchArrayBenchmark
{
    private static readonly string[] Patterns =
    [
        "System.*",
        "System.Collections.*",
        "*.Generic",
        "*Collections*",
        "System.*.Generic",
        "Autodesk.Revit.*",
        "System.Collections.Generic"
    ];

    private static readonly string[] Inputs =
    [
        "Autodesk.Revit.DB.Wall", // matches a late pattern
        "System.Collections.Generic", // matches the first pattern
        "Foo.Bar.Baz" // matches nothing
    ];

    [Params(0, 1, 2)] public int InputIndex { get; set; }

    private string _input = null!;
    private WildcardImpl.CompiledPattern[] _compiled = null!;

    [GlobalSetup]
    public void Setup()
    {
        _input = Inputs[InputIndex];

        // Precompilation performed once when the policy is created.
        _compiled = new WildcardImpl.CompiledPattern[Patterns.Length];
        for (var index = 0; index < Patterns.Length; index++)
        {
            _compiled[index] = new WildcardImpl.CompiledPattern(Patterns[index]);
        }
    }

    [Benchmark]
    public bool TwoPointer()
    {
        foreach (var pattern in Patterns)
        {
            if (WildcardImpl.MatchesWildcard(_input, pattern)) return true;
        }

        return false;
    }

    [Benchmark]
    public bool SegmentSplit()
    {
        foreach (var pattern in Patterns)
        {
            if (WildcardImpl.MatchesWildcardSegments(_input, pattern)) return true;
        }

        return false;
    }

    [Benchmark(Baseline = true)]
    public bool Precompiled()
    {
        foreach (var compiled in _compiled)
        {
            if (compiled.Matches(_input)) return true;
        }

        return false;
    }
}

/// <summary>
///     Shared wildcard matching implementations used by both the single-pattern and array benchmarks.
/// </summary>
internal static class WildcardImpl
{
    /// <summary>
    ///     A namespace pattern parsed once. Trailing-star patterns ("Autodesk.Revit.*", "*")
    ///     collapse to a vectorized prefix check; everything else uses the segment matcher.
    /// </summary>
    public readonly struct CompiledPattern(string pattern)
    {
        private readonly string? _prefix = pattern[^1] == '*' && pattern.IndexOf('*') == pattern.Length - 1
            ? pattern[..^1]
            : null;

        public bool Matches(string input)
        {
            return _prefix is not null
                ? input.StartsWith(_prefix, StringComparison.Ordinal)
                : MatchesWildcardSegments(input, pattern);
        }
    }

    /// <summary>
    ///     Iterative two-pointer over <see cref="string" /> indexers
    /// </summary>
    public static bool MatchesWildcard(string input, string pattern)
    {
        var inputIndex = 0;
        var patternIndex = 0;
        var starIndex = -1;
        var starMatchIndex = 0;

        while (inputIndex < input.Length)
        {
            if (patternIndex < pattern.Length && pattern[patternIndex] == input[inputIndex])
            {
                inputIndex++;
                patternIndex++;
            }
            else if (patternIndex < pattern.Length && pattern[patternIndex] == '*')
            {
                starIndex = patternIndex;
                starMatchIndex = inputIndex;
                patternIndex++;
            }
            else if (starIndex >= 0)
            {
                patternIndex = starIndex + 1;
                starMatchIndex++;
                inputIndex = starMatchIndex;
            }
            else
            {
                return false;
            }
        }

        while (patternIndex < pattern.Length && pattern[patternIndex] == '*')
        {
            patternIndex++;
        }

        return patternIndex == pattern.Length;
    }

    /// <summary>
    ///     Splits the pattern on <c>*</c> and matches each literal segment with vectorized
    ///     <c>StartsWith</c>/<c>IndexOf</c>/<c>EndsWith</c>.
    /// </summary>
    public static bool MatchesWildcardSegments(ReadOnlySpan<char> input, ReadOnlySpan<char> pattern)
    {
        var firstStar = pattern.IndexOf('*');
        if (firstStar < 0) return input.SequenceEqual(pattern);

        var prefix = pattern[..firstStar];
        if (!input.StartsWith(prefix, StringComparison.Ordinal)) return false;
        input = input[prefix.Length..];
        pattern = pattern[(firstStar + 1)..];

        while (true)
        {
            var nextStar = pattern.IndexOf('*');
            if (nextStar < 0)
            {
                return input.EndsWith(pattern, StringComparison.Ordinal);
            }

            var segment = pattern[..nextStar];
            if (!segment.IsEmpty)
            {
                var index = input.IndexOf(segment, StringComparison.Ordinal);
                if (index < 0) return false;
                input = input[(index + segment.Length)..];
            }

            pattern = pattern[(nextStar + 1)..];
        }
    }
}