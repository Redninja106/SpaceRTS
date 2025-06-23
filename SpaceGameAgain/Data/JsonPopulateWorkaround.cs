using System.Diagnostics;
using System.Reflection.Metadata;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

class PrototypePopulateWrapper
{
    internal static Prototype? populateInstance;

    [JsonConverter(typeof(PopulatePrototypeConverter))]
    public required Prototype Prototype { get; set; }

    public PrototypePopulateWrapper()
    {
        Console.WriteLine("h");
    }

    public static string WrapText(string text) => $$"""{"prototype":{{text}}}""";
}

class PopulatePrototypeConverter : JsonConverter<Prototype>
{
    public override Prototype? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (PrototypePopulateWrapper.populateInstance != null)
        {
            Prototype result = PrototypePopulateWrapper.populateInstance;
            PrototypePopulateWrapper.populateInstance = null;
            JsonPopulateWorkaround.PopulateObjectWithPopulateResolver(ref reader, result.GetType(), result, options);

            return result;
        }
        throw new();
    }

    public override void Write(Utf8JsonWriter writer, Prototype value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}

// WORKAROUND based off of https://github.com/dotnet/runtime/issues/78556
static class JsonPopulateWorkaround
{
    public static void PopulateObjectWithPopulateResolver(ref Utf8JsonReader reader, Type returnType, object destination, JsonSerializerOptions options)
    {

        options = GetOptionsWithPopulateResolver(options);
        PopulateTypeInfoResolver.t_populateObject = destination;
        try
        {
            object? result = JsonSerializer.Deserialize(ref reader, returnType, options);
            Debug.Assert(ReferenceEquals(result, destination));
        }
        finally
        {
            PopulateTypeInfoResolver.t_populateObject = null;
        }
    }

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