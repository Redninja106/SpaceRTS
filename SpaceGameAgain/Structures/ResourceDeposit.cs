using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;
internal class ResourceDeposit : Structure
{
    public ResourceDeposit(StructurePrototype prototype, ulong id, ActorReference<Grid> grid, HexCoordinate location, int rotation, ActorReference<Team> team) : base(prototype, id, grid, location, rotation, team)
    {
    }
}

class ResourceDepositPrototype : StructurePrototype
{
    public ResourceDepositPrototype(string title, int price, Model model, string? presetModel, HexCoordinate[] footprint) : base(title, price, model, presetModel, footprint)
    {
    }
}
