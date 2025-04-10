using SpaceGame.Commands;
using SpaceGame.Orders;
using SpaceGame.Ships;
using SpaceGame.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Interaction;
internal class ConstructionInteractionContext : IInteractionContext
{
    private StructurePrototype? prototype;
    private Grid? hoveredGrid;
    private HexCoordinate hoveredLocation;
    private int rotation;
    private Ship constructionShip;

    public void Update(MouseState leftMouse, MouseState rightMouse)
    {
        if (rightMouse.Released && !rightMouse.Dragged)
        {
            World.CurrentInteractionContext = World.SelectInteractionContext;
            Reset();
            return;
        }

        if (prototype == null)
        {
            return;
        }

        if (prototype.CanBeRotated && Keyboard.IsKeyPressed(Key.R))
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
            Vector2 hoveredPosition = hoveredGrid.Transform.WorldToLocal(World.MousePosition.ToVector2());
            hoveredLocation = HexCoordinate.FromCartesian(hoveredPosition);

            if (leftMouse.Released)
            {
                var cell = hoveredGrid.GetCell(hoveredLocation);
                if (cell != null && !hoveredGrid.IsStructureObstructed(prototype, hoveredLocation, rotation))
                {
                    var command = new ConstructionCommand(
                        Prototypes.Get<ConstructionCommandPrototype>("construction_command"),
                        constructionShip,
                        hoveredGrid.AsReference(),
                        hoveredLocation,
                        rotation,
                        prototype
                        );

                    var commandProcessor = (PlayerCommandProcessor)World.PlayerTeam.Actor!.CommandProcessor;
                    commandProcessor.AddCommand(command);
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
            if (planet.Grid.GetCellFromPoint(World.MousePosition) != null)
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
        rotation = 0;
    }

    public void Render(ICanvas canvas, MouseState leftMouse, MouseState rightMouse)
    {
        //    foreach (var cell in draggedCells)
        //    {
        //        canvas.PushState();
        //        canvas.Translate(hoveredGrid.Transform.LocalToWorld(cell.ToCartesian()));
        //        canvas.Rotate(rotation * (MathF.Tau / 6f));
        //        prototype.Model.Render(canvas);
        //        canvas.PopState();
        //    }

        // TODO: draw relative to grid to avoid precision issues!

        bool obstructed = false;
        if (hoveredGrid is not null)
        {
            hoveredGrid.InterpolatedTransform.ApplyTo(canvas, World.Camera);
            obstructed = hoveredGrid.IsStructureObstructed(prototype, hoveredLocation, rotation);
            canvas.Translate(hoveredLocation.ToCartesian());
            canvas.Rotate(rotation * (MathF.Tau / 6f));
        }
        else
        {
            Transform.Default.ApplyTo(canvas, World.Camera);
            canvas.Translate(World.MousePosition.ToVector2());
            canvas.Rotate(rotation * (MathF.Tau / 6f));
        }

        canvas.Stroke(Color.Gray);
        canvas.StrokeWidth(0);
        for (int i = 0; i < prototype.Outline.Length; i += 2)
        {
            canvas.DrawLine(prototype.Outline[i], prototype.Outline[i + 1]);
        }

        ColorF color = obstructed ? ColorF.Red : ColorF.White;
        color.A = 100;

        canvas.Rotate(-rotation * (MathF.Tau / 6f));
        prototype.Model.Render(canvas, this.rotation, color);
    }

    [DebugOverlay]
    public static void ShowHoveredGridCoordinate()
    {
        foreach (var planet in World.Planets)
        {
            if (planet.Grid.GetCellFromPoint(World.MousePosition) != null)
            {
                Vector2 hoveredPosition = planet.Grid.Transform.WorldToLocal(World.MousePosition.ToVector2());
                HexCoordinate location = HexCoordinate.FromCartesian(hoveredPosition);
                DebugDraw.Text(location.ToString(), World.Camera.VerticalSize / 30, location.ToCartesian(), planet.Grid.Transform);
                return;
            }
        }
    }

    public void BeginPlacing(StructurePrototype prototype, Ship constructionShip)
    {
        Reset();
        this.prototype = prototype;
        this.constructionShip = constructionShip;
        World.CurrentInteractionContext = this;
    }

}
