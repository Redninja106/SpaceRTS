using SpaceGame.Orders;
using SpaceGame.Serialization;
using SpaceGame.Ships;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Commands;

[Serializable]
internal class AttackCommand : Command
{
    [Serialize] public Unit unit;
    [Serialize] public Unit target;

    public AttackCommand(Unit unit, Unit target)
    {
        this.unit = unit;
        this.target = target;
    }

    public override void Apply()
    {
        var order = new AttackOrder()
        {
            target = target
        };

        if (unit is Ship s)
        {
            s.EnqueueOrder(order);
        }
    }
}
