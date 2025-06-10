using SpaceGame.Combat;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;
internal class Turret : Structure
{
    public override TurretPrototype Prototype => (TurretPrototype)base.Prototype;

    public ActorReference<WeaponSystem>[] weaponSystems;
    public override bool CanAttack => true;

    public Turret(TurretPrototype prototype, ulong id, ActorReference<Grid> grid, HexCoordinate location, int rotation, ActorReference<Team> team) : base(prototype, id, grid, location, rotation, team)
    {
    }

    public override void Render(ICanvas canvas)
    {
        base.Render(canvas);
        // Prototype.TurretModel?.Render(canvas, this.InterpolatedTransform with { Rotation = weaponSystem.Actor!.InterpolatedTransform.Rotation }, ColorF.White);
    }

    public override void DrawHighlightAbove(ICanvas canvas, Camera camera, bool selected)
    {
        base.DrawHighlightAbove(canvas, camera, selected);

        foreach (var system in weaponSystems)
        {
            canvas.PushState();
            Transform.Create(GetCenter(), 0).ApplyTo(canvas, camera);
            canvas.Stroke(Color.White with { A = 40 });
            canvas.DrawCircle(0, 0, system.Actor!.Prototype.Range);
            canvas.PopState();
        }
    }

    public override void Tick()
    {
        base.Tick();
    }

    public override void Serialize(BinaryWriter writer)
    {
        base.Serialize(writer);
        writer.Write(weaponSystems.Length);
        for (int i = 0; i < weaponSystems.Length; i++)
        {
            writer.Write(weaponSystems.Length);
        }
    }
}
