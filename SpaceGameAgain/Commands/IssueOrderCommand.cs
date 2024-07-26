using SpaceGame.Ships.Orders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Commands;
internal class IssueOrderCommand : Command
{
    public Order order;
}
