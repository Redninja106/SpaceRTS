using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SpaceGame.Data.Converters;

internal class ColorFConverter : JsonConverter<ColorF>
{
    public override void WriteJson(JsonWriter writer, ColorF value, JsonSerializer serializer)
    {
        throw new NotSupportedException();
    }

    public override ColorF ReadJson(JsonReader reader, Type objectType, ColorF existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.String)
        {
            string value = (string)reader.Value!;

            if (Color.TryParse(value, out Color result))
            {
                return result.ToColorF();
            }
        }
        else if (reader.TokenType == JsonToken.StartArray)
        {
            JArray array = JArray.Load(reader);
            float r = (float)array[0];
            float g = (float)array[1];
            float b = (float)array[2];
            float a = 1.0f;
            if (array.Count > 3)
            {
                a = (float)array[3];
            }
            return new ColorF(r, g, b, a);
        }

        throw new Exception("invalid color");
    }
}

//class NullableConverter : JsonConverter
//{
//    public override bool CanConvert(Type objectType)
//    {
//        return objectType.IsConstructedGenericType && objectType.GetGenericTypeDefinition() == typeof(Nullable<>);
//    }

//    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
//    {
//        Type type = objectType.GetGenericArguments()[0];
//        object baseObject = serializer.Deserialize(reader, type)!;
//        object nullable = Activator.CreateInstance(objectType)!;

//        nullable.GetType().GetField("value")!.SetValue(nullable, baseObject);
//        return nullable;
//    }

//    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
//    {
//        throw new NotImplementedException();
//    }
//}

internal class NullableColorFConverter : JsonConverter<ColorF?>
{
    public override void WriteJson(JsonWriter writer, ColorF? value, JsonSerializer serializer)
    {
        throw new NotSupportedException();
    }

    public override ColorF? ReadJson(JsonReader reader, Type objectType, ColorF? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.String)
        {
            string value = (string)reader.Value!;

            if (Color.TryParse(value, out Color result))
            {
                return result.ToColorF();
            }
        }
        else if (reader.TokenType == JsonToken.StartArray)
        {
            JArray array = JArray.Load(reader);
            float r = (float)array[0];
            float g = (float)array[1];
            float b = (float)array[2];
            float a = 1.0f;
            if (array.Count > 3)
            {
                a = (float)array[3];
            }
            return new ColorF(r, g, b, a);
        }

        throw new Exception("invalid color");
    }
}

