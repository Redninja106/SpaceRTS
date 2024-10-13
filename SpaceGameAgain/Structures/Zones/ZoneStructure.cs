using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;
internal class ZoneStructure : Structure
{
    public ColorF Color { get; }

    public ZoneStructure(string name, int price, Model model, List<HexCoordinate> footprint, Type? behaviorType, ColorF color) : base(name, price, model, footprint, behaviorType)
    {
        this.Color = color;
    }
}
