using SpaceGame.Bots;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.AI;
internal abstract class Strategy
{
    protected BotCommandProcessor Processor { get; private set; }

    public Strategy(BotCommandProcessor processor)
    {
        this.Processor = processor;
    }

    public abstract void Think();
}

class ScoutingStrategy : Strategy
{
    public ScoutingStrategy(BotCommandProcessor processor) : base(processor)
    {
    }

    public override void Think()
    {
        // find idle scouting ships and move them to unknown planets
        
        //  if not enough scouting ships
        //      request scouting ship from shipbuilding strategy

        //  for each scouting ship:
        //      if is under threat:
        //          find closest safe planet & move there
    }
}
