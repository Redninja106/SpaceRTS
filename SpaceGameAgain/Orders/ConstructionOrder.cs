using SpaceGame.Extensions;
using SpaceGame.Ships;
using SpaceGame.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Orders;

[Serializable]
internal class ConstructionOrder : Order
{
    [Serialize]
    public Grid Grid;
    [Serialize]
    public HexCoordinate Location;
    [Serialize]
    public int Rotation;
    [Serialize]
    public StructurePrototype Structure;

    public override void Tick()
    {
        if (!MoveTo(DoubleVector.FromVector2(Grid.Transform.LocalToWorld(Location.ToCartesian()) + Structure.Center.Rotated(Rotation * MathF.Tau / 6f))))
        {
            return;
        }

        if (!Grid.IsStructureObstructed(Structure, Location, Rotation))
        {
            Unit.Team.Money -= Structure.Cost;
            Grid.PlaceStructure(Structure, Location, Rotation, Unit.Team);
            // Unit.Team.Resources["metals"] -= Structure.Price;
            Complete();
        }
    }

    public override void RenderOverlay(ICanvas canvas)
    {
        //Grid.Transform.ApplyTo(canvas, World.Camera);
        // canvas.Rotate(Rotation * (MathF.Tau / 6f));
        //Structure.Model.Render(canvas, this.InterpolatedTransform with { Rotation = 0 }, ColorF.White with { A = 100 });

        base.RenderOverlay(canvas);
    }


    //public override void Serialize(BinaryWriter writer)
    //{
    //    base.Serialize(writer);
    //    writer.Write(Grid);
    //    writer.Write(Structure.Name);
    //    writer.Write(Location);
    //    writer.Write(Rotation);
    //}
}
