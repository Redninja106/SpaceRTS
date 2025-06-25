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
    public static void Layout(GUIWindow window)
    {
        var handler = Program.World.SelectionHandler;

        window.Anchor = Alignment.BottomCenter;
        window.Alignment = Alignment.BottomCenter;
        window.Visible = handler.SelectedFleet != null || handler.SelectedCount > 0;

        if (handler.SelectedFleet != null)
        {
            handler.SelectedFleet.Layout(window);
        }
        else if (handler.SelectedCount == 1)
        {
            LayoutSingleUnitMenu(window);
        }
        else if (handler.SelectedCount > 1)
        {
            LayoutMultiUnitMenu(window);
        }
    }

    private static void LayoutMultiUnitMenu(GUIWindow window)
    {
        Unit[] units = Program.World.SelectionHandler.GetSelectedUnits().ToArray();

        using (window.Row())
        {
            window.Text("Selected Units");
            
            if (window.TextButton("Create Fleet", 12))
            {
                PlayerCommandProcessor playerCommandProcessor = (PlayerCommandProcessor)Program.World.PlayerTeam.GetCommandProcessor();
                playerCommandProcessor.AddCommand(new CreateFleetCommand("fleet", Program.World.PlayerTeam, units.OfType<Ship>().ToArray()));
            }
        }

        using (window.Row())
        {
            for (int i = 0; i < units.Length; i++)
            {
                var unit = units[i];
                window.Image(unit.Prototype.Icon.Texture32x32, new Vector2(24, 24));

                if (window.LastItemClicked(MouseButton.Left))
                {
                    Program.World.SelectionHandler.ClearSelection();
                    Program.World.SelectionHandler.Select(unit);
                }
                else if (window.LastItemClicked(MouseButton.Right))
                {
                    Program.World.SelectionHandler.Deselect(unit);
                }
                else if (window.LastItemHovered())
                {
                    Program.World.SelectionHandler.VisualFocus = unit;
                    Program.World.GUIViewport.SetTooltip(window => window.Text(unit.Prototype.Title));
                }
            }
        }
    }

    private static void LayoutSingleUnitMenu(GUIWindow window)
    {
        var unit = Program.World.SelectionHandler.GetSingleUnit()!;
        
        using (window.Row())
        {
            window.Image(unit.Prototype.Icon.Texture64x64);

            using (window.Column())
            {
                using (window.Row())
                {
                    window.Text(unit.Prototype.Title, 24);
                    if (unit is Ship s1 && s1.Fleet is Fleet fleet)
                    {
                        window.Text("(fleet 1)", color: Color.Gray);
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

                if (unit.Team != Program.World.PlayerTeam)
                {
                    window.Text(unit.Team.Name, color: Color.Gray);
                }
                else
                {
                    using (window.Row())
                    {
                        window.Text(status, color: Color.Gray);
                        if (window.LastItemHovered())
                        {
                            Program.World.GUIViewport.SetTooltip(w => w.Text($"{unit.Health}/{unit.Prototype.MaxHealth}hp"));
                        }

                        if (unit is Ship s)
                        {
                            foreach (var module in s.modules)
                            {
                                window.Image(module.Prototype.Icon.Texture16x16);
                                if (window.LastItemHovered())
                                {
                                    Program.World.GUIViewport.SetTooltip(module.Layout);
                                }
                                if (window.LastItemClicked(MouseButton.Left))
                                {
                                    module.Activate();
                                }
                            }
                        }
                    }
                }

                unit.Layout(window);
            }
        }
    }
}
