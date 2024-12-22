using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;
internal class TurretPrototype : StructurePrototype
{
    public override Type ActorType => typeof(Turret);

    public string TurretKind { get; set; }

    public override Structure CreateStructure(ulong id, Grid grid, HexCoordinate location, int rotation, Team team)
    {
        return new Turret(this, id, grid, location, rotation, team.AsReference());
    }

    public TurretPrototype(string title, int price, Model model, string? presetModel, HexCoordinate[] footprint) : base(title, price, model, presetModel, footprint)
    {
    }
}
