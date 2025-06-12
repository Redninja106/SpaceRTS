using SpaceGame.Extensions;
using SpaceGame.Interaction;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Ships.Fleets;
internal class Fleet : Actor
{
    public ActorReference<Team> team;
    public ActorReference<Ship>[] ships;

    public Fleet(FleetPrototype prototype, ulong id, Transform transform, ActorReference<Team> team, ActorReference<Ship>[] ships) : base(prototype, id, transform)
    {
        this.team = team;
        this.ships = ships;
    }

    // public ITexture Icon { get; } => Icons.Ship;

    public override void Serialize(BinaryWriter writer)
    {
        writer.Write(ID);
        writer.Write(Transform);
        writer.Write(team);
        writer.Write(ships.Length);
        foreach (var ship in ships)
        {
            writer.Write(ship.ID);
        }
    }
}


class FleetPrototype : ActorPrototype
{
    public override Actor Deserialize(BinaryReader reader)
    {
        ulong id = reader.ReadUInt64();
        Transform transform = reader.ReadTransform();
        int shipCount = reader.ReadInt32();
        ActorReference<Team> team = reader.ReadActorReference<Team>();
        ActorReference<Ship>[] ships = new ActorReference<Ship>[shipCount];
        for (int i = 0; i < shipCount; i++)
        {
            ships[i] = reader.ReadActorReference<Ship>();
        }
        return new Fleet(this, id, transform, team, ships);
    }
}