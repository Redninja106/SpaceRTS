using SpaceGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;
internal class Orbit
{
    public Actor center;
    public float radius;
    public float phase;

    public Orbit(Actor center, float radius, float phase, float distance)
    {
        this.center = center;
        this.radius = radius;
        this.phase = phase;
    }

    public void Update(Actor actor, float speed)
    {
        phase += speed / radius;
        actor.Transform.Position = center.Transform.Position + Angle.ToVector(phase) * radius;
    }
}
