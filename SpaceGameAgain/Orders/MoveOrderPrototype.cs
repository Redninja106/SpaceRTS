using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Orders;
internal class MoveOrderPrototype : OrderPrototype
{
    public override WorldActor Deserialize(BinaryReader reader)
    {
        DeserializeArgs(reader, out var id, unit: out var unit);
        Vector2 target = reader.ReadVector2();
        return new MoveOrder(this, id, unit, target);
    }
}
