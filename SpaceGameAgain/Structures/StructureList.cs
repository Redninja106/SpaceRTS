using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;
internal class StructureList
{
    public Structure MissileTurret { get; } = new(
        "missile turret",
        15,
        new Model([new Vector2(-.5f, -.5f), new Vector2(-.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, -.5f)], Color.BlueViolet),
        [new(0, 0)],
        typeof(MissileTurretBehavior)
        );
    public Structure ChaingunTurret { get; } = new(
        "chaingun turret",
        10,
        new Model([new Vector2(-.5f, -.5f), new Vector2(-.5f, .5f), new Vector2(.5f, .5f), new Vector2(.5f, -.5f)], Color.DarkCyan),
        [new(0, 0)],
        typeof(ChaingunTurretBehavior)
        );

    public Structure Generator { get; } = new(
        "generator",
        20,
        new Model(new Rectangle(-.5f, .85f, 1f, 2.9f, Alignment.CenterLeft).ToPolygon(), Color.Yellow),
        [new(0, 0), new(0, 1)],
        null
        );

    public Structure Manufactory { get; } = new(
        "manufactory",
        40,
        new Model([
            new PolygonShape(new Rectangle(-.4f, .85f, .9f, 2.9f, Alignment.CenterLeft).ToPolygon(), Color.Blue),
            new PolygonShape(new Rectangle(-.4f + .9f, .85f, 2.4f - .9f, 1, Alignment.CenterLeft).ToPolygon(), Color.Blue),
        ]),
        [new(0, 0), new(0, 1), new(1, 0)],
        null
        );

    public Structure Headquarters { get; } = new(
        "headquarters",
        100,
        new Model([
            new PolygonShape(new Rectangle(0, 0, 3, 3, Alignment.Center).ToPolygon(), Color.Orange),
        ]),
        [
            new(0, 0),
            new(0, 1),
            new(1, 0),
            new(-1, 1),
            new(0, -1),
            new(-1, 0),
            new(1, -1),
        ],
        null
        );

    public Structure Shipyard { get; } = new(
        "shipyard",
        5,
        new Model([
            new PolygonShape(new Rectangle(0, -1, 3, 4.5f, Alignment.Center).ToPolygon(), Color.Purple),
        ]),
        [
            new(0, 0),
            new(0, 1),
            new(1, 0),
            new(-1, 1),
            new(0, -1),
            new(-1, 0),
            new(1, -1),
            new(-1, -1),
            new(0, -2),
            new(1, -2),
        ],
        typeof(ShipyardBehavior)
        );

    public Structure ParticleAccelerator { get; } = new(
        "particle accelerator",
        200,
        new Model([
            new CanvasShape((canvas, parameters) =>
            {
                canvas.Stroke(parameters.GetOverriddenColor(Color.SkyBlue));
                canvas.StrokeWidth(.5f);
                canvas.DrawCircle(0, 0, 3.15f);
            },
            (canvas, offset, color) =>
            {
                canvas.Stroke(color);
                canvas.StrokeWidth(.5f);
                canvas.DrawCircle(offset, 3.15f);
            }),
        ]),
        [
            new(0, -2),
            new(1, -2),
            new(2, -2),
            new(2, -1),
            new(2, 0),
            new(1, 1),
            new(0, 2),
            new(-1, 2),
            new(-2, 2),
            new(-2, 1),
            new(-2, 0),
            new(-1, -1),
        ],
        typeof(ParticleAcceleratorBehavior)
        );
}
