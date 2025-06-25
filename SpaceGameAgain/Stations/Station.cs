using SpaceGame;
using SpaceGame.GUI;
using SpaceGame.Planets;
using SpaceGame.Rendering;
using SpaceGame.Ships;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Stations;
internal abstract class Station(StationPrototype prototype, GameWorld world, ulong id) : Unit(prototype, world, id)
{
    public Orbit? orbit;

    // public override ITexture Icon => Icons.Defensive;

    public override bool TestPoint(DoubleVector point)
    {
        return DoubleVector.DistanceSquared(this.Transform.Position, point) <= this.GetCollisionRadius() * this.GetCollisionRadius();
    }

    public override void Tick()
    {
        base.Tick();

        SphereOfInfluence? soi = World.GetSphereOfInfluence(this.Transform.Position);
        soi?.ApplyTickTo(this);
    }

    public override void Layout(GUIWindow window)
    {
    }
}


abstract class StationPrototype : UnitPrototype
{
    public override Type ActorType => typeof(Station);
}
