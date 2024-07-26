using SpaceGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Stations;
internal class Station : Actor
{
    public Orbit? orbit;

    public Station()
    {
        orbit = new(World.Planets[0], 50, 0, 0);
    }

    public override void Update(float tickProgress)
    {
        orbit?.Tick(this, 0);
    }

    public override void Render(ICanvas canvas)
    {
        canvas.DrawRect(0, 0, 10, 10, Alignment.Center);
    }
}
