using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SpaceGame.Structures;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;

namespace SpaceGame.Data.Converters;

class HexCoordinateConverter : Newtonsoft.Json.JsonConverter<HexCoordinate>
{
    public JsonNode GetSchema()
    {
        return JsonNode.Parse($$"""
            {
                "type": "array",
                "prefixItems": [
                    { "type": "integer" },
                    { "type": "integer" }
                ],
                "items": false
            }
            """)!;
    }

    public override HexCoordinate ReadJson(JsonReader reader, Type objectType, HexCoordinate existingValue, bool hasExistingValue, Newtonsoft.Json.JsonSerializer serializer)
    {
        var array = JArray.Load(reader);
        int q = (int)array[0];
        int r = (int)array[1];
        return new HexCoordinate(q, r);
    }


    public override void WriteJson(JsonWriter writer, HexCoordinate value, Newtonsoft.Json.JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
