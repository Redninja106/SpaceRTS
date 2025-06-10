using SpaceGame.Debugging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Combat;
internal abstract class WeaponSystem : WorldActor, IDestructable
{
    public override WeaponSystemPrototype Prototype => (WeaponSystemPrototype)base.Prototype;

    [DebugOverlay]
    public static bool ShowWeaponRotation;

    public ActorReference<Unit> unit;
    public Vector2 Offset { get; set; }

    protected WeaponSystem(WeaponSystemPrototype prototype, ulong id, ActorReference<Unit> unit) : base(prototype, id, Transform.Default)
    {
        this.unit = unit;
    }

    public bool IsDestroyed => ((IDestructable)unit.Actor!).IsDestroyed;

    public override void Render(ICanvas canvas)
    {
        canvas.Rotate(-this.InterpolatedTransform.Rotation);
        Prototype.Model?.Render(canvas, this.InterpolatedTransform , ColorF.White);

        // Prototype.Model?.Render(canvas, Transform.Default with { Rotation = this.InterpolatedTransform.Rotation }, ColorF.White);
        base.Render(canvas);
    }

    public override void Tick()
    {
        base.Tick();

        this.Transform.Position = unit.Actor.Transform.Position + DoubleVector.FromVector2(Offset.Rotated(unit.Actor.Transform.Rotation));

        if (ShowWeaponRotation)
        {
            DebugDraw.Ray(Vector2.Zero, Angle.ToVector(this.Transform.Rotation), this.Transform with { Rotation = 0 });
        }
    }

    public void OnDestroyed()
    {
    }
}
