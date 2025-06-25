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
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SpaceGame.Structures.Shipyards;

[Serializable]
internal class AssemblyBay : Structure
{
    public override AssemblyBayPrototype Prototype => (AssemblyBayPrototype)base.Prototype;

    [Serialize]
    internal bool isBuildingShip;
    [Serialize]
    internal int productionProgress;

    // computed
    private int manufactoryCount;

    public AssemblyBay(AssemblyBayPrototype prototype, GameWorld world, ulong id) : base(prototype, world, id)
    {
        // SelectionGUI = new TextButton("make ship", () =>
        // {
        //     var cmdProc = (PlayerCommandProcessor)World.PlayerTeam.CommandProcessor;
        //     cmdProc.AddCommand(new AssembleShipCommand(Prototypes.Get<AssembleShipCommandPrototype>("assemble_ship_command"), this));
        // });
    }

    [DebugButton]
    public void BuildShip()
    {
        ResourcePrototype aluminum = Prototypes.Get<ResourcePrototype>("aluminum");

        if (this.Team.Money >= this.Prototype.ProductionCost)
        {
            this.Team.Money -= this.Prototype.ProductionCost;
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

            var ship = new Ship(Prototype.ShipPrototype, World, World.NewID()) { Team = this.Team };
            ship.Teleport(shipTransform);
            foreach (var moduleFactory in neighbors.OfType<ModuleFactory>())
            {
                if (moduleFactory.Enabled)
                {
                    var module = moduleFactory.Prototype.ProvidedModule.CreateActor(World, World.NewID());
                    module.Ship = ship;
                    ship.modules.Add(module);
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

    public override void FinishDeserialization()
    {
        base.FinishDeserialization();
        manufactoryCount = neighbors.Count(n => n is Manufactory);
    }

    //public override void Serialize(BinaryWriter writer)
    //{
    //    base.Serialize(writer);
    //    writer.Write(isBuildingShip);
    //    writer.Write(productionProgress);
    //}

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
                    var commandProcessor = (PlayerCommandProcessor)World.PlayerTeam.GetCommandProcessor();
                    commandProcessor.AddCommand(new AssembleShipCommand() { assemblyBay = this });
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
    public override Type ActorType => typeof(AssemblyBay);

    public ShipPrototype ShipPrototype { get; set; }
    public int ProductionTime { get; set; }
    public int ProductionCost { get; set; }

    //public override Structure CreateStructure(ulong id, ActorReference<Team> team, ActorReference<Grid> grid, HexCoordinate location, int rotation)
    //{
    //    return new AssemblyBay(this, id, grid, location, rotation, team);
    //}

    //public override Actor Deserialize(BinaryReader reader)
    //{
    //    base.DeserializeArgs(reader, out var id, out var team, out var grid, out var location, out var rotation);
    //    bool isBuildingShip = reader.ReadBoolean();
    //    int progress = reader.ReadInt32();

    //    return new AssemblyBay(this, id, grid, location, rotation, team)
    //    {
    //        isBuildingShip = isBuildingShip,
    //        productionProgress = progress,
    //    };
    //}

    public override void RenderAdjacencyOverlay(ICanvas canvas, Vector2 position, StructurePrototype otherPrototype)
    {
        if (otherPrototype is ManufactoryPrototype)
        {
            ITexture icon = Rendering.Icon.Get("industrial_icon").Texture64x64;
            canvas.DrawTexture(icon, position, new Vector2(.25f, .25f), Alignment.Center);
        }

        if (otherPrototype is ModuleFactoryPrototype)
        {
            ITexture icon = Rendering.Icon.Get("construction_icon").Texture64x64;
            canvas.DrawTexture(icon, position, new Vector2(.25f, .25f), Alignment.Center);
        }

        base.RenderAdjacencyOverlay(canvas, position, otherPrototype);
    }
}
