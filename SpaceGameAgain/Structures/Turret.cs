using SpaceGame.Combat;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;

[Serializable]
internal class Turret(TurretPrototype prototype, GameWorld world, ulong id) : Structure(prototype, world, id)
{
    public override TurretPrototype Prototype => (TurretPrototype)base.Prototype;
    
    [Serialize]
    public WeaponSystem[] weaponSystems;
    public override bool CanAttack => true;

    public override void InitializeActor()
    {
        base.InitializeActor();

        weaponSystems = new WeaponSystem[Prototype.WeaponSystems.Length];
        for (int i = 0; i < weaponSystems.Length; i++)
        {
            var weapon = Prototype.WeaponSystems[i].Prototype.CreateActor(World, World.NewID());
            weapon.Unit = this;
            weapon.Offset = this.Prototype.Center + Prototype.WeaponSystems[i].Offset.Rotated(this.Transform.Rotation);
            weaponSystems[i] = weapon;
            World.Add(weapon);
        }
    }

    public override void Render(ICanvas canvas)
    {
        base.Render(canvas);
        // Prototype.TurretModel?.Render(canvas, this.InterpolatedTransform with { Rotation = weaponSystem.InterpolatedTransform.Rotation }, ColorF.White);
    }

    public override void RenderBackgroundOverlay(ICanvas canvas, Camera camera, bool selected)
    {
        base.RenderBackgroundOverlay(canvas, camera, selected);

        foreach (var system in weaponSystems)
        {
            canvas.PushState();
            Transform.Create(GetCenter(), 0).ApplyTo(canvas, camera);
            canvas.Stroke(Color.White with { A = 40 });
            canvas.DrawCircle(0, 0, system.Prototype.Range);
            canvas.PopState();
        }
    }

    public override void Tick()
    {
        base.Tick();
    }
}
