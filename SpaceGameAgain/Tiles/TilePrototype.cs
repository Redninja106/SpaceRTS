using SpaceGame.Economy;
using SpaceGame.Planets;
using SpaceGame.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpaceGame.Tiles;

class TilePrototype : Prototype
{
    public bool BlocksStructures { get; set; }
    public ColorF? Color { get; set; } = null;
    public ResourcePrototype? Resource { get; set; }
    public BackgroundMaterial? Material { get; set; } = null;

    public override void InitializePrototype()
    {
        base.InitializePrototype();
    }

    public override Actor Deserialize(BinaryReader reader)
    {
        throw new NotSupportedException();
    }

    public void RenderTile(ICanvas canvas, Grid grid, GridCell cell, HexCoordinate coord)
    {
        if (Material != null)
        {
        }
        else if (Color != null)
        {
            canvas.Fill(Color.Value);
            canvas.DrawPolygon(Grid.hexagon);
        }
    }
}