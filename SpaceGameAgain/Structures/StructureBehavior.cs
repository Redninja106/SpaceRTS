using SpaceGame.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;
internal abstract class StructureBehavior
{
    public StructureInstance Instance { get; }

    public abstract Element[] SelectGUI { get; }

    public abstract void Update();
    
    public StructureBehavior(StructureInstance instance)
    {
        this.Instance = instance;
    }

}
