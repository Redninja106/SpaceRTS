using ImGuiNET;
using SimulationFramework.Desktop;
using SpaceGame.GUI;
using SpaceGame.Ships.Modules;
using SpaceGame.Ships;
using SpaceGame.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceGame.Commands;
using SpaceGame.Ships.Fleets;

namespace SpaceGame.Interaction;
internal class UnitBar : GUIWindow
{
    private List<PopupWindow> windows = [];

    public UnitBar()
    {
        this.Anchor = Alignment.BottomCenter;
        this.LayoutMode = LayoutMode.Horizontal;

        // this.Anchor = this.Origin = Alignment.TopLeft;
        // this.Visible = false;
        // 
        // this.Width = 100;
        // this.Height = 20;
    }

    //public override void Render(ICanvas canvas, float displayWidth, float displayHeight)
    //{
    //    this.Offset = new Vector2(0, -100);
    //    this.Anchor = this.Origin = Alignment.BottomCenter;
    //    base.Render(canvas, displayWidth, displayHeight);
    //}

    //public override void Update(float displayWidth, float displayHeight)
    //{
    //    base.Update(displayWidth, displayHeight);
    //    /*
    //    ImGui.SetNextWindowPos(ImGui.GetMainViewport().Size * new Vector2(.5f, 1), ImGuiCond.Always, new Vector2(.5f, 1));
    //    if (ImGui.Begin("utilityBar", ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoResize))
    //    {
    //        ImGui.Image(ShipIcon.GetImGuiID(), new(32, 32));
    //        if (ImGui.BeginItemTooltip())
    //        {
    //            ImGui.EndTooltip();
    //        }
    //        ImGui.SameLine();

    //        ImGui.Image(ConstructionIcon.GetImGuiID(), new(32, 32));
    //        if (ImGui.BeginItemTooltip())
    //        {
    //            ImGui.EndTooltip();
    //        }
    //    }
    //    ImGui.End();
    //    */
    //}

    //public void UpdateButtons()
    //{
    //    foreach (var window in windows)
    //    {
    //        World.GUIViewport.windows.Remove(window);
    //    }
    //    windows.Clear();

    //    if (World.SelectionHandler.SelectedCount == 1)
    //    {
    //        var u = World.SelectionHandler.GetSelectedUnit()!;
    //        windows.Add(new(u));

    //        if (u is Ship s)
    //        {
    //            foreach (var m in s.modules)
    //            {
    //                windows.Add(new(m.Actor!));
    //            }
    //        }
    //        else
    //        {
    //        }
    //    }
    //    else if (World.SelectionHandler.SelectedCount > 1)
    //    {
    //        foreach (var u in World.SelectionHandler.GetSelectedUnits())
    //        {
    //            windows.Add(new(u));
    //        }
    //    }

    //    World.GUIViewport.windows.AddRange(windows);

    //}

    public override void Layout()
    {
        if (World.SelectionHandler.SelectedCount == 1)
        {
            LayoutSingleUnitMenu();
        }
        else if (World.SelectionHandler.SelectedCount > 1)
        {
            LayoutMultiUnitMenu();
        }



        return;

        // for (int i = 0; i < windows.Count; i++)
        // {
        //     Image(windows[i].GUIProvider.Icon);
        //     if (LastItemHovered())
        //     {
        //         Vector2 offset = LastItemBounds.GetAlignedPoint(Alignment.TopCenter) - World.GUIViewport.Bounds.GetAlignedPoint(Alignment.BottomCenter);
        //         windows[i].Show(offset);
        //     }
        // }
        // return;

        var selectedCount = World.SelectionHandler.SelectedCount;

        if (selectedCount == 1)
        {
            switch (World.SelectionHandler.GetSingleUnit())
            {
                case Ship s:
                    Image(Icons.Ship);
                    if (LastItemHovered())
                    {
                        Vector2 offset = LastItemBounds.GetAlignedPoint(Alignment.TopCenter) - World.GUIViewport.Bounds.GetAlignedPoint(Alignment.BottomCenter);
                        // World.InfoMenu.Show(s, offset);
                    }

                    foreach (var module in s.modules)
                    {
                        Image(Icons.Construction);
                    }

                    if (s.modules.Select(ar => ar.Actor!).OfType<ConstructionModule>().Any())
                    {
                        if (LastItemHovered())
                        {
                            Vector2 offset = LastItemBounds.GetAlignedPoint(Alignment.TopCenter) - World.GUIViewport.Bounds.GetAlignedPoint(Alignment.BottomCenter);
                            // World.ConstructionMenu.Open(s, offset);
                        }
                    }
                    break;
                case Structure:
                    Image(Icons.Structure);
                    break;
                default:
                    Text("?");
                    break;
            }
        }
        else
        {
            foreach (var selected in World.SelectionHandler.GetSelectedUnits())
            {
                Image(selected switch
                {
                    Ship => Icons.Ship,
                    Structure => Icons.Structure,
                    _ => throw new()
                });
            }
        }

        base.Layout();
    }

    private void LayoutMultiUnitMenu()
    {
        Unit[] units = World.SelectionHandler.GetSelectedUnits().ToArray();

        LayoutMode = LayoutMode.Horizontal;
        Text("Selected Units");
        PushState();
        if (TextButton("Create Fleet", 12))
        {
            PlayerCommandProcessor playerCommandProcessor = (PlayerCommandProcessor)World.PlayerTeam.Actor.GetCommandProcessor();
            ActorReference<Ship>[] ships = units.OfType<Ship>().Select(u => u.AsReference()).ToArray();
            playerCommandProcessor.AddCommand(new CreateFleetCommand(Prototypes.Get<CreateFleetCommandPrototype>("create_fleet_command"), "fleet", World.PlayerTeam, ships));
        }
        PopState();

        LayoutMode = LayoutMode.Vertical;

        for (int i = 0; i < units.Length; i++)
        {
            var unit = units[i];
            Image(unit.Icon, new(24, 24));
            if (LastItemClicked(MouseButton.Left))
            {
                World.SelectionHandler.ClearSelection();
                World.SelectionHandler.Select(unit);
            }
            else if (LastItemClicked(MouseButton.Right))
            {
                World.SelectionHandler.Deselect(unit);
            }
            else if (LastItemHovered())
            {
                World.SelectionHandler.VisualFocus = unit;
                World.SetTooltip(window => window.Text(unit.Prototype.Title));
            }
            LayoutMode = LayoutMode.Horizontal;

        }
    }

    private void LayoutSingleUnitMenu()
    {
        var unit = World.SelectionHandler.GetSingleUnit()!;

        LayoutMode = LayoutMode.Horizontal;
        Image(unit.Icon);
        Text(unit.Prototype.Title, 24);
        if (unit is Ship s1 && s1.Fleet is Fleet fleet)
        {
            PushState();
            Text("(fleet 1)", color: Color.Gray);
            PopState();
        }
        LayoutMode = LayoutMode.Vertical;

        PushState();
        LayoutMode = LayoutMode.Horizontal;
        if (unit is Ship s)
        {
            Cursor += new Vector2(0, 4);
            foreach (var module in s.modules)
            {
                Image(module.Actor!.Icon, new(24, 24));
                if (LastItemHovered())
                {
                    World.SetTooltip(w =>
                    {
                        w.Text(module.Actor.Prototype.Name);
                    });
                }
            }
        }
        PopState();

        LayoutMode = LayoutMode.Vertical;
        string status = (unit.Health / (float)unit.Prototype.MaxHealth) switch
        {
            1 => "operational",
            >= .5f => "damaged",
            _ => "critical"
        };

        if (unit is Structure str && !str.Powered)
        {
            status = "unpowered";
        }

        if (unit.Team.Actor != World.PlayerTeam.Actor)
        {
            Text(unit.Team.Actor!.Name, color: Color.Gray);
        }
        else
        {
            Text(status, color: Color.Gray);
            if (LastItemHovered())
            {
                World.SetTooltip(w => w.Text($"{unit.Health}/{unit.Prototype.MaxHealth}hp"));
            }
        }

        LayoutMode = LayoutMode.Vertical;
        unit.Layout(this);
    }

    public override void Update(GUIViewport viewport)
    {
        this.Visible = World.SelectionHandler.SelectedCount > 0;
        base.Update(viewport);
    }
}

class PopupWindow : GUIWindow
{
    private bool justShown;
    private IGUIProvider provider;

    public void Show(IGUIProvider provider, Vector2 offset)
    {
        Visible = true;
        Anchor = Alignment.BottomCenter;
        Offset = offset;
        justShown = true;
        this.provider = provider;
    }

    public PopupWindow()
    {
    }

    public override void Update(GUIViewport viewport)
    {
        base.Update(viewport);
        if (Visible)
        {
            if (World.leftMouse.Pressed || World.rightMouse.Pressed)
            {
                Visible = false;
            }
            justShown = false;
        }
    }

    public override void Layout()
    {
        provider?.Layout(this);
        base.Layout();
    }
}