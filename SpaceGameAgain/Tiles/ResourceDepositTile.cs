using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Tiles;
internal class ResourceDepositTile : Tile
{
    public int ResourceCount;

    public override ResourceDepositTilePrototype Prototype => (ResourceDepositTilePrototype)base.Prototype;

    public ResourceDepositTile(WorldActorPrototype prototype, ulong id, Transform transform, int resourceCount) : base(prototype, id, transform)
    {
        ResourceCount = resourceCount;
    }

    public override void Serialize(BinaryWriter writer)
    {
        base.Serialize(writer);
        writer.Write(ResourceCount);
    }
}

class ResourceDepositTilePrototype : TilePrototype
{
    public string Resource { get; set; }

    public override WorldActor Deserialize(BinaryReader reader)
    {
        ulong id = reader.ReadUInt64();
        int resourceCount = reader.ReadInt32();
        return new ResourceDepositTile(this, id, Transform.Default, resourceCount);
    }
}

