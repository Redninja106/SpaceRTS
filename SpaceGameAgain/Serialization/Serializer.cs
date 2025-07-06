namespace SpaceGame.Serialization;

abstract class Serializer
{
    public abstract void Serialize(BinaryWriter writer, object value);
    public abstract object? Deserialize(BinaryReader reader);
}
