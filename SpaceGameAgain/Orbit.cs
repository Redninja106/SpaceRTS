using SpaceGame;
using SpaceGame.Planets;
using SpaceGame.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;
internal class Orbit : ISavable
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

    public Orbit()
    {
    }

    public void Load(Stream stream)
    {
        center = World.NetworkMap.GetActor(stream.ReadValue<int>());
        radius = stream.ReadValue<float>();
        phase = stream.ReadValue<float>();
    }

    public void Save(Stream stream)
    {
        stream.WriteValue(World.NetworkMap.GetID(center));
        stream.WriteValue(radius);
        stream.WriteValue(phase);
    }

    public void Tick(Actor actor, float speed)
    {
        phase += speed / radius;
        actor.Transform.Position = center.Transform.Position + Angle.ToVector(phase) * radius;
    }
}
