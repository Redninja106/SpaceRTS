using SpaceGame.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpaceGame.Tiles;
internal class Tile : WorldActor
{
    public Tile(WorldActorPrototype prototype, ulong id, Transform transform) : base(prototype, id, transform)
    {
    }

    public override TilePrototype Prototype => (TilePrototype)base.Prototype;

    public void RenderTile(ICanvas canvas, Grid grid, HexCoordinate coord)
    {
        if (Prototype.Color != null)
        {
            canvas.Fill(SimulationFramework.Color.Parse(Prototype.Color));
            canvas.DrawPolygon(Grid.hexagon);
        }
    }

    public override void Serialize(BinaryWriter writer)
    {
        writer.Write(ID);
    }
}

class TilePrototype : WorldActorPrototype
{
    public bool BlocksStructures { get; set; }
    public string? Color { get; set; }

    public override WorldActor Deserialize(BinaryReader reader)
    {
        ulong id = reader.ReadUInt64();
        return new Tile(this, id, Transform.Default);
    }
}