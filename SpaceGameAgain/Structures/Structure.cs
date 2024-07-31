using SpaceGame.Serialization;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;

[RegistryItem(nameof(Registries.Structures))]
internal class Structure
{
    public string Name { get; }
    public string Title { get; }
    public Model Model { get; }
    public List<HexCoordinate> Footprint { get; }
    public Type? BehaviorType { get; }
    public Vector2[] Outline { get; }
    public int Price { get; }

    public Structure(string name, int price, Model model, List<HexCoordinate> footprint, Type? behaviorType)
    {
        this.Name = name;
        this.Price = price;
        this.Title = name.ToUpper();
        this.Model = model;
        this.Footprint = footprint;
        this.BehaviorType = behaviorType;

        this.Outline = CreateOutline(footprint);
    }

    private Vector2[] CreateOutline(List<HexCoordinate> footprint)
    {
        List<Vector2> segments = [];

        foreach (var coordinate in footprint)
        {
            for (int i = 0; i < 6; i++)
            {
                if (!footprint.Contains(coordinate + HexCoordinate.UnitQ.Rotated(i)))
                {
                    segments.Add(coordinate.ToCartesian() + Angle.ToVector((i + 0) * (MathF.Tau / 6f)));
                    segments.Add(coordinate.ToCartesian() + Angle.ToVector((i + 1) * (MathF.Tau / 6f)));
                }
            }
        }

        return segments.ToArray();
    }

    public StructureInstance CreateInstance(Grid grid, HexCoordinate location, int rotation, Team team)
    {
        return new StructureInstance(location, rotation, grid, this, team, BehaviorType);
    }
}
