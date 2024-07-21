using SpaceGame.Asteroids;
using SpaceGame.Interaction;
using SpaceGame.Ships;
using SpaceGame.Ships.Formations;
using SpaceGame.Ships.Orders;
using SpaceGame.Structures;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Interaction;
internal class SelectInteractionHandler : IInteractionContext
{
    public void Update(MouseState leftMouse, MouseState rightMouse)
    {
        if (leftMouse.Released)
        {
            if (leftMouse.Dragged)
            {
                World.SelectionHandler.ClearSelection();
                foreach (var ship in PickArea(Rectangle.FromPoints(leftMouse.DragStart, World.MousePosition)))
                {
                    World.SelectionHandler.Add(ship);
                }
            }
            else
            {
                World.SelectionHandler.ClearSelection();
                var pickedUnit= PickUnit();
                if (pickedUnit != null)
                {
                    World.SelectionHandler.Add(pickedUnit);
                }
                else
                {
                    var pickedStructure = PickStructure();
                    if (pickedStructure != null)
                    {
                        World.SelectionHandler.Add(pickedStructure);
                    }
                    else
                    {
                        World.SelectionHandler.ClearSelection();
                    }
                }
            }
        }

        if (rightMouse.Released && !rightMouse.Dragged)
        {
            var target = PickUnit();
            if (target is not null && target.Team.GetRelation(World.PlayerTeam) is TeamRelation.Enemies)
            {
                foreach (var selectedObject in World.SelectionHandler.GetSelectedObjects())
                {
                    if (selectedObject is Ship ship && ship.Team == World.PlayerTeam)
                    {
                        if (!Keyboard.IsKeyDown(Key.LeftShift))
                            ship.orders.Clear();
                        ship.orders.Enqueue(new AttackOrder(target));
                    }
                    else
                    {
                        World.SelectionHandler.Deselect(selectedObject);
                    }
                }
            }
            else
            {
                List<Ship> ships = [];
                foreach (var selectedObject in World.SelectionHandler.GetSelectedObjects())
                {
                    if (selectedObject is Ship ship && ship.Team == World.PlayerTeam)
                    {
                        ships.Add(ship);
                    }
                    else
                    {
                        World.SelectionHandler.Deselect(selectedObject);
                    }
                }

                var positions = ClusterFormation.PlaceShips(ships.ToArray());

                for (int i = 0; i < ships.Count; i++)
                {
                    if (!Keyboard.IsKeyDown(Key.LeftShift))
                    {
                        ships[i].orders.Clear();
                    }

                    if (ships[i].orders.TryPeek(out Order? order) && order is MoveOrder moveOrder)
                    {
                        moveOrder.targets.Add(World.MousePosition + positions[i]);
                    }
                    else
                    {
                        ships[i].orders.Enqueue(new MoveOrder(World.MousePosition + positions[i]));
                    }
                }
            }
        }

        if (Keyboard.IsKeyPressed(Key.E))
        {
            var a = new Asteroid();
            a.Transform.Position = World.MousePosition;
            var delta = a.Transform.Position - World.Planets[0].Transform.Position;
            a.Orbit = new(World.Planets[0], delta.Length(), Angle.FromVector(delta), 0);
            a.size = 1;
            World.Asteroids.Add(a);
        }
    }

    public void Render(ICanvas canvas, MouseState leftMouse, MouseState rightMouse)
    {
        if (leftMouse.Dragging)
        {
            canvas.Stroke(Color.White);
            canvas.StrokeWidth(0);
            canvas.DrawRect(Rectangle.FromPoints(leftMouse.DragStart, World.MousePosition));
        }
    }

    private UnitBase? PickUnit()
    {
        foreach (var ship in World.Ships)
        {
            if (ship.TestPoint(World.MousePosition, Transform.Default, 2f))
            {
                return ship;
            }
        }
        return PickStructure();
    }

    private StructureInstance? PickStructure()
    {
        foreach (var planet in World.Planets)
        {
            var mp = planet.Grid.Transform.WorldToLocal(World.MousePosition);
            var cell = planet.Grid.GetCell(HexCoordinate.FromCartesian(mp));
            if (cell is not null && cell.Structure is not null)
            {
                return cell.Structure;
            }
        }
        return null;
    }

    private IEnumerable<Ship> PickArea(Rectangle area)
    {
        foreach (var ship in World.Ships)
        {
            if (area.ContainsPoint(ship.Transform.Position))
            {
                yield return ship;
            }
        }
    }

}