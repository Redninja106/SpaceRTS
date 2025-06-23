using SpaceGame.Orders;
using SpaceGame.Ships;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Commands;
//internal class MoveCommand : Command
//{
//    public Ship ship;
//    public DoubleVector target;

//    public MoveCommand(Ship ship, DoubleVector target)
//    {
//        this.ship = ship;
//        this.target = target;
//    }

//    //public MoveCommand(MoveCommandPrototype prototype, Ship ship, DoubleVector target) : base(prototype)
//    //{
//    //    this.ship = ship;
//    //    this.target = target;
//    //}

//    public override void Apply()
//    {
//        var order = new MoveOrder() { Unit = ship, target = target };
//        ship.orders.Enqueue(order);
//    }

//    //public override void Serialize(BinaryWriter writer)
//    //{
//    //    writer.Write(ship.AsReference());
//    //    writer.Write(target);
//    //}
//}

[Serializable]
class IssueOrdersCommand : Command
{
    [field: Serialize]
    public required Order[] Orders { get; set; }
    [field: Serialize]
    public required bool ClearOrders { get; set; }
    [field: Serialize]
    public required Ship Ship { get; set; }

    public override void Apply()
    {
        if (ClearOrders)
        {
            Ship.ClearOrders();
        }

        foreach (var order in Orders)
        {
            Ship.EnqueueOrder(order);
        }
    }
}

//class MoveCommandPrototype : CommandPrototype
//{
//    public override MoveCommand Deserialize(BinaryReader reader)
//    {
//        ActorReference<Ship> s = reader.ReadActorReference<Ship>();
//        DoubleVector v = reader.ReadDoubleVector();

//        return new MoveCommand(this, s, v);
//    }

//    //public override void Issue(Unit? target, HashSet<Unit> selected, PlayerCommandProcessor processor)
//    //{
//    //    foreach (var unit in selected)
//    //    {
//    //        DoubleVector targetPosition;
//    //        if (target == null)
//    //        {
//    //            targetPosition = World.MousePosition;
//    //        }
//    //        else
//    //        {
//    //            targetPosition = target.Transform.Position;
//    //        }

//    //        processor.AddCommand(new MoveCommand(this, (Ship)unit!, targetPosition));
//    //    }
//    //}

//    //public override bool Applies(Unit? target, HashSet<Unit> selected)
//    //{
//    //    return selected.Count > 0;
//    //}
//}
