using SpaceGame.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Bots;

// index of friendly units and which strategies use them
internal class FriendlyUnitIndex
{
    // the strategy each unit belongs to
    // construction ships belong to expansion strategies
    // small fast ships belong to scouting strategies etc
    private Dictionary<Unit, Strategy> unitStrategies = [];


}

// index of enemy units on a single team
class EnemyUnitIndex
{
    private HashSet<UnitKnowledge> knownUnits;
}

struct UnitKnowledge
{
    public int id;
    public int threat;
}