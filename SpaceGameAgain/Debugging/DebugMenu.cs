using ImGuiNET;
using Silk.NET.Core.Native;
using Silk.NET.OpenGL;
using SpaceGame.Commands;
using SpaceGame.GUI;
using SpaceGame.Networking;
using SpaceGame.Serialization;
using SpaceGame.Ships;
using SpaceGame.Ships.Modules;
using SpaceGame.Stations;
using SpaceGame.Structures;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using static Program;

namespace SpaceGame.Debugging;
internal static class DebugMenu
{
    public static bool Open = false;
    internal static ObjectViewer objectViewer = new();
    private static bool showImGuiDemo;
    private static bool pinned = true;
    public static bool renderNoise = true;

    static DebugMenu()
    {
        ImGui.GetStyle().WindowRounding = GUIWindow.CornerRadius;
    }

    public static void Layout()
    {
        if (Keyboard.IsKeyPressed(Key.F1))
        {
            Open = !Open;
        }

        if (World == null)
        {
            return;
        }

        if (Open && pinned)
        {
            ImGui.SetNextWindowPos(new(ImGui.GetIO().DisplaySize.X, 0), ImGuiCond.Always, new(1, 0));
            float minimapSize = Minimap.TextureSize + World.GUIViewport.Scale * 12;
            ImGui.SetNextWindowSize(new(minimapSize, ImGui.GetIO().DisplaySize.Y - minimapSize));
        }

        if (Open && ImGui.Begin("debug menu", ref Open, ImGuiWindowFlags.MenuBar | (pinned ? ImGuiWindowFlags.NoResize : 0)))
        {
            if (ImGui.BeginMenuBar())
            {
                LayoutFileMenuBar();

                if (ImGui.BeginMenu("menu"))
                {
                    ImGui.MenuItem("demo window", null, ref showImGuiDemo);
                    ImGui.MenuItem("pinned", null, ref pinned);
                    if (ImGui.MenuItem("go to main menu", null))
                    {
                        Program.NavigateToMainMenu();
                    }

                    ImGui.EndMenu();
                }

                ImGui.Checkbox("render noise", ref renderNoise);

                ImGui.EndMenuBar();
            }

            if (ImGui.BeginTabBar("tabbar", 0))
            {
                if (ImGui.BeginTabItem("world"))
                {
                    LayoutWorldTab();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("prototypes"))
                {
                    LayoutPrototypesTab();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("editor"))
                {
                    LayoutEditorTab();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("network"))
                {
                    LayoutNetworkTab();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("overlays"))
                {
                    DebugOverlays.Layout();
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("performance"))
                {
                    LayoutPerformanceTab();
                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }
        }
        ImGui.End();

        if (objectViewer.WantsFocus)
        {
            objectViewer.WantsFocus = false;
            ImGui.SetNextWindowFocus();
        }

        if (objectViewer.Open && ImGui.Begin("object viewer", ref objectViewer.Open, ImGuiWindowFlags.MenuBar))
        {
            objectViewer.Layout();
        }
        ImGui.End();

        if (showImGuiDemo)
        {
            ImGui.ShowDemoWindow();
        }
    }

    private static Team? editorTeam = null;
    private static List<ModulePrototype> editorModules = [];
    private static ShipPrototype? editorShipPrototype = null;
    private static StructurePrototype? editorStructurePrototype = null;
    private static StationPrototype? editorStationPrototype = null;
    private static int editorStructureRotation = 0;

    private static void LayoutEditorTab()
    {
        if (Program.Lobby != null)
        {
            ImGui.TextDisabled("editor only works in singleplayer");
            return;
        }

        Team[] teams = World.Actors.Select(a => a.Value as Team).Where(t => t != null).ToArray()!;

        if (editorTeam == null)
        {
            editorTeam = teams[0];
        }
        int teamIdx = Array.IndexOf(teams, editorTeam);
        if (ImGui.Combo("team", ref teamIdx, string.Join("\0", teams.Select(t => t.Name))))
        {
            editorTeam = teams[teamIdx];
        }

        if (ImGui.Button("delete selected units"))
        {
            foreach (var unit in World.SelectionHandler.GetSelectedUnits())
            {
                unit.Health = 0;
            }
        }

        {
            ImGui.SeparatorText("Place Ship (F2)");
            ShipPrototype[] shipPrototypes = Prototypes.GetAll<ShipPrototype>();
            int idx = Array.IndexOf(shipPrototypes, editorShipPrototype);
            if (ImGui.Combo("prototype", ref idx, string.Join("\0", shipPrototypes.Select(s => s.Name))))
            {
                editorShipPrototype = shipPrototypes[idx];
            }

            if (ImGui.BeginListBox("modules"))
            {
                for (int i = 0; i < editorModules.Count; i++)
                {
                    ImGui.Text(editorModules[i].Name);
                    ImGui.SameLine();
                    if (ImGui.SmallButton("delete##" + i))
                    {
                        editorModules.RemoveAt(i);
                        i--;
                    }
                }
                if (ImGui.SmallButton("add"))
                {
                    ImGui.OpenPopup("add_module");
                }

                if (ImGui.BeginPopup("add_module"))
                {
                    ModulePrototype[] modulePrototypes = Prototypes.GetAll<ModulePrototype>();
                    foreach (var proto in modulePrototypes)
                    {
                        if (ImGui.Selectable(proto.Name))
                        {
                            editorModules.Add(proto);
                            ImGui.CloseCurrentPopup();
                        }
                    }
                    ImGui.EndPopup();
                }

                ImGui.EndListBox();
            }

            if (editorShipPrototype != null && Keyboard.IsKeyPressed(Key.F2))
            {
                var ship = new Ship(editorShipPrototype, World, World.NewID()) { Team = editorTeam };
                ship.Teleport(new Transform() { Position = World.MousePosition });

                foreach (var modulePrototype in editorModules)
                {
                    var module = modulePrototype.CreateActor(World, World.NewID());
                    module.Ship = ship;
                    ship.modules.Add(module);
                    World.Add(module);
                }
                World.Add(ship);

                // give ship initial velocity
                var soi = World.GetSphereOfInfluence(ship.Transform.Position);
                if (soi != null)
                {
                    ship.PreviousTransform.Position -= soi.lastTickVelocity;
                }
            }
        }

        {
            ImGui.SeparatorText("Place Structure (F3, F4 to rotate)");
            StructurePrototype[] structurePrototypes = Prototypes.GetAll<StructurePrototype>();
            int idx = Array.IndexOf(structurePrototypes, editorStructurePrototype);
            if (ImGui.Combo("prototype##structure", ref idx, string.Join("\0", structurePrototypes.Select(s => s.Name))))
            {
                editorStructurePrototype = structurePrototypes[idx];
            }

            if (editorStructurePrototype != null)
            {
                Grid? grid = World.Grids.FirstOrDefault(g => g.GetCellFromPoint(World.MousePosition) != null);
                if (grid != null)
                {
                    ImGui.TextDisabled("grid: " + grid.ID);
                    HexCoordinate hoveredLocation = HexCoordinate.FromCartesian(grid.Transform.WorldToLocal(World.MousePosition.ToVector2()));
                    ImGui.TextDisabled("placing at " + hoveredLocation.ToString());

                    if (Keyboard.IsKeyDown(Key.F3))
                    {
                        Color color = grid.IsStructureObstructed(editorStructurePrototype, hoveredLocation, editorStructureRotation) ? Color.Red : Color.Green;
                        var outline = editorStructurePrototype.Outline;

                        if (Keyboard.IsKeyPressed(Key.F4))
                        {
                            editorStructureRotation++;
                        }

                        float angle = editorStructureRotation * (float.Tau / 6f);
                        for (int i = 0; i < outline.Length; i += 2)
                        {
                            DebugDraw.Line(outline[i].Rotated(angle) + hoveredLocation.ToCartesian(), outline[i + 1].Rotated(angle) + hoveredLocation.ToCartesian(), grid.Transform, color);
                        }
                    }

                    if (Keyboard.IsKeyReleased(Key.F3))
                    {
                        if (!grid.IsStructureObstructed(editorStructurePrototype, hoveredLocation, editorStructureRotation))
                        {
                            grid.PlaceStructure(editorStructurePrototype, hoveredLocation, editorStructureRotation, editorTeam);
                        }
                    }
                }
            }
        }

        {
            ImGui.PushID("station");

            ImGui.SeparatorText("Place Station (F5)");
            StationPrototype[] stationPrototypes = Prototypes.GetAll<StationPrototype>();
            int idx = Array.IndexOf(stationPrototypes, editorStationPrototype);
            if (ImGui.Combo("prototype", ref idx, string.Join("\0", stationPrototypes.Select(s => s.Name))))
            {
                editorStationPrototype = stationPrototypes[idx];
            }

            if (editorStationPrototype != null && Keyboard.IsKeyPressed(Key.F5))
            {
                var station = (Station)editorStationPrototype.CreateActor(World, World.NewID());
                station.Team = editorTeam;
                station.Teleport(Transform.Default with { Position = World.MousePosition });
                World.Add(station);
            }

            ImGui.PopID();
        }
    }

    private static string addressOrPort = "45454";
    private static void LayoutNetworkTab()
    {
        if (Program.Lobby == null)
        {
            ImGui.InputText("address/port", ref addressOrPort, 64);

            if (ImGui.Button("host"))
            {
                if (ParseAddressAndPort(out var _, out var port))
                {
                    SocketServer server = new(port);
                    Program.Lobby = new HostedLobby(Program.World, server);
                }
            }

            ImGui.SameLine();
            if (ImGui.Button("join"))
            {
                if (ParseAddressAndPort(out var address, out var port))
                {
                    SocketClient client = new(address, port);
                    Program.Lobby = new RemoteLobby(Program.World, client);
                }
            }

            static bool ParseAddressAndPort([NotNullWhen(true)] out string? address, out int port)
            {
                string[] addressParts = addressOrPort.Split(":");

                if (addressParts.Length == 2 && int.TryParse(addressParts[1], out port))
                {
                    address = addressParts[0];
                    return true;
                }

                if (int.TryParse(addressParts[0], out port))
                {
                    address = "localhost";
                    return true;
                }
                else
                {
                    address = addressParts[0];
                    port = NetworkSettings.DefaultPort;
                    return true;
                }
            }
        }
        else
        {
            if (Program.Lobby is RemoteLobby remote)
            {
                ImGui.TextDisabled("connected to " + remote.client.GetEndPoint());
            }
            if (Program.Lobby is HostedLobby host)
            {
                ImGui.TextDisabled("hosting");
            }
        }


        ImGui.SeparatorText("Command buffers");

        foreach (var team in World.Teams)
        {
            if (ImGui.TreeNode($"{team.Name}: {team.GetCommandProcessor()?.GetType()?.Name ?? "(none)"}"))
            {
                Dictionary<ulong, Command[]>? commands = team.GetCommandProcessor() switch
                {
                    NetworkCommandProcessor network => network.commands,
                    PlayerCommandProcessor player => player.commands,
                    _ => null
                };

                if (commands != null)
                {
                    foreach (var (turn, cmds) in commands)
                    {
                        if (ImGui.TreeNode("turn " + turn))
                        {
                            foreach (var cmd in cmds)
                            {
                                ImGui.Text(cmd.ToString());
                            }

                            ImGui.TreePop();
                        }
                    }
                }
                else
                {
                    ImGui.Text("(none)");
                }

                ImGui.TreePop();
            }
        }
    }

    private static DebugSearch<Prototype>? prototypeSearch = null;
    private static void LayoutPrototypesTab()
    {
        prototypeSearch ??= new DebugSearch<Prototype>(() => Prototypes.RegisteredPrototypes.ToList(), proto => [proto.Name, proto.ToString()!]);

        prototypeSearch.Layout();
        prototypeSearch.Prune();

        foreach (var prototype in prototypeSearch.QueryItems)
        {
            if (ImGui.Selectable($"{prototype.Name} ({prototype.GetType().Name})"))
            {
                objectViewer.View(prototype);
            }
        }
    }

    private static void LayoutFileMenuBar()
    {
        if (ImGui.BeginMenu("file"))
        {
            if (ImGui.MenuItem("save"))
            {
                WorldSerializer serializer = new();
                using var fs = new FileStream("./level", FileMode.Create);
                BinaryWriter writer = new(fs, Encoding.UTF8);
                serializer.Serialize(World, Program.SerializationContext, writer);
            }
            if (ImGui.MenuItem("load"))
            {
                WorldSerializer serializer = new();
                using var fs = new FileStream("./level", FileMode.Open);
                BinaryReader reader = new(fs, Encoding.UTF8);

                Program.World = serializer.Deserialize(reader);
                Program.CurrentScene = World;
            }

            ImGui.EndMenu();
        }
    }

    private static string actorListQuery = "";
    private static int lastActorCount = 0;

    private static void LayoutWorldTab()
    {
        ImGui.Text($"turn: {World.TurnProcessor.turn}");
        ImGui.Text($"remaining ticks: {World.TurnProcessor.RemainingTicks}");

        ImGui.Text($"tick: {World.tick}");
        float tickProgress = MathF.Min(Program.timeAccumulated * Program.GameSpeed / Program.Timestep, 1);
        if (Program.GameSpeed == 0)
        {
            tickProgress = 1;
        }
        ImGui.Text($"tick progress: {tickProgress}");

        if (ImGui.TreeNode("speed controls"))
        {
            if (ImGui.SmallButton("reset"))
            {
                Program.GameSpeed = 1;
            }
            ImGui.SameLine();
            if (ImGui.SmallButton("force tick"))
            {
                Program.forceTickThisFrame = true;
            }
            ImGui.SliderFloat("game speed", ref Program.GameSpeed, 0.0f, 5);
            ImGui.TreePop();
        }

        ImGui.Separator();

        ImGui.Text("next id: " + World.NextID);
        ImGui.Text("interaction context: " + (World.CurrentInteractionContext?.GetType()?.Name ?? "null"));

        ImGui.Separator();

        if (ImGui.TreeNode("camera"))
        {
            World.Camera.DebugLayout();
            ImGui.TreePop();
        }

        ImGui.SeparatorText("actors");

        ImGui.InputTextWithHint("##search", "search", ref actorListQuery, 128);
        foreach (var (id, actor) in World.Actors)
        {
            if (actor.ToString().Contains(actorListQuery, StringComparison.OrdinalIgnoreCase))
            {
                if (ImGui.Selectable(actor.ToString()))
                {
                    objectViewer.View(actor);
                }
            }
        }
    }

    public static void ViewObject(object? obj)
    {
        objectViewer.View(obj);
    }

    private static Stack<PerformanceMetric> currentMetrics = [];
    private static List<PerformanceMetric> rootMetrics = [];

    public static void PushMetric(string? name = null)
    {
        if (name == null)
        {
            StackFrame stackFrame = new StackFrame(1);
            MethodBase? method = stackFrame.GetMethod();
            if (method != null)
            {
                name = $"{method?.DeclaringType?.Name}.{method?.Name}";
            }
            else
            {
                name = "unknown";
            }
        }

        PerformanceMetric newMetric = new(name, Stopwatch.GetTimestamp());

        if (currentMetrics.TryPeek(out PerformanceMetric? parent))
        {
            parent.submetrics.Add(newMetric);
        }
        else
        {
            var prevMetric = rootMetrics.FirstOrDefault(m => m.name == name);
            if (prevMetric != null)
            {
                rootMetrics.Remove(prevMetric);
            }
            rootMetrics.Add(newMetric);
        }

        currentMetrics.Push(newMetric);
    }

    public static void PopMetric()
    {
        var metric = currentMetrics.Pop();
        metric.endTimestamp = Stopwatch.GetTimestamp();
    }

    public static void ClearMetrics()
    {
        currentMetrics.Clear();
        //frameMetrics.Clear();
        //currentMetrics.Clear();
    }

    public static void LayoutPerformanceTab()
    {
        float elapsedX = ImGui.GetWindowSize().X - 70;
        double totalMs = 1000 * ((rootMetrics.Last().endTimestamp - rootMetrics.First().startTimestamp) / (double)Stopwatch.Frequency);
        foreach (var rootMetric in rootMetrics)
        {
            rootMetric.Layout(elapsedX, totalMs);
        }
    }

    class PerformanceMetric(string name, long timestamp)
    {
        public string name = name;
        public long startTimestamp = timestamp;
        public long endTimestamp = timestamp;
        public List<PerformanceMetric> submetrics = [];

        public void Layout(float elapsedX, double totalElapsedMs)
        {
            double elapsedMs = 1000 * ((endTimestamp - startTimestamp) / (double)Stopwatch.Frequency);

            bool open = ImGui.TreeNode($"{name} {100 * (elapsedMs / totalElapsedMs):n1}%###{name}");

            ImGui.SameLine();
            ImGui.SetCursorPosX(elapsedX);
            ImGui.Text($"{elapsedMs:n3}ms");

            if (open)
            {
                foreach (var submetric in submetrics)
                {
                    submetric.Layout(elapsedX, totalElapsedMs);
                }

                ImGui.TreePop();
            }
        }
    }

    class DebugSearch<T>
    {
        private string query = "";
        private int pruneIndex = 0;
        private Func<T, string[]>? stringProvider;
        private Func<List<T>> allItems;

        public List<T> QueryItems { get; private set; }

        public string Query
        {
            get => query;
            set
            {
                query = value;
                pruneIndex = 0;
            }
        }

        public DebugSearch(Func<List<T>> allItems, Func<T, string[]>? stringProvider = null)
        {
            this.allItems = allItems;
            this.stringProvider = stringProvider;

            QueryItems = this.allItems();
        }

        public void Layout()
        {
            string prevQuery = query;
            if (ImGui.InputTextWithHint("##search", "search", ref query, 128))
            {
                pruneIndex = 0;
                if (!query.Contains(prevQuery) || string.IsNullOrWhiteSpace(query))
                {
                    QueryItems = allItems();
                }
            }
        }

        public void Prune(int count = 100)
        {
            for (int i = 0; i < count; i++)
            {
                if (QueryItems.Count > pruneIndex)
                {
                    T item = QueryItems[pruneIndex];
                    string[] strings = stringProvider?.Invoke(item) ?? [item?.ToString() ?? ""];

                    if (strings.Any(s => s != null && s.Contains(query, StringComparison.InvariantCultureIgnoreCase)))
                    {
                        pruneIndex++;
                    }
                    else
                    {
                        QueryItems.RemoveAt(pruneIndex);
                    }
                }
            }
        }
    }
}
