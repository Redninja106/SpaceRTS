using SpaceGame.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;
internal abstract class StructureBehavior
{
    public StructureInstance Instance { get; }

    public abstract Element[] SelectGUI { get; }

    public abstract void Update();

    public virtual void RenderCell(ICanvas canvas, HexCoordinate localCell)
    {
    }

    public virtual void RenderBeforeCells(ICanvas canvas)
    {

    }

    public virtual void RenderAfterCells(ICanvas canvas)
    {

    }

    public virtual void RenderCellShadow(ICanvas canvas, Vector2 offset, HexCoordinate cell)
    {
    }

    public StructureBehavior(StructureInstance instance)
    {
        this.Instance = instance;
    }

}
