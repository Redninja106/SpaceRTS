//using SpaceGame.GUI;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.InteropServices;
//using System.Text;
//using System.Threading.Tasks;

//namespace SpaceGame.Structures;
//internal abstract class ZoneBehavior : StructureBehavior
//{
//    protected StructurePrototype[] upgrades;

//    public ZoneBehavior(Structure instance) : base(instance)
//    {
//    }

//    public override Element[] SelectGUI => gui;
//    private Element[] gui;

//    public override void Update()
//    {
//    }

//    public void OnFootprintChanged()
//    {
//        bool matched = false;
//        foreach (var upgrade in upgrades)
//        {
//            if (MatchShape(upgrade.Footprint, out var location, out int rotation))
//            {
//                matched = true;
//                gui = [new TextButton("upgrade to " + upgrade.Title, () =>
//                {
//                    Structure.Grid.RemoveStructure(Structure);
//                    World.SelectionHandler.Deselect(Structure);
//                    Structure.Grid.PlaceStructure(upgrade, location, rotation, World.PlayerTeam);
//                })];
//            }
//        }

//        if (!matched)
//        {
//            gui = [];
//        }
//    }

//    public static bool CompareFootprints(Span<HexCoordinate> footprint1, Span<HexCoordinate> footprint2, HexCoordinate location2, int rotation2)
//    {
//        if (footprint1.Length != footprint2.Length)
//            return false;

//        HashSet<HexCoordinate> set = [];

//        for (int i = 0; i < footprint1.Length; i++)
//        {
//            set.Add(footprint1[i]);
//        }

//        for (int i = 0; i < footprint2.Length; i++)
//        {
//            if (!set.Contains(location2 + footprint2[i].Rotated(rotation2)))
//            {
//                return false;
//            }
//        }

//        return true;
//    }

//    public bool MatchShape(HexCoordinate[] shape, out HexCoordinate location, out int rotation)
//    {
//        if (this.Structure.Footprint?.Count != shape.Length)
//        {
//            location = default;
//            rotation = default;
//            return false;
//        }

//        var footprintSpan = CollectionsMarshal.AsSpan(Structure.Footprint);
//        for (int i = 0; i < this.Structure.Footprint.Count; i++)
//        {
//            for (int j = 0; j < 6; j++)
//            {
//                if (CompareFootprints(footprintSpan, shape, footprintSpan[i], j))
//                {
//                    location = Structure.Location + footprintSpan[i];
//                    rotation = j;
//                    return true;
//                }
//            }
//        }

//        location = default;
//        rotation = default;
//        return false;
//    }
//}
