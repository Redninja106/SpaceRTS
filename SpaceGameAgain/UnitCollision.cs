using SpaceGame.Ships;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;
internal class UnitCollision
{
    public const double BinSize = 5;

    private Dictionary<BinPosition, CollisionBin> bins = [];

    public UnitCollision()
    {
    }

    public List<Unit> GetBin(int binX, int binY)
    {
        return bins.TryGetValue(new(binX, binY), out var value) ? value.Units : [];
    }

    public void ClearBins()
    {
        foreach (var (pos, bin) in bins)
        {
            bin.Clear();

            if (bin.Units.Count == 0)
            {
                bins.Remove(pos);
            }
        }
    }

    public Unit? TestPoint(DoubleVector point)
    {
        BinPosition binPosition = new((int)Math.Floor(point.X / BinSize), (int)Math.Floor(point.Y / BinSize));

        if (bins.TryGetValue(binPosition, out CollisionBin bin))
        {
            foreach (var unit in bin.Units)
            {
                if (unit.TestPoint(point))
                {
                    return unit;
                }
            }
        }

        return null;
    }

    public void Update()
    {
        foreach (var ship in World.Ships)
        {
            InsertUnit(ship);
        }
        foreach (var structure in World.Structures)
        {
            InsertUnit(structure);
        }
    }

    public void InsertUnit(Unit unit)
    {
        double collisionRadius = unit.GetCollisionRadius();
        DoubleVector binPosition = unit.Transform.Position;
        BinPosition min = new BinPosition(
            (int)Math.Floor((unit.Transform.Position.X - collisionRadius) / BinSize), 
            (int)Math.Floor((unit.Transform.Position.Y - collisionRadius) / BinSize)
            ); 
        BinPosition max = new BinPosition(
            (int)Math.Ceiling((unit.Transform.Position.X + collisionRadius) / BinSize),
            (int)Math.Ceiling((unit.Transform.Position.Y + collisionRadius) / BinSize)
            );

        for (int y = min.Y; y < max.Y; y++)
        {
            for (int x = min.X; x < max.X; x++)
            {
                BinPosition pos = new(x, y);
                if (bins.TryGetValue(pos, out CollisionBin bin))
                {
                    bin.Units.Add(unit);
                }
                else 
                {
                    bins.Add(pos, new() { Units = [unit] });
                }
            }
        }
    }

    [DebugOverlay]
    public static void ShowCollisionBins()
    {
        foreach (var (pos, bin) in World.Collision.bins)
        {
            DebugDraw.Rectangle(new(0, 0, (float)BinSize, (float)BinSize), new Transform { Position = new(pos.X * BinSize, pos.Y * BinSize) });
        }
    }

    [DebugOverlay]
    public static void ShowCollisionRadius()
    {
        foreach (var ship in World.Ships)
        {
            DebugDraw.Circle(new(0, 0, (float)ship.GetCollisionRadius()), ship.Transform);
        }
        foreach (var structure in World.Structures)
        {
            DebugDraw.Circle(new(0, 0, (float)structure.GetCollisionRadius()), structure.Transform);
        }
    }

    private struct BinPosition
    {
        public int X;
        public int Y;

        public BinPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }
    }

    struct CollisionBin
    {
        public List<Unit> Units = [];

        public CollisionBin()
        {
        }

        public void Clear() 
        {
            Units.Clear();
        }
    }
}