using SpaceGame.Ships;
using SpaceGame.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Planets;
internal class SphereOfInfluence
{
    public float Radius => planet.Radius * 4;

    public Planet planet;
    public DoubleVector lastUpdatePosition;
    public DoubleVector lastTickPosition;

    public SphereOfInfluence(Planet planet)
    {
        this.planet = planet;
        this.lastUpdatePosition = planet.InterpolatedTransform.Position;
        this.lastTickPosition = planet.Transform.Position;
    }

    public void Update()
    {
        lastUpdatePosition = planet.InterpolatedTransform.Position;
    }

    public void Tick()
    {
        lastTickPosition = planet.Transform.Position;
    }

    public void ApplyTickTo(WorldActor actor)
    {
        ApplyTickTo(ref actor.Transform);
    }

    public void ApplyTickTo(ref Transform transform)
    {
        var delta = planet.Transform.Position - lastTickPosition;
        transform.Position += delta;
    }

    public DoubleVector ApplyTickTo(DoubleVector position)
    {
        var delta = planet.Transform.Position - lastTickPosition;
        return position + delta;
    }

    public void ApplyUpdateTo(ref Transform transform)
    {
        var delta = planet.InterpolatedTransform.Position - lastUpdatePosition;
        transform.Position += delta;
    }

    public DoubleVector ApplyUpdateTo(DoubleVector position)
    {
        var delta = planet.InterpolatedTransform.Position - lastUpdatePosition;
        return position + delta;
    }

    public void Render(ICanvas canvas)
    {
        canvas.Fill(Color.White);
        canvas.Stroke(Color.White with { A = 10 });
        canvas.DrawCircle(0, 0, Radius);
    }

    internal bool ContainsPoint(DoubleVector point)
    {
        return Vector2.Distance(planet.Transform.Position.ToVector2(), point.ToVector2()) <= Radius;
    }
}
