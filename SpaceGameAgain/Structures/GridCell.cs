using SpaceGame.Tiles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;

[Serializable]
internal class GridCell
{
    [field: Serialize]
    public Structure? Structure { get; set; }
    [field: Serialize]
    public TilePrototype Tile { get; set; }
    //[field: Serialize]
    public ColorF Tint { get; set; }

    public GridCell()
    {
    }
}
