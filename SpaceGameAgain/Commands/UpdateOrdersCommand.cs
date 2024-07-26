using SpaceGame.Ships;
using SpaceGame.Ships.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Commands;
internal class UpdateOrdersCommand : Command
{
    public Ship ship;
    public Order[] orders;

    public override void Apply()
    {
        ship.orders = [];
        for (int i = 0; i < orders.Length; i++)
        {
            ship.orders.Enqueue(orders[i]);
        }
    }
}
