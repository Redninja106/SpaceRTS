using SpaceGame.Ships.Fleets;
using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

class PrototypeReferenceConverter2 : Newtonsoft.Json.JsonConverter
{
    public override bool CanWrite => false;
    
    public override bool CanConvert(Type objectType)
    {
        return objectType.IsSubclassOf(typeof(Prototype));
    }

    public override object? ReadJson(Newtonsoft.Json.JsonReader reader, Type objectType, object? existingValue, Newtonsoft.Json.JsonSerializer serializer)
    {
        string name = reader.ReadAsString() ?? throw new Exception("expected string!");
        if (name == "null")
        {
            return null;
        }
        return Prototypes.Get(name);
    }

    public override void WriteJson(Newtonsoft.Json.JsonWriter writer, object? value, Newtonsoft.Json.JsonSerializer serializer)
    {
        throw new NotSupportedException();
    }
}
