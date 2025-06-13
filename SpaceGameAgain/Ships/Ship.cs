using ImGuiNET;
using Silk.NET.Core.Native;
using Silk.NET.Input;
using SimulationFramework;
using SimulationFramework.Drawing;
using SpaceGame.Debugging;
using SpaceGame.Extensions;
using SpaceGame.GUI;
using SpaceGame.Interaction;
using SpaceGame.Orders;
using SpaceGame.Planets;
using SpaceGame.Rendering;
using SpaceGame.Serialization;
using SpaceGame.Ships;
using SpaceGame.Ships.Fleets;
using SpaceGame.Ships.Modules;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Ships;

[Serializable]
internal class Ship(ShipPrototype prototype, ulong id) : Unit(prototype, id)
{
    public override ShipPrototype Prototype => (ShipPrototype)base.Prototype;

    public override ITexture Icon => Icons.Ship;

    public static Vector2[] verts = [
        new(.5f / 2f, 0),
        new(-.5f / 2f, .2f / 2f),
        new(-.5f / 2f, -.2f / 2f),
    ];

    [Serialize]
    public DoubleVector velocity;
    [Serialize]
    public float angularVelocity;
    
    [Serialize]
    public float height = 0;
    public Stance stance;

    [Serialize]
    public Queue<Order> orders = [];
    [Serialize]
    public List<Module> modules = [];

    [Serialize]
    public Fleet? Fleet;

    public override bool CanAttack => modules.Any(m => m is WeaponModule);

    // PER CLIENT -- an order the player submitted that hasn't been processed yet
    public Order? potentialOrder = null;

    public override void Render(ICanvas canvas)
    {
        canvas.PushState();

        //if (selected)
        //{
        //    Team playerTeam = World.PlayerTeam;
        //    canvas.Stroke(playerTeam.GetRelationColor(Team));
        //    canvas.StrokeWidth(0);
        //    canvas.DrawCircle(0, 0, MathF.Max((float)GetCollisionRadius(), World.Camera.ScreenDistanceToWorldDistance(2.5f/2f)));
        //}

        canvas.Scale(Prototype.Scale);

        if (Prototype.Model == null)
        {
            canvas.Fill(Color.White);
            canvas.DrawPolygon(verts);
        }
        else
        {
            canvas.Rotate(-this.InterpolatedTransform.Rotation);
            canvas.Translate(0, Prototype.FlyHeight - height);
            Prototype.Model.Render(canvas, this.InterpolatedTransform, ColorF.White);
        }

        canvas.PopState();
    }

    public override void DrawHighlightAbove(ICanvas canvas, Camera camera, bool selected)
    {
        if (this.Team == World.PlayerTeam)
        {
            if (potentialOrder != null)
            {
                canvas.PushState();
                potentialOrder.RenderOverlay(canvas);
                canvas.PopState();
            }
            else if (orders.Count > 0)
            {
                var order = orders.Peek();
                canvas.PushState();
                order.RenderOverlay(canvas);
                canvas.PopState();
            }
        }

        base.DrawHighlightAbove(canvas, camera, selected);
    }

    public override void DrawHighlightBelow(ICanvas canvas, Camera camera, bool selected)
    {
        canvas.Transform(World.Camera.CreateRelativeMatrix(InterpolatedTransform));
        canvas.Stroke(World.PlayerTeam.GetRelationColor(Team) with { A = (byte)(selected ? 255 : 100) });
        canvas.DrawCircle(0, 0, (float)GetCollisionRadius());

        base.DrawHighlightBelow(canvas, camera, selected);
    }

    public override void Tick()
    {
        base.Tick();

        height = MathHelper.Step(height, Prototype.FlyHeight, Program.Timestep * Prototype.RiseSpeed);

        if (height >= Prototype.FlyHeight)
        {
            foreach (var module in modules)
            {
                module.Tick();
            }

            if (orders.Count > 0)
            {
                var order = orders.Peek();
                order.Tick();
                if (order.IsCompleted)
                {
                    orders.Dequeue();
                }
            }

            this.Transform.Position += this.velocity * Program.Timestep;
            this.Transform.Rotation += this.angularVelocity * Program.Timestep;

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
            //order.Update(tickProgress);
        }
    }

    public override bool TestPoint(DoubleVector point)
    {
        return Util.TestPoint(verts.Select(v => v *= Prototype.Scale * 2).ToArray(), this.Transform, point.ToVector2(), Transform.Default);
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

        //canvas.Translate(0, Prototype.FlyHeight);
        canvas.Translate(0, Prototype.FlyHeight - height);
        canvas.Translate(InterpolatedTransform.Position.ToVector2().Normalized() * (height - floorHeight));

        canvas.Rotate(this.InterpolatedTransform.Rotation);

        canvas.Fill(Color.Black with { A = 100 });

        canvas.DrawPolygon(verts);
        canvas.PopState();
    }

    //public override void Serialize(BinaryWriter writer)
    //{
    //    writer.Write(ID);
    //    writer.Write(Transform);
    //    writer.Write(Team);
    //    writer.Write(height);

    //    writer.Write(modules.Count);
    //    foreach (var module in modules)
    //    {
    //        writer.Write(module);
    //    }
    //}

    public override void DebugLayout()
    {
        base.DebugLayout();
        ObjectViewer.ReflectionLayoutObjectFields(this);
    }
    

    public override void Layout(GUIWindow window)
    {
        if (this.Team == World.PlayerTeam && this.modules.FirstOrDefault(m => m is ConstructionModule) is Module m)
        {
            if (window.TextButton("build"))
            {
                World.GUIViewport.SetPopup(m.Layout, window.LastItemBounds.GetAlignedPoint(Alignment.TopCenter), Alignment.BottomCenter);
            }
        }

        // if (window.TextButton(stance.ToString().ToLower()))
        // {
        //     stance = (Stance)(((int)stance + 1) % (int)Stance.StanceCount);
        // }
        // if (window.LastItemHovered())
        // {
        //     World.SetTooltip(w => w.Text(stanceDescs[(int)stance]));
        // }

        //window.LayoutMode = LayoutMode.Horizontal;
        //foreach (var mod in modules)
        //{
        //    window.Image(mod.Icon, new(16));
        //    if (window.LastItemHovered())
        //    {
        //        World.SetTooltip(w => w.Text(mod.Actor.Prototype.Name));
        //    }
        //}
    }

    public void Rotate(float throttle)
    {
        this.angularVelocity += (float.Tau * Prototype.TurnSpeed) * float.Clamp(throttle, -1, 1) * Program.Timestep;
    }

    internal void Fly(float throttle)
    {
        this.velocity += this.Transform.Forward * Prototype.FlySpeed * float.Clamp(throttle, -1, 1) * Program.Timestep;
    }

    static string[] stanceDescs = [
            "ship will not attack under any circumstances",
            "ship will only attack when attacked first",
            "ship will attack nearby enemy units",
            "ship will attack and pursue nearby enemy units"
        ];
}

enum Stance
{
    Passive,
    Neutral,
    Defensive,
    Agressive,
    StanceCount
}