using SpaceGame.Ships;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Commands;

[Serializable]
internal class CancelOrdersCommand : Command
{
    [Serialize]
    public required Unit target;

    public override void Apply()
    {
        Ship s = (Ship)target;
        s.orders.Clear();
    }
}
