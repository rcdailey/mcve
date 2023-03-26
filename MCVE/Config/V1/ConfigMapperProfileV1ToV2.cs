using System.Collections.ObjectModel;
using AutoMapper;

namespace MCVE.Config.V1;

public class ConfigMapperProfileV1ToV2 : Profile
{
    private static int _instanceNameCounter = 1;

    private static string BuildInstanceName()
    {
        return $"instance{_instanceNameCounter++}";
    }

    public ConfigMapperProfileV1ToV2()
    {
        CreateMap<QualityScoreConfigYaml, V2.QualityScoreConfigYaml>();
        CreateMap<CustomFormatConfigYaml, V2.CustomFormatConfigYaml>();
        CreateMap<QualitySizeConfigYaml, V2.QualitySizeConfigYaml>();
        CreateMap<QualityProfileConfigYaml, V2.QualityProfileConfigYaml>();
        CreateMap<ServiceConfigYaml, V2.ServiceConfigYaml>();
        CreateMap<ReleaseProfileFilterConfigYaml, V2.ReleaseProfileFilterConfigYaml>();
        CreateMap<ReleaseProfileConfigYaml, V2.ReleaseProfileConfigYaml>();
        CreateMap<RadarrConfigYaml, V2.RadarrConfigYaml>();
        CreateMap<SonarrConfigYaml, V2.SonarrConfigYaml>();
        CreateMap<RootConfigYaml, V2.RootConfigYaml>();

        // Backward Compatibility: Convert list-based instances to mapping-based ones.
        // The key is auto-generated.
        CreateMap<Collection<RadarrConfigYaml>, Dictionary<string, V2.RadarrConfigYaml>>()
            .ConstructUsing((x, c) => x.ToDictionary(_ => BuildInstanceName(),
                y => c.Mapper.Map<V2.RadarrConfigYaml>(y)));
    }
}
