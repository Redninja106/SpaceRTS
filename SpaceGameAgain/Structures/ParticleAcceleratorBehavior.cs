using SpaceGame.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;
internal class ParticleAcceleratorBehavior : StructureBehavior
{
    public override Element[] SelectGUI { get; } = [
        new TextButton("run tests!", () => Console.WriteLine("I'm from the science team!"))
    ];

    public ParticleAcceleratorBehavior(StructureInstance instance) : base(instance)
    {
    }

    public override void Tick()
    {
    }
}
