using System.Text.Json;
using System.Text.Json.Serialization;

class PrototypeConverter(Dictionary<string, Prototypes.PrototypeFile> files) : JsonConverter<Prototype>
{
    public override bool HandleNull => true;

    public override bool CanConvert(Type typeToConvert)
    {
        if (Prototypes.PrototypeFile.CurrentPrototypeType == typeToConvert)
        {
            Prototypes.PrototypeFile.CurrentPrototypeType = null;
            return false;
        }

        return base.CanConvert(typeToConvert) || typeToConvert.IsSubclassOf(typeof(Prototype));
    }

    public override Prototype? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string value = reader.GetString()!;

        if (value == "null")
        {
            return null;
        }

        return files[value].GetInstance();
    }

    public override void Write(Utf8JsonWriter writer, Prototype value, JsonSerializerOptions options)
    {
        throw new NotSupportedException();
    }

    public override Prototype ReadAsPropertyName(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return Read(ref reader, typeToConvert, options)!;
    }
}
