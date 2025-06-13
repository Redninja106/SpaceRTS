using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceGame.Commands;
using SpaceGame.GUI;
using SpaceGame.Ships;
using SpaceGame.Ships.Fleets;
using SpaceGame.Ships.Modules;
using SpaceGame.Structures;
using SpaceGame.Teams;

namespace SpaceGame.Interaction;
internal class SelectionHandler
{
    private HashSet<Unit> selected = [];

    public Fleet? SelectedFleet;
    public Unit? VisualFocus;
    public int SelectedCount => selected.Count;

    public void ClearSelection()
    {
        selected.Clear();
        //World.UnitBar.UpdateButtons();
        //UpdateGUI();
    }

    public void Select(Unit unit)
    {
        selected.Add(unit);
        //World.UnitBar.UpdateButtons();
        //UpdateGUI();
    }

    public bool IsSelected(Unit selectable)
    {
        return selected.Contains(selectable);
    }

    public void RenderBackgroundOverlay(ICanvas canvas, Camera camera)
    {
        foreach (var selectable in GetFocusedOrSelected())
        {
            if (selectable is Structure structure)
            {
                canvas.PushState();
                structure.DrawHighlightBelow(canvas, camera, true);
                canvas.PopState();
            }
        }
    }

    public void RenderGroundOverlay(ICanvas canvas, Camera camera)
    {
        foreach (var selectable in GetFocusedOrSelected())
        {
            if (selectable is Structure structure)
            {
                canvas.PushState();
                structure.DrawHighlightAbove(canvas, camera, true);
                canvas.PopState();
            }
            else if (selectable is Ship ship)
            {
                canvas.PushState();
                ship.DrawHighlightBelow(canvas, camera, true);
                canvas.PopState();
            }
            //else if (selectable is Fleet fleet)
            //{
            //    foreach (var fleetShip in fleet.ships)
            //    {
            //        canvas.PushState();
            //        fleetShip.DrawHighlightAbove(canvas, camera, true);
            //        canvas.PopState();
            //    }
            //}
        }
    }

    public void RenderSkyOverlay(ICanvas canvas, Camera camera)
    {
        foreach (var selectable in GetFocusedOrSelected())
        {
            if (selectable is Ship ship)
            {
                canvas.PushState();
                ship.DrawHighlightAbove(canvas, camera, true);
                canvas.PopState();
            }
            //else if (selectable is Fleet fleet)
            //{
            //    foreach (var fleetShip in fleet.ships)
            //    {
            //        canvas.PushState();
            //        fleetShip.DrawHighlightAbove(canvas, camera, true);
            //        canvas.PopState();
            //    }
            //}
        }
    }

    private IEnumerable<Unit> GetFocusedOrSelected()
    {
        if (VisualFocus != null)
        {
            return [VisualFocus];
        }

        return GetSelectedUnits();
    }

    public void Tick()
    {
        selected.RemoveWhere(u => u is IDestructable des && des.IsDestroyed);
    }

    public Unit? GetSingleUnit()
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
        //World.UnitBar.UpdateButtons();
        //UpdateGUI();
    }

}