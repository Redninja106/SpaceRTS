using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;
internal class Radar : Structure
{
    public override RadarPrototype Prototype => (RadarPrototype)base.Prototype;

    public Radar(RadarPrototype prototype, ulong id, ActorReference<Grid> grid, HexCoordinate location, int rotation, ActorReference<Team> team) : base(prototype, id, grid, location, rotation, team)
    {
    }

    public override double GetRevealRadius()
    {
        if (Powered)
        {
            return Prototype.PoweredRevealRadius;
        }

        return base.GetRevealRadius();
    }
}

class RadarPrototype : StructurePrototype
{
    public float PoweredRevealRadius { get; set; }

    public override Structure CreateStructure(ulong id, ActorReference<Team> team, ActorReference<Grid> grid, HexCoordinate location, int rotation)
    {
        return new Radar(this, id, grid, location, rotation, team);
    }
}
