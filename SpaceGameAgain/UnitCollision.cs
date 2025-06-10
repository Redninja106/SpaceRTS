using Silk.NET.Input;
using SimulationFramework.Input;
using SpaceGame.Debugging;
using SpaceGame.Ships;
using SpaceGame.Structures;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;

class UnitCollision
{
    public const int LayerCount = 7;

    struct Bin(List<Unit> units, ulong lastClientVisibleTick)
    {
        public List<Unit>? Units { get; set; } = units;
        public ulong LastClientVisibleTick { get; set; } = lastClientVisibleTick;
    }
    
    private readonly ChunkedSpatialHash<Bin>[] layers;

    public UnitCollision()
    {
        layers = new ChunkedSpatialHash<Bin>[LayerCount];
        for (int i = 0; i < layers.Length; i++)
        {
            layers[i] = new(1 << (14 - i), 1 << (14 - i), 1 << i);
        }
    }

    public void Update()
    {
        DebugMenu.PushMetric();
        //foreach (var layer in layers)
        //{
        //    //if (World.tick - bin.LastClientVisibleTick > 50 && bin.Units.Count == 0)
        //    //{

        //    //}
        //}

        DebugMenu.PushMetric("Clear bins");
        foreach (var layer in layers)
        {
            foreach (var chunk in layer.Chunks)
            {
                for (int i = 0; i < chunk.Count; i++)
                {
                    chunk[i].Units?.Clear();
                }
            }
        }
        DebugMenu.PopMetric();

        DebugMenu.PushMetric("Insert Units");
        InsertUnits(World.Ships);
        InsertUnits(World.Structures);
        DebugMenu.PopMetric();

        DebugMenu.PushMetric("Reveal Units");
        RevealUnits(World.Ships);
        RevealUnits(World.Structures);
        DebugMenu.PopMetric();

        DebugMenu.PushMetric("Prune bins");
        foreach (var layer in layers)
        {
            layer.Prune(100);
        }
        DebugMenu.PopMetric();

        DebugMenu.PopMetric();
    }

    private void RevealUnits(IEnumerable<Unit> units)
    {
        foreach (var unit in units)
        {
            if (unit.Team == World.PlayerTeam)
            {
                DoubleVector position = unit switch
                {
                    Ship ship => ship.Transform.Position,
                    Structure structure => structure.GetCenter()
                };

                RevealCircle(position, (float)unit.GetRevealRadius());
            }
        }
    }

    private void InsertUnits(IEnumerable<Unit> units)
    {
        foreach (var unit in units)
        {
            InsertUnit(unit);
        }
    }

    // just the most recent from any layer
    public ulong GetLastClientVisibleTick(DoubleVector position)
    {
        ulong latest = 0;
        for (int i = 0; i < LayerCount; i++)
        {
            if (!layers[i].TryGetValue(position, out Bin bin))
            {
                continue;
            }

            ulong lastClientVisibleTick = bin.LastClientVisibleTick;
            if (lastClientVisibleTick > latest)
            {
                latest = lastClientVisibleTick;
            }
        }
        return latest;
    }

    public bool IsClientVisible(DoubleVector position)
    {
        return World.tick - GetLastClientVisibleTick(position) < 50;
    }

    public IEnumerable<Unit> TestPoint(DoubleVector point)
    {
        for (int i = 0; i < layers.Length; i++)
        {
            if (layers[i].TryGetValue(point, out Bin bin)) 
            {
                if (bin.Units != null)
                {
                    foreach (var u in bin.Units)
                    {
                        if (u.TestPoint(point))
                        {
                            yield return u;
                        }
                    }
                }
            }
        }
    }

    public void InsertUnit(Unit unit)
    {
        double collisionRadius = unit.GetCollisionRadius();
        int targetLayer = int.Clamp((int)double.Ceiling(double.Log2(collisionRadius * 2)), 0, LayerCount);
        DoubleVector position = unit switch
        {
            Ship ship => ship.Transform.Position,
            Structure structure => structure.GetCenter()
        };

        DoubleVector tl = position + new DoubleVector(-collisionRadius, -collisionRadius);
        DoubleVector tr = position + new DoubleVector(+collisionRadius, -collisionRadius);
        DoubleVector bl = position + new DoubleVector(-collisionRadius, +collisionRadius);
        DoubleVector br = position + new DoubleVector(+collisionRadius, +collisionRadius);

        AddUnit(unit, tl, targetLayer, layers);
        AddUnit(unit, tr, targetLayer, layers);
        AddUnit(unit, bl, targetLayer, layers);
        AddUnit(unit, br, targetLayer, layers);

        static void AddUnit(Unit unit, DoubleVector position, int targetLayer, ChunkedSpatialHash<Bin>[] layers)
        {
            ref Bin bin = ref layers[targetLayer][position];
            bin.Units ??= [];
            bin.Units.Add(unit);
        }
    }

    public void RevealCircle(DoubleVector position, float radius)
    {
        int initialLayer = LayerCount - 1;
        int initialScale = 1 << initialLayer;

        int minX = (int)Math.Floor((position.X - radius) / initialScale);
        int minY = (int)Math.Floor((position.Y - radius) / initialScale);
        int maxX = (int)Math.Ceiling((position.X + radius) / initialScale);
        int maxY = (int)Math.Ceiling((position.Y + radius) / initialScale);

        for (int y = minY; y < maxY; y++)
        {
            for (int x = minX; x < maxX; x++)
            {
                RevealCircleHelper(x, y, initialLayer, position, radius);
            }
        }

        void RevealCircleHelper(int x, int y, int layer, DoubleVector circlePosition, float circleRadius)
        {
            // early out - don't reveal something that's already revealed
            if (layers[layer].TryGetValue(x, y, out Bin b) && b.LastClientVisibleTick == World.tick)
            {
                return;
            }

            int scale = 1 << layer;

            double left = x * scale;
            double right = (x + 1) * scale;
            double top = y * scale;
            double bottom = (y + 1) * scale;

            bool fullCoverage = PointInCircle(new(left, top), circlePosition, circleRadius)
                                && PointInCircle(new(right, top), circlePosition, circleRadius)
                                && PointInCircle(new(left, bottom), circlePosition, circleRadius)
                                && PointInCircle(new(right, bottom), circlePosition, circleRadius);

            if (fullCoverage)
            {
                // this cell is entirely in the circle, set it and return
                // if the cell doesn't exist, we recursive since it might exist on a lower level
                //if (this.layers[layer].TryGetValue(x, y, out Bin bin)) 
                //{
                    this.layers[layer][x, y].LastClientVisibleTick = World.tick;
                //}
                return;

                //if (this.layers[layer][x, y] is Bin bin)
                //{o
                //    bin.LastClientVisibleTick = World.tick;
                //}
                //else
                //{
                //    this.layers[layer][x, y] = new Bin([], World.tick);
                //}
            }

            if (fullCoverage || RectCircleIntersect(top, left, right, bottom, circlePosition, circleRadius))
            {
                // partial coverage

                if (layer == 0)
                {
                    // cannot recurse, set the layer 0 cell
                    //this.layers[0][x, y].LastClientVisibleTick = World.tick;
                    if (this.layers[layer].TryGetValue(x, y, out Bin bin))
                    {
                        this.layers[layer][x, y].LastClientVisibleTick = World.tick;
                    }
                    return;
                }

                RevealCircleHelper(x * 2 + 0, y * 2 + 0, layer - 1, circlePosition, circleRadius);
                RevealCircleHelper(x * 2 + 1, y * 2 + 0, layer - 1, circlePosition, circleRadius);
                RevealCircleHelper(x * 2 + 0, y * 2 + 1, layer - 1, circlePosition, circleRadius);
                RevealCircleHelper(x * 2 + 1, y * 2 + 1, layer - 1, circlePosition, circleRadius);
            }
        }
    }

    // https://stackoverflow.com/questions/401847/circle-rectangle-collision-detection-intersection
    static bool RectCircleIntersect(double top, double left, double right, double bottom, DoubleVector circlePosition, double circleRadius)
    {
        double halfWidth = (right - left) / 2;
        double halfHeight = (bottom - top) / 2;

        double cdx = double.Abs(circlePosition.X - (left + halfWidth));
        double cdy = double.Abs(circlePosition.Y - (top + halfHeight));

        if (cdx > (halfWidth + circleRadius) || cdy > (halfHeight + circleRadius)) return false;
        if (cdx <= halfWidth || cdy <= halfHeight) return true;
        
        return (cdx - halfWidth) * (cdx - halfWidth) + (cdy - halfHeight) * (cdy - halfHeight) <= (circleRadius * circleRadius);
    }

    //private static void CheckCellAgainstCircle(int x, int y, int scale, DoubleVector circlePosition, float circleRadius, out bool whole, out bool partial)
    //{
    //    double left = x * scale;
    //    double right = (x + 1) * scale;
    //    double top = y * scale;
    //    double bottom = (y + 1) * scale;

    //    bool tl = PointInCircle(new(left, top), circlePosition, circleRadius);
    //    bool tr = PointInCircle(new(right, top), circlePosition, circleRadius);
    //    bool bl = PointInCircle(new(left, bottom), circlePosition, circleRadius);
    //    bool br = PointInCircle(new(right, bottom), circlePosition, circleRadius);

    //    whole   = tl && tr && bl && br;
    //    partial = tl || tr || bl || br;
    //}

    private static bool PointInCircle(DoubleVector point, DoubleVector circlePosition, float circleRadius)
    {
        double ox = point.X - circlePosition.X;
        double oy = point.Y - circlePosition.Y;

        return ox * ox + oy * oy <= circleRadius * circleRadius;
    }


    [DebugOverlay]
    public static void ShowBinVisibility()
    {
        double cx = World.Camera.SmoothTransform.Position.X;
        double cy = World.Camera.SmoothTransform.Position.Y;
        double w = World.Camera.SmoothVerticalSize * World.Camera.AspectRatio;
        double h = World.Camera.SmoothVerticalSize;

        int minX = (int)Math.Floor(cx - w * .5);
        int minY = (int)Math.Floor(cy - h * .5);
        int maxX = (int)Math.Ceiling(cx + w * .5);
        int maxY = (int)Math.Ceiling(cy + h * .5);

        if (maxX - minX > 500 || maxY - minY > 500)
        {
            return;
        }

        for (int y = minY; y < maxY; y++)
        {
            for (int x = minX; x < maxX; x++)
            {
                ulong lastClientVisibleTick = World.Collision.GetLastClientVisibleTick(new(x, y));
                long tickDelta = (long)(World.tick - lastClientVisibleTick);
                if (tickDelta <= 1)
                {
                    int scale = 1;
                    DebugDraw.Line(new(0, 0), new(scale, scale), new Transform { Position = new(x * scale, y * scale) }, Color.Blue);
                    DebugDraw.Line(new(0, scale), new(scale, 0), new Transform { Position = new(x * scale, y * scale) }, Color.Blue);
                }
            }
        }
    }

    [DebugOverlay]
    public static void ShowCollisionBins()
    {
        double cx = World.Camera.SmoothTransform.Position.X;
        double cy = World.Camera.SmoothTransform.Position.Y;
        double w = World.Camera.SmoothVerticalSize * World.Camera.AspectRatio;
        double h = World.Camera.SmoothVerticalSize;

        Color[] colors = [Color.Red, Color.OrangeRed, Color.Orange, Color.Yellow, Color.LightYellow, Color.White, Color.LightBlue];

        for (int i = 0; i < LayerCount; i++)
        {
            ShowCollisionLayer(i);
        }

        void ShowCollisionLayer(int layer)
        {
            int scale = 1 << layer;

            int minX = ((int)Math.Floor(cx - w * .5)) / scale;
            int minY = ((int)Math.Floor(cy - h * .5)) / scale;
            int maxX = ((int)Math.Ceiling(cx + w * .5)) / scale;
            int maxY = ((int)Math.Ceiling(cy + h * .5)) / scale;

            if (maxX - minX > 500 || maxY - minY > 500)
            {
                return;
            }

            for (int y = minY; y < maxY; y++)
            {
                for (int x = minX; x < maxX; x++)
                {
                    if (World.Collision.layers[layer].TryGetValue(x, y, out Bin bin))
                    {
                        DebugDraw.Rectangle(new(0, 0, scale, scale), new Transform { Position = new(x * scale, y * scale) }, colors[layer]);
                    }
                }
            }
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

    [DebugOverlay]
    public static void ShowRevealRadius()
    {
        foreach (var ship in World.Ships)
        {
            DebugDraw.Circle(new(0, 0, (float)ship.GetRevealRadius()), ship.Transform, Color.Blue);
        }
        foreach (var structure in World.Structures)
        {
            DebugDraw.Circle(new(0, 0, (float)structure.GetRevealRadius()), Transform.Default with { Position = structure.GetCenter() }, Color.Blue);
        }
    }
}

class SparseSpatialHash<T> 
    where T : class
{
    private readonly float scale;
    private readonly Dictionary<ulong, T> dictionary = new();

    public IEnumerable<T> Values => dictionary.Values;

    public SparseSpatialHash(float scale)
    {
        this.scale = scale;
    }

    private ulong CalculateBinPos(DoubleVector worldPosition)
    {
        int cellX = (int)double.Floor(worldPosition.X / scale);
        int cellY = (int)double.Floor(worldPosition.Y / scale);

        return CalculateBinPos(cellX, cellY);
    }

    private ulong CalculateBinPos(int cellX, int cellY)
    {
        return (uint)cellX | (ulong)cellY << 32;
    }

    public bool TryGetValue(DoubleVector worldPosition, [NotNullWhen(true)] out T? value)
    {
        return dictionary.TryGetValue(CalculateBinPos(worldPosition), out value);
    }

    public bool TryGetValue(int cellX, int cellY, [NotNullWhen(true)] out T? value)
    {
        return dictionary.TryGetValue(CalculateBinPos(cellX, cellY), out value);
    }

    public T? this[DoubleVector worldPosition]
    {
        get
        {
            return dictionary.TryGetValue(CalculateBinPos(worldPosition), out T? bin) ? bin : null;
        }
        set
        {
            if (value != null)
            {
                dictionary[CalculateBinPos(worldPosition)] = value;
            }
            else
            {
                dictionary.Remove(CalculateBinPos(worldPosition), out _);
            }
        }
    }

    public T? this[int x, int y]
    {
        get
        {
            return dictionary.TryGetValue(CalculateBinPos(x, y), out T? bin) ? bin : null;
        }
        set
        {
            if (value != null)
            {
                dictionary[CalculateBinPos(x, y)] = value;
            }
            else
            {
                dictionary.Remove(CalculateBinPos(x, y), out _);
            }
        }
    }
}

class ChunkedSpatialHash<T>
    where T : struct
{
    public int Width { get; private set; }
    public int Height { get; private set; }

    private float scale;
    private int centerX, centerY;
    private int chunkWidth, chunkHeight;
    private Chunk?[] chunks;
    private List<Chunk> chunkList;
    private int pruneIndex;

    public List<Chunk> Chunks => chunkList;

    public ChunkedSpatialHash(int width, int height, float scale)
    {
        this.Width = width;
        this.Height = height;
        this.scale = scale;

        centerX = width / 2;
        centerY = height / 2;

        chunkWidth = width / Chunk.Size;
        chunkHeight = height / Chunk.Size;

        chunks = new Chunk[chunkWidth * chunkHeight];
        chunkList = [];
    }

    public ref T this[DoubleVector position] 
    { 
        get 
        {
            GetBinPosition(position, out int binX, out int binY);
            return ref this[binX, binY];
        } 
    }

    public ref T this[int x, int y]
    {
        get
        {
            ref Chunk? chunk = ref GetChunkRef(centerX + x, centerY + y);
            if (chunk == null)
            {
                chunk = new((centerX + x) >> Chunk.SizeBits, (centerY + y) >> Chunk.SizeBits);
                this.chunkList.Add(chunk);
            }
            chunk.LastModifiedTick = World.tick;
            return ref chunk[x & Chunk.Mask, y & Chunk.Mask];
        }
    }


    public void GetBinPosition(DoubleVector position, out int binX, out int binY)
    {
        DoubleVector scaledPos = position / scale;
        binX = (int)double.Floor(scaledPos.X);
        binY = (int)double.Floor(scaledPos.Y);
    }

    public bool TryGetValue(DoubleVector position, out T value)
    {
        GetBinPosition(position, out int binX, out int binY);
        return TryGetValue(binX, binY, out value);
    }

    public bool TryGetValue(int binX, int binY, out T value)
    {
        if (!InBounds(centerX + binX, centerY + binY))
        {
            value = default;
            return false;
        }

        Chunk? chunk = GetChunkRef(centerX + binX, centerY + binY);
        if (chunk != null)
        {
            value = chunk[binX & Chunk.Mask, binY & Chunk.Mask];
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }

    private bool InBounds(int binX, int binY)
    {
        int chunkX = binX >> Chunk.SizeBits;
        int chunkY = binY >> Chunk.SizeBits;

        return (uint)chunkX < this.Width && (uint)chunkY < this.Height;
    }

    private ref Chunk? GetChunkRef(int binX, int binY)
    {
        int chunkX = binX >> Chunk.SizeBits;
        int chunkY = binY >> Chunk.SizeBits;
        int chunkIndex = chunkY * chunkWidth + chunkX;
        return ref chunks[chunkIndex];
    }

    public void Prune(int scanCount)
    {
        scanCount = int.Min(scanCount, Chunks.Count);
        for (int i = 0; i < scanCount; i++)
        {
            if (pruneIndex >= Chunks.Count) 
            {
                pruneIndex = 0;
            }

            Chunk chunk = Chunks[pruneIndex];
            if (chunk.LastModifiedTick != World.tick)
            {
                Chunks.RemoveAt(pruneIndex);
                chunks[chunk.Y * chunkWidth + chunk.X] = null;
            }
            else
            {
                pruneIndex++;
            }
        }
    }

    public class Chunk
    {
        public const int SizeBits = 4;
        public const int Size = 1 << SizeBits;
        public const int Mask = (1 << SizeBits) - 1;

        private int x, y;
        private ChunkElementsArray elements;

        public ulong LastModifiedTick;

        public int X => x;
        public int Y => y;
        public int Count => Size * Size;

        public Chunk(int x, int y)
        {
            this.x = x;
            this.y = y;
            LastModifiedTick = World.tick;
        }


        public ref T this[int localX, int localY]
        {
            get => ref elements[localY * Size + localX];
        }

        public ref T this[int index]
        {
            get => ref elements[index];
        }

        [InlineArray(Size * Size)]
        struct ChunkElementsArray
        {
            private T value;
        }
    }
}
