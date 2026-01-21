using System.Text;
using Build.Options;
using Microsoft.Extensions.Options;
using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.Git.Extensions;
using ModularPipelines.Modules;
using File = ModularPipelines.FileSystem.File;

namespace Build.Modules;

/// <summary>
///     Generate the changelog for publishing the templates.
/// </summary>
[DependsOn<ResolveBuildVersionModule>]
public sealed class GenerateChangelogModule(IOptions<PublishOptions> publishOptions) : Module<string>
{
    protected override async Task<string?> ExecuteAsync(IModuleContext context, CancellationToken cancellationToken)
    {
        var versioningResult = await context.GetModule<ResolveBuildVersionModule>();
        var versioning = versioningResult.ValueOrDefault!;

        var changelogFile = context.Git().RootDirectory.GetFile(publishOptions.Value.ChangelogFile);
        var changelog = await ParseChangelog(changelogFile, versioning.Version);

        return changelog.Length > 0 ? changelog.ToString() : string.Empty;
    }

    /// <summary>
    ///     Parse the changelog file to extract the entries for a specific version.
    /// </summary>
    private static async Task<StringBuilder> ParseChangelog(File changelogFile, string version)
    {
        const string separator = "# ";

        var isChangelogEntryFound = false;
        var changelog = new StringBuilder();

        await foreach (var line in changelogFile.ReadLinesAsync())
        {
            if (isChangelogEntryFound)
            {
                if (line.StartsWith(separator)) break;

                changelog.AppendLine(line);
                continue;
            }

            if (line.StartsWith(separator) && line.Contains(version))
            {
                isChangelogEntryFound = true;
            }
        }

        TrimEmptyLines(changelog);
        return changelog;
    }

    /// <summary>
    ///     Remove empty lines from the beginning and end of the changelog builder.
    /// </summary>
    private static void TrimEmptyLines(StringBuilder changelog)
    {
        if (changelog.Length == 0) return;

        var start = 0;
        var end = changelog.Length - 1;

        while (start < changelog.Length && (changelog[start] == '\r' || changelog[start] == '\n')) start++;
        while (end >= start && (changelog[end] == '\r' || changelog[end] == '\n')) end--;

        if (end < changelog.Length - 1)
        {
            changelog.Remove(end + 1, changelog.Length - (end + 1));
        }

        if (start > 0)
        {
            changelog.Remove(0, start);
        }
    }
}