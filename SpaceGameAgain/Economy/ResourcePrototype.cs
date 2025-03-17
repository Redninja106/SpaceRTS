using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Economy;
internal class ResourcePrototype : Prototype
{
    public override Actor Deserialize(BinaryReader reader)
    {
        throw new NotSupportedException();
    }
}
