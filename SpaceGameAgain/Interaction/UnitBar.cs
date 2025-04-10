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

namespace SpaceGame.Interaction;
internal class UnitBar : GUIWindow
{
    private List<UtilityWindow> windows = [];

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

    public void UpdateButtons()
    {
        foreach (var window in windows)
        {
            World.GUIViewport.windows.Remove(window);
        }
        windows.Clear();

        if (World.SelectionHandler.SelectedCount == 1)
        {
            var u = World.SelectionHandler.GetSelectedUnit()!;
            windows.Add(new(u));

            if (u is Ship s)
            {
                foreach (var m in s.modules)
                {
                    windows.Add(new(m.Actor!));
                }
            }
            else
            {
            }
        }
        else if (World.SelectionHandler.SelectedCount > 1)
        {
            foreach (var u in World.SelectionHandler.GetSelectedUnits())
            {
                windows.Add(new(u));
            }
        }

        World.GUIViewport.windows.AddRange(windows);

    }

    public override void Layout()
    {
        for (int i = 0; i < windows.Count; i++)
        {
            Image(windows[i].GUIProvider.Icon);
            if (LastItemHovered())
            {
                Vector2 offset = LastItemBounds.GetAlignedPoint(Alignment.TopCenter) - World.GUIViewport.Bounds.GetAlignedPoint(Alignment.BottomCenter);
                windows[i].Show(offset);
            }
        }
        return;

        var selectedCount = World.SelectionHandler.SelectedCount;

        if (selectedCount == 1)
        {
            switch (World.SelectionHandler.GetSelectedUnit())
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

    public override void Update(GUIViewport viewport)
    {
        this.Visible = World.SelectionHandler.SelectedCount > 0;
        base.Update(viewport);
    }
}

class UtilityWindow : GUIWindow
{
    public IGUIProvider GUIProvider { get; set; }

    private bool justShown;

    public void Show(Vector2 offset)
    {
        Visible = true;
        Anchor = Alignment.BottomCenter;
        Offset = offset;
        justShown = true;
    }

    public UtilityWindow(IGUIProvider guiProvider)
    {
        this.GUIProvider = guiProvider;
    }

    public override void Update(GUIViewport viewport)
    {
        base.Update(viewport);
        if (Visible)
        {
            if (!Hovered && !justShown)
            {
                Visible = false;
            }
            justShown = false;
        }
    }

    public override void Layout()
    {
        base.Layout();
        GUIProvider.Layout(this);
    }
}