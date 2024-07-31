using NAudio.MediaFoundation;
using Silk.NET.Input;
using SpaceGame.GUI;
using SpaceGame.Ships;
using SpaceGame.Ships.Modules;
using SpaceGame.Ships.Orders;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;
internal class ShipyardBehavior : StructureBehavior
{
    public override Element[] SelectGUI { get; }

    private ElementStack moduleList;
    private Label slotsLabel;
    private ModuleSlot?[] moduleSlots = new ModuleSlot?[2];

    private List<BuildAction> buildQueue = [];

    private Element buildQueueElement;
    private Element currentElement;
    private TextButton buildButton;

    public ShipyardBehavior(StructureInstance instance) : base(instance)
    {
        UpdateQueueUI();
        SelectGUI = [
            buildButton = new TextButton("build ship | 20m")
            { FitContainer = true, Alignment = Alignment.Center },

            new Gap(32),

            new ElementReference(() => this.currentElement),
            new ElementReference(() => this.buildQueueElement),

            new Gap(32),
            
            new Label("CONFIGURATION", 24, Alignment.Center),
            new Separator(),
            moduleList = new ElementStack([
            ]) {  DrawBorder = true, },

            new ElementStack([
                new TextButton("construction kit", () => AddModule("construction kit", ship => new ConstructionModule(ship))),
                new TextButton("missile launcher", () => AddModule("missile launcher", ship => new MissileModule(ship))),
                new TextButton("chain gun", () => AddModule("chaingun turret", ship => new ChaingunModule(ship))),
            ])
            { DrawBorder = true, },
        ];

        UpdateModuleUI();

    }

    private void BuildShip()
    {
        Instance.Team.Materials -= 20;
        buildQueue.Add(new BuildAction()
        {
            partCounts = moduleSlots.Select(m => 0).ToArray(),
            timeWorked = 0,
            cost = 20,
        });
        UpdateQueueUI();
    }

    private void UpdateModuleUI()
    {
        moduleList.Clear();
        for (int i = 0; i < moduleSlots.Length; i++)
        {
            var slot = moduleSlots[i];
            if (slot is null)
            {
                moduleList.Add(new ElementRow([
                    new Label("empty")
                ]));
            }
            else
            {
                int v = i;
                moduleList.Add(new ElementRow([
                    new Label(slot.name),
                    new TextButton("x", () => {
                        moduleSlots[v] = null;
                        UpdateModuleUI();
                        }) { Alignment = Alignment.CenterRight },
                ])
                {

                });
            }
        }
    }

    [MemberNotNull(nameof(buildQueueElement), nameof(currentElement))]
    private void UpdateQueueUI()
    {
        if (buildQueue.Count <= 1)
        {
            buildQueueElement = new Label("--- queue empty ---", alignment: Alignment.Center);
        }
        else
        {
            buildQueueElement = new ElementStack([
                new Gap(32),
                new Label("QUEUE", 24, Alignment.Center),
                new Gap(Element.DefaultMargin),
                new ElementStack(
                    buildQueue.Skip(1).Select(b => new ElementRow([
                            new Label("ship") {  },
                            b.cancel = new TextButton("x", () => { }) { Alignment = Alignment.CenterRight },
                            b.now = new TextButton("^", () => { }) { Alignment = Alignment.CenterRight },
                            ]) { Margin = 2 }
                        ) 
                    )
                { DrawBorder = true,  },
                ])
            { Margin = 0 };
        }
        
        if (buildQueue.Count > 0)
        {
            var status = buildQueue[0];
            currentElement = new ElementStack([
                new ElementRow([
                    new Label("ship"),
                    new DynamicLabel(() => $"{15 - status.timeWorked:n0}s", alignment: Alignment.CenterRight),
                ]),
                new ProgressBar(() => status.timeWorked / 15f),
                new ElementStack(Enumerable.Range(0, status.partCounts.Length).Select(i => new DynamicLabel(() => $"{status.partCounts[i]}/5 parts"))),
            ]) { DrawBorder = true };
        }
        else
        {
            currentElement = new Gap(0);
        }
    }

    private void AddModule(string name, Func<Ship, Module> factory)
    {
        for (int i = 0; i < moduleSlots.Length; i++)
        {
            if (moduleSlots[i] is null)
            {
                moduleSlots[i] = new()
                {
                    name = name,
                    provider = factory,
                };
                UpdateModuleUI();
                return;
            }
        }
    }

    public override void Tick()
    {
        if (buildButton.clicked && Instance.Team.Materials >= 20)
        {
            BuildShip();
        }

        for (int i = 0; i < buildQueue.Count; i++)
        {
            var action = buildQueue[i];
            if (action.cancel?.clicked ?? false)
            {
                buildQueue.RemoveAt(i);
                i--;
                UpdateQueueUI();
            }
            if (action.now?.clicked ?? false)
            {
                buildQueue.RemoveAt(i);
                buildQueue.Insert(0, action);
                UpdateQueueUI();
            }
        }

        if (buildQueue.Count > 0)
        {
            buildQueue[0].timeWorked += Time.DeltaTime;

            if (buildQueue[0].timeWorked > 15)
            {
                var ship = new Ship(Instance.Team);
                ship.Transform.Position = Instance.Grid.Transform.Position + Instance.Location.ToCartesian();
                ship.Transform.Position -= HexCoordinate.UnitR.Rotated(Instance.Rotation).ToCartesian() * .5f;
                ship.Transform.Rotation = Instance.Rotation * (MathF.Tau / 6f) - MathF.PI / 2f;

                ship.modules.AddRange(moduleSlots.Where(s => s != null).Select(s => s.provider(ship)));

                ship.orders.Enqueue(new MoveOrder(ship.Transform.Position + (1 + 2 * Random.Shared.NextSingle()) * Random.Shared.NextUnitVector2()));
                World.Ships.Add(ship);

                buildQueue.RemoveAt(0);
                UpdateQueueUI();
            }
        }
    }

    private class BuildAction
    {
        public float timeWorked;
        public int[] partCounts;
        public int cost;
        internal TextButton cancel;
        internal TextButton now;
    }

    private class ModuleSlot
    {
        public string name;
        public Func<Ship, Module> provider;
    }
}
