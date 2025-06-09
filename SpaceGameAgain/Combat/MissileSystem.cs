using Silk.NET.OpenGL;
using SpaceGame.GUI;
using SpaceGame.Orders;
using SpaceGame.Ships;
using SpaceGame.Ships.Modules;
using SpaceGame.Structures;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Combat;
internal class MissileSystem(MissileSystemPrototype prototype, ulong id, ActorReference<Unit> unit) : WeaponSystem(prototype, id, unit)
{
    public override MissileSystemPrototype Prototype => (MissileSystemPrototype)base.Prototype;

    public int MissilesRemaining { get; set; } = prototype.SalvoSize;

    public int timeSinceMissile;
    public ActorReference<Unit> target;

    public override void Tick()
    {
        base.Tick();
        if (unit.Actor is Ship ship && ship.orders.Count > 0 && ship.orders.Peek().Actor is AttackOrder attackOrder)
        {
            target = attackOrder.target;
        }
        else if (target.IsNull)
        {
            // TODO: replace this awful, no good, terrible way of doing this with some kind of bin system
            foreach (var s in World.Ships)
            {
                if (unit.Actor!.Team.Actor!.GetRelation(s.Team.Actor!) is TeamRelation.Enemies && unit.Actor!.Transform.Distance(s.Transform) < Prototype.Range)
                {
                    target = ActorReference<Unit>.Create(s);
                    break;
                }
            }
            foreach (var s in World.Structures)
            {
                if (unit.Actor!.Team.Actor!.GetRelation(s.Team.Actor!) is TeamRelation.Enemies && unit.Actor!.Transform.Distance(s.Transform) < Prototype.Range)
                {
                    target = ActorReference<Unit>.Create(s);
                    break;
                }
            }

        }

        if (!target.IsNull)
        {
            this.Transform.Rotation = MathHelper.Step(this.Transform.Rotation, Angle.FromVector((target.Actor!.Transform.Position - this.unit.Actor!.Transform.Position).ToVector2()), .1f);
            
            if (MissilesRemaining > 0 && timeSinceMissile > Prototype.FireInterval)
            {
                Fire(target.Actor);
            }
            if (target.Actor.Health <= 0 || target.Actor.Transform.Distance(this.Transform) > Prototype.Range)
            {
                target = ActorReference<Unit>.Null;
            }
        }

        if (MissilesRemaining <= 0 && timeSinceMissile > Prototype.SalvoInterval)
        {
            MissilesRemaining = Prototype.SalvoSize;
        }

        timeSinceMissile++;
    }

    private void Fire(Unit target)
    {
        World.Add(new Missile(
            Prototypes.Get<MissilePrototype>("missile"),
            World.NewID(),
            Transform.Create(this.unit.Actor!.GetCenter(), this.Transform.Rotation + (World.TickRandom.NextSingle() - .5f) * MathF.PI / 10f),
            ActorReference<Unit>.Create(target),
            DoubleVector.FromVector2(World.TickRandom.NextUnitVector2() * World.TickRandom.NextSingle() * 1.5f)
            ));

        MissilesRemaining--;
        timeSinceMissile = 0;
    }

    public override void Serialize(BinaryWriter writer)
    {
        writer.Write(ID);
        writer.Write(unit);
        writer.Write(target);
        writer.Write(MissilesRemaining);
        writer.Write(timeSinceMissile);
    }
}