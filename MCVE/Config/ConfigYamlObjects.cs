using System.Collections.ObjectModel;

namespace MCVE.Config;

public record QualityScoreConfigYaml
{
    public required string Name { get; init; }
    public int? Score { get; init; }
}

public record CustomFormatConfigYaml
{
    public Collection<string>? TrashIds { get; init; }
    public Collection<QualityScoreConfigYaml>? QualityProfiles { get; init; }
}

public record QualitySizeConfigYaml
{
    public required string Type { get; init; }
    public decimal? PreferredRatio { get; init; }
}

public record QualityProfileConfigYaml
{
    public required string Name { get; init; }
    public bool ResetUnmatchedScores { get; init; }
}

public record ServiceConfigYaml
{
    public required string BaseUrl { get; init; }
    public required string ApiKey { get; init; }
    public bool DeleteOldCustomFormats { get; init; }
    public bool ReplaceExistingCustomFormats { get; init; }
    public Collection<CustomFormatConfigYaml>? CustomFormats { get; init; }
    public QualitySizeConfigYaml? QualityDefinition { get; init; }
    public Collection<QualityProfileConfigYaml>? QualityProfiles { get; init; }
}

public record ReleaseProfileFilterConfigYaml
{
    public Collection<string>? Include { get; init; }
    public Collection<string>? Exclude { get; init; }
}

public record ReleaseProfileConfigYaml
{
    public Collection<string>? TrashIds { get; init; }
    public bool StrictNegativeScores { get; init; }
    public Collection<string>? Tags { get; init; }
    public ReleaseProfileFilterConfigYaml? Filter { get; init; }
}

public record RadarrConfigYaml : ServiceConfigYaml;

public record SonarrConfigYaml : ServiceConfigYaml
{
    public Collection<ReleaseProfileConfigYaml>? ReleaseProfiles { get; init; }
}

public record RootConfigYaml
{
    public Dictionary<string, RadarrConfigYaml>? Radarr { get; init; }
    public Dictionary<string, SonarrConfigYaml>? Sonarr { get; init; }
}
