using SpaceGame.Ships;
using SpaceGame.Ships.Orders;
using SpaceGame.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Interaction;
internal class ConstructionInteractionContext : IInteractionContext
{
    private StructurePrototype prototype;
    private Grid? hoveredGrid;
    private HexCoordinate hoveredLocation;
    // private int rotation;
    private HashSet<HexCoordinate> draggedCells = [];
    private Structure? appendInstance;

    public void Update(MouseState leftMouse, MouseState rightMouse)
    {
        if (rightMouse.Released && !rightMouse.Dragged)
        {
            World.CurrentInteractionContext = World.SelectInteractionContext;
            Reset();
            return;
        }

        // if (Keyboard.IsKeyPressed(Key.R))
        // {
        //     if (Keyboard.IsKeyDown(Key.LeftShift))
        //     {
        //         rotation--;
        //     }
        //     else
        //     {
        //         rotation++;
        //     }
        // }

        UpdateHoveredGrid();
        if (hoveredGrid != null)
        {
            hoveredLocation = HexCoordinate.FromCartesian(hoveredGrid.Transform.WorldToLocal(World.MousePosition));

            if (leftMouse.Holding)
            {
                if (hoveredGrid.IsStructureObstructed(prototype, hoveredLocation, 0))
                {
                    if (appendInstance == null && draggedCells.Count == 0)
                    {
                        var cell = hoveredGrid.GetCell(hoveredLocation);
                        if (cell?.Structure.Actor?.Prototype == prototype)
                        {
                            appendInstance = cell.Structure.Actor;
                        }
                    }
                }
                else
                {
                    bool isNeighbor = false;
                    for (int i = 0; i < 6; i++)
                    {
                        if (draggedCells.Contains(hoveredLocation + HexCoordinate.UnitQ.Rotated(i)))
                        {
                            isNeighbor = true;
                        }
                    }

                    if (isNeighbor || draggedCells.Count == 0)
                    {
                        draggedCells.Add(hoveredLocation);
                    }
                }
            }

            if (leftMouse.Released && draggedCells.Count > 0)
            {
                // make sure the player ends on an empty tile that exists, or the structure we're appending to
                var cell = hoveredGrid.GetCell(hoveredLocation);
                bool isOnAppend = appendInstance != null && cell.Structure.Actor == appendInstance;
                if (isOnAppend || (cell != null && cell.Structure.IsNull))
                {
                    if (appendInstance is null)
                    {
                        // placing fresh structure
                        HexCoordinate[] cells = draggedCells.ToArray();
                        HexCoordinate origin = cells[0];
                        for (int i = 0; i < cells.Length; i++)
                        {
                            cells[i] -= origin;
                        }

                        hoveredGrid.PlaceStructure(prototype, origin, 0, World.PlayerTeam.Actor!, [.. cells]);
                    }
                    else
                    {
                        // appending to an existing structure
                        foreach (var c in draggedCells)
                        {
                            appendInstance.Footprint!.Add(c - appendInstance.Location);
                            hoveredGrid.GetCell(c).Structure = appendInstance.AsReference();
                            // (appendInstance.Behavior as ZoneBehavior)?.OnFootprintChanged();
                        }
                        appendInstance.ComputeOutline();
                    }
                }

                World.CurrentInteractionContext = World.SelectInteractionContext;
                Reset();
            }
        }
    }

    private void UpdateHoveredGrid()
    {
        foreach (var planet in World.Planets)
        {
            if (planet.Grid.GetCellFromPoint(World.MousePosition, Transform.Default) != null)
            {
                if (planet.Grid != hoveredGrid)
                {
                    Reset();
                }

                hoveredGrid = planet.Grid;
                return;
            }
        }
    }

    private void Reset()
    {
        draggedCells.Clear();
        appendInstance = null;
    }

    public void Render(ICanvas canvas, MouseState leftMouse, MouseState rightMouse)
    {
        foreach (var cell in draggedCells)
        {
            canvas.PushState();
            canvas.Translate(hoveredGrid.Transform.LocalToWorld(cell.ToCartesian()));
            // canvas.Rotate(rotation * (MathF.Tau / 6f));
            prototype.Model.Render(canvas);
            canvas.PopState();
        }

        bool obstructed = false;
        if (hoveredGrid is not null)
        {
            obstructed = hoveredGrid.IsStructureObstructed(prototype, hoveredLocation, 0);
            canvas.Translate(hoveredGrid.Transform.LocalToWorld(hoveredLocation.ToCartesian()));
        }
        else
        {
            canvas.Translate(World.MousePosition);
        }
        prototype.Model.Render(canvas, alpha: 100, color: obstructed ? Color.Red : null);
    }

    public void BeginPlacing(StructurePrototype structre)
    {
        Reset();
        this.prototype = structre;
        World.CurrentInteractionContext = this;
    }

}
