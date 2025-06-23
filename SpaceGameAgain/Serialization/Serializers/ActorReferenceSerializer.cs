namespace SpaceGame.Serialization.Serializers;

class ActorReferenceSerializer : Serializer
{
    private readonly GameWorld world;

    public ActorReferenceSerializer(GameWorld world)
    {
        this.world = world;
    }

    public override Actor Deserialize(BinaryReader reader)
    {
        ulong id = reader.ReadUInt64();
        if (id == 0)
            return null;
        return world.Actors[id];
    }

    public override void Serialize(BinaryWriter writer, object value)
    {
        writer.Write((value as Actor)?.ID ?? 0);
    }
}
