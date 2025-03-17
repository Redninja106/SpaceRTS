using SpaceGame.Ships;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Commands;
internal class CancelOrdersCommand : Command
{
    public Unit target;

    public CancelOrdersCommand(CommandPrototype prototype, Unit target) : base(prototype)
    {
        this.target = target;
    }

    public override void Apply()
    {
        Ship s = (Ship)target;
        s.orders.Clear();
    }

    public override void Serialize(BinaryWriter writer)
    {
        writer.Write(target.AsReference());
    }
}

class CancelOrdersCommandPrototype : CommandPrototype
{
    public override Actor Deserialize(BinaryReader reader)
    {
        ActorReference<Unit> target = reader.ReadActorReference<Unit>();

        return new CancelOrdersCommand(this, target.Actor!);
    }

    public override void Issue(Unit? target, HashSet<Unit> selected, PlayerCommandProcessor processor)
    {
        throw new NotImplementedException();
    }
    public override bool Applies(Unit? target, HashSet<Unit> selected)
    {
        throw new NotImplementedException();
    }
}