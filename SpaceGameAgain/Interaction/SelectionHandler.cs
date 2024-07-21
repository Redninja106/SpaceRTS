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
        if (selected.Count is 0)
        {
            World.RightSidebar.Stack.Clear();
            World.RightSidebar.Stack.AddRange([
                new Label("--- nothing selected ---", 16, Alignment.Center),
            ]);
        }
        else if (selected.Count is 1)
        {
            var singleSelected = selected.Single();
            if (singleSelected is Ship ship)
            {
                World.RightSidebar.Stack.Clear();
                World.RightSidebar.Stack.AddRange([
                    new Label("SHIP", 32, Alignment.Center),
                    new Separator(),

                    new Label("status: operational", 16, Alignment.Center),
                    new Gap(48),

                    new Label("MODULES", 24, Alignment.Center),
                    new Separator(),
                ]);
                foreach (var module in ship.modules)
                {
                    World.RightSidebar.Stack.AddRange(module.BuildGUI());
                }
                if (ship.modules.Count is 0)
                {
                    World.RightSidebar.Stack.Add(new Label("--- none ---", 16, Alignment.Center));
                }
            }
            else if (singleSelected is StructureInstance structureInstance)
            {
                World.RightSidebar.Stack.Clear();
                World.RightSidebar.Stack.AddRange([
                    new Label(structureInstance.Structure.Title, 32, Alignment.Center),
                    new Separator(),

                    new Label("status: operational", 16, Alignment.Center),

                    ..(structureInstance.Behavior?.SelectGUI ?? [])
                ]);
            }
            else
            {
                throw new Exception();
            }
        }
        else
        {
            World.RightSidebar.Stack.Clear();
            World.RightSidebar.Stack.AddRange([
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