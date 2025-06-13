using ImGuiNET;
using SpaceGame.Commands;
using SpaceGame.Interaction;
using SpaceGame.Orders;
using SpaceGame.Ships;
using SpaceGame.Ships.Fleets;
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
    public Unit? target;

    public void Update(MouseState leftMouse, MouseState rightMouse)
    {
        World.SelectionHandler.VisualFocus = null;
        if (World.Collision.IsClientVisible(World.MousePosition))
        {
            target = World.Collision.TestPoint(World.MousePosition).FirstOrDefault();
        }
        else
        {
            target = null;
        }

        if (leftMouse.Released)
        {
            if (leftMouse.Dragged)
            {
                World.SelectionHandler.ClearSelection();
                foreach (var ship in TestRegion(leftMouse.DragStart, World.MousePosition))
                {
                    if (ship.Fleet == null)
                    {
                        World.SelectionHandler.Select(ship);
                    }
                }
            }
            else
            {
                if (target != null)
                {
                    if (Keyboard.IsKeyDown(Key.LeftShift))
                    {
                        World.SelectionHandler.Select(target);
                    }
                    else if (World.SelectionHandler.SelectedCount == 1 && World.SelectionHandler.IsSelected(target))
                    {
                        World.SelectionHandler.Deselect(target);
                    }
                    else
                    {
                        World.SelectionHandler.ClearSelection();
                        World.SelectionHandler.Select(target);
                    }
                }
                else
                {
                    World.SelectionHandler.ClearSelection();
                }
            }
        }

        if (rightMouse.Released)
        {
            if (target?.Team.GetRelation(World.PlayerTeam!) == TeamRelation.Enemies)
            {
                foreach (var selectedObject in World.SelectionHandler.GetSelectedUnits())
                {
                    if (selectedObject is Ship ship && ship.Team == World.PlayerTeam && ship.CanAttack)
                    {
                        // IssueOrder(target, order.AsReference().Cast<Order>(), ship.orders.ToList());
                        var commandProcessor = (PlayerCommandProcessor)World.PlayerTeam.GetCommandProcessor();

                        if (!Keyboard.IsKeyDown(Key.LeftShift))
                        {
                            commandProcessor.AddCommand(new CancelOrdersCommand() { target = ship });
                        }

                        var command = new AttackCommand(
                            ship,
                            target
                            );

                        commandProcessor.AddCommand(command);
                    }
                }
            }
            else
            {
                List<Ship> ships = [];
                foreach (var selectedObject in World.SelectionHandler.GetSelectedUnits())
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
                    if (Keyboard.IsKeyDown(Key.LeftShift) && ships[i].orders.TryPeek(out Order? order) && order is MoveOrder)
                    {
                        // moveOrder.targets.Add(World.MousePosition + positions[i]);
                    }
                    else
                    {
                        var commandProcessor = (PlayerCommandProcessor)World.PlayerTeam.GetCommandProcessor();
                        // MoveOrder moveOrder = new MoveOrder(Prototypes.Get<MoveOrderPrototype>("move_order"), World.NewID(), ships[i].AsReference().Cast<Unit>(), World.MousePosition + positions[i]);

                        if (!Keyboard.IsKeyDown(Key.LeftShift))
                        {
                            commandProcessor.AddCommand(new CancelOrdersCommand() { target = ships[i] });
                        }

                        IssueOrderCommand command = new()
                        {
                            Order = new MoveOrder()
                            {
                                Unit = ships[i],
                                target = World.MousePosition + positions[i]
                            }
                        };

                        commandProcessor.AddCommand(command);
                        // ships[i].Team.SubmitCommand(new UpdateOrdersCommand());
                        // IssueOrder(ships[i], o.AsReference().Cast<Order>(), ships[i].orders.ToList());
                        // ships[i].orders.Enqueue(o.AsReference().Cast<Order>());

                    }
                }
            }

            //if (target != null)
            //{
            //    World.ContextMenu.Show();
            //    World.ContextMenu.Offset = World.WindowManager.MousePosition;
            //    World.ContextMenu.SetTarget(target);
            //}
            //else
            //{
            //    World.ContextMenu.Show();
            //    World.ContextMenu.Offset = World.WindowManager.MousePosition;
            //    World.ContextMenu.SetTarget(target);
            //}

            //if (target is not null && target.Team.Actor?.GetRelation(World.PlayerTeam!.Actor) is TeamRelation.Enemies)
            //{
            //    foreach (var selectedObject in World.SelectionHandler.GetSelectedObjects())
            //    {
            //        if (selectedObject is Ship ship && ship.Team == World.PlayerTeam)
            //        {
            //            // IssueOrder(target, order.AsReference().Cast<Order>(), ship.orders.ToList());
            //            var commandProcessor = (PlayerCommandProcessor)World.PlayerTeam.CommandProcessor;

            //            if (!Keyboard.IsKeyDown(Key.LeftShift))
            //            {
            //                commandProcessor.AddCommand(new CancelOrdersCommand(Prototypes.Get<CommandPrototype>("cancel_orders_command"), ship));
            //            }

            //            var command = new AttackCommand(
            //                Prototypes.Get<AttackCommandPrototype>("attack_command"),
            //                ship.AsReference().Cast<Unit>(),
            //                target.AsReference()
            //                );

            //            commandProcessor.AddCommand(command);
            //        }
            //        else
            //        {
            //            World.SelectionHandler.Deselect(selectedObject);
            //        }
            //    }
            //}
            //else
            //{
            //    List<Ship> ships = [];
            //    foreach (var selectedObject in World.SelectionHandler.GetSelectedObjects())
            //    {
            //        if (selectedObject is Ship ship && ship.Team == World.PlayerTeam)
            //        {
            //            ships.Add(ship);
            //        }
            //        else
            //        {
            //            World.SelectionHandler.Deselect(selectedObject);
            //        }
            //    }

            //    var positions = ClusterFormation.PlaceShips(ships.ToArray());

            //    for (int i = 0; i < ships.Count; i++)
            //    {
            //        if (Keyboard.IsKeyDown(Key.LeftShift) && ships[i].orders.TryPeek(out ActorReference<Order> order) && order.Actor is MoveOrder)
            //        {
            //            // moveOrder.targets.Add(World.MousePosition + positions[i]);
            //        }
            //        else
            //        {
            //            var commandProcessor = (PlayerCommandProcessor)World.PlayerTeam.CommandProcessor;
            //            // MoveOrder moveOrder = new MoveOrder(Prototypes.Get<MoveOrderPrototype>("move_order"), World.NewID(), ships[i].AsReference().Cast<Unit>(), World.MousePosition + positions[i]);

            //            if (!Keyboard.IsKeyDown(Key.LeftShift))
            //            {
            //                commandProcessor.AddCommand(new CancelOrdersCommand(Prototypes.Get<CommandPrototype>("cancel_orders_command"), ships[i]));
            //            }

            //            commandProcessor.AddCommand(new MoveCommand(Prototypes.Get<MoveCommandPrototype>("move_command"), ships[i], World.MousePosition + positions[i]));
            //            // ships[i].Team.SubmitCommand(new UpdateOrdersCommand());
            //            // IssueOrder(ships[i], o.AsReference().Cast<Order>(), ships[i].orders.ToList());
            //            // ships[i].orders.Enqueue(o.AsReference().Cast<Order>());

            //        }
            //    }
            //}
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
    //    var cmdProc = (PlayerCommandProcessor)World.PlayerTeam.CommandProcessor;

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

    public void RenderBackgroundOverlay(ICanvas canvas, MouseState leftMouse, MouseState rightMouse)
    {
        if (target != null && !World.SelectionHandler.IsSelected(target) && target is Structure structure)
        {
            canvas.PushState();
            structure.DrawHighlightBelow(canvas, World.Camera, false);
            canvas.PopState();
        }
    }

    public void RenderGroundOverlay(ICanvas canvas, MouseState leftMouse, MouseState rightMouse)
    {
        if (target != null && !World.SelectionHandler.IsSelected(target))
        {
            if (target is Ship ship)
            {
                canvas.PushState();
                ship.DrawHighlightBelow(canvas, World.Camera, false);
                canvas.PopState();
            }
            else if (target is Structure structure)
            {
                canvas.PushState();
                structure.DrawHighlightAbove(canvas, World.Camera, false);
                canvas.PopState();
            }
        }
    }

    public void RenderSkyOverlay(ICanvas canvas, MouseState leftMouse, MouseState rightMouse)
    {
        if (target != null && !World.SelectionHandler.IsSelected(target) && target is Ship ship)
        {
            canvas.PushState();
            ship.DrawHighlightAbove(canvas, World.Camera, false);
            canvas.PopState();
        }

        if (leftMouse.Dragging)
        {
            canvas.Transform(World.Camera.CreateRelativeMatrix(Transform.Default with { Position = leftMouse.DragStart }));
            canvas.Stroke(Color.White);
            canvas.StrokeWidth(0);
            canvas.DrawRect(Rectangle.FromPoints(Vector2.Zero, (World.MousePosition - leftMouse.DragStart).ToVector2()));
        }
    }

    public IEnumerable<Ship> TestRegion(DoubleVector from, DoubleVector to)
    {
        DoubleVector min = new(Math.Min(from.X, to.X), Math.Min(from.Y, to.Y));
        DoubleVector max = new(Math.Max(from.X, to.X), Math.Max(from.Y, to.Y));

        foreach (var ship in World.Ships)
        {
            if (ship.Team == World.PlayerTeam)
            {
                DoubleVector pos = ship.Transform.Position;
                if (pos.X > min.X && pos.Y > min.Y && pos.X < max.X && pos.Y < max.Y)
                {
                    yield return ship;
                }
            }
        }
    }

}

interface ISelectable
{
    ITexture Icon { get; }
}