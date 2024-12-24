using System.Transactions;

namespace SpaceGame;

abstract class Prototype
{
    public string Name { get; set; }

    public abstract Actor? Deserialize(BinaryReader reader);

    public void DeserializeArgs(BinaryReader reader, out ulong id, out Transform transform)
    {
        id = reader.ReadUInt64();
        transform = reader.ReadTransform();
    }
}
