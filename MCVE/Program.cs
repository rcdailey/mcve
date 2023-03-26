using AutoMapper;
using AutoMapper.EquivalencyExpression;
using MCVE.Config.V2;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace MCVE;

public static class Program
{
    private static readonly IDeserializer Deserializer = new DeserializerBuilder()
        // .WithTypeConverter(new StringToNumberConverter())
        // .WithTypeConverter(new ArrayToMapYamlConverter())
        .WithNamingConvention(UnderscoredNamingConvention.Instance)
        .Build();

    private static IMapper _mapper;

    public static RootConfigYaml DeserializeConfig(string yaml)
    {
        using TextReader reader = new StringReader(yaml);
        return Deserializer.Deserialize<RootConfigYaml>(reader);
    }

    private static RootConfigYaml DeserializeOldConfig(string yaml)
    {
        try
        {
            using TextReader reader = new StringReader(yaml);
            // todo: If successful, log a deprecation message
            var obj = Deserializer.Deserialize<Config.V1.RootConfigYaml>(reader);
            return _mapper.Map<RootConfigYaml>(obj);
        }
        catch (Exception e)
        {
            // todo: Log exception and rethrow
            throw;
        }
    }

    public static async Task Main()
    {
        const string yaml = @"
radarr:
  - api_key: key0
    base_url: asdf

#radarr:
#  myinstance1:
#    api_key: key1
#    base_url: url1
#  myinstance2:
#    api_key: key2
#    base_url: url2
#    custom_formats:
#      - trash_ids:
#          - abc123
#        quality_profiles:
#          - name: MyProfile
#            score: 1000
#sonarr: 
#  myinstance3:
#    base_url: url3
";
        var mapperConfig = new MapperConfiguration(o =>
        {
            o.AddCollectionMappers();
            o.AddGlobalIgnore("Item");
            o.AddProfile<ConfigMapperProfileV1ToV2>();
        });
        mapperConfig.AssertConfigurationIsValid();
        _mapper = mapperConfig.CreateMapper();

        // Try to deserialize the latest object first
        RootConfigYaml? theYaml = null;
        try
        {
            theYaml = DeserializeConfig(yaml);
        }
        catch (YamlException e)
        {
            // todo: Log exception (maybe debug?)
            try
            {
                // todo: Try to check property exception originated from and conditionally parse old config based on that?
                theYaml = DeserializeOldConfig(yaml);
            }
            catch
            {
                throw e;
            }
        }

        // var serializedObject = JsonConvert.SerializeObject(categories, Formatting.Indented);

        var jsonSerializer = new JsonSerializer
        {
            // Formatting = Formatting.Indented,
            // NullValueHandling = NullValueHandling.Ignore,
            ContractResolver = new DefaultContractResolver()
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };

        var json = JToken.FromObject(theYaml, jsonSerializer);
        var theObject = json.ToObject<RootConfigYaml>();

        // var schemaUrl =
        //     "https://raw.githubusercontent.com/recyclarr/recyclarr/master/schemas/config-schema.json";
        // var schemaData = await schemaUrl.GetStringAsync();
        // var jsonData = new JsonTextReader(File.OpenText("config-schema.json"));

        // var resolver = new JSchemaUrlResolver();
        // var schema = JSchema.Load(jsonData, resolver);
        //
        // var isValid = json.IsValid(schema, out IList<ValidationError> errors);
        // Console.WriteLine($"Valid: {isValid}");
        // foreach (var err in errors)
        // {
        //     Console.WriteLine($"Error ({err.Path}): {err.Message}");
        // }
    }
}

//
// public class ArrayToMapJsonConverter : JsonConverter
// {
//     public override bool CanConvert(Type objectType)
//     {
//         // return objectType.IsArray;
//         // return typeof(IDictionary<,>).IsAssignableFrom(objectType.GetGenericTypeDefinition());
//         throw new NotImplementedException();
//     }
//
//     public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue,
//         JsonSerializer serializer)
//     {
//         if (reader.TokenType == JsonToken.StartObject)
//         {
//             return serializer.Deserialize(reader, objectType);
//         }
//
//         if (reader.TokenType == JsonToken.StartArray)
//         {
//             var array = serializer.Deserialize<JArray>(reader);
//             foreach (var obj in array)
//             {
//             }
//             // var genericArgs = objectType.GetGenericArguments();
//             // var arrayType = genericArgs[1].MakeArrayType();
//             // var array = serializer.Deserialize(reader, arrayType);
//         }
//
//         return null;
//     }
//
//     public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
//     {
//         serializer.Serialize(writer, value);
//     }
// }


// internal class StringToNumberConverter : IYamlTypeConverter
// {
//     public bool Accepts(Type type)
//     {
//         return type == typeof(object);
//     }
//
//     public object? ReadYaml(IParser parser, Type type)
//     {
//         if (parser.Current is not Scalar)
//         {
//             return null;
//         }
//
//         var scalar = parser.Consume<Scalar>();
//         if (int.TryParse(scalar.Value, out var number))
//         {
//             return number;
//         }
//
//         return scalar.Value;
//     }
//
//     public void WriteYaml(IEmitter emitter, object? value, Type type)
//     {
//         throw new NotImplementedException();
//     }
// }
//
// // public class YamlConfigContractResolver : DefaultContractResolver
// // {
// //     public static readonly YamlConfigContractResolver Instance = new();
// //
// //     public YamlConfigContractResolver()
// //     {
// //         NamingStrategy = new SnakeCaseNamingStrategy();
// //     }
// //
// //     protected override JsonProperty CreateProperty(
// //         MemberInfo member,
// //         MemberSerialization memberSerialization)
// //     {
// //         var property = base.CreateProperty(member, memberSerialization);
// //         if (property.PropertyType == typeof(string))
// //         {
// //             return property;
// //         }
// //
// //         if (property.PropertyType?.GetInterface(nameof(IEnumerable)) != null)
// //         {
// //             property.ShouldSerialize = instance => ShouldSerialize(property, instance);
// //         }
// //
// //         return property;
// //     }
// //
// //     private static bool ShouldSerialize(JsonProperty property, object instance)
// //     {
// //         if (property.PropertyName is null)
// //         {
// //             return false;
// //         }
// //
// //         var instanceProperty = instance.GetType().GetProperty(property.PropertyName);
// //         var enumerable = instanceProperty?.GetValue(instance) as IEnumerable<object>;
// //         return enumerable?.Count() > 0;
// //     }
// // }
//
// class ArrayToMapNodeDeserializer : INodeDeserializer
// {
//     public bool Deserialize(
//         IParser reader,
//         Type expectedType,
//         Func<IParser, Type, object?> nestedObjectDeserializer,
//         out object? value)
//     {
//         value = null;
//         return false;
//     }
// }
//
// class ArrayToMapYamlConverter : IYamlTypeConverter
// {
//     public ArrayToMapYamlConverter()
//     {
//
//     }
//
//     public bool Accepts(Type type)
//     {
//         return typeof(IDictionary<,>).IsAssignableFrom(type);
//     }
//
//     public object? ReadYaml(IParser parser, Type type)
//     {
//         var dict = new Dictionary<string, object>();
//         switch (parser.Current)
//         {
//             case MappingStart:
//                 ParseAndAdd<MappingStart, MappingEnd>(parser, dict);
//                 break;
//
//             case SequenceStart:
//                 ParseAndAdd<SequenceStart, SequenceEnd>(parser, dict);
//                 break;
//
//             default:
//                 return null;
//         }
//
//         return null; // temporary
//     }
//
//     private void ParseAndAdd<TStart, TEnd>(IParser parser, IDictionary<string, object> dict)
//         where TStart : ParsingEvent
//         where TEnd : ParsingEvent
//     {
//         parser.Consume<TStart>();
//         while (!parser.TryConsume<TEnd>(out _))
//         {
//             ParseAndAddConfig(parser, dict);
//         }
//     }
//
//     private static int _instanceCount = 1;
//
//     private void ParseAndAddConfig(IParser parser, IDictionary<string, object> dict)
//     {
//         var instanceName = parser.TryConsume<Scalar>(out var key)
//             ? key.Value
//             : $"instance{_instanceCount++}";
//
//         // var newConfig = (ServiceConfiguration?)_deserializer.Deserialize(parser, configType);
//         // if (newConfig is null)
//         // {
//         //     throw new YamlException(
//         //         $"Unable to deserialize instance at line {lineNumber} using configuration type {_currentSection}");
//         // }
//         //
//         // newConfig.InstanceName = instanceName;
//         // newConfig.LineNumber = lineNumber ?? 0;
//         //
//         // if (!_validator.Validate(newConfig))
//         // {
//         //     throw new YamlException("Validation failed");
//         // }
//         //
//         // _configs.Add(newConfig);
//     }
//
//     public void WriteYaml(IEmitter emitter, object? value, Type type)
//     {
//         throw new NotImplementedException();
//     }
// }
