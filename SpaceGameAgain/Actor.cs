using SimulationFramework.Drawing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;
internal class Actor : ITransformed
{
    public virtual ref Transform Transform => ref transform;
        
    private Transform transform = Transform.Default;

    Transform ITransformed.Transform => this.Transform;

    public virtual void Update()
    {
    }

    public virtual void Render(ICanvas canvas)
    {
    }
}

interface ITransformed
{
    Transform Transform { get; }
}
