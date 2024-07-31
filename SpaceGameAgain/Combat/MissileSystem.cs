using SpaceGame.GUI;
using SpaceGame.Ships;
using SpaceGame.Ships.Modules;
using SpaceGame.Ships.Orders;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Combat;
internal class MissileSystem(UnitBase unit)
{
    public int SalvoSize { get; } = 5;
    public int MissilesRemaining { get; set; } = 5;
    public float FireRate { get; } = 2f;

    private float timeSinceMissile;
    private UnitBase? target;

    public void Update()
    {
        if (unit is Ship ship && ship.orders.Count > 0 && ship.orders.Peek() is AttackOrder attackOrder)
        {
            target = attackOrder.target;
        }
        else if (target is null)
        {
            foreach (var s in World.Ships)
            {
                if (unit.Team.GetRelation(s.Team) is TeamRelation.Enemies && unit.Transform.Distance(s.Transform) < 12)
                {
                    target = s;
                    break;
                }
            }
            foreach (var p in World.Planets)
            {
                foreach (var structure in p.Grid.Structures)
                {
                    if (unit.Team.GetRelation(structure.Team) is TeamRelation.Enemies && unit.Transform.Distance(structure.Transform) < 12)
                    {
                        target = structure;
                        break;
                    }
                }
            }
        }

        if (target != null)
        {
            if (MissilesRemaining > 0 && timeSinceMissile > 1f / FireRate)
            {
                Fire(target);
            }
            if (target.IsDestroyed)
            {
                target = null;
            }
        }

        if (MissilesRemaining <= 0 && timeSinceMissile > 2.5f)
        {
            MissilesRemaining = SalvoSize;
        }

        timeSinceMissile += Program.TickDelta;
    }

    private void Fire(UnitBase target)
    {
        World.Missiles.Add(new Missile(
            unit.Transform.Rotated(0 * (Random.Shared.NextSingle() - .5f) * MathF.PI / 10f),
            target,
            0 * Random.Shared.NextUnitVector2() * Random.Shared.NextSingle() * 1.5f,
            4,
            4
            ));

        MissilesRemaining--;
        timeSinceMissile = 0;
    }


    public void RenderSelected(ICanvas canvas)
    {
        canvas.Stroke(Color.Red);
        canvas.DrawCircle(0, 0, 12);
    }
}
