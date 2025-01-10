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
    public Tile Tile { get; set; }

    public GridCell(Tile tile)
    {
        this.Tile = tile;
    }
}
