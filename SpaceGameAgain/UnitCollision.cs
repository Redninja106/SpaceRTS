using SpaceGame.Ships;
using SpaceGame.Structures;
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

    public List<WorldActor> GetBin(int binX, int binY)
    {
        return bins.TryGetValue(new(binX, binY), out var value) ? value.actors : [];
    }

    public void ClearBins()
    {
        foreach (var (pos, bin) in bins)
        {
            bin.Clear();

            if (bin.actors.Count == 0)
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
            foreach (var actor in bin.actors)
            {
                switch (actor)
                {
                    case Unit unit:
                        if (unit.TestPoint(point))
                        {
                            return unit;
                        }
                        break;
                    case Grid grid:
                        if (grid.GetCellFromPoint(point) is GridCell cell)
                        {
                            return cell.Structure.Actor!;
                        }
                        break;
                }
            }
        }

        return null;
    }

    public void Update()
    {
        foreach (var ship in World.Ships)
        {
            Insert(ship, ship.GetCollisionRadius());
        }
        
        foreach (var grid in World.Grids)
        {
            Insert(grid, grid.CollisionRadius);
        }
    }

    public void Insert(WorldActor actor, double collisionRadius)
    {
        DoubleVector binPosition = actor.Transform.Position;
        BinPosition min = new BinPosition(
            (int)Math.Floor((actor.Transform.Position.X - collisionRadius) / BinSize), 
            (int)Math.Floor((actor.Transform.Position.Y - collisionRadius) / BinSize)
            ); 
        BinPosition max = new BinPosition(
            (int)Math.Ceiling((actor.Transform.Position.X + collisionRadius) / BinSize),
            (int)Math.Ceiling((actor.Transform.Position.Y + collisionRadius) / BinSize)
            );

        for (int y = min.Y; y < max.Y; y++)
        {
            for (int x = min.X; x < max.X; x++)
            {
                BinPosition pos = new(x, y);
                if (bins.TryGetValue(pos, out CollisionBin bin))
                {
                    bin.actors.Add(actor);
                }
                else 
                {
                    bins.Add(pos, new() { actors = [actor] });
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
        foreach (var grid in World.Grids)
        {
            DebugDraw.Circle(new(0, 0, (float)grid.CollisionRadius), grid.Transform);
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
        public List<WorldActor> actors = [];

        public CollisionBin()
        {
        }

        public void Clear() 
        {
            actors.Clear();
        }
    }
}