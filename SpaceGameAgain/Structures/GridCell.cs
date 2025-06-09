using SpaceGame.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;
internal class GridCell
{
    public ActorReference<Structure> Structure { get; set; }
    public TilePrototype Tile { get; set; }
    public ColorF Tint { get; set; }

    public GridCell(TilePrototype tile)
    {
        this.Tile = tile;
    }
}
