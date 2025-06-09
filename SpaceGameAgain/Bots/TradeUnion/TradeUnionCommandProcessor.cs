using SpaceGame.Commands;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Bots.TradeUnion;
internal class TradeUnionCommandProcessor : BotCommandProcessor
{
    public TradeUnionCommandProcessor(Team team) : base(team)
    {
    }
}
