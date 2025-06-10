using System.Text.Json;
using System.Text.Json.Serialization;

internal class ColorFConverter : JsonConverter<ColorF>
{
    public override ColorF Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            reader.Read();
            float r = reader.GetSingle();
            reader.Read();
            float g = reader.GetSingle();
            reader.Read();
            float b = reader.GetSingle();
            reader.Read();
            float a = 1.0f;
            if (reader.TokenType != JsonTokenType.EndArray)
            {
                a = reader.GetSingle();
                reader.Read();
            }
            return new ColorF(r, g, b, a);
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            string value = reader.GetString()!;

            if (Color.TryParse(value, out Color result))
            {
                return result.ToColorF();
            }
        }

        throw new Exception("invalid color");
    }

    public override void Write(Utf8JsonWriter writer, ColorF value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}