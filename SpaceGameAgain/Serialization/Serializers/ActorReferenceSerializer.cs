namespace SpaceGame.Serialization.Serializers;

class ActorReferenceSerializer : Serializer
{
    public override Actor Deserialize(BinaryReader reader)
    {
        ulong id = reader.ReadUInt64();
        if (id == 0)
            return null;
        return World.Actors[id];
    }

    public override void Serialize(BinaryWriter writer, object value)
    {
        writer.Write((value as Actor)?.ID ?? 0);
    }
}
