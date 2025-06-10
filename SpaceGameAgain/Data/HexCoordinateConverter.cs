using SpaceGame.Structures;
using System.Text.Json;
using System.Text.Json.Serialization;

class HexCoordinateConverter : JsonConverter<HexCoordinate>
{
    public override HexCoordinate Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            reader.Read();
            int q = reader.GetInt32();
            reader.Read();
            int r = reader.GetInt32();
            reader.Read();
            return new HexCoordinate(q, r);
        }

        throw new();
    }

    public override void Write(Utf8JsonWriter writer, HexCoordinate value, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }
}
