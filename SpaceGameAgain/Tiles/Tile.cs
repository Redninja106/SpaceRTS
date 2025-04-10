using SpaceGame.Economy;
using SpaceGame.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpaceGame.Tiles;
internal class Tile : Actor
{
    public Tile(TilePrototype prototype) : base(prototype)
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
    }
}

class TilePrototype : Prototype
{
    public bool BlocksStructures { get; set; }
    public string? Color { get; set; }
    public ResourcePrototype? Resource { get; set; }

    public override Tile Deserialize(BinaryReader reader)
    {
        // ulong id = reader.ReadUInt64();
        return new Tile(this);
    }
}