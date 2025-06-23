using SpaceGame;
using SpaceGame.Data;
using SpaceGame.Ships;
using SpaceGame.Ships.Modules;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGameAgain.Tests;
internal class TestWorld : GameWorld
{
    public TestWorld()
    {
        this.PlayerTeam = (Team)Prototypes.Get("player_team").CreateActor(this, this.NewID());
    }

    public Ship AddShip(ShipPrototype prototype, ModulePrototype[]? modules = null, Transform? transform = null, Team? team = null, float? height = null)
    {
        Ship ship = new Ship(prototype, this, this.NewID()) { Team = team ?? PlayerTeam, height = height ?? prototype.FlyHeight };
        ship.Teleport(transform ?? Transform.Default);
        foreach (var mod in modules ?? [])
        {
            ship.modules.Add(mod.CreateActor(this, this.NewID()));
        }

        Add(ship);
        return ship;
    }

    public void DoTicks(int count, Action? ontick = null)
    {
        for (int i = 0; i < count; i++)
        {
            Tick(Vector2.Zero);
            ontick?.Invoke();
        }
    }

    public void TickWhile(Func<bool> condition, int limit = 1000)
    {
        while (condition() && limit >= 0)
        {
            Tick(Vector2.Zero);
            limit--;
        }
    }
}
