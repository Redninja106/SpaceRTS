using SimulationFramework.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;
internal class Actor
{
    public virtual ref Transform Transform => ref transform;
        
    private Transform transform = Transform.Default;

    public virtual void Update()
    {
    }

    public virtual void Render(ICanvas canvas)
    {
    }

    public virtual void Tick()
    {
    }
}