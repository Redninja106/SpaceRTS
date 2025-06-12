using SpaceGame.Commands;
using SpaceGame.Economy;
using SpaceGame.GUI;
using SpaceGame.Planets;
using SpaceGame.Rendering;
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

    // serialized
    internal bool isBuildingShip;
    internal int productionProgress;

    // computed
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

        if (this.Team.Actor!.Money >= this.Prototype.ProductionCost)
        {
            this.Team.Actor!.Money -= this.Prototype.ProductionCost;
            isBuildingShip = true;
        }
    }

    public override void Tick()
    {
        base.Tick();
        
        manufactoryCount = this.neighbors.Count(n => n is Manufactory m && m.Enabled);

        if (isBuildingShip)
        {
            productionProgress += manufactoryCount;
        }

        if (productionProgress >= Prototype.ProductionTime)
        {
            productionProgress -= Prototype.ProductionTime;
            isBuildingShip = false;

            Transform shipTransform = this.Transform;
            shipTransform = shipTransform.Translated(DoubleVector.FromVector2(this.Prototype.Center.Rotated(this.Rotation * MathF.Tau / 6f)));
            shipTransform.Rotation = this.Rotation * MathF.Tau / 6f - (MathF.PI / 2f);
            shipTransform.Position.Y -= Prototype.ShipPrototype.FlyHeight;

            var ship = new Ship(Prototype.ShipPrototype, World.NewID(), shipTransform, this.Team);
            foreach (var moduleFactory in neighbors.OfType<ModuleFactory>())
            {
                if (moduleFactory.Enabled)
                {
                    var module = moduleFactory.Prototype.ProvidedModule.CreateModule(World.NewID(), ship.AsReference());
                    ship.modules.Add(module.AsReference());
                    World.Add(module);
                }
            }

            World.Add(ship);
            Reset();
        }
    }

    private void Reset()
    {
        isBuildingShip = false;
        // SelectionGUI = new TextButton("make ship", BuildShip);
        productionProgress = 0;
    }

    // public override Element[]? GetSelectionGUI()
    // {
    //     return [new ElementReference(() => SelectionGUI)];
    // }

    public override void FinalizeDeserialization()
    {
        base.FinalizeDeserialization();
        manufactoryCount = neighbors.Count(n => n is Manufactory);
    }

    public override void Serialize(BinaryWriter writer)
    {
        base.Serialize(writer);
        writer.Write(isBuildingShip);
        writer.Write(productionProgress);
    }

    public override void Layout(GUIWindow window)
    {
        if (this.Team == World.PlayerTeam)
        {
            if (isBuildingShip)
            {
                window.ProgressBar(this.productionProgress / (float)this.Prototype.ProductionTime, 100);
            }
            else
            {
                if (window.TextButton("assemble ship") && manufactoryCount > 0)
                {
                    var commandProcessor = (PlayerCommandProcessor)World.PlayerTeam.Actor!.GetCommandProcessor();
                    commandProcessor.AddCommand(new AssembleShipCommand(Prototypes.Get<AssembleShipCommandPrototype>("assemble_ship_command"), this));
                }

                if (window.LastItemHovered() && manufactoryCount == 0)
                {
                    World.GUIViewport.SetTooltip(w => w.Text("requires at least one adjacent operational manufactory!"));
                }
            }
        }

        base.Layout(window);
    }

}

class AssemblyBayPrototype : StructurePrototype
{
    public ShipPrototype ShipPrototype { get; set; }
    public int ProductionTime { get; set; }
    public int ProductionCost { get; set; }

    public override Structure CreateStructure(ulong id, ActorReference<Team> team, ActorReference<Grid> grid, HexCoordinate location, int rotation)
    {
        return new AssemblyBay(this, id, grid, location, rotation, team);
    }

    public override Actor Deserialize(BinaryReader reader)
    {
        base.DeserializeArgs(reader, out var id, out var team, out var grid, out var location, out var rotation);
        bool isBuildingShip = reader.ReadBoolean();
        int progress = reader.ReadInt32();

        return new AssemblyBay(this, id, grid, location, rotation, team)
        {
            isBuildingShip = isBuildingShip,
            productionProgress = progress,
        };
    }

    public override void RenderAdjacencyOverlay(ICanvas canvas, Vector2 position, StructurePrototype otherPrototype)
    {
        if (otherPrototype is ManufactoryPrototype)
        {
            canvas.DrawTexture(Icons.Industrial, position, new Vector2(.25f, .25f), Alignment.Center);
        }

        if (otherPrototype is ModuleFactoryPrototype)
        {
            ITexture icon = Icons.Construction;
            canvas.DrawTexture(icon, position, new Vector2(.25f, .25f), Alignment.Center);
        }

        base.RenderAdjacencyOverlay(canvas, position, otherPrototype);
    }
}
