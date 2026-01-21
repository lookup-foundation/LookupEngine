namespace Build.Options;

[Serializable]
public sealed record PublishOptions
{
    public string? Version { get; init; }
    public string ChangelogFile { get; init; } = "Changelog.md";
}