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

    public virtual Transform TweenedTransform { get; private set; }

    private Transform lastTransform = Transform.Default;
    private Transform transform = Transform.Default;
    private static int nextId = 1;

    public int ID { get; init; } = nextId++;

    public Actor()
    {
        World.NetworkMap.Register(ID, this);
    }

    public virtual void Update(float tickProgress)
    {
        TweenedTransform = Transform.Lerp(lastTransform, transform, tickProgress);
    }

    public virtual void Render(ICanvas canvas)
    {
    }

    public virtual void Tick()
    {
        lastTransform = transform;
    }
}