using SpaceGame.GUI;
using SpaceGame.Ships;
using SpaceGame.Ships.Modules;
using SpaceGame.Ships.Orders;
using SpaceGame.Structures;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Combat;
internal class MissileSystem(MissileSystemPrototype prototype, ulong id, Transform transform, Unit unit) : WeaponSystem(prototype, id, transform)
{
    public int SalvoSize { get; } = 5;
    public int MissilesRemaining { get; set; } = 5;
    public float FireRate { get; } = 2f;

    private float timeSinceMissile;
    private ActorReference<Unit> target;

    public override void Update()
    {
        if (unit is Ship ship && ship.orders.Count > 0 && ship.orders.Peek() is AttackOrder attackOrder)
        {
            target = attackOrder.target;
        }
        else if (target == null)
        {
            // TODO: replace this awful, no good, terrible way of doing this with some kind of bin system
            foreach (var s in World.Ships)
            {
                if (unit.Team.Actor!.GetRelation(s.Team.Actor!) is TeamRelation.Enemies && unit.Transform.Distance(s.Transform) < 12)
                {
                    target = ActorReference<Unit>.Create(s);
                    break;
                }
            }
            foreach (var s in World.Structures)
            {
                if (unit.Team.Actor!.GetRelation(s.Team.Actor!) is TeamRelation.Enemies && unit.Transform.Distance(s.Transform) < 12)
                {
                    target = ActorReference<Unit>.Create(s);
                    break;
                }
            }

        }

        if (!target.IsNull)
        {
            if (MissilesRemaining > 0 && timeSinceMissile > 1f / FireRate)
            {
                Fire(target.Actor);
            }
            if (target.Actor.Health <= 0)
            {
                target = ActorReference<Unit>.Null;
            }
        }

        if (MissilesRemaining <= 0 && timeSinceMissile > 2.5f)
        {
            MissilesRemaining = SalvoSize;
        }

        timeSinceMissile += Time.DeltaTime;
    }

    private void Fire(Unit target)
    {
        World.Add(new Missile(
            Prototypes.Get<MissilePrototype>("missile"),
            World.NewID(),
            unit.Transform.Rotated((Random.Shared.NextSingle() - .5f) * MathF.PI / 10f),
            ActorReference<Unit>.Create(target),
            Random.Shared.NextUnitVector2() * Random.Shared.NextSingle() * 1.5f
            ));

        MissilesRemaining--;
        timeSinceMissile = 0;
    }


    public void RenderSelected(ICanvas canvas)
    {
        canvas.Stroke(Color.Red);
        canvas.DrawCircle(0, 0, 12);
    }

    public override void Serialize(BinaryWriter writer)
    {
    }
}