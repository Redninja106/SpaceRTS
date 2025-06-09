using SpaceGame.Commands;
using SpaceGame.Teams;

namespace SpaceGame;

abstract class UnitPrototype : WorldActorPrototype
{
    public int MaxHealth { get; set; } = 1;
    public string Title { get; set; } = "";
    public double CollisionRadius { get; set; } = .5f;
    public double RevealRadius { get; set; } = 1;

    public void DeserializeArgs(BinaryReader reader, out ulong id, out Transform transform, out ActorReference<Team> team, out int health)
    {
        base.DeserializeArgs(reader, out id, out transform);
        team = reader.ReadActorReference<Team>();
        health = reader.ReadInt32();
    }
}
