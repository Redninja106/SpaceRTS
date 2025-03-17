using SpaceGame.Economy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Tiles;
internal class ResourceDepositTile : Tile
{
    public override ResourceDepositTilePrototype Prototype => (ResourceDepositTilePrototype)base.Prototype;

    public ResourceDepositTile(ResourceDepositTilePrototype prototype, ulong id, Transform transform) : base(prototype, id, transform)
    {
    }

    public override void Serialize(BinaryWriter writer)
    {
        base.Serialize(writer);
    }
}

class ResourceDepositTilePrototype : TilePrototype
{
    public ResourcePrototype Resource { get; set; }

    public override WorldActor Deserialize(BinaryReader reader)
    {
        ulong id = reader.ReadUInt64();
        return new ResourceDepositTile(this, id, Transform.Default);
    }
}

