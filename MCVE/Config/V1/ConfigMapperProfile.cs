using System.Collections.ObjectModel;
using AutoMapper;

namespace MCVE.Config.V1;

public class ConfigMapperProfile : Profile
{
    private static int _instanceNameCounter = 1;

    private static string BuildInstanceName()
    {
        return $"instance{_instanceNameCounter++}";
    }

    public ConfigMapperProfile()
    {
        CreateMap<QualityScoreConfigYaml, Config.QualityScoreConfigYaml>();
        CreateMap<CustomFormatConfigYaml, Config.CustomFormatConfigYaml>();
        CreateMap<QualitySizeConfigYaml, Config.QualitySizeConfigYaml>();
        CreateMap<QualityProfileConfigYaml, Config.QualityProfileConfigYaml>();
        CreateMap<ServiceConfigYaml, Config.ServiceConfigYaml>();
        CreateMap<ReleaseProfileFilterConfigYaml, Config.ReleaseProfileFilterConfigYaml>();
        CreateMap<ReleaseProfileConfigYaml, Config.ReleaseProfileConfigYaml>();
        CreateMap<RadarrConfigYaml, Config.RadarrConfigYaml>();
        CreateMap<SonarrConfigYaml, Config.SonarrConfigYaml>();
        CreateMap<RootConfigYaml, Config.RootConfigYaml>();

        // Backward Compatibility: Convert list-based instances to mapping-based ones.
        // The key is auto-generated.
        CreateMap<Collection<RadarrConfigYaml>, Dictionary<string, Config.RadarrConfigYaml>>()
            .ConstructUsing((x, c) => x.ToDictionary(_ => BuildInstanceName(),
                y => c.Mapper.Map<Config.RadarrConfigYaml>(y)));
    }
}
