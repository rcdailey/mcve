// See https://aka.ms/new-console-template for more information

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using YamlDotNet.Serialization;

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

        var deserializer = new DeserializerBuilder().Build();

        using TextReader reader = new StringReader(yaml);
        var categories = deserializer.Deserialize<object>(reader);

        var json = JToken.FromObject(categories);

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
            Console.WriteLine($"Error Line {err.LineNumber}: {err.Message}");
        }
    }
}
