namespace SpaceGame;

abstract class Prototype
{
    public virtual Type ActorType => typeof(Actor);

    public string Name { get; set; }

    public abstract Actor? Deserialize(BinaryReader reader);
}
