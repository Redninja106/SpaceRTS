using SpaceGame.Commands;
using SpaceGame.Interaction;
using SpaceGame.Orders;
using SpaceGame.Ships;
using SpaceGame.Ships.Formations;
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
                var pickedUnit = PickUnit();
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
            if (target is not null && target.Team.Actor!.GetRelation(World.PlayerTeam!.Actor) is TeamRelation.Enemies)
            {
                foreach (var selectedObject in World.SelectionHandler.GetSelectedObjects())
                {
                    if (selectedObject is Ship ship && ship.Team == World.PlayerTeam)
                    {
                        // IssueOrder(target, order.AsReference().Cast<Order>(), ship.orders.ToList());

                        var command = new AttackCommand(
                            Prototypes.Get<AttackCommandPrototype>("attack_command"),
                            ship.AsReference().Cast<Unit>(),
                            target.AsReference()
                            );

                        var commandProcessor = (PlayerCommandProcessor)World.PlayerTeam.Actor!.CommandProcessor;
                        commandProcessor.AddCommand(command);
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
                    if (Keyboard.IsKeyDown(Key.LeftShift) && ships[i].orders.TryPeek(out ActorReference<Order> order) && order.Actor is MoveOrder)
                    {
                        // moveOrder.targets.Add(World.MousePosition + positions[i]);
                    }
                    else
                    {
                        var commandProcessor = (PlayerCommandProcessor)World.PlayerTeam.Actor!.CommandProcessor;
                        // MoveOrder moveOrder = new MoveOrder(Prototypes.Get<MoveOrderPrototype>("move_order"), World.NewID(), ships[i].AsReference().Cast<Unit>(), World.MousePosition + positions[i]);

                        commandProcessor.AddCommand(new MoveCommand(Prototypes.Get<MoveCommandPrototype>("move_command"), ships[i], World.MousePosition + positions[i]));
                        // ships[i].Team.SubmitCommand(new UpdateOrdersCommand());
                        // IssueOrder(ships[i], o.AsReference().Cast<Order>(), ships[i].orders.ToList());
                        // ships[i].orders.Enqueue(o.AsReference().Cast<Order>());

                    }
                }
            }
        }

        //if (Keyboard.IsKeyPressed(Key.E))
        //{
        //    var a = new Asteroid();
        //    a.Transform.Position = World.MousePosition;
        //    var delta = a.Transform.Position - World.Planets[0].Transform.Position;
        //    a.Orbit = new(World.Planets[0], delta.Length(), Angle.FromVector(delta), 0);
        //    a.size = 1;
        //    World.Asteroids.Add(a);
        //}

        //if (Keyboard.IsKeyDown(Key.Z))
        //{
        //    World.ConstructionInteractionContext.BeginPlacing(World.Structures.DefensiveZone);
        //    World.CurrentInteractionContext = World.ConstructionInteractionContext;
        //}
        //if (Keyboard.IsKeyDown(Key.X))
        //{
        //    World.ConstructionInteractionContext.BeginPlacing(World.Structures.IndustrialZone);
        //    World.CurrentInteractionContext = World.ConstructionInteractionContext;
        //}
        //if (Keyboard.IsKeyDown(Key.C))
        //{
        //    World.ConstructionInteractionContext.BeginPlacing(World.Structures.ResearchZone);
        //    World.CurrentInteractionContext = World.ConstructionInteractionContext;
        //}
        //if (Keyboard.IsKeyDown(Key.V))
        //{
        //    World.ConstructionInteractionContext.BeginPlacing(World.Structures.EconomicZone);
        //    World.CurrentInteractionContext = World.ConstructionInteractionContext;
        //}
    }

    //private void IssueOrder(Unit target, ActorReference<Order> order, List<ActorReference<Order>>? prevOrders)
    //{
    //    var cmdProc = (PlayerCommandProcessor)World.PlayerTeam.Actor!.CommandProcessor;

    //    // var order = new AttackOrder(new(), World.NewID(), Transform.Default, ActorReference<Unit>.Create(target)).AsReference().Cast<Order>();

    //    var prototype = Prototypes.Get<MoveOrderPrototype>("update_orders_command");

    //    // if shift is held append to existing commands
    //    if (Keyboard.IsKeyDown(Key.LeftShift))
    //    {
    //        cmdProc.AddCommand(new CancelOrdersCommand(
    //            Prototypes.Get<CancelOrdersCommandPrototype>("cancel_orders_command"),
    //            World.NewID(),
    //            target.AsReference()
    //            ));
    //    }
        
    //    cmdProc.AddCommand(new MoveCommand(prototype, World.NewID(), target.AsReference(), orders));
    //}

    public void Render(ICanvas canvas, MouseState leftMouse, MouseState rightMouse)
    {
        if (leftMouse.Dragging)
        {
            canvas.Stroke(Color.White);
            canvas.StrokeWidth(0);
            canvas.DrawRect(Rectangle.FromPoints(leftMouse.DragStart, World.MousePosition));
        }
    }

    private Unit? PickUnit()
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

    private Structure? PickStructure()
    {
        foreach (var planet in World.Planets)
        {
            var mp = planet.Grid.Transform.WorldToLocal(World.MousePosition);
            var cell = planet.Grid.GetCell(HexCoordinate.FromCartesian(mp));
            if (cell is not null && !cell.Structure.IsNull)
            {
                return cell.Structure.Actor!;
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