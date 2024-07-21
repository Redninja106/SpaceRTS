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
    private Structure structure;
    private Grid? hoveredGrid;
    private HexCoordinate hoveredLocation;
    private int rotation;

    public void Update(MouseState leftMouse, MouseState rightMouse)
    {
        if (rightMouse.Released && !rightMouse.Dragged)
        {
            World.CurrentInteractionContext = World.SelectInteractionContext;
        }

        if (Keyboard.IsKeyPressed(Key.R))
        {
            if (Keyboard.IsKeyDown(Key.LeftShift))
            {
                rotation--;
            }
            else
            {
                rotation++;
            }
        }

        UpdateHoveredGrid();
        if (hoveredGrid != null)
        {
            hoveredLocation = HexCoordinate.FromCartesian(hoveredGrid.Transform.WorldToLocal(World.MousePosition));
            if (leftMouse.Released && !hoveredGrid.IsStructureObstructed(structure, hoveredLocation, rotation))
            {
                var ship = World.SelectionHandler.GetSelectedObject() as Ship;

                if (ship is not null)
                {
                    ship.orders.Enqueue(new ConstructionOrder()
                    {
                        Grid = hoveredGrid,
                        Location = hoveredLocation,
                        Rotation = rotation,
                        Structure = structure,
                    });
                }

                World.CurrentInteractionContext = World.SelectInteractionContext;
            }
        }
    }

    private void UpdateHoveredGrid()
    {
        foreach (var planet in World.Planets)
        {
            if (planet.Grid.GetCellFromPoint(World.MousePosition, Transform.Default) != null)
            {
                hoveredGrid = planet.Grid;
                return;
            }
        }
    }

    public void Render(ICanvas canvas, MouseState leftMouse, MouseState rightMouse)
    {
        bool obstructed = false;
        if (hoveredGrid is not null)
        {
            obstructed = hoveredGrid.IsStructureObstructed(structure, hoveredLocation, rotation);
            canvas.Translate(hoveredGrid.Transform.LocalToWorld(hoveredLocation.ToCartesian()));
        }
        else
        {
            canvas.Translate(World.MousePosition);
        }

        canvas.Rotate(rotation * (MathF.Tau / 6f));
        structure.Model.Render(canvas, alpha: 100, color: obstructed ? Color.Red : null);
    }

    public void BeginPlacing(Structure structre)
    {
        this.structure = structre;
        this.rotation = 0;
        World.CurrentInteractionContext = this;
    }

}
