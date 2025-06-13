namespace SpaceGame.Serialization.Serializers;

class ObjectSerializer : Serializer
{
    FieldSerializer fieldSerializer;

    public ObjectSerializer(Type objectType)
    {
        fieldSerializer = new(objectType);
    }

    public override void Serialize(BinaryWriter writer, object value)
    {
        fieldSerializer.Serialize(writer, value);
    }

    public override object Deserialize(BinaryReader reader)
    {
        object value = Activator.CreateInstance(fieldSerializer.ObjectType)!;
        fieldSerializer.Deserialize(reader, value);
        return value;
    }
}
