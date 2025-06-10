using SpaceGame.Commands;

namespace SpaceGame;

class TurnInfo
{
    public SortedDictionary<ulong, List<Command>> playerCommands = [];
}