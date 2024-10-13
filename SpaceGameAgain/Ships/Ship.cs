using Silk.NET.Core.Native;
using SimulationFramework;
using SimulationFramework.Drawing;
using SpaceGame.Interaction;
using SpaceGame.Planets;
using SpaceGame.Ships;
using SpaceGame.Ships.Modules;
using SpaceGame.Ships.Orders;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Ships;
internal class Ship : UnitBase
{
    public static Vector2[] verts = [
        new(.5f / 2f, 0),
        new(-.5f / 2f, .2f / 2f),
        new(-.5f / 2f, -.2f / 2f),
    ];

    public Queue<Order> orders = [];
    public List<Module> modules = [];

    public float health = 5;
    public float height;

    public Ship(Team team)
    {
        this.Team = team;
    }

    public override void Render(ICanvas canvas)
    {
        bool selected = World.SelectionHandler.IsSelected(this);
        if (selected)
        {
            canvas.Stroke(World.PlayerTeam.GetRelationColor(Team));
            canvas.StrokeWidth(0);
            canvas.DrawCircle(0, 0, MathF.Max(.65f/2f, World.Camera.ScreenDistanceToWorldDistance(2.5f/2f)));
        }

        canvas.Fill(Color.White);

        for (int i = 0; i < 8; i++)
        {
            canvas.Rotate(-this.Transform.Rotation);
            canvas.Translate(0, -.005f);
            canvas.Rotate(this.Transform.Rotation);
            canvas.DrawPolygon(verts);
        }

        canvas.ResetState();

        canvas.PopState();

        if (selected)
        {
            if (orders.Count > 0)
            {
                var order = orders.Peek();
                canvas.PushState();
                order.Render(this, canvas);
                canvas.PopState();
            }
        }

        canvas.PushState();

    }

    public override void Damage()
    {
        health--;
    }

    public override void Update()
    {
        if (height < .4f)
            height += Time.DeltaTime * .5f;

        foreach (var module in modules)
        {
            module.Update();
        }

        if (orders.Count > 0 && height >= .4f) 
        {
            var order = orders.Peek();
            if (order.Complete(this))
            {
                orders.Dequeue();
            }
        }

        if (health <= 0)
        {
            IsDestroyed = true;
            if (World.SelectionHandler.IsSelected(this))
                World.SelectionHandler.Deselect(this);
        }

        SphereOfInfluence? soi = World.GetSphereOfInfluence(this.Transform.Position);
        soi?.ApplyTo(this);
    }

    public override bool TestPoint(Vector2 point, Transform transform)
    {
        return TestPoint(point, transform, 1f);
    }
    public bool TestPoint(Vector2 point, Transform transform, float scale)
    {
        return Util.TestPoint(verts, this.Transform, point, transform);
    }

    public void RenderShadow(ICanvas canvas, float floorHeight)
    {
        canvas.Translate(Transform.Position);
        canvas.Translate(Transform.Position.Normalized() * (height - floorHeight));
        canvas.Fill(Color.Black with { A = 100 });

        for (int i = 0; i < 8; i++)
        {
            canvas.Translate(Transform.Position.Normalized() * .005f);
            canvas.Rotate(Transform.Rotation);
            canvas.DrawPolygon(verts);
            canvas.Rotate(-Transform.Rotation);
        }
    }
}
