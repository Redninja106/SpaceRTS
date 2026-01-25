using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpaceGame.Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Data.Converters;
internal class RandomNumberConverter : JsonConverter<RandomNumber>
{
    public override RandomNumber ReadJson(JsonReader reader, Type objectType, RandomNumber? existingValue, bool hasExistingValue, JsonSerializer serializer)
    {
        if (reader.TokenType == JsonToken.Integer)
        {
            double value = Convert.ToDouble(reader.Value!);
            return new RandomNumber() { Minimum = value, Maximum = value };
        }
        if (reader.TokenType == JsonToken.StartObject)
        {
            JObject obj = JObject.Load(reader);
            return new RandomNumber()
            {
                Minimum = ((double)obj.GetValue("minimum")!),
                Maximum = ((double)obj.GetValue("maximum")!),
            };
        }

        throw new NotSupportedException("Error reading RandomInt! expected object or int value");
    }

    public override void WriteJson(JsonWriter writer, RandomNumber? value, JsonSerializer serializer)
    {
        throw new NotSupportedException();
    }
}
