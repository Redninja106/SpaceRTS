using SpaceGame.GUI;
using SpaceGame.Teams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Structures;
internal class UrbanDistrict : Structure
{
    public override UrbanDistrictPrototype Prototype => (UrbanDistrictPrototype)base.Prototype;

    int nearbyTradeHubs = 0;
    int payoutCooldown;

    float widgetHeight = 0;
    string widgetText = "";

    public UrbanDistrict(UrbanDistrictPrototype prototype, ulong id, ActorReference<Grid> grid, HexCoordinate location, int rotation, ActorReference<Team> team) : base(prototype, id, grid, location, rotation, team)
    {
        this.payoutCooldown = prototype.PayoutInterval;
    }

    public override void Tick()
    {
        payoutCooldown--;

        if (payoutCooldown <= 0)
        {
            int payout = Prototype.PayoutAmount;

            if (nearbyTradeHubs > 0)
            {
                payout *= 2;
            }

            this.Team.Actor!.Money += payout;
            payoutCooldown = Prototype.PayoutInterval;

            World.TextWidgets.AddEventWidget(new TextWidget(this.Transform, $"${payout}k"));
        }

        base.Tick();
    }

    public override void Render(ICanvas canvas)
    {
        base.Render(canvas);
    }

    public override void OnNeighborAdded(Structure neighbor)
    {
        if (neighbor is Spaceport spaceport)
        {
            nearbyTradeHubs++;
        }

        base.OnNeighborAdded(neighbor);
    }

    public override void OnNeighborRemoved(Structure neighbor)
    {
        if (neighbor is Spaceport spaceport)
        {
            nearbyTradeHubs--;
        }

        base.OnNeighborRemoved(neighbor);
    }
}
