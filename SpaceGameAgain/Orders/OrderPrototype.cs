using SpaceGame.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Orders;
internal abstract class OrderPrototype : WorldActorPrototype
{
    public void DeserializeArgs(BinaryReader reader, out ulong id, out ActorReference<Unit> unit)
    {
        id = reader.ReadUInt64();
        unit = reader.ReadActorReference<Unit>();
    }
}
