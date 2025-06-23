using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Planets;

[Serializable]
internal class Orbit
{
    [Serialize]
    public required Actor center;
    [Serialize]
    public required float radius;
    [Serialize]
    public required float phase;
    [Serialize]
    public required float speed = 1;

    public Orbit()
    {
    }

    [SetsRequiredMembers]
    public Orbit(Actor center, float radius, float phase)
    {
        this.center = center;
        this.radius = radius;
        this.phase = phase;
    }

    public void Tick()
    {
        phase += Program.Timestep * speed / radius;
    }

    public Transform GetLocation()
    {
        return center.Transform.Translated(DoubleVector.FromVector2(Angle.ToVector(phase) * radius));
    }

    public DoubleVector Forecast(float time)
    {
        DoubleVector newCenter = (center as Planet)!.orbit?.Forecast(time) ?? center.Transform.Position;
        float newPhase = phase + time * speed / radius;

        return newCenter + new DoubleVector(double.Cos(newPhase) * radius, double.Sin(newPhase) * radius);
    }

    // public void Apply(WorldActor actor)
    // {
    //     actor.Transform.Position = center.Transform.Position + DoubleVector.FromVector2(Angle.ToVector(phase) * radius);
    // }
}
