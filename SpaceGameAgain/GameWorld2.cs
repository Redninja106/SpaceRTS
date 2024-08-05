using SpaceGame.Networking;
using SpaceGame.Planets;
using SpaceGame.Ships;
using SpaceGame.Structures;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;
internal class GameWorld2
{
    public List<Grid> Grids { get; }
    public List<Ship> Ships { get; }
    public List<Planet> Planets { get; }
    public List<Team> Teams { get; }

    private NetworkMap networkMap;

    public GameWorld2(WorldProvider worldProvider)
    {
    }

    public void Tick()
    {
    }
}
