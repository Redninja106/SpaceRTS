using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Orders;
internal class AttackOrderPrototype : OrderPrototype
{
    public override WorldActor Deserialize(BinaryReader reader)
    {
        DeserializeArgs(reader, out var id, unit: out var unit);
        ActorReference<Unit> target = reader.ReadActorReference<Unit>();

        return new AttackOrder(this, id, unit, target);
    }
}
