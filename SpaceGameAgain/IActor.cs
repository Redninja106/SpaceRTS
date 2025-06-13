using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;
internal interface IActor
{
    public Prototype Prototype { get; }
    public ulong ID { get; }
    public Transform InterpolatedTransform { get; }
    public ref Transform Transform { get; }
    public ref Transform PreviousTransform { get; }

    void Teleport(Transform transform);
}

