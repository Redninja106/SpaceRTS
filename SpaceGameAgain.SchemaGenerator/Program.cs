using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using SpaceGame.Data;
using SpaceGameAgain.SchemaGenerator;
using System.Xml;
using System.Xml.Linq;

string outputDirectory = "../../../../SpaceGameAgain/Schemas/";

// ==== ==== ==== ==== PART 1 ==== ==== ==== ====
// search prototype instances to make autocomplete enums

string[] prototypeFileNames = Prototypes.FindPrototypeFiles(true);
Dictionary<Type, List<string>> prototypesByClass = [];
string schemaProperties = "";

foreach (var prototypeFile in prototypeFileNames)
{
    JObject doc = JObject.Parse(File.ReadAllText(prototypeFile), new JsonLoadSettings() 
    {
        CommentHandling = CommentHandling.Ignore,
    });

    string prototype = (string?)doc.GetValue("prototype") ?? throw new("prototype has no type");
    string name = (string?)doc.GetValue("name") ?? throw new("prototype has no name");

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

Dictionary<string, JObject> prototypeDefs = []; 
Dictionary<string, JObject> valueDefs = [];

SchemaGenerator schemaGenerator = new(prototypesByClass);

foreach (var prototypeClass in Prototypes.PrototypeClasses)
{
    JObject schema = schemaGenerator.GenerateSchema(prototypeClass);
    string schemaText = schema.ToString();
    string file = $"{outputDirectory}{prototypeClass.Name}.json";
    File.WriteAllText(file, schemaText);

}

Console.WriteLine("done!");
