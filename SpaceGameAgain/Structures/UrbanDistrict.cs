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


    public UrbanDistrict(StructurePrototype prototype, ulong id, ActorReference<Grid> grid, HexCoordinate location, int rotation, ActorReference<Team> team) : base(prototype, id, grid, location, rotation, team)
    {
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

            // World.Add(new TextWidget(Prototypes.Get<TextWidgetPrototype>("income_text_widget"), World.NewID(), this.Transform, $"${payout}k"));
        }

        base.Tick();
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
