using SpaceGame.Commands;
using SpaceGame.Extensions;
using SpaceGame.Rendering;
using SpaceGame.Teams;

namespace SpaceGame;

abstract class UnitPrototype : Prototype
{
    public int MaxHealth { get; set; } = 1;
    public string Title { get; set; } = "";
    public double CollisionRadius { get; set; } = .5f;
    public double RevealRadius { get; set; } = 1;
    public SpriteModel Model { get; set; }

    public int Armor { get; set; } = 0;
    public float ArmorEffectiveness { get; set; } = .5f;

    public DefenseInfo BaseDefenseInfo { get; set; } = DefenseInfo.Default;

    public override void InitializePrototype()
    {
        if (Model == null)
        {
            Model = Prototypes.Get<SpriteModel>("default_model");
        }
        base.InitializePrototype();
    }

}
