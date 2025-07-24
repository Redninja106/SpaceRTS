using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using SpaceGame.Data.Converters;
using SpaceGame.Debugging;
using SpaceGame.Rendering;
using SpaceGame.Structures;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization.Metadata;

namespace SpaceGame.Data;

public static class Prototypes
{
    private static Dictionary<string, Prototype> prototypes = [];
    private static Dictionary<string, Type> prototypeTypes = [];
    private static Dictionary<string, PrototypeFile> files = [];

    [DebugOverlay]
    private static bool LogPrototypeLoads = false;

    static Prototypes()
    {
        foreach (var type in Assembly.GetExecutingAssembly().DefinedTypes)
        {
            if (type.IsSubclassOf(typeof(Prototype)))
            {
                prototypeTypes.Add(type.Name, type);
            }
        }
    }

    public static IEnumerable<Prototype> RegisteredPrototypes => prototypes.Values;

    public static Type[] PrototypeClasses => prototypeTypes.Values.ToArray();

    public static Prototype Get(string name)
    {
        return prototypes[name];
    }
    public static TPrototype Get<TPrototype>(string name) where TPrototype : Prototype
    {
        return (TPrototype)Get(name);
    }

    public static TPrototype[] GetAll<TPrototype>() where TPrototype : Prototype
    {
        return prototypes.Values.OfType<TPrototype>().ToArray();
    }

    public static string GetPrototypeDirectory(string prototypeName)
    {
        return Path.GetDirectoryName(files[prototypeName].PrototypePath)! + "\\";
    }

    public static string[] FindPrototypeFiles(bool includeAssets)
    {
        string[] prototypeFiles = Directory.GetFiles("Prototypes", "*.json", SearchOption.AllDirectories);
        string[] assetFiles = includeAssets ? Directory.GetFiles("Assets", "*.json", SearchOption.AllDirectories) : [];
        return [.. prototypeFiles, .. assetFiles];
    }

    public static void Load(bool loadAssets = true)
    {
        string[] fileNames = FindPrototypeFiles(loadAssets);

        foreach (var fileName in fileNames)
        {
            PrototypeFile file = new(fileName);
            files.Add(file.PrototypeName, file);
        }

        foreach (var (_, file) in files)
        {
            prototypes.Add(file.PrototypeName, file.GetInstance());
        }

        foreach (var (_, file) in files)
        {
            file.Load();
        }

        foreach (var prototype in prototypes)
        {
            DebugLog.Assert(loadAssets || prototype.Value is not AssetPrototype);

            prototype.Value.InitializePrototype();
        }

        DebugLog.Message($"Loaded {prototypes.Count} prototypes...");
    }

    public static void ReloadPrototype(Prototype prototype)
    {
        PrototypeFile file = files.Single(f => f.Value.GetInstance() == prototype).Value;

        // var options = CreateJsonOptions();
        // file.Load(options);
        // prototype.InitializePrototype();
    }

    public class PrototypeFile
    {
        public string PrototypeName;
        public string PrototypePath;
        private string prototypeJson;
        public Type PrototypeType;

        private Prototype? prototypeInstance;
        public PrototypeFile(string file)
        {
            PrototypePath = file;
            prototypeJson = File.ReadAllText(file);
            JObject prototypeObject = JObject.Parse(prototypeJson);
            PrototypeType = prototypeTypes[(string?)prototypeObject.GetValue("prototype") ?? throw new("prototype type missing!")];
            PrototypeName = (string?)prototypeObject.GetValue("name") ?? throw new("prototype name missing!");
        }

        public Prototype GetInstance()
        {
            if (this.prototypeInstance == null)
            {
                this.prototypeInstance = (Prototype)Activator.CreateInstance(PrototypeType)!;
                // this.prototypeInstance.Name = PrototypeName;
            }
            return this.prototypeInstance;
        }

        public Prototype Load()
        {
            if (LogPrototypeLoads)
            {
                DebugLog.Message("Loading " + PrototypeName + "...");
            }

            IContractResolver contractResolver = new DerivedContractResolver()
            {
                NamingStrategy = new SnakeCaseNamingStrategy(true, false),
            };

            JsonSerializerSettings settings = new()
            {
                ContractResolver = contractResolver,
                Converters = [
                    new HexCoordinateConverter(),
                    new Vector2Converter(),
                    new ColorConverter(),
                    new ColorFConverter(),
                    new NullableColorFConverter(),
                    // new NullableConverter(),
                    new PrototypeConverter()
                    ]
            };

            Prototype instance = GetInstance();
            JsonConvert.PopulateObject(prototypeJson, instance, settings);

            //CurrentPrototypeType = PrototypeType;
            //Prototype instance = GetInstance();
            //PrototypePopulateWrapper.populateInstance = instance;
            //current = this;
            //PrototypePopulateWrapper wrapper = document.Deserialize<PrototypePopulateWrapper>(options);
            //current = null;
            ////JsonPopulateWorkaround.PopulateObjectWithPopulateResolver(document, PrototypeType, instance, options);

            return instance;
        }
    }

    class PrototypeConverter : JsonConverter
    {
        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            return objectType.IsSubclassOf(typeof(Prototype));
        }

        public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.String)
            {
                string name = (string?)reader.Value ?? throw new Exception("expected string!");
                if (name == "null")
                {
                    return null;
                }
                return Get(name);
            }
            else if (reader.TokenType == JsonToken.StartObject)
            {
                JObject obj = JObject.Load(reader);
                Type prototypeType;
                if (obj.TryGetValue("prototype", out JToken? value))
                {
                    string prototypeTypeName = (string)value!;
                    prototypeType = prototypeTypes[prototypeTypeName];
                }
                else if (objectType.IsSubclassOf(typeof(Prototype)))
                {
                    prototypeType = objectType;
                }
                else
                {
                    throw new();
                }
                Prototype prototype = (Prototype)Activator.CreateInstance(prototypeType)!;
                prototype.IsAnonymous = true;
                serializer.Populate(obj.CreateReader(), prototype);
                return prototype;
            }

            throw new Exception();
        }

        public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
        {
            throw new NotSupportedException();
        }
    }
}
