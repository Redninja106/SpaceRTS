using SpaceGame.Commands;
using SpaceGame.Extensions;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Ships.Fleets;
internal class CreateFleetCommand : Command
{
    public string fleetPrototype;
    public ActorReference<Team> team;
    public ActorReference<Ship>[] ships;

    public CreateFleetCommand(CreateFleetCommandPrototype prototype, string fleetPrototype, ActorReference<Team> team, ActorReference<Ship>[] ships) : base(prototype)
    {
        this.team = team;
        this.ships = ships;
        this.fleetPrototype = fleetPrototype;
    }

    public override void Apply()
    {
        var fleet = new Fleet(Prototypes.Get<FleetPrototype>(fleetPrototype), World.NewID(), ships[0].Actor!.Transform, team, ships);
        foreach (var ship in ships)
        {
            ship.Actor!.Fleet = fleet;
        }

        World.Add(fleet);
    }

    public override void Serialize(BinaryWriter writer)
    {
        writer.Write(fleetPrototype);
        writer.Write(team);
        writer.Write(ships.Length);
        foreach (var ship in ships)
        {
            writer.Write(ship.ID);
        }
    }
}

class CreateFleetCommandPrototype : CommandPrototype
{
    public override CreateFleetCommand Deserialize(BinaryReader reader)
    {
        string fleetPrototype = reader.ReadString();
        ActorReference<Team> team = reader.ReadActorReference<Team>();
        int shipCount = reader.ReadInt32();
        ActorReference<Ship>[] ships = new ActorReference<Ship>[shipCount];
        for (int i = 0; i < shipCount; i++)
        {
            ships[i] = reader.ReadActorReference<Ship>();
        }
        return new CreateFleetCommand(this, fleetPrototype, team, ships);
    }
}

//class SummonActorCommand : Command
//{
//    private ISummonablePrototype actorPrototype;
//    private WorldActor templateActor;
//    private byte[] serializedTemplateActor;

//    public SummonActorCommand(SummonActorCommandPrototype prototype, ISummonablePrototype actorPrototype, Actor templateActor) : base(prototype)
//    {
//        templateActor.Serialize()
//    }

//    public override void Apply()
//    {
//        WorldActor actor = actorPrototype.Deserialize(new BinaryReader(new MemoryStream(serializedTemplateActor)));
//        WorldActor.OverwriteID(actor, World.NewID());
//    }

//    public override void Serialize(BinaryWriter writer)
//    {

//    }
//}

//class SummonActorCommandPrototype : CommandPrototype
//{
//    public override SummonActorCommand Deserialize(BinaryReader reader)
//    {
//        throw new NotImplementedException();
//    }
//}

//interface ISummonablePrototype
//{

//}