using ImGuiNET;
using Silk.NET.Core.Native;
using Silk.NET.Input;
using SimulationFramework;
using SimulationFramework.Drawing;
using SpaceGame.Combat;
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
using SpaceGame.Stations;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Ships;

[Serializable]
internal class Ship(ShipPrototype prototype, GameWorld world, ulong id) : Unit(prototype, world, id), ITargetable
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
    private Queue<Order> orders = [];
    [Serialize]
    public List<Module> modules = [];

    [Serialize]
    public Fleet? Fleet;

    [Serialize]
    public bool IsNavigating;

    [Serialize]
    public bool wasNavigating;

    public WormholeStation? WormholeStation;

    public override bool CanAttack => modules.Any(m => m is WeaponModule);
    public override bool CanReveal => base.CanReveal && WormholeStation == null;

    public DoubleVector Velocity => velocity;
    public DoubleVector CurrentAcceleration { get; set; }
    public DoubleVector LastAcceleration { get; set; }

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
            canvas.Translate(0, Prototype.FlyHeight - height);
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

    public override void RenderBackgroundOverlay(ICanvas canvas, Camera camera, bool selected)
    {
        if (this.Team == World.PlayerTeam)
        {
            Transform startTransform = this.InterpolatedTransform;
            Transform forecastedTransform = this.InterpolatedTransform;

            if (potentialOrder != null)
            {
                canvas.PushState();
                potentialOrder.RenderOverlay(canvas, ref startTransform, ref forecastedTransform);
                canvas.PopState();
            }
            else if (orders.Count > 0)
            {
                Transform transform = this.InterpolatedTransform;
                foreach (var order in orders)
                {
                    canvas.PushState();
                    order.RenderOverlay(canvas, ref startTransform, ref forecastedTransform);
                    canvas.PopState();

                    if (order is MoveOrder moveOrder)
                    {
                        transform.Position = moveOrder.TargetPosition;
                    }
                }
            }
        }

        base.RenderBackgroundOverlay(canvas, camera, selected);
    }

    public override void RenderGroundOverlay(ICanvas canvas, Camera camera, bool selected)
    {
        base.RenderGroundOverlay(canvas, camera, selected);
    }

    public override void Tick()
    {
        LastAcceleration = CurrentAcceleration;
        CurrentAcceleration = DoubleVector.Zero;

        base.Tick();

        if (height < Prototype.FlyHeight)
        {
            height = MathHelper.Step(height, Prototype.FlyHeight, Program.Timestep * Prototype.RiseSpeed);
            return;
        }

        foreach (var module in modules)
        {
            module.Tick();
        }

        if (orders.Count > 0)
        {
            // IsNavigating = orders.Peek() is MoveOrder;
            var order = orders.Peek();
            order.Tick();
            if (order.IsCompleted)
            {
                orders.Dequeue();

                //if (IsNavigating)
                //{
                //    if (orders.Count > 0 && orders.Peek() is MoveOrder)
                //    {
                //        IsNavigating = true;
                //        SphereOfInfluence? soi = World.GetSphereOfInfluence(this.Transform.Position);
                //        if (soi != null)
                //        {
                //            this.velocity = soi.planet.Transform.Position - soi.lastTickPosition;
                //        }
                //    }
                //}
                //else
                //{
                //    IsNavigating = false;
                //}
            }
        }

        if (orders.Count > 0 && orders.Peek() is MoveOrder)
        {
            this.Transform.Position += this.velocity * Program.Timestep;
            this.Transform.Rotation += this.angularVelocity * Program.Timestep;
        }
        else
        {
            SphereOfInfluence? soi = World.GetSphereOfInfluence(this.Transform.Position);
            soi?.ApplyTickTo(this);
        }

        //if (health <= 0)
        //{
        //    IsDestroyed = true;
        //    if (World.SelectionHandler.IsSelected(this))
        //        World.SelectionHandler.Deselect(this);
        //}

        //SphereOfInfluence? targetSoi = World.GetSphereOfInfluence(targetPosition);
        //targetPosition = targetSoi?.ApplyTickTo(targetPosition) ?? targetPosition;

        //Navigate();
    }

    public void EnqueueOrder(Order order)
    {
        bool wasNavigating = orders.Count == 0 || orders.Peek() is not MoveOrder;

        orders.Enqueue(order);

        order.OnEnqueued(this);

        if (wasNavigating && order is MoveOrder)
        {
            SphereOfInfluence? soi = World.GetSphereOfInfluence(this.Transform.Position);
            if (soi != null)
            {
                this.velocity = soi.planet.Transform.Position - soi.lastTickPosition;
            }
        }
    }

    public void ClearOrders()
    {
        orders.Clear();
    }

    //private void Navigate()
    //{
    //    DoubleVector targetDelta = targetPosition - Transform.Position;
    //    float targetRotation;
    //        targetRotation = Angle.FromVector(targetDelta.ToVector2());
    //    // if we have velocity we need to cancel it out
    //    //if (DoubleVector.Dot(velocity.Normalized(), targetDelta.Normalized()) < 0.999)
    //    //{
    //    //    targetRotation = Angle.FromVector(-velocity.ToVector2());
    //    //}
    //    //else
    //    //{
    //    //}
    //    this.Transform.Rotation = Angle.Step(Transform.Rotation, targetRotation, Prototype.TurnSpeed * Program.Timestep);

    //    float angleDelta = Angle.SignedDistance(targetRotation, Transform.Rotation);
    //    if (MathF.Abs(angleDelta) > 0.001f)
    //    {
    //        // not facing target, turn to face it

    //        float turnSpeedRadians = prototype.TurnSpeed * MathF.Tau;

    //        float timeToStop = MathF.Abs(angularVelocity) / turnSpeedRadians;
    //        float timeToTarget = MathF.Abs(angleDelta / angularVelocity);


    //        //if (timeToTarget <= timeToStop)
    //        //{
    //        //    DoAngularThrust(-MathF.Sign(angularVelocity), targetRotation);
    //        //}
    //        //else
    //        //{
    //        //    DoAngularThrust(MathF.Sign(angleDelta), targetRotation);
    //        //}
    //    }
    //    else
    //    {
    //        this.Transform.Rotation = targetRotation;

    //        // accelerate towards/away from target depending on distance & speed

    //        float timeToTarget = (float)targetDelta.Length() / (float)velocity.Length();
    //        float timeToStop = (float)velocity.Length() / prototype.FlySpeed;

    //        DoubleVector targetVelocity;
    //        if (timeToTarget <= timeToStop)
    //        {
    //            targetVelocity = DoubleVector.Zero;
    //        }
    //        else
    //        {
    //            float timeToMidpoint;
    //            if (velocity.LengthSquared() > 0)
    //            {
    //                timeToMidpoint = (timeToTarget - timeToStop);
    //                targetVelocity = this.velocity + (targetDelta.Normalized() * Prototype.FlySpeed * timeToMidpoint);
    //            }
    //            else
    //            {
    //                targetVelocity = targetDelta;
    //            }
    //        }

    //        this.velocity = Util.Step(this.velocity, targetVelocity, Prototype.FlySpeed * Program.Timestep);

    //        // this.Transform.Position = DoubleVector.Step(this.Transform.Position, targetPosition, this.Prototype.FlySpeed * Program.Timestep);

    //        // sqrt ( a/d )

    //    }


    //    //if (ShowTurningAngles)
    //    //{
    //    //    Color c = timeToTarget <= timeToStop ? Color.Red : Color.Green;
    //    //    DebugDraw.Ray(Vector2.Zero, Angle.ToVector(delta), s.Transform, c);
    //    //    DebugDraw.Ray(Vector2.Zero, Vector2.UnitX, s.Transform, c);
    //    //}


    //}


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

    public void MoveTo(DoubleVector target)
    {

    }

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

    public void DoAngularThrust(float throttle)
    {
        float turnAmount = (float.Tau * Prototype.TurnSpeed) * float.Clamp(throttle, -1, 1) * Program.Timestep;
        this.angularVelocity += turnAmount;
    }

    internal void ApplyLinearThrust(float throttle, DoubleVector targetVelocity)
    {
        float distance = Prototype.FlySpeed * float.Clamp(throttle, -1, 1) * Program.Timestep;
        velocity = Util.Step(velocity, targetVelocity, distance);
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