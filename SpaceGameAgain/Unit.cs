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

internal abstract class Unit(UnitPrototype prototype, ulong id) : Actor(prototype, id), IDestructable, IGUIProvider, ISelectable, IDamagable
{
    public override UnitPrototype Prototype => (UnitPrototype)base.Prototype;

    [field: Serialize]
    public required Team Team { get; set; }

    [field: Serialize]
    public int Health { get; set; } = prototype.MaxHealth;

    public bool ClientVisible => World.tick - LastClientVisibleTick < 50;
    public ulong LastClientVisibleTick { get; set; }
    public virtual bool CanAttack => false;

    public abstract ITexture Icon { get; }

    bool IDestructable.IsDestroyed => Health <= 0;

    public virtual void OnDestroyed()
    {
    }

    //public virtual Element[]? GetSelectionGUI()
    //{
    //    return null;
    //}

    public override void Tick()
    {
        base.Tick();
    }

    public virtual double GetCollisionRadius()
    {
        return Prototype.CollisionRadius;
    }

    public virtual double GetRevealRadius()
    {
        return Prototype.RevealRadius;
    }

    //public virtual CommandPrototype[] GetCommands()
    //{
    //    return [];
    //}

    public abstract bool TestPoint(DoubleVector point);
    public abstract void Layout(GUIWindow window);

    public virtual DoubleVector GetCenter()
    {
        return Transform.Position;
    }

    public virtual void DrawHighlightAbove(ICanvas canvas, Camera camera, bool selected)
    {
    }

    public virtual void DrawHighlightBelow(ICanvas canvas, Camera camera, bool selected)
    {
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