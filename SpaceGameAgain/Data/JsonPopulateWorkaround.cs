using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
// WORKAROUND based off of https://github.com/dotnet/runtime/issues/78556
static class JsonPopulateWorkaround
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