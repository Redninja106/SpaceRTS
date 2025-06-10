using System.Text.Json;
using System.Text.Json.Serialization;

internal class ColorConverter : JsonConverter<Color>
{
    public override Color Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            reader.Read();
            byte r = reader.GetByte();
            reader.Read();
            byte g = reader.GetByte();
            reader.Read();
            byte b = reader.GetByte();
            reader.Read();
            byte a = 255;
            if (reader.TokenType != JsonTokenType.EndArray)
            {
                a = reader.GetByte();
                reader.Read();
            }
            return new Color(r, g, b, a);
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            string value = reader.GetString()!;

            if (Color.TryParse(value, out Color result))
            {
                return result;
            }
        }

        throw new Exception("invalid color");
    }

    public override void Write(Utf8JsonWriter writer, Color value, JsonSerializerOptions options)
    {
        throw new NotImplementedException();
    }
}
