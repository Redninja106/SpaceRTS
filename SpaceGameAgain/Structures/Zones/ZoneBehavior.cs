using SpaceGame.GUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;
internal abstract class ZoneBehavior : StructureBehavior
{
    protected Structure[] upgrades;

    public ZoneBehavior(StructureInstance instance) : base(instance)
    {
    }

    public override Element[] SelectGUI => gui;
    private Element[] gui;

    public override void Update()
    {
    }

    public void OnFootprintChanged()
    {
        bool matched = false;
        foreach (var upgrade in upgrades)
        {
            if (MatchShape(upgrade.Footprint, out var location, out int rotation))
            {
                matched = true;
                gui = [new TextButton("upgrade to " + upgrade.Name, () =>
                {
                    Instance.Grid.RemoveStructure(Instance);
                    World.SelectionHandler.Deselect(Instance);
                    Instance.Grid.PlaceStructure(upgrade, location, rotation, World.PlayerTeam);
                })];
            }
        }

        if (!matched)
        {
            gui = [];
        }
    }

    public static bool CompareFootprints(Span<HexCoordinate> footprint1, Span<HexCoordinate> footprint2, HexCoordinate location2, int rotation2)
    {
        if (footprint1.Length != footprint2.Length)
            return false;

        HashSet<HexCoordinate> set = [];

        for (int i = 0; i < footprint1.Length; i++)
        {
            set.Add(footprint1[i]);
        }

        for (int i = 0; i < footprint2.Length; i++)
        {
            if (!set.Contains(location2 + footprint2[i].Rotated(rotation2)))
            {
                return false;
            }
        }

        return true;
    }

    public bool MatchShape(List<HexCoordinate> shape, out HexCoordinate location, out int rotation)
    {
        if (this.Instance.Footprint?.Count != shape.Count)
        {
            location = default;
            rotation = default;
            return false;
        }

        var footprintSpan = CollectionsMarshal.AsSpan(Instance.Footprint);
        var shapeSpan = CollectionsMarshal.AsSpan(shape);
        for (int i = 0; i < this.Instance.Footprint.Count; i++)
        {
            for (int j = 0; j < 6; j++)
            {
                if (CompareFootprints(footprintSpan, shapeSpan, footprintSpan[i], j))
                {
                    location = Instance.Location + footprintSpan[i];
                    rotation = j;
                    return true;
                }
            }
        }

        location = default;
        rotation = default;
        return false;
    }
}
