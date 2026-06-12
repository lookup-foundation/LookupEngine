using System.Reflection;

// ReSharper disable once CheckNamespace
namespace LookupEngine;

/// <summary>
///     Controls which methods are evaluated automatically during decomposition
/// </summary>
[PublicAPI]
public sealed class MethodEvaluationPolicy
{
    /// <summary>
    ///     Namespace wildcard patterns of declaring types whose methods are evaluated automatically
    /// </summary>
    /// <remarks>
    ///     The <c>*</c> wildcard matches zero or more characters, matching is ordinal and case-sensitive.
    ///     Empty by default: no methods are evaluated, all are deferred
    /// </remarks>
    public string[] IncludedNamespaces { get; init; } = [];

    /// <summary>
    ///     Return types excluded from automatic evaluation even when the namespace matches
    /// </summary>
    /// <remarks>
    ///     Excluded methods are deferred and available for force evaluation.
    ///     By default, methods without a return value are only executed explicitly
    /// </remarks>
    public Type[] ExcludedReturnTypes { get; init; } = [typeof(void)];

    /// <summary>
    ///     Defer evaluation of all methods
    /// </summary>
    public static MethodEvaluationPolicy None { get; } = new();

    /// <summary>
    ///     Evaluate all methods except the excluded return types
    /// </summary>
    public static MethodEvaluationPolicy All { get; } = new() { IncludedNamespaces = ["*"] };

    /// <summary>
    ///     Check if the method is allowed to be evaluated during decomposition
    /// </summary>
    internal bool IsEvaluationAllowed(MethodInfo member, Type declaringType)
    {
        foreach (var returnType in ExcludedReturnTypes)
        {
            if (member.ReturnType == returnType) return false;
        }

        var memberNamespace = declaringType.Namespace;
        foreach (var pattern in IncludedNamespaces)
        {
            if (pattern == "*") return true;
            if (memberNamespace is null) continue;
            if (MatchesWildcard(memberNamespace, pattern)) return true;
        }

        return false;
    }

    /// <summary>
    ///     Match the input against a pattern where <c>*</c> matches zero or more characters
    /// </summary>
    private static bool MatchesWildcard(string input, string pattern)
    {
        var inputIndex = 0;
        var patternIndex = 0;
        var starIndex = -1;
        var starMatchIndex = 0;

        while (inputIndex < input.Length)
        {
            if (patternIndex < pattern.Length && (pattern[patternIndex] == input[inputIndex]))
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
}
