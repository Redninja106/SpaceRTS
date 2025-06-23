using SpaceGame.Combat;
using SpaceGame.Commands;
using SpaceGame.GUI;
using SpaceGame.Interaction;
using SpaceGame.Planets;
using SpaceGame.Serialization;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;

internal abstract class Unit(UnitPrototype prototype, GameWorld world, ulong id) : Actor(prototype, world, id), IDestructable, IGUIProvider, ISelectable, IDamagable
{
    public override UnitPrototype Prototype => (UnitPrototype)base.Prototype;

    [field: Serialize]
    public required Team Team { get; set; }

    [field: Serialize]
    public int Health { get; set; } = prototype.MaxHealth;

    public bool ClientVisible => World.tick - LastClientVisibleTick < 50;
    public ulong LastClientVisibleTick { get; set; }
    public virtual bool CanAttack => false;

    bool IDestructable.IsDestroyed => Health <= 0;

    public virtual bool CanReveal => Team == World.PlayerTeam;

    public abstract ITexture Icon { get; }
    public abstract void Layout(GUIWindow window);

    public override void Render(ICanvas canvas)
    {
        base.Render(canvas);

        canvas.PushState();
        canvas.Rotate(-this.Transform.Rotation);
        this.Prototype.Model?.Render(canvas, this.InterpolatedTransform, ColorF.White);
        canvas.PopState();
    }

    public virtual void RenderBackgroundOverlay(ICanvas canvas, Camera camera, bool selected)
    {
    }

    public virtual void RenderGroundOverlay(ICanvas canvas, Camera camera, bool selected)
    {
        canvas.Transform(World.Camera.CreateRelativeMatrix(InterpolatedTransform));
        canvas.Stroke(World.PlayerTeam.GetRelationColor(Team) with { A = (byte)(selected ? 255 : 100) });
        canvas.DrawCircle(0, 0, (float)GetCollisionRadius());
    }

    public virtual void RenderSkyOverlay(ICanvas canvas, Camera camera, bool selected)
    {
    }

    public override void Tick()
    {
        base.Tick();
    }

    public virtual void OnDestroyed()
    {
    }

    public virtual double GetCollisionRadius()
    {
        return Prototype.CollisionRadius;
    }

    public virtual double GetRevealRadius()
    {
        return double.Max(GetCollisionRadius(), Prototype.RevealRadius);
    }

    public virtual bool TestPoint(DoubleVector point)
    {
        double collisionRadius = this.GetCollisionRadius();
        return DoubleVector.DistanceSquared(this.Transform.Position, point) <= collisionRadius * collisionRadius;
    }

    public virtual DoubleVector GetCenter()
    {
        return Transform.Position;
    }

    public void Damage(DamageInfo damage)
    {
        float effectiveDamage = Prototype.BaseDefenseInfo.GetEffectiveDamage(damage);

        float wholeDamage = MathF.Floor(effectiveDamage);
        float partialDamage = effectiveDamage - wholeDamage;
        
        Health -= (int)wholeDamage;
        // partial damage is probability based: .5 incoming damage has a 50% chance reduce health by 1
        if (World.TickRandom.NextSingle() <= partialDamage)
        {
            Health--;
        }
    }
}

struct DamageInfo
{
    public float Amount;
    public DamageKind Kind;
    public Unit source;
}

struct DefenseInfo
{
    public static readonly DefenseInfo Default = new()
    {
        Armor = 0,
        ArmorEffectiveness = .5f,
    };

    public int Armor;
    public float ArmorEffectiveness;

    public float Shield;

    public readonly float GetEffectiveDamage(DamageInfo info)
    {
        float damage = info.Amount;

        int effectiveArmor = Armor;
        if (info.Kind == DamageKind.ArmorPiercing)
        {
            effectiveArmor = int.Max(0, (int)damage);
        }

        damage = ApplyArmor(damage, effectiveArmor, this.ArmorEffectiveness);

        return damage;
    }

    private static float ApplyArmor(float baseDamage, float armor, float effectiveness)
    {
        return (1f - effectiveness) * baseDamage + effectiveness * float.Max(baseDamage - armor, 0);
    }
}

enum DamageKind
{
    Normal,
    // Energy,
    ArmorPiercing
}