using SpaceGame.Data;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Schema;
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

    string schemaProperty = $"""{prototypeFile.ToLower().Replace("_", "_3").Replace("\\", "_4").Replace(".", "_1")}__JsonSchema="..\Schemas\{prototype}.schema.json" """;
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

foreach (var prototypeClass in Prototypes.PrototypeClasses)
{
    Prototypes.PrototypeFile.CurrentPrototypeType = prototypeClass;
    JsonSerializerOptions options = Prototypes.CreateJsonOptions();

    JsonSchemaExporterOptions exporterOptions = new()
    {
        TransformSchemaNode = (JsonSchemaExporterContext context, JsonNode schema) =>
        {
            if (!context.Path.IsEmpty && context.TypeInfo.Type.IsSubclassOf(typeof(Prototype)))
            {
                if (prototypesByClass.TryGetValue(context.TypeInfo.Type, out var values))
                {
                    return new JsonObject([
                        new("enum", new JsonArray(values.Select(n => JsonValue.Create(n)).ToArray()))
                        ]);
                }
            }

            return schema;
        }
    };

    JsonNode schema = JsonSchemaExporter.GetJsonSchemaAsNode(options, prototypeClass, exporterOptions);
    string schemaString = JsonSerializer.Serialize(schema);

    string file = $"{outputDirectory}{prototypeClass.Name}.schema.json";
    File.WriteAllText(file, schemaString);
}
