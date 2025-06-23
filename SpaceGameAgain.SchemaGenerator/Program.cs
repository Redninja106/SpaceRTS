using SpaceGame.Data;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using System.Xml;
using System.Xml.Linq;

string outputDirectory = "../../../../SpaceGameAgain/Schemas/";

// ==== ==== ==== ==== PART 1 ==== ==== ==== ====
// search prototype instances to make autocomplete enums

string[] prototypeFileNames = Directory.GetFiles("Prototypes", "*", SearchOption.AllDirectories);
Dictionary<Type, List<string>> prototypesByClass = [];
string schemaProperties = "";

foreach (var prototypeFile in prototypeFileNames)
{
    JsonDocumentOptions opts = new()
    {
        CommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        MaxDepth = 64,
    };
    
    JsonDocument doc = JsonDocument.Parse(File.ReadAllText(prototypeFile), opts);

    string name = doc.RootElement.GetProperty("name").GetString()!;
    string prototype = doc.RootElement.GetProperty("prototype").GetString()!;

    Type prototypeClass = Prototypes.PrototypeClasses.Single(pc => pc.Name == prototype);

    while (prototypeClass != typeof(object))
    {
        if (!prototypesByClass.TryAdd(prototypeClass, [name]))
        {
            prototypesByClass[prototypeClass].Add(name);
        }
        prototypeClass = prototypeClass.BaseType!;
    }
    var relativePath = Path.GetRelativePath(Path.GetDirectoryName(prototypeFile)!, $"Schemas/{prototype}.json");
    string schemaProperty = $"""{prototypeFile.ToLower().Replace("_", "_3").Replace("\\", "_4").Replace(".", "_1")}__JsonSchema="{relativePath}" """;
    schemaProperties += schemaProperty;
}

// ==== ==== ==== ==== PART 2 ==== ==== ==== ====
// create a set the file schemas in visual studio

// <ProjectExtensions><VisualStudio><UserProperties prototypes_4small_3ship_1json__JsonSchema="..\Schemas\StructurePrototype.schema.json" prototypes_4structures_4shipyards_4large_3assembly_3bay_1json__JsonSchema="..\..\..\Schemas\AssemblyBayPrototype.json" prototypes_4structures_4shipyards_4manufactory_1json__JsonSchema="..\..\..\Schemas\ManufactoryPrototype.schema.json" /></VisualStudio></ProjectExtensions>
string propsFile = $"""
<ProjectExtensions>
    <VisualStudio>
        <UserProperties {schemaProperties}/>
    </VisualStudio>
</ProjectExtensions>
""";

string userFilePath = "../../../../SpaceGameAgain/SpaceGameAgain.csproj";

var userFile = new XmlDocument();
userFile.Load(userFilePath);
if (userFile.DocumentElement?.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.Name == "ProjectExtensions") is XmlNode oldExtensions)
{
    userFile.DocumentElement.RemoveChild(oldExtensions);
    userFile.DocumentElement.InnerXml += propsFile;
    userFile.Save(userFilePath);
}

// ==== ==== ==== ==== PART 3 ==== ==== ==== ====
// generate actual schemas

Dictionary<string, JsonNode> prototypeDefs = []; 
Dictionary<string, JsonNode> valueDefs = []; 

JsonSerializerOptions options = new()
{
    ReadCommentHandling = JsonCommentHandling.Skip,
    PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
    TypeInfoResolver = new DefaultJsonTypeInfoResolver(),
    WriteIndented = true,
};

foreach (var prototypeClass in Prototypes.PrototypeClasses)
{
    JsonSchemaExporterOptions exporterOptions = new()
    {
        TransformSchemaNode = TransformNode,
    };

    // prototypeDefs.Add(prototypeClass.Name, (Prototype)Activator.CreateInstance(prototypeClass)!);
    prototypesByClass.TryGetValue(prototypeClass, out List<string>? values);

    JsonNode schema = JsonSchemaExporter.GetJsonSchemaAsNode(options, prototypeClass, exporterOptions);

    string schemaString = JsonSerializer.Serialize(schema, options);
    string file = $"{outputDirectory}{prototypeClass.Name}.json";
    File.WriteAllText(file, schemaString);

    static JsonNode TransformNode(JsonSchemaExporterContext context, JsonNode schema)
    {
        if (schema is JsonObject refObj && refObj.TryGetPropertyValue("$ref", out var r))
        {
            refObj["$ref"] = $"#/$defs/{context.TypeInfo.Type.Name}{((string)refObj["$ref"]!).TrimStart('#')}";
            Console.WriteLine(schema.ToJsonString());
        }

        if (context.TypeInfo.Type.IsSubclassOf(typeof(Prototype)))
        {
            if (!context.Path.IsEmpty)
            {
                return new JsonObject([
                    new ("oneOf", new JsonArray([
                                new JsonObject([new("$ref", $"{context.TypeInfo.Type.Name}.json")]),
                                // new JsonObject([new("$ref", $"#/$defs/{context.TypeInfo.Type.Name}_values")])
                            ]))
                    ]);
            }
            else
            {
                var obj = schema.AsObject();
                obj.Add("prototype", new JsonObject([new("const", context.TypeInfo.Type.Name)]));
                return obj;
            }
        }

        return schema;
    }
}

JsonObject finalSchema = new([
    new("$defs", new JsonObject(prototypeDefs!))
    ]);

// string schemaString = JsonSerializer.Serialize(finalSchema, options);
// string file = $"{outputDirectory}prototypes.schema.json";
// File.WriteAllText(file, schemaString);

Console.WriteLine("done!");

