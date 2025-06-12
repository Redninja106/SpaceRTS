using SpaceGame.Data;
using SpaceGame.Extensions;
using System.Transactions;

namespace SpaceGame;

abstract class ActorPrototype : Prototype
{
    public void DeserializeArgs(BinaryReader reader, out ulong id, out Transform transform)
    {
        id = reader.ReadUInt64();
        transform = reader.ReadTransform();
    }

    public abstract override Actor Deserialize(BinaryReader reader);
}
