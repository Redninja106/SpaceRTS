using SpaceGame.Commands;
using SpaceGame.Extensions;
using SpaceGame.GUI;
using SpaceGame.Rendering;
using SpaceGame.Structures;
using SpaceGame.Teams;

namespace SpaceGame;

abstract class UnitPrototype : Prototype
{
    public int MaxHealth { get; set; } = 1;
    public string Title { get; set; } = "";
    public double CollisionRadius { get; set; } = .5f;
    public double RevealRadius { get; set; } = 1;
    public SpriteModel Model { get; set; } = null!;
    public Icon Icon { get; set; } = null!;
    public ConstructionCategory Category { get; set; } = null!;

    public int Armor { get; set; } = 0;
    public float ArmorEffectiveness { get; set; } = .5f;
    public int Cost { get; set; }

    public string? Description { get; set; }

    public DefenseInfo BaseDefenseInfo { get; set; } = DefenseInfo.Default;

    public override void InitializePrototype()
    {
        Model ??= Prototypes.Get<SpriteModel>("default_model");
        Icon ??= Prototypes.Get<Icon>("default_icon");
        Category ??= Prototypes.Get<ConstructionCategory>("default_category");

        base.InitializePrototype();
    }

    public virtual void Layout(GUIWindow window)
    {
    }
}
