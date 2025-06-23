using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace SpaceGame.Data.Converters;

class Vector2Converter : JsonConverter<Vector2>
{
    public override Vector2 ReadJson(JsonReader reader, Type objectType, Vector2 existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        JArray array = JArray.Load(reader);
        float x = (float)array[0];
        float y = (float)array[1];
        return new(x, y);
    }


    public override void WriteJson(JsonWriter writer, Vector2 value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
