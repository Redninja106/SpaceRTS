using SpaceGame;
using SpaceGame.Structures;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Reflection;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

static class Prototypes
{
    private static Dictionary<string, Prototype> prototypes = [];
    private static Dictionary<string, Type> prototypeTypes = [];

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
        Dictionary<string, PrototypeFile> files = [];

        foreach (var fileName in fileNames)
        {
            PrototypeFile file = new PrototypeFile(fileName);
            files.Add(file.PrototypeName, file);
        }

        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            AllowTrailingCommas = true,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
            ReadCommentHandling = JsonCommentHandling.Skip,
        };
        options.Converters.Add(new HexCoordinateConverter());
        options.Converters.Add(new PrototypeConverter(files));

        options = JsonPopulate.GetOptionsWithPopulateResolver(options);

        foreach (var (_, file) in files)
        {
            Prototype prototype = file.Load(options);
            prototype.InitializePrototype();
            prototypes.Add(file.PrototypeName, prototype);
        }
    }

    class PrototypeFile
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
            Console.Write("Loading " + PrototypeName + "...");

            CurrentPrototypeType = PrototypeType;
            Prototype instance = GetInstance();
            JsonPopulate.PopulateObjectWithPopulateResolver(document, PrototypeType, instance, options);
            
            Console.WriteLine("done");
            return instance;
        }
    }

    class HexCoordinateConverter : JsonConverter<HexCoordinate>
    {
        public override HexCoordinate Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (reader.TokenType == JsonTokenType.StartArray)
            {
                reader.Read();
                int q = reader.GetInt32();
                reader.Read();
                int r = reader.GetInt32();
                reader.Read();
                return new HexCoordinate(q, r);
            }

            throw new();
        }

        public override void Write(Utf8JsonWriter writer, HexCoordinate value, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }
    }

    class PrototypeConverter(Dictionary<string, PrototypeFile> files) : JsonConverter<Prototype>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            if (PrototypeFile.CurrentPrototypeType == typeToConvert)
            {
                PrototypeFile.CurrentPrototypeType = null;
                return false;
            }

            return base.CanConvert(typeToConvert) || typeToConvert.IsSubclassOf(typeof(Prototype));
        }

        public override Prototype? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return files[reader.GetString()!].GetInstance();
        }

        public override void Write(Utf8JsonWriter writer, Prototype value, JsonSerializerOptions options)
        {
            throw new NotSupportedException();
        }

        public override Prototype ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            return Read(ref reader, typeToConvert, options)!;
        }
    }

    // WORKAROUND based off of https://github.com/dotnet/runtime/issues/78556
    static class JsonPopulate
    {
        public static void PopulateObjectWithPopulateResolver(JsonDocument document, Type returnType, object destination, JsonSerializerOptions options)
        {
            options = GetOptionsWithPopulateResolver(options);
            PopulateTypeInfoResolver.t_populateObject = destination;
            try
            {
                object? result = JsonSerializer.Deserialize(document, returnType, options);
                Debug.Assert(ReferenceEquals(result, destination));
            }
            finally
            {
                PopulateTypeInfoResolver.t_populateObject = null;
            }
        }

        public static JsonSerializerOptions GetOptionsWithPopulateResolver(JsonSerializerOptions options)
        {
            var populateResolverOptions = new JsonSerializerOptions(options)
            {
                TypeInfoResolver = new PopulateTypeInfoResolver(options.TypeInfoResolver ?? new DefaultJsonTypeInfoResolver())
            };

            return populateResolverOptions;
        }

        private class PopulateTypeInfoResolver : IJsonTypeInfoResolver
        {
            private readonly IJsonTypeInfoResolver _jsonTypeInfoResolver;
            [ThreadStatic]
            internal static object? t_populateObject;

            public PopulateTypeInfoResolver(IJsonTypeInfoResolver jsonTypeInfoResolver)
            {
                _jsonTypeInfoResolver = jsonTypeInfoResolver;
            }

            public JsonTypeInfo? GetTypeInfo(Type type, JsonSerializerOptions options)
            {
                var typeInfo = _jsonTypeInfoResolver.GetTypeInfo(type, options);
                if (typeInfo != null && typeInfo.Kind == JsonTypeInfoKind.Object && typeInfo.Type.IsSubclassOf(typeof(Prototype)))
                {
                    Func<object>? defaultCreateObjectDelegate = typeInfo.CreateObject;
                    typeInfo.CreateObject = () =>
                    {
                        object? result = t_populateObject;
                        if (result != null)
                        {
                            // clean up to prevent reuse in recursive scenaria
                            t_populateObject = null;
                        }
                        else
                        {
                            // fall back to the default delegate
                            result = defaultCreateObjectDelegate?.Invoke();
                        }

                        return result!;
                    };
                }

                return typeInfo;
            }
        }
    }
}
