using SpaceGame.Data;
using SpaceGame.Extensions;
using System.Text.Json.Serialization;
using System.Transactions;

namespace SpaceGame;

public abstract class Prototype
{
    [JsonInclude]
    public string Name { get; private set; }

    [JsonIgnore]
    public abstract Type ActorType { get; }

    // public abstract PrototypeObject Deserialize(BinaryReader reader);

    public virtual Actor CreateActor(ulong id)
    {
        return (Actor)Activator.CreateInstance(this.ActorType, [this, id])!;
    }

    public virtual void InitializePrototype()
    {
    }

    public override string ToString()
    {
        return $"{Name} ({GetType().Name})";
    }

    [DebugButton]
    public void Reload()
    {
        Prototypes.ReloadPrototype(this);
    }

    //public void DeserializeArgs(BinaryReader reader, out ulong id, out Transform transform)
    //{
    //    id = reader.ReadUInt64();
    //    transform = reader.ReadTransform();
    //}

    // public abstract override Actor Deserialize(BinaryReader reader);

}
