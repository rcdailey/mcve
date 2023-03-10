using System.Collections;
using System.Collections.ObjectModel;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MCVE;

public record QualityScoreConfigYaml
{
    public required string Name { get; init; }
    public int? Score { get; init; }
}

public record CustomFormatConfigYaml
{
    public Collection<string> TrashIds { get; init; } = new();
    public Collection<QualityScoreConfigYaml> QualityProfiles { get; init; } = new();
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
    public Collection<QualityProfileConfigYaml> QualityProfiles { get; init; } = new();
}

public record ReleaseProfileFilterConfigYaml
{
    public Collection<string> Include { get; init; } = new();
    public Collection<string> Exclude { get; init; } = new();
}

public record ReleaseProfileConfigYaml
{
    public Collection<string> TrashIds { get; init; } = new();
    public bool StrictNegativeScores { get; init; }
    public Collection<string> Tags { get; init; } = new();
    public ReleaseProfileFilterConfigYaml? Filter { get; init; }
}

public record RadarrConfigYaml : ServiceConfigYaml;

public record SonarrConfigYaml : ServiceConfigYaml
{
    public Collection<ReleaseProfileConfigYaml> ReleaseProfiles { get; init; } = new();
}

public record RootConfigYaml
{
    public Dictionary<string, RadarrConfigYaml> Radarr { get; init; } = new();
    public Dictionary<string, SonarrConfigYaml> Sonarr { get; init; } = new();
}

internal class StringToNumberConverter : IYamlTypeConverter
{
    public bool Accepts(Type type)
    {
        return type == typeof(object);
    }

    public object? ReadYaml(IParser parser, Type type)
    {
        if (parser.Current is not Scalar)
        {
            return null;
        }

        var scalar = parser.Consume<Scalar>();
        if (int.TryParse(scalar.Value, out var number))
        {
            return number;
        }

        return scalar.Value;
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        throw new NotImplementedException();
    }
}

public class YamlConfigContractResolver : DefaultContractResolver
{
    public static readonly YamlConfigContractResolver Instance = new();

    public YamlConfigContractResolver()
    {
    }

    protected override JsonProperty CreateProperty(
        MemberInfo member,
        MemberSerialization memberSerialization)
    {
        var property = base.CreateProperty(member, memberSerialization);
        if (property.PropertyType == typeof(string))
        {
            return property;
        }

        if (property.PropertyType?.GetInterface(nameof(IEnumerable)) != null)
        {
            property.ShouldSerialize = instance => ShouldSerialize(property, instance);
        }

        return property;
    }

    private static bool ShouldSerialize(JsonProperty property, object instance)
    {
        if (property.PropertyName is null)
        {
            return false;
        }

        var instanceProperty = instance.GetType().GetProperty(property.PropertyName);
        var enumerable = instanceProperty?.GetValue(instance) as IEnumerable<object>;
        return enumerable?.Count() > 0;
    }
}

public static class Program
{
    public static async Task Main()
    {
        const string yaml = @"
radarr:
  myinstance1:
    api_key: key1
    base_url: url1
  myinstance2:
    api_key: key2
    base_url: url2
    custom_formats:
      - trash_ids:
          - abc123
        quality_profiles:
          - name: MyProfile
            score: 1000
sonarr: 
  myinstance3:
    base_url: url3
";

        var deserializer = new DeserializerBuilder()
            // .WithTypeConverter(new StringToNumberConverter())
            .WithNamingConvention(UnderscoredNamingConvention.Instance)
            .Build();

        using TextReader reader = new StringReader(yaml);
        var categories = deserializer.Deserialize<RootConfigYaml>(reader);

        // var serializedObject = JsonConvert.SerializeObject(categories, Formatting.Indented);

        var jsonSerializer = new JsonSerializer
        {
            Formatting = Formatting.Indented,
            ContractResolver = new DefaultContractResolver()
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };

        var json = JToken.FromObject(categories, jsonSerializer);

        // var schemaUrl =
        //     "https://raw.githubusercontent.com/recyclarr/recyclarr/master/schemas/config-schema.json";
        // var schemaData = await schemaUrl.GetStringAsync();
        var jsonData = new JsonTextReader(File.OpenText("config-schema.json"));

        var resolver = new JSchemaUrlResolver();
        // var schema = JSchema.Parse(schemaData, resolver);
        var schema = JSchema.Load(jsonData, resolver);

        var isValid = json.IsValid(schema, out IList<ValidationError> errors);
        Console.WriteLine($"Valid: {isValid}");
        foreach (var err in errors)
        {
            Console.WriteLine($"Error ({err.Path}): {err.Message}");
        }
    }
}
