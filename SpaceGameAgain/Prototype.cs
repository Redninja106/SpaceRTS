using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;
internal abstract class Prototype
{
    public string Name { get; set; }

    public abstract Actor Deserialize(BinaryReader reader);
}
