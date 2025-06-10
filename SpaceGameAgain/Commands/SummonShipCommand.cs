using SpaceGame.Debugging;
using SpaceGame.Ships;
using SpaceGame.Ships.Modules;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Commands;
internal class SummonShipCommand : Command
{
    private ActorReference<Team> team;
    private ShipPrototype shipPrototype;
    private ModulePrototype[] modulePrototypes;
    private Transform transform;

    public SummonShipCommand(SummonShipCommandPrototype prototype, ActorReference<Team> team, ShipPrototype shipPrototype, ModulePrototype[] modulePrototypes, Transform transform) : base(prototype)
    {
        this.team = team;
        this.shipPrototype = shipPrototype;
        this.modulePrototypes = modulePrototypes;
        this.transform = transform;
    }

    public override void Apply()
    {
        var ship = new Ship(shipPrototype, World.NewID(), transform, team);
        foreach (var modulePrototype in modulePrototypes)
        {
            var module = modulePrototype.CreateModule(World.NewID(), ship.AsReference());
            ship.modules.Add(module.AsReference());
            World.Add(module);
        }
        DebugLog.Message("summoned ship");
        World.Add(ship);
    }

    public override void Serialize(BinaryWriter writer)
    {
        writer.Write(team);
        writer.Write(shipPrototype.Name);
        
        writer.Write(modulePrototypes.Length);
        foreach (var module in modulePrototypes)
        {
            writer.Write(module.Name);
        }
        writer.Write(transform);
    }
}

class SummonShipCommandPrototype : CommandPrototype
{
    public override SummonShipCommand Deserialize(BinaryReader reader)
    {
        ActorReference<Team> team = reader.ReadActorReference<Team>();
        ShipPrototype shipPrototype = Prototypes.Get<ShipPrototype>(reader.ReadString());

        int moduleCount = reader.ReadInt32();
        ModulePrototype[] modulePrototypes = new ModulePrototype[moduleCount];
        for (int i = 0; i < moduleCount; i++)
        {
            modulePrototypes[i] = Prototypes.Get<ModulePrototype>(reader.ReadString());
        }

        Transform transform = reader.ReadTransform();

        return new SummonShipCommand(this, team, shipPrototype, modulePrototypes, transform);
    }
}