//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace SpaceGame.Structures.Zones;
//internal class EconomicZoneBehavior : ZoneBehavior
//{
//    static Vector2[] roof = [
//        new(.25f, .3f),
//        new(-.25f, .3f),
//        new(-.25f, -.7f),
//        new(-.25f, -.7f),
//        new(0, -.85f),
//        new(.25f, -.7f),
//    ];

//    static Vector2[] wall = [
//            new(-.25f, .5f),
//        new(.25f, .5f),
//        new(.25f, .3f),
//        new(0, .15f),
//        new(-.25f, .3f),
//    ];
//    static Vector2[] shadow = new Rectangle(0, 0, .5f, 1f, Alignment.Center).ToPolygon();

//    public EconomicZoneBehavior(Structure instance) : base(instance)
//    {
//        upgrades = [
//            // World.Structures.Generator
//        ];

//        OnFootprintChanged();
//    }

//    public override void RenderCell(ICanvas canvas, HexCoordinate localCell)
//    {
//        canvas.Fill(Color.Lerp(Color.Yellow, Color.Gray, .6f));
//        canvas.DrawPolygon(roof);
//        canvas.Fill(Color.Gray); 
//        canvas.DrawPolygon(wall);

//        base.RenderCell(canvas, localCell);
//    }

//    public override void RenderCellShadow(ICanvas canvas, Vector2 offset, HexCoordinate cell)
//    {
//        canvas.Fill(Color.Black with { A = 100 });
//        PolygonShape.RenderShadowPolygon(canvas, shadow, offset);

//        base.RenderCellShadow(canvas, offset, cell);
//    }
//}
