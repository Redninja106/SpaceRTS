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
internal static class UnitBar
{
    // private List<PopupWindow> windows = [];
    //private GUIWindow window;

    //public UnitBar()
    //{
    //    window = new()
    //    {
    //        Anchor = Alignment.BottomCenter
    //    };
    //    // this.LayoutMode = LayoutMode.Row;

    //    // this.Anchor = this.Origin = Alignment.TopLeft;
    //    // this.Visible = false;
    //    // 
    //    // this.Width = 100;
    //    // this.Height = 20;
    //}

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
    //                windows.Add(new(m));
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

    public static void Layout(GUIWindow window)
    {
        window.Anchor = Alignment.BottomCenter;
        window.Alignment = Alignment.BottomCenter;
        window.Visible = World.SelectionHandler.SelectedCount > 0;

        if (World.SelectionHandler.SelectedCount == 1)
        {
            LayoutSingleUnitMenu(window);
        }
        else if (World.SelectionHandler.SelectedCount > 1)
        {
            LayoutMultiUnitMenu(window);
        }


        //return;

        //// for (int i = 0; i < windows.Count; i++)
        //// {
        ////     Image(windows[i].GUIProvider.Icon);
        ////     if (LastItemHovered())
        ////     {
        ////         Vector2 offset = LastItemBounds.GetAlignedPoint(Alignment.TopCenter) - World.GUIViewport.Bounds.GetAlignedPoint(Alignment.BottomCenter);
        ////         windows[i].Show(offset);
        ////     }
        //// }
        //// return;

        //var selectedCount = World.SelectionHandler.SelectedCount;

        //if (selectedCount == 1)
        //{
        //    switch (World.SelectionHandler.GetSingleUnit())
        //    {
        //        case Ship s:
        //            Image(Icons.Ship);
        //            if (LastItemHovered())
        //            {
        //                Vector2 offset = LastItemBounds.GetAlignedPoint(Alignment.TopCenter) - World.GUIViewport.Bounds.GetAlignedPoint(Alignment.BottomCenter);
        //                // World.InfoMenu.Show(s, offset);
        //            }

        //            foreach (var module in s.modules)
        //            {
        //                Image(Icons.Construction);
        //            }

        //            if (s.modules.Select(ar => ar).OfType<ConstructionModule>().Any())
        //            {
        //                if (LastItemHovered())
        //                {
        //                    Vector2 offset = LastItemBounds.GetAlignedPoint(Alignment.TopCenter) - World.GUIViewport.Bounds.GetAlignedPoint(Alignment.BottomCenter);
        //                    // World.ConstructionMenu.Open(s, offset);
        //                }
        //            }
        //            break;
        //        case Structure:
        //            Image(Icons.Structure);
        //            break;
        //        default:
        //            Text("?");
        //            break;
        //    }
        //}
        //else
        //{
        //    foreach (var selected in World.SelectionHandler.GetSelectedUnits())
        //    {
        //        Image(selected switch
        //        {
        //            Ship => Icons.Ship,
        //            Structure => Icons.Structure,
        //            _ => throw new()
        //        });
        //    }
        //}

        //base.Layout();
    }

    private static void LayoutMultiUnitMenu(GUIWindow window)
    {
        Unit[] units = World.SelectionHandler.GetSelectedUnits().ToArray();

        using (window.Row())
        {
            window.Text("Selected Units");
            
            if (window.TextButton("Create Fleet", 12))
            {
                PlayerCommandProcessor playerCommandProcessor = (PlayerCommandProcessor)World.PlayerTeam.GetCommandProcessor();
                playerCommandProcessor.AddCommand(new CreateFleetCommand("fleet", World.PlayerTeam, units.OfType<Ship>().ToArray()));
            }
        }

        using (window.Row())
        {
            for (int i = 0; i < units.Length; i++)
            {
                var unit = units[i];
                window.Image(unit.Icon, new(24, 24));
                if (window.LastItemClicked(MouseButton.Left))
                {
                    World.SelectionHandler.ClearSelection();
                    World.SelectionHandler.Select(unit);
                }
                else if (window.LastItemClicked(MouseButton.Right))
                {
                    World.SelectionHandler.Deselect(unit);
                }
                else if (window.LastItemHovered())
                {
                    World.SelectionHandler.VisualFocus = unit;
                    World.GUIViewport.SetTooltip(window => window.Text(unit.Prototype.Title));
                }
            }
        }
    }

    private static void LayoutSingleUnitMenu(GUIWindow window)
    {
        var unit = World.SelectionHandler.GetSingleUnit()!;
        
        using (window.Row())
        {
            // LayoutMode = LayoutMode.Horizontal;
            window.Image(unit.Icon);

            using (window.Column())
            {
                using (window.Row())
                {
                    window.Text(unit.Prototype.Title, 24);
                    if (unit is Ship s1 && s1.Fleet is Fleet fleet)
                    {
                        //PushState();
                        window.Text("(fleet 1)", color: Color.Gray);
                        //PopState();
                    }
                    // LayoutMode = LayoutMode.Vertical;

                    // PushState();
                    if (unit is Ship s)
                    {
                        //window.Cursor += new Vector2(0, 4);
                        foreach (var module in s.modules)
                        {
                            window.Image(module.Icon, new(24, 24));
                            if (window.LastItemHovered())
                            {
                                World.GUIViewport.SetTooltip(w =>
                                {
                                    w.Text(module.Prototype.Name);
                                });
                            }
                        }
                    }
                }

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

                if (unit.Team != World.PlayerTeam)
                {
                    window.Text(unit.Team.Name, color: Color.Gray);
                }
                else
                {
                    window.Text(status, color: Color.Gray);
                    if (window.LastItemHovered())
                    {
                        World.GUIViewport.SetTooltip(w => w.Text($"{unit.Health}/{unit.Prototype.MaxHealth}hp"));
                    }
                }

                // LayoutMode = LayoutMode.Vertical;
                unit.Layout(window);
            }

            // LayoutMode = LayoutMode.Horizontal;
            
            //PopState();

            //LayoutMode = LayoutMode.Vertical;
            
        }
    }
}

delegate void GUILayout(GUIWindow window);

//class GUIPopup : GUIWindow
//{
//    private bool justShown;
//    private IGUIProvider provider;

//    public void Show(IGUIProvider provider, Vector2 offset)
//    {
//        Visible = true;
//        Anchor = Alignment.BottomCenter;
//        Offset = offset;
//        justShown = true;
//        this.provider = provider;
//    }

//    public GUIPopup()
//    {
//    }

//    public override void Update(GUIViewport viewport)
//    {
//        base.Update(viewport);
//        if (Visible)
//        {
//            if (World.leftMouse.Pressed || World.rightMouse.Pressed)
//            {
//                Visible = false;
//            }
//            justShown = false;
//        }
//    }

//    public override void Layout()
//    {
//        provider?.Layout(this);
//        base.Layout();
//    }
//}