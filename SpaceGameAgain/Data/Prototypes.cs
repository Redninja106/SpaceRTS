using SpaceGame.Data;
using SpaceGame.Debugging;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

public static class Prototypes
{
    private static Dictionary<string, Prototype> prototypes = [];
    private static Dictionary<string, Type> prototypeTypes = [];
    private static Dictionary<string, PrototypeFile> files = [];

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

    public static void Load()
    {
        string[] fileNames = Directory.GetFiles("Prototypes", "*", SearchOption.AllDirectories);

        foreach (var fileName in fileNames)
        {
            PrototypeFile file = new(fileName);
            files.Add(file.PrototypeName, file);
        }

        var options = CreateJsonOptions();

        foreach (var (_, file) in files)
        {
            Prototype prototype = file.Load(options);
            prototypes.Add(file.PrototypeName, prototype);
        }

        foreach (var prototype in prototypes)
        {
            prototype.Value.InitializePrototype();
        }
    }

    public static JsonSerializerOptions CreateJsonOptions()
    {
        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            AllowTrailingCommas = true,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
            ReadCommentHandling = JsonCommentHandling.Skip,
        };
        options.Converters.Add(new HexCoordinateConverter());
        options.Converters.Add(new Vector2Converter());
        options.Converters.Add(new ColorConverter());
        options.Converters.Add(new ColorFConverter());
        options.Converters.Add(new PrototypeConverter(files));

        options = JsonPopulateWorkaround.GetOptionsWithPopulateResolver(options);

        return options;
    }

    public static void ReloadPrototype(Prototype prototype)
    {
        PrototypeFile file = files.Single(f => f.Value.GetInstance() == prototype).Value;

        var options = CreateJsonOptions();
        file.Load(options);
        prototype.InitializePrototype();
    }

    public class PrototypeFile
    {
        public static Type? CurrentPrototypeType;

        private JsonDocument document;
        public string PrototypeName;
        public Type PrototypeType;

        private Prototype? prototypeInstance;

        public PrototypeFile(string file)
        {
            JsonDocumentOptions options = new()
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip,
            };

            document = JsonDocument.Parse(File.ReadAllText(file), options);

            foreach (var property in document.RootElement.EnumerateObject())
            {
                if (property.Name == "prototype")
                {
                    PrototypeType = prototypeTypes[property.Value.GetString()!];
                }

                if (property.Name == "name")
                {
                    PrototypeName = property.Value.GetString()!;
                }
            }

            if (PrototypeType is null)
            {
                throw new($"prototype type missing");
            }

            if (PrototypeName is null)
            {
                throw new($"prototype name missing");
            }
        }

        public Prototype GetInstance()
        {
            if (this.prototypeInstance == null)
            {
                this.prototypeInstance = (Prototype)Activator.CreateInstance(PrototypeType)!;
            }
            return this.prototypeInstance;
        }

        public Prototype Load(JsonSerializerOptions options)
        {
            DebugLog.Message("Loading " + PrototypeName + "...");

            CurrentPrototypeType = PrototypeType;
            Prototype instance = GetInstance();
            JsonPopulateWorkaround.PopulateObjectWithPopulateResolver(document, PrototypeType, instance, options);

            return instance;
        }
    }

}
