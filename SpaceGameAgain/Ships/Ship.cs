using ImGuiNET;
using Silk.NET.Core.Native;
using Silk.NET.Input;
using SimulationFramework;
using SimulationFramework.Drawing;
using SpaceGame.GUI;
using SpaceGame.Interaction;
using SpaceGame.Orders;
using SpaceGame.Planets;
using SpaceGame.Ships;
using SpaceGame.Ships.Modules;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Ships;

internal class Ship(ShipPrototype prototype, ulong id, Transform transform, ActorReference<Team> team, float height = 0) : Unit(prototype, id, transform, team)
{
    public override ShipPrototype Prototype => (ShipPrototype)base.Prototype;

    public static Vector2[] verts = [
        new(.5f / 2f, 0),
        new(-.5f / 2f, .2f / 2f),
        new(-.5f / 2f, -.2f / 2f),
    ];

    public Queue<ActorReference<Order>> orders = [];
    public List<ActorReference<Module>> modules = [];
    public Order? potentialOrder = null;

    public float height = height;

    public override void Render(ICanvas canvas)
    {
        bool selected = World.SelectionHandler.IsSelected(this);

        canvas.PushState();
        canvas.Scale(Prototype.Scale);

        if (selected)
        {
            Team playerTeam = World.PlayerTeam.Actor!;
            canvas.Stroke(playerTeam.GetRelationColor(Team.Actor!));
            canvas.StrokeWidth(0);
            canvas.DrawCircle(0, 0, MathF.Max((float)GetCollisionRadius(), World.Camera.ScreenDistanceToWorldDistance(2.5f/2f)));
        }

        canvas.Fill(Color.White);

        for (int i = 0; i < 8; i++)
        {
            canvas.Rotate(-this.Transform.Rotation);
            canvas.Translate(0, -.005f);
            canvas.Rotate(this.Transform.Rotation);
            canvas.DrawPolygon(verts);
        }

        canvas.PopState();

        if (selected)
        {
            if (potentialOrder != null)
            {
                canvas.PushState();
                potentialOrder.Render(canvas);
                canvas.PopState();
            }
            else if (orders.Count > 0)
            {
                var order = orders.Peek();
                canvas.PushState();
                order.Actor!.Render(canvas);
                canvas.PopState();
            }
            //canvas.Flush();
        }
    }

    public override void Tick()
    {
        base.Tick();
        if (height < .4f)
        {
            height = height + Program.Timestep * Prototype.RiseSpeed;
        }

        foreach (var module in modules)
        {
            module.Actor!.Tick();
        }

        if (orders.Count > 0 && height >= .4f) 
        {
            var order = orders.Peek();
            order.Actor!.Tick();
            if (order.Actor!.IsCompleted)
            {
                orders.Dequeue();
            }
        }

        //if (health <= 0)
        //{
        //    IsDestroyed = true;
        //    if (World.SelectionHandler.IsSelected(this))
        //        World.SelectionHandler.Deselect(this);
        //}

        SphereOfInfluence? soi = World.GetSphereOfInfluence(this.Transform.Position);
        soi?.ApplyTickTo(this);
    }

    public override void Update(float tickProgress)
    {
        base.Update(tickProgress);

        foreach (var order in orders)
        {
            order.Actor!.Update(tickProgress);
        }
    }

    public override bool TestPoint(DoubleVector point)
    {
        return Util.TestPoint(verts.Select(v => v *= Prototype.Scale).ToArray(), this.Transform, point.ToVector2(), Transform.Default);
    }

    public void RenderShadow(ICanvas canvas, float floorHeight)
    {
        canvas.PushState();
        Transform unrotatedTransform = InterpolatedTransform with
        {
            Rotation = 0,
            Scale = new(Prototype.Scale),
        };
        unrotatedTransform.ApplyTo(canvas, World.Camera);

        canvas.Translate(InterpolatedTransform.Position.ToVector2().Normalized() * (height - floorHeight));
        canvas.Fill(Color.Black with { A = 100 });

        for (int i = 0; i < 8; i++)
        {
            canvas.Translate(InterpolatedTransform.Position.ToVector2().Normalized() * .005f);
            canvas.Rotate(InterpolatedTransform.Rotation);
            canvas.DrawPolygon(verts);
            canvas.Rotate(-InterpolatedTransform.Rotation);
        }
        canvas.PopState();
    }

    public override Element[]? GetSelectionGUI()
    {
        return [
            new Label("hello there"),
            ];
    }

    public override void Serialize(BinaryWriter writer)
    {
        writer.Write(ID);
        writer.Write(Transform);
        writer.Write(Team);
        writer.Write(height);

        writer.Write(modules.Count);
        foreach (var module in modules)
        {
            writer.Write(module);
        }
    }
}
