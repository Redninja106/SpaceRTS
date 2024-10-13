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
    private HashSet<UnitBase> selected = [];

    public int SelectedCount => selected.Count;

    public void ClearSelection(UnitBase? except = null)
    {
        selected.Clear();
        if (except != null)
            selected.Add(except);
        UpdateGUI();
    }

    public void Add(UnitBase selectable)
    {
        selected.Add(selectable);
        UpdateGUI();
    }

    public bool IsSelected(UnitBase selectable)
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
                    stack.AddRange(module.BuildGUI());
                }
            }
            else if (singleSelected is StructureInstance structureInstance)
            {
                stack.Clear();
                stack.AddRange([
                    new Label(structureInstance.Structure.Title, 16, Alignment.CenterLeft),
                    new Label("status: operational", 12, Alignment.CenterLeft),
                    new Separator(),
                     
                    .. (structureInstance.Behavior?.SelectGUI ?? [])
                ]);
            }
            else
            {
                throw new Exception();
            }
        }
        else
        {
            stack.Clear();
            stack.AddRange([
                new Label("Selected Units", 32),
                new Separator(),
                new ElementStack(
                    selected.Select(u => new TextButton(
                        u.ToString() ?? "unit", 
                        () => {
                            this.ClearSelection(u);
                        }
                        )
                    )
                ){ DrawBorder = true, }
                ]);
        }
    }

    public UnitBase? GetSelectedObject()
    {
        if (selected.Count is 1)
        {
            return selected.Single();
        }
        return null;
    }

    public IEnumerable<UnitBase> GetSelectedObjects()
    {
        return selected;
    }

    public void Deselect(UnitBase selectable)
    {
        selected.Remove(selectable);
        UpdateGUI();
    }
}