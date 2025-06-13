using Silk.NET.OpenGL;
using SpaceGame.Extensions;
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
internal class MissileSystem(MissileSystemPrototype prototype, ulong id) : WeaponSystem(prototype, id)
{
    public override MissileSystemPrototype Prototype => (MissileSystemPrototype)base.Prototype;

    public int MissilesRemaining { get; set; } = prototype.SalvoSize;

    public int timeSinceMissile;
    public Unit target;

    public override void Tick()
    {
        base.Tick();
        if (Unit is Ship ship && ship.orders.Count > 0 && ship.orders.Peek() is AttackOrder attackOrder)
        {
            target = attackOrder.target;
        }
        else if (target == null)
        {
            // TODO: replace this awful, no good, terrible way of doing this with some kind of bin system
            foreach (var s in World.Ships)
            {
                if (Unit.Team.GetRelation(s.Team) is TeamRelation.Enemies && Unit.Transform.Distance(s.Transform) < Prototype.Range)
                {
                    target = s;
                    break;
                }
            }
            foreach (var s in World.Structures)
            {
                if (Unit.Team.GetRelation(s.Team) is TeamRelation.Enemies && Unit.Transform.Distance(s.Transform) < Prototype.Range)
                {
                    target = s;
                    break;
                }
            }

        }

        if (target != null)
        {
            this.Transform.Rotation = MathHelper.Step(this.Transform.Rotation, Angle.FromVector((target.Transform.Position - this.Unit.Transform.Position).ToVector2()), .1f);
            
            if (MissilesRemaining > 0 && timeSinceMissile > Prototype.FireInterval)
            {
                Fire(target);
            }
            if (target.Health <= 0 || target.Transform.Distance(this.Transform) > Prototype.Range)
            {
                target = null;
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
        var missile = new Missile(Prototypes.Get<MissilePrototype>("missile"), World.NewID())
        {
            Target = target,
            TargetOffset = DoubleVector.FromVector2(World.TickRandom.NextUnitVector2() * World.TickRandom.NextSingle() * 1.5f),
        };

        missile.Teleport(Transform.Create(this.Unit.GetCenter(), this.Transform.Rotation + (World.TickRandom.NextSingle() - .5f) * MathF.PI / 10f));
        World.Add(missile);

        MissilesRemaining--;
        timeSinceMissile = 0;
    }

    //public override void Serialize(BinaryWriter writer)
    //{
    //    writer.Write(ID);
    //    writer.Write(Unit);
    //    writer.Write(target);
    //    writer.Write(MissilesRemaining);
    //    writer.Write(timeSinceMissile);
    //}
}