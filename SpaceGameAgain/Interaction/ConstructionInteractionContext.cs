using SpaceGame.Commands;
using SpaceGame.Debugging;
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
            Vector2 hoveredPosition = hoveredGrid.Transform.WorldToLocal(World.MousePosition.ToVector2()) - prototype.Center.Rotated(rotation * MathF.Tau / 6f);
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

                    var commandProcessor = (PlayerCommandProcessor)World.PlayerTeam.Actor!.GetCommandProcessor();
                    commandProcessor.AddCommand(command);

                    World.CurrentInteractionContext = World.SelectInteractionContext;
                    Reset();
                }
                else
                {
                    World.TextWidgets.AddNotficationWidget(new(Transform.Create(
                        hoveredGrid.Transform.Position + DoubleVector.FromVector2(hoveredLocation.ToCartesian()), 0),
                        "Obstructed!"
                        ));
                }
            }
        }
    }

    private void UpdateHoveredGrid()
    {
        hoveredGrid = null;
        foreach (var planet in World.Planets)
        {
            if (planet.Grid.GetCellFromPoint(World.MousePosition) != null)
            {
                if (planet.Grid != hoveredGrid)
                {
                    //Reset();
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

    public void RenderBackgroundOverlay(ICanvas canvas, MouseState leftMouse, MouseState rightMouse)
    {
        bool obstructed = true;
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
            canvas.Translate(World.MousePosition.ToVector2() - prototype.Center.Rotated(rotation * MathF.Tau / 6f));
            canvas.Rotate(rotation * (MathF.Tau / 6f));
        }

        canvas.Stroke(Color.Gray);
        canvas.StrokeWidth(0);
        for (int i = 0; i < prototype.Outline.Length; i += 2)
        {
            canvas.DrawLine(prototype.Outline[i], prototype.Outline[i + 1]);
        }
    }

    public void RenderGroundOverlay(ICanvas canvas, MouseState leftMouse, MouseState rightMouse)
    {
        bool obstructed = true;
        if (hoveredGrid is not null)
        {
            hoveredGrid.InterpolatedTransform.ApplyTo(canvas, World.Camera);
            obstructed = hoveredGrid.IsStructureObstructed(prototype, hoveredLocation, rotation);
            canvas.Translate(hoveredLocation.ToCartesian());
        }
        else
        {
            Transform.Default.ApplyTo(canvas, World.Camera);
            canvas.Translate(World.MousePosition.ToVector2() - prototype.Center.Rotated(rotation * MathF.Tau / 6f));
        }
        canvas.Translate(prototype.Center.Rotated(this.rotation * MathF.Tau / 6f));

        if (prototype is TurretPrototype turret)
        {
            canvas.PushState();
            // Transform.Create(World.MousePosition, 0).ApplyTo(canvas, World.Camera);
            canvas.Stroke(Color.White with { A = 40 });
            // canvas.DrawCircle(0, 0, turret.WeaponSystemPrototype.Range);
            canvas.PopState();
        }

        ColorF color = obstructed ? ColorF.Red : ColorF.White;
        color.A = 100;

        prototype.Model.Render(canvas, Transform.Default with { Position = World.MousePosition, Rotation = this.rotation * MathF.Tau / 6f }, color);

        if (prototype is TurretPrototype t)
        {
            // t.WeaponSystemPrototype.Model?.Render(canvas, Transform.Default with { Position = World.MousePosition, Rotation = this.rotation * MathF.Tau / 6f }, color);
        }

        if (hoveredGrid is not null)
        {
            foreach (HexCoordinate adjacent in prototype.AdjacentCells)
            {
                HexCoordinate neighborCell = this.hoveredLocation + adjacent.Rotated(this.rotation);
                var neighbor = hoveredGrid.GetCell(neighborCell)?.Structure.Actor;
                if (neighbor != null && neighbor.Team == World.PlayerTeam)
                {
                    canvas.PushState();
                    Transform transform = Transform.Default with { Position = World.MousePosition };
                    transform.ApplyTo(canvas, World.Camera);
                    neighbor.Prototype.RenderAdjacencyOverlay(canvas, neighborCell.ToCartesian(), prototype);
                    prototype.RenderAdjacencyOverlay(canvas, neighborCell.ToCartesian(), neighbor.Prototype);
                    canvas.PopState();
                }
            }
        }
    }

    public void RenderSkyOverlay(ICanvas canvas, MouseState leftMouse, MouseState rightMouse)
    {
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
