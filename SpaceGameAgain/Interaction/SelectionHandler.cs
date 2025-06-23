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
using SpaceGame.Stations;
using SpaceGame.Structures;
using SpaceGame.Teams;

namespace SpaceGame.Interaction;
internal class SelectionHandler(GameWorld World)
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
        foreach (var unit in GetFocusedOrSelected())
        {
            canvas.PushState();
            //unit.InterpolatedTransform.ApplyTo(canvas, camera);
            if (unit is Structure)
            {
                RenderUnitOutline(canvas, unit, unit.Team.GetRelationColor(World.PlayerTeam));
            }
            unit.RenderBackgroundOverlay(canvas, camera, true);
            canvas.PopState();
        }
    }

    public void RenderGroundOverlay(ICanvas canvas, Camera camera)
    {
        foreach (var unit in GetFocusedOrSelected())
        {
            canvas.PushState();
            // unit.InterpolatedTransform.ApplyTo(canvas, camera);
            unit.RenderGroundOverlay(canvas, camera, true);

            if (unit is not Structure)
            {
                RenderUnitOutline(canvas, unit, unit.Team.GetRelationColor(World.PlayerTeam));
            }
            canvas.PopState();
        }

        //foreach (var selectable in GetFocusedOrSelected())
        //{
        //    canvas.PushState();
        //    selectable.RenderGroundOverlay(canvas, camera, true);
        //    canvas.PopState();

        //    //else if (selectable is Fleet fleet)
        //    //{
        //    //    foreach (var fleetShip in fleet.ships)
        //    //    {
        //    //        canvas.PushState();
        //    //        fleetShip.DrawHighlightAbove(canvas, camera, true);
        //    //        canvas.PopState();
        //    //    }
        //    //}
        //}
    }

    public void RenderSkyOverlay(ICanvas canvas, Camera camera)
    {
        foreach (var unit in GetFocusedOrSelected())
        {
            canvas.PushState();
            unit.InterpolatedTransform.ApplyTo(canvas, camera);
            unit.RenderSkyOverlay(canvas, camera, true);
            canvas.PopState();
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

    public static void RenderUnitOutline(ICanvas canvas, Unit unit, Color color)
    {
        canvas.PushState();
        canvas.Stroke(color);
        if (unit is Structure structure)
        {
            for (int i = 0; i < structure.Prototype.Outline.Length; i += 2)
            {
                canvas.DrawLine(structure.Prototype.Outline[i], structure.Prototype.Outline[i + 1]);
            }
        }
        else
        {
            canvas.DrawCircle(0, 0, (float)unit.GetCollisionRadius());
        }

        canvas.PopState();
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