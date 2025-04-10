using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpaceGame.Structures.Zones;

namespace SpaceGame.Structures;
internal class StructureList
{
//    public StructurePrototype MissileTurret { get; } = new(
//        "missile turret",
//        15,
//        new Model([new Vector2(0, 0), HexCoordinate.UnitQ.ToCartesian(), HexCoordinate.UnitR.ToCartesian()], Color.Gray),
//        [new(0, 0), new(1, 0), new(0, 1)],
//        typeof(MissileTurretBehavior)
//        );
//    public StructurePrototype ChaingunTurret { get; } = new(
//        "chaingun turret",
//        10,
//        new Model([new Vector2(-.5f, -.5f), new Vector2(-.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, -.5f)], Color.DarkCyan),
//        [new(0, 0)],
//        typeof(ChaingunTurretBehavior)
//        );

//    public StructurePrototype Generator { get; } = new(
//        "generator",
//        20,
//        new Model(new Rectangle(-.5f, .85f, 1f, 2.9f, Alignment.CenterLeft).ToPolygon(), Color.Yellow),
//        [new(0, 0), new(0, 1)],
//        null
//        );

//    public StructurePrototype Manufactory { get; } = new(
//        "manufactory",
//        40,
//        new Model([
//            new PolygonShape(new Rectangle(-.4f, .85f, .9f, 2.9f, Alignment.CenterLeft).ToPolygon(), Color.Blue),
//            new PolygonShape(new Rectangle(-.4f + .9f, .85f, 2.4f - .9f, 1, Alignment.CenterLeft).ToPolygon(), Color.Blue),
//        ]),
//        [new(0, 0), new(0, 1), new(1, 0)],
//        null
//        );

//    public StructurePrototype Headquarters { get; } = new(
//        "headquarters",
//        100,
//        new Model([
//            new PolygonShape(new Rectangle(0, 0, 3, 3, Alignment.Center).ToPolygon(), Color.Orange),
//        ]),
//        [
//            new(0, 0),
//            new(0, 1),
//            new(1, 0),
//            new(-1, 1),
//            new(0, -1),
//            new(-1, 0),
//            new(1, -1),
//        ],
//        null
//        );

//    public StructurePrototype LargeShipyard { get; } = new(
//        "large shipyard",
//        5,
//        new Model([
//            new PolygonShape(new Rectangle(0, -1, 3, 4.5f, Alignment.Center).ToPolygon(), Color.Purple),
//        ]),
//        [
//            new(0, 0),
//            new(0, 1),
//            new(1, 0),
//            new(-1, 1),
//            new(0, -1),
//            new(-1, 0),
//            new(1, -1),
//            new(-1, -1),
//            new(0, -2),
//            new(1, -2),
//        ],
//        typeof(ShipyardBehavior)
//        );

//    public StructurePrototype SmallShipyard { get; } = new(
//        "small shipyard",
//        5,
//        new Model([
//            new PolygonShape(new Rectangle(new HexCoordinate(1, 0).ToCartesian(), Vector2.One, Alignment.Center).ToPolygon(), Color.Lerp(Color.Purple, Color.Gray, .5f), .2f),
//            new PolygonShape(new Rectangle(new HexCoordinate(-1, 1).ToCartesian(), Vector2.One, Alignment.Center).ToPolygon(), Color.Lerp(Color.Purple, Color.Gray, .5f), .2f),
//            new CanvasShape(
//                (canvas, parameters) =>
//                {
//                    canvas.Fill(Color.Gray);
//                    canvas.DrawRect(new Vector2(0, -.45f), new Vector2(1, .2f), Alignment.TopCenter);
//                    canvas.Fill(Color.FromHSV(0, 0, .3f));
//                    canvas.DrawRect(new Vector2(0, -.25f), new Vector2(1, 2), Alignment.TopCenter);
//                }
//                )
//        ]),
//        [
//            new(0, 0),
//            new(0, 1),
//            new(1, 0),
//            new(-1, 1),
//        ],
//        typeof(ShipyardBehavior)
//        );

//    public StructurePrototype ResourceNode { get; } = new(
//        "Resource Deposit",
//        int.MaxValue,
//        new Model([
//                new PolygonShape(new Rectangle(0, 0, 1, 1, Alignment.TopCenter).ToPolygon(), Color.Blue),
//        ]),
//        [
//            new(0, 0),
//            new(1, 0),
//            new(0, 1),
//        ],
//        null
//        );

//    public StructurePrototype ParticleAccelerator { get; } = new(
//        "particle accelerator",
//        200,
//        new Model([
//            new CanvasShape((canvas, parameters) =>
//            {
//                canvas.StrokeWidth(.4f);
                
//                canvas.Stroke(parameters.GetOverriddenColor((ColorF.SkyBlue * .5f).ToColor() with { A = 255 }));
//                canvas.DrawCircle(new HexCoordinate(2, 0).ToCartesian(), 3.15f);
                
//                canvas.Translate(0, -.25f);
//                canvas.Stroke(parameters.GetOverriddenColor(Color.SkyBlue));
//                canvas.DrawCircle(new HexCoordinate(2, 0).ToCartesian(), 3.15f);
//            },
//            (canvas, offset, color) =>
//            {
//                canvas.Stroke(color);
//                canvas.StrokeWidth(.4f);
//                canvas.DrawCircle(new HexCoordinate(2, 0).ToCartesian() + offset, 3.15f);
//            }),
//        ]),
//        [
//            new(2 + 0, -2),
//            new(2 + 1, -2),
//            new(2 + 2, -2),
//            new(2 + 2, -1),
//            new(2 + 2, 0),
//            new(2 + 1, 1),
//            new(2 + 0, 2),
//            new(2 + -1, 2),
//            new(2 + -2, 2),
//            new(2 + -2, 1),
//            new(2 + -2, 0),
//            new(2 + -1, -1),
//        ],
//        typeof(ParticleAcceleratorBehavior)
//        );

//    public StructurePrototype DefensiveZone { get; } = new ZoneStructure(
//        "Defensive Zone",
//        10,
//        new Model([new CanvasShape((c, r) => 
//            {
//                c.Fill(Color.Red);
//                c.DrawRect(0, 0, 1, 1, Alignment.Center);
//            })]),
//        [
//            new(0, 0),
//        ],
//        typeof(DefensiveZoneBehavior),
//        ColorF.Red
//        ); 

//    public StructurePrototype IndustrialZone { get; } = new ZoneStructure(
//        "Industrial Zone",
//        10,
//        new Model([new CanvasShape((c, r) =>
//        {
//            // zc.Fill(Color.Red);
//            // zc.DrawRect(0, 0, 1, 1, Alignment.Center);
//        })]),
//        [
//            new(0, 0),
//        ],
//        typeof(IndustrialZoneBehavior),
//        ColorF.OrangeRed
//        ); 

//    public StructurePrototype EconomicZone { get; } = new ZoneStructure(
//        "Economic Zone",
//        10,
//        new Model([new CanvasShape((c, r) =>
//        {
//            // zc.Fill(Color.Red);
//            // zc.DrawRect(0, 0, 1, 1, Alignment.Center);
//        })]),
//        [
//            new(0, 0),
//        ],
//        typeof(EconomicZoneBehavior),
//        ColorF.Yellow
//        ); 

//    public StructurePrototype ResearchZone { get; } = new ZoneStructure(
//        "Research Zone",
//        10,
//        new Model([new CanvasShape((c, r) =>
//        {
//            // zc.Fill(Color.Red);
//            // zc.DrawRect(0, 0, 1, 1, Alignment.Center);
//        })]),
//        [
//            new(0, 0),
//        ],
//        typeof(ResearchZoneBehavior),
//        ColorF.Cyan
//        );
}

//static class PresetModels
//{
//    public static Dictionary<string, Model> presetModels = new()
//    {
//        ["headquarters"] = new Model([
//            new PolygonShape(new Rectangle(0, 0, 3, 3, Alignment.Center).ToPolygon(), Color.Orange),
//        ]),
//        ["particle_accelerator"] = new Model([
//        new CanvasShape((canvas, parameters) =>
//            {
//                canvas.StrokeWidth(.4f);

//                canvas.Stroke(parameters.GetOverriddenColor((ColorF.SkyBlue * .5f).ToColor() with { A = 255 }));
//                canvas.DrawCircle(new HexCoordinate(2, 0).ToCartesian(), 3.15f);

//                canvas.Translate(0, -.25f);
//                canvas.Stroke(parameters.GetOverriddenColor(Color.SkyBlue));
//                canvas.DrawCircle(new HexCoordinate(2, 0).ToCartesian(), 3.15f);
//            },
//            (canvas, offset, color) =>
//            {
//                canvas.Stroke(color);
//                canvas.StrokeWidth(.4f);
//                canvas.DrawCircle(new HexCoordinate(2, 0).ToCartesian() + offset, 3.15f);
//            }),
//        ]),
//        ["shipyard"] = new Model([
//            new PolygonShape(new Rectangle(new HexCoordinate(1, 0).ToCartesian(), Vector2.One, Alignment.Center).ToPolygon(), Color.Lerp(Color.Purple, Color.Gray, .5f), .2f),
//            new PolygonShape(new Rectangle(new HexCoordinate(-1, 1).ToCartesian(), Vector2.One, Alignment.Center).ToPolygon(), Color.Lerp(Color.Purple, Color.Gray, .5f), .2f),
//            new CanvasShape(
//                (canvas, parameters) =>
//                {
//                    canvas.Fill(Color.Gray);
//                    canvas.DrawRect(new Vector2(0, -.45f), new Vector2(1, .2f), Alignment.TopCenter);
//                    canvas.Fill(Color.FromHSV(0, 0, .3f));
//                    canvas.DrawRect(new Vector2(0, -.25f), new Vector2(1, 2), Alignment.TopCenter);
//                }
//                )
//        ]),
//        ["chaingun_turret"] = new Model([new Vector2(-.5f, -.5f), new Vector2(-.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, -.5f)], Color.DarkCyan),
//        ["missile_turret"] = new Model([new Vector2(0, 0), HexCoordinate.UnitQ.ToCartesian(), HexCoordinate.UnitR.ToCartesian()], Color.Gray),
//        ["manufactory"] = new Model([
//            new PolygonShape(new Rectangle(-.4f, .85f, .9f, 2.9f, Alignment.CenterLeft).ToPolygon(), Color.Blue),
//            new PolygonShape(new Rectangle(-.4f + .9f, .85f, 2.4f - .9f, 1, Alignment.CenterLeft).ToPolygon(), Color.Blue),
//        ]),
//        ["default"] = new Model([new Vector2(-.5f, -.5f), new Vector2(-.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, -.5f)], Color.Gray),

//    };
//}