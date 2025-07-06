using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Planets;
internal class BlackHole : Planet
{
    public BlackHole(PlanetPrototype prototype, GameWorld world, ulong id) : base(prototype, world, id)
    {
    }

    public override void Render(ICanvas canvas)
    {
        canvas.Fill(ColorF.Black);
        canvas.DrawCircle(0, 0, Radius);
    }
}

class BlackHolePrototype : PlanetPrototype
{
    public override Type ActorType => typeof(BlackHole);
}
