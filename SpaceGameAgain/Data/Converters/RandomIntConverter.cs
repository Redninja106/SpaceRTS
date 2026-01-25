using Newtonsoft.Json;
using SpaceGame.Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Data.Converters;
internal class RandomIntConverter : JsonConverter<RandomInt>
{
    public override RandomInt ReadJson(JsonReader reader, Type objectType, RandomInt? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Integer)
        {
            int value = reader.ReadAsInt32()!.Value;
            return new RandomInt() { Minimum = value, Maximum = value };
        }
        if (reader.TokenType == JsonToken.StartObject)
        {
            return new RandomInt()
            {
                Minimum = reader.ReadAsInt32()!.Value,
                Maximum = reader.ReadAsInt32()!.Value,
            };
        }

        throw new NotSupportedException("Error reading RandomInt! expected object or int value");
    }

    public override void WriteJson(JsonWriter writer, RandomInt? value, JsonSerializer serializer)
    {
        throw new NotSupportedException();
    }
}
