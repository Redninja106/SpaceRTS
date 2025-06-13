using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Data;
internal class DataPrototype : Prototype
{
    public override Type ActorType => throw new Exception("DataPrototype does not have an actor type");
}
