using ImGuiNET;
using SpaceGame.GUI;
using SpaceGame.Networking;
using SpaceGame.Networking.Packets;
using SpaceGame.Networking.Sockets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Scenes;
internal class MainMenuScene : Scene
{
    Menu currentMenu;
    Func<Menu>? nextMenu;

    public MainMenuScene()
    {
        currentMenu = new StartMenu(this);
    }

    public override void Render(ICanvas canvas)
    {
        canvas.Clear(Color.FromHSV(0, 0, .15f));
        canvas.Translate(0, canvas.Height / 2f);
        canvas.Fill(new Color(00, 00, 00, 100));
        canvas.DrawRect(0, -canvas.Height / 20f, canvas.Width, canvas.Height / 3f);
        canvas.Translate(canvas.Width / 2f, 0);
        currentMenu.Elements.Render(canvas);
    }

    public override void Tick()
    {
    }

    public override void Update(float tickProgress)
    {
        if (nextMenu != null)
        {
            currentMenu = nextMenu();
            nextMenu = null;
        }

        currentMenu.Update();
        currentMenu.Elements.UpdateSize(Window.Width / 10f, Window.Height);
        currentMenu.Elements.Update(Window.Width / 2f, Window.Height / 2f);
    }

    public void SwitchMenus(Func<Menu> menufunc)
    {
        nextMenu = menufunc;
    }
}


abstract class Menu(MainMenuScene scene)
{
    protected readonly MainMenuScene scene = scene;
    public ElementStack Elements { get; protected set; }

    public virtual void Update() { }
}

class StartMenu : Menu
{
    public StartMenu(MainMenuScene scene) : base(scene)
    {
        Elements = new([
            new TextButton("Singleplayer", OnSingleplayer) { Alignment = Alignment.Center, FitContainer = true },
            new TextButton("Host Game", OnHostGame) { Alignment = Alignment.Center, FitContainer = true },
            new TextButton("Join Game", OnJoinGame) { Alignment = Alignment.Center, FitContainer = true },
            new TextButton("Settings") { Alignment = Alignment.Center, FitContainer = true },
            new TextButton("Credits") { Alignment = Alignment.Center, FitContainer = true },
            new TextButton("Exit Game") { Alignment = Alignment.Center, FitContainer = true },
        ])
        {
            Alignment = Alignment.Center,
        };
    }

    private void OnSingleplayer()
    {
        Program.SwitchScenes(() => new GameScene());
    }

    private void OnHostGame()
    {
        scene.SwitchMenus(() => 
        {
            int port = NetworkSettings.DefaultPort;
            
            var networkHost = new SocketHost(port);
            Program.Host = new HostLobby(networkHost);

            var networkClient = new SocketClient("localhost", port);
            Program.Lobby = new ClientLobby(networkClient);
            
            return new LobbyMenu(scene);
        });
    }
    private void OnJoinGame()
    {
        scene.SwitchMenus(() => 
        {
            return new JoinGameMenu(scene); 
        });
    }
}

class LobbyMenu : Menu
{
    public LobbyMenu(MainMenuScene scene) : base(scene)
    {
        Elements = new([]);
    }

    public override void Update()
    {
        foreach (var (id, name) in Program.Lobby.players)
        {
            ImGui.Text(name);

            if (id == Program.Lobby.ClientID)
            {
                ImGui.SameLine();
                ImGui.Text(" (you)");
            }
        } 

        if (Program.Host is not null)
        {
            if (ImGui.Button("Start!"))
            {
                Program.Host.StartGame();
            }
        }

        if (Program.Lobby.network.ReceivePacket(out StartGamePacket? startPacket))
        {
            Program.SwitchScenes(() => new GameScene());
        }

        base.Update();
    }
}

class JoinGameMenu : Menu
{
    string host = "localhost";
    string port = NetworkSettings.DefaultPort.ToString();
    public JoinGameMenu(MainMenuScene scene) : base(scene)
    {
        Elements = new([
            new TextButton("Join", OnJoin),
        ]);
    }

    public override void Update()
    {
        ImGui.InputText("ip", ref host, 100);
        ImGui.InputText("port", ref port, 100);

        base.Update();
    }

    public void OnJoin()
    {
        SocketClient client = new SocketClient(host, int.Parse(port));
        Program.Lobby = new(client);
        scene.SwitchMenus(() => new LobbyMenu(scene));
    }

}