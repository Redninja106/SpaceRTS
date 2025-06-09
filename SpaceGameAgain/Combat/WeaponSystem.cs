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

    protected WeaponSystem(WeaponSystemPrototype prototype, ulong id, ActorReference<Unit> unit) : base(prototype, id, Transform.Default)
    {
        this.unit = unit;
    }

    public bool IsDestroyed => ((IDestructable)unit.Actor!).IsDestroyed;

    public override void Tick()
    {
        base.Tick();

        if (ShowWeaponRotation)
        {
            DebugDraw.Ray(Vector2.Zero, Angle.ToVector(this.Transform.Rotation), unit.Actor!.Transform);
        }
    }

    public void OnDestroyed()
    {
    }
}
