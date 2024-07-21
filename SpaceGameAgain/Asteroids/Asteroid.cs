using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Asteroids;
internal class Asteroid : UnitBase
{
    public Orbit? Orbit;
    public float size;

    public Asteroid()
    {
    }

    public override void Damage()
    {
    }

    public override void Render(ICanvas canvas)
    {
        canvas.Fill(Color.DarkSlateGray);
        canvas.DrawCircle(0, 0, size);
    }

    public override bool TestPoint(Vector2 point, Transform transform)
    {
        return Vector2.Distance(transform.LocalToWorld(point), this.Transform.Position) < size;
    }

    public override void Update()
    {
        Orbit?.Update(this, Time.DeltaTime);
    }
}
