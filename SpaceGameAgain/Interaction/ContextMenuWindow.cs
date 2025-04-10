using SpaceGame.Commands;
using SpaceGame.GUI;
using SpaceGame.Ships;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Interaction;

//internal class ContextMenuWindow : ElementWindow
//{
//    public DoubleVector WorldPosition;

//    public ContextMenuWindow()
//    {
//        Border = 0;
//    }

//    public void SetTarget(Unit? target)
//    {
//        List<Element> elements = [];
//        GetGUI(target, elements);

//        if (elements.Count > 0)
//        {
//            Show();
//        }
//        else
//        {
//            Hide();
//        }

//        RootElement = new ElementStack(elements);
//    }

//    private void GetGUI(Unit? target, List<Element> elements)
//    {
//        if (target != null)
//        {
//            foreach (var command in target.GetCommands())
//            {
//                if (command.Applies(target, World.SelectionHandler.GetSelectedUnits()))
//                {
//                    elements.Add(CreateCommandButton(command));
//                }
//            }
//        }

//        if (World.SelectionHandler.SelectedCount > 0)
//        {
//            foreach (var command in World.SelectionHandler.GetCommonCommands())
//            {
//                if (command.Applies(target, World.SelectionHandler.GetSelectedUnits()))
//                {
//                    elements.Add(CreateCommandButton(command));
//                }
//            }

//            //if (target != null && World.PlayerTeam.Actor!.GetRelation(target.Team.Actor!) == Teams.TeamRelation.Enemies)
//            //{
//            //    elements.Add(new TextButton("attack", () =>
//            //    {
//            //        World.ContextMenu.Hide();
//            //        foreach (var unit in World.SelectionHandler.GetSelectedUnits())
//            //        {
//            //            PlayerCommandProcessor commandProcessor = (PlayerCommandProcessor)World.PlayerTeam.Actor!.CommandProcessor;
//            //            commandProcessor.AddCommand(new AttackCommand(Prototypes.Get<AttackCommandPrototype>("attack_command"), unit.AsReference(), target.AsReference()));
//            //        }
//            //    }, true));
//            //}

//            //DoubleVector mousePos = World.MousePosition;
//            //elements.Add(new TextButton("move here", () =>
//            //{
//            //    World.ContextMenu.Hide();
//            //    foreach (var unit in World.SelectionHandler.GetSelectedUnits())
//            //    {
//            //        PlayerCommandProcessor commandProcessor = (PlayerCommandProcessor)World.PlayerTeam.Actor!.CommandProcessor;
//            //        commandProcessor.AddCommand(new MoveCommand(Prototypes.Get<MoveCommandPrototype>("move_command"), (Ship)unit, mousePos));
//            //    }
//            //}, true));
//        }

//        //if (target != null)
//        //{
//        //    if (World.SelectionHandler.IsSelected(target))
//        //    {
//        //        elements.Add(new TextButton("deselect"));
//        //    }
//        //    else
//        //    {
//        //        elements.Add(new TextButton("select"));
//        //    }
//        //}

//    }

//    private TextButton CreateCommandButton(CommandPrototype prototype)
//    {
//        return new TextButton(prototype.Name, () => { }, true);
//    }

//    public override void Update(float displayWidth, float displayHeight)
//    {
//        var soi = World.GetSphereOfInfluence(WorldPosition);
//        if (soi != null)
//        {
//            WorldPosition = soi.ApplyUpdateTo(WorldPosition);
//        }

//        //Offset = World.Camera.WorldToScreen(WorldPosition.ToVector2()) * Program.ViewportScale; /// World.WindowManager.Scale;

//        base.Update(displayWidth, displayHeight);
//    }

//    public override void Render(ICanvas canvas, float displayWidth, float displayHeight)
//    {
//        base.Render(canvas, displayWidth, displayHeight);
//    }
//}
