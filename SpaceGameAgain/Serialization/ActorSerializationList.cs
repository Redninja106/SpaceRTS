using SpaceGame.Serialization.Serializers;

namespace SpaceGame.Serialization;

class ActorSerializationList
{
    private IReadOnlyList<Actor> actors;
    private Prototype prototype;
    private FieldSerializer fieldSerializer;

    public ActorSerializationList(Prototype prototype, IReadOnlyList<Actor> actors)
    {
        this.prototype = prototype;
        this.actors = actors;
        this.fieldSerializer = new(prototype.ActorType);
    }

    public void SerializeIDs(BinaryWriter writer)
    {
        writer.Write(prototype.Name);
        writer.Write(actors.Count);

        for (int i = 0; i < actors.Count; i++)
        {
            writer.Write(actors[i].ID);
        }
    }

    public void SerializeFields(BinaryWriter writer)
    {
        for (int i = 0; i < actors.Count; i++)
        {
            fieldSerializer.Serialize(writer, actors[i]);
        }
    }
}
