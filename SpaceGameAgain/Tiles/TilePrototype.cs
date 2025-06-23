using Newtonsoft.Json;
using SpaceGame.Data;
using SpaceGame.Data.Converters;
using SpaceGame.Economy;
using SpaceGame.Planets;
using SpaceGame.Rendering;
using SpaceGame.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Tiles;

class TilePrototype : DataPrototype
{
    public bool BlocksStructures { get; set; }
    [JsonConverter(typeof(NullableColorFConverter))]
    public ColorF? Color { get; set; }
    public ResourcePrototype? Resource { get; set; }
    public BackgroundMaterial? Material { get; set; } = null;

    public override void InitializePrototype()
    {
        base.InitializePrototype();
    }

    public void RenderTile(ICanvas canvas, Grid grid, GridCell cell, HexCoordinate coord)
    {
        if (Material != null)
        {
        }
        else if (Color != null)
        {
            canvas.Fill(Color!.Value);
            canvas.DrawPolygon(Grid.hexagon);
        }
    }
}