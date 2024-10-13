

namespace SpaceGame.Structures.Zones;

internal class IndustrialZoneBehavior : ZoneBehavior
{
    public IndustrialZoneBehavior(StructureInstance instance) : base(instance)
    {
        upgrades = [
            World.Structures.SmallShipyard,
            World.Structures.LargeShipyard,
        ];
        OnFootprintChanged();
    }

    public override void RenderCell(ICanvas canvas, HexCoordinate localCell)
    { 
        canvas.Fill(Color.FromHSV(0f, .4f, .3f));
        canvas.DrawRect(0, -.3f, 1, 1, Alignment.Center);
        canvas.Fill(Color.FromHSV(0f, 0, .3f));
        canvas.DrawRect(0, .3f, 1, .3f, Alignment.Center);

        canvas.Fill(Color.FromHSV(0f, 0, .3f));
        canvas.DrawRect(-.25f, -.7f, .2f, .7f, Alignment.Center);
        canvas.Fill(Color.FromHSV(0f, 0, .2f));
        canvas.DrawRect(-.25f, -1.15f, .2f, .2f, Alignment.Center);

        base.RenderCell(canvas, localCell);
    }

    public override void RenderCellShadow(ICanvas canvas, Vector2 offset, HexCoordinate cell)
    {
        base.RenderCellShadow(canvas, offset, cell);
    }
}