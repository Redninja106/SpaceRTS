using SpaceGame.Serialization.Serializers;

namespace SpaceGame.Serialization;

class ActorDeserializationList
{
    public Prototype Prototype;
    public int ActorCount;
    public Actor[] actors;
    private FieldSerializer fieldSerializer;

    public ActorDeserializationList(BinaryReader reader, GameWorld world)
    {
        this.Prototype = Prototypes.Get(reader.ReadString());
        this.ActorCount = reader.ReadInt32();
        this.actors = new Actor[this.ActorCount];

        for (int i = 0; i < actors.Length; i++)
        {
            ulong id = reader.ReadUInt64();
            Actor actor = Prototype.CreateActor(world, id);

            world.Add(actor, true);
            actors[i] = actor;
        }

        fieldSerializer = new(Prototype.ActorType);
    }

    public void Deserialize(BinaryReader reader)
    {
        for (int i = 0; i < actors.Length; i++)
        {
            fieldSerializer.Deserialize(reader, actors[i]);
        }
    }

    public void FinishDeserialization()
    {
        for (int i = 0; i < actors.Length; i++)
        {
            actors[i].FinishDeserialization();
            // actors[i].InitializeActor();
        }
    }
}
