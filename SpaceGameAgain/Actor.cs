using SimulationFramework.Drawing;
using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    public Actor()
    {
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

    public virtual void Save(Stream stream)
    {
        stream.WriteValue(World.NetworkMap.GetID(this));
        stream.WriteValue(Transform);
    }
    public virtual void Load(Stream stream)
    {
        World.NetworkMap.Register(this, stream.ReadValue<int>());
        lastTransform = Transform = stream.ReadValue<Transform>();
    }

    internal uint ComputeCRC(uint crc)
    {
        crc = Util.CRC(crc, World.NetworkMap.GetID(this));
        crc = Util.CRC(crc, this.transform);
        return crc;
    }
}
