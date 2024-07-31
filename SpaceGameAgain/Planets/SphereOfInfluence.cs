using SpaceGame.Serialization;
using SpaceGame.Ships;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Planets;
internal class SphereOfInfluence : ISavable
{
    public float Radius => 4 * planet.Radius;

    public Planet planet;
    public Vector2 lastPosition;

    public SphereOfInfluence(Planet planet)
    {
        this.planet = planet;
        this.lastPosition = planet.Transform.Position;
    }

    public void Update()
    {
        lastPosition = planet.Transform.Position;
    }

    public void ApplyTo(Actor actor)
    {
        ApplyTo(ref actor.Transform);
    }

    public void ApplyTo(ref Transform transform)
    {
        var delta = planet.Transform.Position - lastPosition;
        transform.Position += delta;
    }

    public void ApplyTo(ref Vector2 position)
    {
        var delta = planet.Transform.Position - lastPosition;
        position += delta;
    }

    public void Render(ICanvas canvas)
    {
        canvas.Stroke(Color.White with { A = 100 });
        canvas.DrawCircle(0, 0, Radius);
    }

    internal bool ContainsPoint(Vector2 point)
    {
        return Vector2.Distance(planet.Transform.Position, point) <= Radius;
    }

    public void Save(Stream stream)
    {
        stream.WriteValue(World.NetworkMap.GetID(planet));
        stream.WriteValue(lastPosition);
    }

    public void Load(Stream stream)
    {
        planet = (Planet)World.NetworkMap.GetActor(stream.ReadValue<int>());
        lastPosition = stream.ReadValue<Vector2>();
    }
}
