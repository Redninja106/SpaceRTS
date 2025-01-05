using SpaceGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;
internal class Orbit
{
    public ActorReference<WorldActor> center;
    public float radius;
    public float phase;

    public Orbit(ActorReference<WorldActor> center, float radius, float phase)
    {
        this.center = center;
        this.radius = radius;
        this.phase = phase;
    }

    public void Update(WorldActor actor, float speed)
    {
        phase += speed / radius;
        actor.Transform.Position = center.Actor!.Transform.Position + Angle.ToVector(phase) * radius;
    }
}
