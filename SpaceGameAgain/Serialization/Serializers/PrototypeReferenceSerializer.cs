namespace SpaceGame.Serialization.Serializers;

class PrototypeReferenceSerializer : Serializer
{
    public override object Deserialize(BinaryReader reader)
    {
        string name = reader.ReadString();
        return Prototypes.Get(name);
    }

    public override void Serialize(BinaryWriter writer, object value)
    {
        writer.Write(((Prototype)value).Name);
    }
}
