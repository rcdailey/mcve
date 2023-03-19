using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using YamlDotNet.Serialization.Utilities;

namespace MCVE;

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
    [TypeConverter(typeof(ArrayToMapYamlConverter<RadarrConfigYaml>))]
    public Dictionary<string, RadarrConfigYaml>? Radarr { get; init; }

    [TypeConverter(typeof(ArrayToMapYamlConverter<SonarrConfigYaml>))]
    public Dictionary<string, SonarrConfigYaml>? Sonarr { get; init; }
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

// public class YamlConfigContractResolver : DefaultContractResolver
// {
//     public static readonly YamlConfigContractResolver Instance = new();
//
//     public YamlConfigContractResolver()
//     {
//         NamingStrategy = new SnakeCaseNamingStrategy();
//     }
//
//     protected override JsonProperty CreateProperty(
//         MemberInfo member,
//         MemberSerialization memberSerialization)
//     {
//         var property = base.CreateProperty(member, memberSerialization);
//         if (property.PropertyType == typeof(string))
//         {
//             return property;
//         }
//
//         if (property.PropertyType?.GetInterface(nameof(IEnumerable)) != null)
//         {
//             property.ShouldSerialize = instance => ShouldSerialize(property, instance);
//         }
//
//         return property;
//     }
//
//     private static bool ShouldSerialize(JsonProperty property, object instance)
//     {
//         if (property.PropertyName is null)
//         {
//             return false;
//         }
//
//         var instanceProperty = instance.GetType().GetProperty(property.PropertyName);
//         var enumerable = instanceProperty?.GetValue(instance) as IEnumerable<object>;
//         return enumerable?.Count() > 0;
//     }
// }

class ArrayToMapNodeDeserializer : INodeDeserializer
{
    public bool Deserialize(
        IParser reader,
        Type expectedType,
        Func<IParser, Type, object?> nestedObjectDeserializer,
        out object? value)
    {
        value = null;
        return false;
    }
}

class ArrayToMapYamlConverter<TConfigYaml> : IYamlTypeConverter
    where TConfigYaml : ServiceConfigYaml
{
    public ArrayToMapYamlConverter()
    {
        
    }

    public bool Accepts(Type type)
    {
        return typeof(IDictionary<string, TConfigYaml>).IsAssignableFrom(type);
    }

    public object? ReadYaml(IParser parser, Type type)
    {
        var dict = new Dictionary<string, object>();
        switch (parser.Current)
        {
            case MappingStart:
                ParseAndAdd<MappingStart, MappingEnd>(parser, dict);
                break;

            case SequenceStart:
                ParseAndAdd<SequenceStart, SequenceEnd>(parser, dict);
                break;

            default:
                return null;
        }

        return null; // temporary
    }

    private void ParseAndAdd<TStart, TEnd>(IParser parser, IDictionary<string, object> dict)
        where TStart : ParsingEvent
        where TEnd : ParsingEvent
    {
        parser.Consume<TStart>();
        while (!parser.TryConsume<TEnd>(out _))
        {
            ParseAndAddConfig(parser, dict);
        }
    }

    private static int _instanceCount = 1;

    private void ParseAndAddConfig(IParser parser, IDictionary<string, object> dict)
    {
        var instanceName = parser.TryConsume<Scalar>(out var key)
            ? key.Value
            : $"instance{_instanceCount++}";

        // var newConfig = (ServiceConfiguration?)_deserializer.Deserialize(parser, configType);
        // if (newConfig is null)
        // {
        //     throw new YamlException(
        //         $"Unable to deserialize instance at line {lineNumber} using configuration type {_currentSection}");
        // }
        //
        // newConfig.InstanceName = instanceName;
        // newConfig.LineNumber = lineNumber ?? 0;
        //
        // if (!_validator.Validate(newConfig))
        // {
        //     throw new YamlException("Validation failed");
        // }
        //
        // _configs.Add(newConfig);
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type)
    {
        throw new NotImplementedException();
    }
}

public static class Program
{
    public static async Task Main()
    {
        const string yaml = @"
radarr:
  - api_key: key0
    base_url: asdf

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
            NullValueHandling = NullValueHandling.Ignore,
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
