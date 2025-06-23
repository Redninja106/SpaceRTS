using Newtonsoft.Json;
using SpaceGame.Extensions;
using System.Transactions;

namespace SpaceGame.Data;

public abstract class Prototype
{
    public string? Name { get; set; }

    [JsonIgnore]
    public bool IsAnonymous { get; set; }

    [JsonIgnore, DebugIgnore]
    public abstract Type ActorType { get; }

    // public abstract PrototypeObject Deserialize(BinaryReader reader);

    public virtual Actor CreateActor(GameWorld world, ulong id)
    {
        return (Actor)Activator.CreateInstance(ActorType, [this, world, id])!;
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
        if (Name != null)
        {
            Prototypes.ReloadPrototype(this);
        }
    }

    //public void DeserializeArgs(BinaryReader reader, out ulong id, out Transform transform)
    //{
    //    id = reader.ReadUInt64();
    //    transform = reader.ReadTransform();
    //}

    // public abstract override Actor Deserialize(BinaryReader reader);

}
