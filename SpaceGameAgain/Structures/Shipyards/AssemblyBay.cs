using SpaceGame.Commands;
using SpaceGame.Economy;
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

    // private Element SelectionGUI;

    public bool isBuildingShip;
    public float progress;

    private int manufactoryCount;

    public AssemblyBay(AssemblyBayPrototype prototype, ulong id, ActorReference<Grid> grid, HexCoordinate location, int rotation, ActorReference<Team> team) : base(prototype, id, grid, location, rotation, team)
    {
        // SelectionGUI = new TextButton("make ship", () =>
        // {
        //     var cmdProc = (PlayerCommandProcessor)World.PlayerTeam.Actor!.CommandProcessor;
        //     cmdProc.AddCommand(new AssembleShipCommand(Prototypes.Get<AssembleShipCommandPrototype>("assemble_ship_command"), this));
        // });
    }

    [DebugButton]
    public void BuildShip()
    {
        ResourcePrototype aluminum = Prototypes.Get<ResourcePrototype>("aluminum");

        //if (Enabled) // && Team.Actor!.resources[aluminum])
        //{
            // Team.Actor!.resources[aluminum] -= 100;
            isBuildingShip = true;
        //}
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
            Transform shipTransform = this.Transform;
            shipTransform = shipTransform.Translated(DoubleVector.FromVector2(this.Prototype.Center.Rotated(this.Rotation * MathF.Tau / 6f)));
            shipTransform.Rotation = this.Rotation * MathF.Tau / 6f - (MathF.PI / 2f);
            shipTransform.Position.Y -= Prototype.ShipPrototype.FlyHeight;

            var ship = new Ship(Prototype.ShipPrototype, World.NewID(), shipTransform, this.Team);
            foreach (var moduleFactory in neighbors.OfType<ModuleFactory>())
            {
                var module = moduleFactory.Prototype.ProvidedModule.CreateModule(World.NewID(), ship.AsReference());
                ship.modules.Add(module.AsReference());
                World.Add(module);
            }

            World.Add(ship);
            Reset();
        }

        base.Tick();
    }

    private void Reset()
    {
        isBuildingShip = false;
        // SelectionGUI = new TextButton("make ship", BuildShip);
        progress = 0;
    }

    // public override Element[]? GetSelectionGUI()
    // {
    //     return [new ElementReference(() => SelectionGUI)];
    // }

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

    public override void Layout(GUIWindow window)
    {
        if (isBuildingShip)
        {
            window.ProgressBar(this.progress, 100);
        }
        else
        {
            if (window.TextButton("assemble ship"))
            {
                var commandProcessor = (PlayerCommandProcessor)World.PlayerTeam.Actor!.CommandProcessor;
                commandProcessor.AddCommand(new AssembleShipCommand(Prototypes.Get<AssembleShipCommandPrototype>("assemble_ship_command"), this));
            }
        }

        base.Layout(window);
    }
}

class AssemblyBayPrototype : StructurePrototype
{
    public ShipPrototype ShipPrototype { get; set; }

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
