using SpaceGame.Commands;
using SpaceGame.GUI;
using SpaceGame.Planets;
using SpaceGame.Ships;
using SpaceGame.Ships.Modules;
using SpaceGame.Teams;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures.Shipyards;
internal class AssemblyBay : Structure
{
    public override AssemblyBayPrototype Prototype => (AssemblyBayPrototype)base.Prototype;

    private Element SelectionGUI;

    public bool isBuildingShip;
    public float progress;

    private int manufactoryCount;

    public AssemblyBay(AssemblyBayPrototype prototype, ulong id, ActorReference<Grid> grid, HexCoordinate location, int rotation, ActorReference<Team> team) : base(prototype, id, grid, location, rotation, team)
    {
        SelectionGUI = new TextButton("make ship", () =>
        {
            var cmdProc = (PlayerCommandProcessor)World.PlayerTeam.Actor!.CommandProcessor;
            cmdProc.AddCommand(new AssembleShipCommand(Prototypes.Get<AssembleShipCommandPrototype>("assemble_ship_command"), this));
        });
    }

    public void BuildShip()
    {
        if (Team.Actor!.Credits >= 100)
        {
            Team.Actor!.Credits -= 100;
            isBuildingShip = true;
            SelectionGUI = new ProgressBar(() => this.progress);
        }
    }

    public override void Tick()
    {
        if (isBuildingShip)
        {
            progress += Program.Timestep * manufactoryCount * .2f;
        }

        if (progress >= 1)
        {
            isBuildingShip = false;
            var ship = new Ship(Prototypes.Get<ShipPrototype>(Prototype.ShipPrototype), World.NewID(), this.Transform, this.Team);
            foreach (var moduleFactory in neighbors.OfType<ModuleFactory>())
            {
                ModulePrototype moduleProto = Prototypes.Get<ModulePrototype>(moduleFactory.Prototype.ProvidedModule);
                var module = moduleProto.CreateModule(World.NewID(), ship.AsReference());
                ship.modules.Add(module.AsReference());
                World.Add(module);
            }

            World.Add(ship);
            Reset();
        }

        base.Tick();
    }

    [MemberNotNull(nameof(SelectionGUI))]
    private void Reset()
    {
        isBuildingShip = false;
        SelectionGUI = new TextButton("make ship", BuildShip);
        progress = 0;
    }

    public override Element[]? GetSelectionGUI()
    {
        return [new ElementReference(() => SelectionGUI)];
    }

    public override void OnNeighborAdded(Structure neighbor)
    {
        base.OnNeighborAdded(neighbor);
        manufactoryCount = neighbors.Count(n => n is Manufactory);
    }

    public override void OnNeighborRemoved(Structure neighbor)
    {
        base.OnNeighborRemoved(neighbor);
        manufactoryCount = neighbors.Count(n => n is Manufactory);
    }

    public override void FinalizeDeserialization()
    {
        base.FinalizeDeserialization();
        manufactoryCount = neighbors.Count(n => n is Manufactory);
    }

    public override void Serialize(BinaryWriter writer)
    {
        base.Serialize(writer);
        writer.Write(isBuildingShip);
        writer.Write(progress);
    }
}

class AssemblyBayPrototype : StructurePrototype
{
    public string ShipPrototype { get; set; }

    public AssemblyBayPrototype(string title, int price, Model model, string? presetModel, HexCoordinate[] footprint) : base(title, price, model, presetModel, footprint)
    {
    }

    public override Structure CreateStructure(ulong id, ActorReference<Team> team, ActorReference<Grid> grid, HexCoordinate location, int rotation)
    {
        return new AssemblyBay(this, id, grid, location, rotation, team);
    }

    public override WorldActor Deserialize(BinaryReader reader)
    {
        base.DeserializeArgs(reader, out var id, out var team, out var grid, out var location, out var rotation);
        bool isBuildingShip = reader.ReadBoolean();
        float progress = reader.ReadSingle();

        return new AssemblyBay(this, id, grid, location, rotation, team)
        {
            isBuildingShip = isBuildingShip,
            progress = progress,
        };

    }
}
