
namespace SpaceGame;

abstract class UnitPrototype : Prototype
{
    public override Type ActorType => typeof(Unit);

    public int MaxHealth { get; set; }
}
