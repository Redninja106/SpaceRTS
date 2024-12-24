using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceGame.GUI;
using SpaceGame.Ships;
using SpaceGame.Structures;

namespace SpaceGame.Interaction;
internal class SelectionHandler
{
    private HashSet<Unit> selected = [];

    public int SelectedCount => selected.Count;

    public void ClearSelection(Unit? except = null)
    {
        selected.Clear();
        if (except != null)
            selected.Add(except);
        UpdateGUI();
    }

    public void Add(Unit selectable)
    {
        selected.Add(selectable);
        UpdateGUI();
    }

    public bool IsSelected(Unit selectable)
    {
        return selected.Contains(selectable);
    }

    private void UpdateGUI()
    {
        var stack = World.InfoWindow.Stack;
        if (selected.Count is 0)
        {
            stack.Clear();
            stack.AddRange([
                new Label("--- nothing selected ---", 12, Alignment.Center),
            ]);
        }
        else if (selected.Count is 1)
        {
            var singleSelected = selected.Single();
            if (singleSelected is Ship ship)
            {
                stack.Clear();
                stack.AddRange([
                    new Label("SHIP", 16, Alignment.CenterLeft),
                    new Label("status: operational", 12, Alignment.CenterLeft),
                    new Separator(),
                ]); 
                
                foreach (var module in ship.modules)
                {
                    stack.AddRange(module.Actor!.BuildGUI());
                }
            }
            else if (singleSelected is Structure structure)
            {
                stack.Clear();
                var s = structure.GetSelectionGUI();
                stack.AddRange([
                    new Label(structure.Prototype.Title, 16, Alignment.CenterLeft),
                    new Label("status: operational", 12, Alignment.CenterLeft),
                    new Separator(),
                     
                    .. (s ?? [])
                ]);
            }
            else
            {
                throw new Exception("cant select this");
            }
        }
        else
        {
            stack.Clear();
            stack.AddRange([
                new Label("Selected Units", 16),
                new Separator(),
                new ElementStack(
                    selected.Select(u => new TextButton(
                        u.GetType().Name ?? "unit", 
                        () => {
                            this.ClearSelection(u);
                        }
                        )
                    )
                ){ DrawBorder = true, }
                ]);
        }
    }

    public Unit? GetSelectedObject()
    {
        if (selected.Count is 1)
        {
            return selected.Single();
        }
        return null;
    }

    public IEnumerable<Unit> GetSelectedObjects()
    {
        return selected;
    }

    public void Deselect(Unit selectable)
    {
        selected.Remove(selectable);
        UpdateGUI();
    }
}