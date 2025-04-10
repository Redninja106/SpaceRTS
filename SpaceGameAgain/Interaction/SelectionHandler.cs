using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceGame.Commands;
using SpaceGame.GUI;
using SpaceGame.Ships;
using SpaceGame.Ships.Modules;
using SpaceGame.Structures;

namespace SpaceGame.Interaction;
internal class SelectionHandler
{
    private HashSet<Unit> selected = [];

    public int SelectedCount => selected.Count;

    public void ClearSelection()
    {
        selected.Clear();
        World.UnitBar.UpdateButtons();
        //UpdateGUI();
    }

    public void Select(Unit unit)
    {
        selected.Add(unit);
        World.UnitBar.UpdateButtons();
        //UpdateGUI();
    }

    public CommandPrototype[] GetCommonCommands()
    {
        IEnumerable<CommandPrototype> result = [];

        foreach (var unit in selected)
        {
            result = result.Intersect(unit.GetCommands());
        }

        return result.ToArray();
    }

    public bool IsSelected(Unit selectable)
    {
        return selected.Contains(selectable);
    }



    public void Update()
    {
    }

    private void UpdateGUI()
    {
        //if (selected.Count == 0)
        //{
        //    World.UtilityBar.RootElement = null;
        //    World.UtilityBar.Hide();
        //    return;
        //}

        //if (selected.Count == 1)
        //    {
        //        switch (GetSelectedUnit())
        //        {
        //            case Ship ship:
        //                World.UtilityBar.RootElement = new ElementRow([
        //                    new ImageButton(UtilityBar.ShipIcon, 8, 8)
        //                    ])
        //                { Margin = 0 };
        //                break;
        //            case Structure structure:
        //                World.UtilityBar.RootElement = new ElementRow([
        //                    new ImageButton(UtilityBar.StructureIcon, 8, 8)
        //                    ])
        //                { Margin = 0 };
        //                break;
        //            default:
        //                break;
        //        }
        //        World.UtilityBar.Show();
        //    }
        //else
        //    {
        //        List<Element> elements = new();
        //        foreach (var selected in GetSelectedUnits())
        //        {
        //            var s = selected;
        //            elements.Add(new ImageButton(selected switch
        //            {
        //                Ship => UtilityBar.ShipIcon,
        //                Structure => UtilityBar.StructureIcon,
        //            }, 8, 8, () => Deselect(s)));
        //        }
        //        World.UtilityBar.RootElement = new ElementRow(elements.ToArray());
        //        World.UtilityBar.Show();
        //    }
        

        //var stack = World.InfoWindow.Stack;
        //if (selected.Count is 0)
        //{
        //    stack.Clear();
        //    stack.AddRange([
        //        new Label("--- nothing selected ---", 12, Alignment.Center),
        //    ]);
        //}
        //else if (selected.Count is 1)
        //{
        //    var singleSelected = selected.Single();
        //    if (singleSelected is Ship ship)
        //    {
        //        stack.Clear();
        //        stack.AddRange([
        //            new Label("SHIP", 16, Alignment.CenterLeft),
        //            new Label("status: operational", 12, Alignment.CenterLeft),
        //            new Separator(),
        //        ]);
        //        if (ship.Team == World.PlayerTeam)
        //        {
        //            foreach (var module in ship.modules)
        //            {
        //                stack.AddRange(module.Actor!.BuildGUI());
        //            }
        //        }
        //    }
        //    else if (singleSelected is Structure structure)
        //    {
        //        stack.Clear();
        //        var s = structure.GetSelectionGUI();
        //        stack.AddRange([
        //            new Label(structure.Prototype.Title, 16, Alignment.CenterLeft),
        //            new Label("status: operational", 12, Alignment.CenterLeft),
        //            new Separator(),

        //            .. (s ?? [])
        //        ]);
        //    }
        //    else
        //    {
        //        throw new Exception("cant select this");
        //    }
        //}
        //else
        //{
        //    stack.Clear();
        //    stack.AddRange([
        //        new Label("Selected Units", 16),
        //        new Separator(),
        //        new ElementStack(
        //            selected.Select(u => new TextButton(
        //                u.GetType().Name ?? "unit", 
        //                () => {
        //                    this.ClearSelection(u);
        //                }
        //                )
        //            )
        //        ){ DrawBorder = true, }
        //        ]);
        //}
    }

    public Unit? GetSelectedUnit()
    {
        if (selected.Count is 1)
        {
            return selected.Single();
        }
        return null;
    }

    public HashSet<Unit> GetSelectedUnits()
    {
        return selected;
    }

    public void Deselect(Unit selectable)
    {
        selected.Remove(selectable);
        World.UnitBar.UpdateButtons();
        //UpdateGUI();
    }
}