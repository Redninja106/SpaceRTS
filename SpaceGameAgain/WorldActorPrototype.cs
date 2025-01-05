using System.Transactions;

namespace SpaceGame;

abstract class WorldActorPrototype : Prototype
{
    public void DeserializeArgs(BinaryReader reader, out ulong id, out Transform transform)
    {
        id = reader.ReadUInt64();
        transform = reader.ReadTransform();
    }

    public abstract override WorldActor Deserialize(BinaryReader reader);
}
