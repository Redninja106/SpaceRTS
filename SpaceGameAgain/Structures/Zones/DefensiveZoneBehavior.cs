using SpaceGame.GUI;
using static SpaceGame.PolygonShape;

namespace SpaceGame.Structures.Zones;

class DefensiveZoneBehavior : ZoneBehavior
{
    private static readonly Vector2[] poly1;
    private static readonly Vector2[] poly2;
    private static readonly PolygonShape shape;

    static DefensiveZoneBehavior()
    {
        poly1 = [
            new Vector2(0, -.1f) + new Vector2(-.05f, -.75f),
            new Vector2(0, -.1f) + new Vector2(.05f, -.75f),
            new Vector2(0, -.1f) + new Vector2(-.05f, -.75f).Rotated(2 * MathF.PI / 3f),
            new Vector2(0, -.1f) + new Vector2(.05f, -.75f).Rotated(2 * MathF.PI / 3f),
            new Vector2(0, -.1f) + new Vector2(-.05f, -.75f).Rotated(4 * MathF.PI / 3f),
            new Vector2(0, -.1f) + new Vector2(.05f, -.75f).Rotated(4 * MathF.PI / 3f),
        ];

        poly2 = [
            poly1[2],
            poly1[2] + new Vector2(0, .1f),
            poly1[3] + new Vector2(0, .1f),
            poly1[4] + new Vector2(0, .1f),
            poly1[5] + new Vector2(0, .1f),
            poly1[5],
            poly1[4],
            poly1[3],
        ];

        shape = new(poly1.Select(p => p + new Vector2(0, .1f)).ToArray(), Color.White);
    }

    public DefensiveZoneBehavior(StructureInstance instance) : base(instance)
    {
        upgrades = [
            World.Structures.MissileTurret,
        ];
        OnFootprintChanged();
    }

    public override void Update()
    {
        base.Update();
    }

    public override void RenderCell(ICanvas canvas, HexCoordinate localCell)
    {
        canvas.Fill(Color.Gray);
        canvas.DrawPolygon(poly1);
        canvas.Fill(Color.FromHSV(0, 0, .25f));
        canvas.DrawPolygon(poly2);

        base.RenderCell(canvas, localCell);
    }

    public override void RenderBeforeCells(ICanvas canvas)
    {
        foreach (var cell in Instance.Footprint ?? Instance.Structure.Footprint)
        {
            ConnectNeighbor(canvas, cell, HexCoordinate.UnitQ);
            ConnectNeighbor(canvas, cell, HexCoordinate.UnitR);
            ConnectNeighbor(canvas, cell, HexCoordinate.UnitR - HexCoordinate.UnitQ);
        }

        base.RenderBeforeCells(canvas);
    }

    private void ConnectNeighbor(ICanvas canvas, HexCoordinate localCell, HexCoordinate offset)
    {
        var gridCell = Instance.Grid.GetCell(Instance.Location + localCell + offset);
        if (gridCell != null && gridCell.Structure == Instance)
        {
            canvas.Fill(Color.FromHSV(0, 0, .3f));
            canvas.StrokeWidth(.2f);
            canvas.DrawLine(localCell.ToCartesian(), (localCell + offset).ToCartesian());
        }
    }

    public override void RenderCellShadow(ICanvas canvas, Vector2 offset, HexCoordinate cell)
    {
        canvas.Fill(Color.Black with { A = 100 });

        ShadowVertexWriter writer = new(stackalloc Vector2[poly1.Length * 2]);
        ProjectVerts(poly1, offset, ref writer);
        canvas.Translate(0, .1f);
        canvas.DrawPolygon(writer.GetBuffer());

        base.RenderCellShadow(canvas, offset, cell);
    }
}