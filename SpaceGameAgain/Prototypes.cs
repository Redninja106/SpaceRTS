using SpaceGame;
using SpaceGame.Structures;
using System.Drawing;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Text.Json.Serialization;

static class Prototypes
{
    private static Dictionary<string, Prototype> prototypes = [];


    public static IEnumerable<Prototype> RegisteredPrototypes => prototypes.Values;

    public static Prototype Get(string name)
    {
        return prototypes[name];
    }
    public static TPrototype Get<TPrototype>(string name) where TPrototype : Prototype
    {
        return (TPrototype)Get(name);
    }

    public static void Load()
    {
        Dictionary<string, Type> prototypeTypes = [];
        foreach (var type in Assembly.GetExecutingAssembly().DefinedTypes)
        {
            if (type.IsSubclassOf(typeof(Prototype)))
            {
                prototypeTypes.Add(type.Name, type);
            }
        }

        JsonSerializerOptions options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
            AllowTrailingCommas = true,
            UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
            ReadCommentHandling = JsonCommentHandling.Skip
        };
        options.Converters.Add(new HexCoordinateConverter());

        JsonDocumentOptions docOptions = new()
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip,
        };

        foreach (var file in Directory.GetFiles("Assets/Prototypes", "*", SearchOption.AllDirectories))
        {
            try
            {
                JsonDocument document = JsonDocument.Parse(File.ReadAllText(file), docOptions);

                Type? prototypeType = null;
                string? prototypeName = null;
                foreach (var property in document.RootElement.EnumerateObject())
                {
                    if (property.Name == "prototype")
                    {
                        prototypeType = prototypeTypes[property.Value.GetString()!];
                    }

                    if (property.Name == "name")
                    {
                        prototypeName = property.Value.GetString()!;
                    }
                }

                if (prototypeType is null)
                {
                    throw new($"could not find prototype type");
                }

                if (prototypeType is null)
                {
                    throw new($"could not find prototype name for");
                }

                Prototype? prototype = (Prototype?)document.Deserialize(prototypeType, options);
                if (prototype is null)
                {
                    throw new($"could not deserialize");
                }
                prototypes.Add(prototypeName, prototype);
            }
            catch (Exception ex)
            {

                Console.WriteLine(file + ": " + ex.Message);
            }
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
}