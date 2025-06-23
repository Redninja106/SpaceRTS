using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SpaceGame.Data.Converters;

internal class ColorConverter : JsonConverter<Color>
{
    public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
    {
        throw new NotSupportedException();
    }

    public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.String)
        {
            string value = (string)reader.Value!;

            if (Color.TryParse(value, out Color result))
            {
                return result;
            }
        }
        else if (reader.TokenType == JsonToken.StartArray)
        {
            JArray array = JArray.Load(reader);
            byte r = (byte)array[0];
            byte g = (byte)array[1];
            byte b = (byte)array[2];
            byte a = 255;
            if (array.Count > 3)
            {
                a = (byte)array[3];
            }
            return new Color(r, g, b, a);
        }

        throw new Exception("invalid color");
    }
}
