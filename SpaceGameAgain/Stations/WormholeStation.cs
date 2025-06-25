using SpaceGame.Economy;
using SpaceGame.GUI;
using SpaceGame.Interaction;
using SpaceGame.Orders;
using SpaceGame.Ships;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Stations;

[Serializable]
internal class WormholeStation : Station
{
    public override WormholeStationPrototype Prototype => (WormholeStationPrototype)base.Prototype;

    [Serialize] public WormholeStation? Link;
    [Serialize] public WormholeStation? EstablishingLink;
    [Serialize] public int EstablishTicksRemaining;

    public HashSet<Ship> ships = [];

    public WormholeStation(StationPrototype prototype, GameWorld world, ulong id) : base(prototype, world, id)
    {
    }

    public override void Render(ICanvas canvas)
    {
        base.Render(canvas);

        if (Link != null) 
        {
            foreach (var ship in Link.ships)
            {
                canvas.PushState();
                DoubleVector relativeLocation = ship.InterpolatedTransform.Position - Link.InterpolatedTransform.Position;
                canvas.Translate(relativeLocation.ToVector2());

                bool shipHovered = World.SelectInteractionContext.target == ship;
                bool shipSelected = World.SelectionHandler.IsSelected(ship);

                if (shipHovered || shipSelected)
                {
                    Color color = ship.Team.GetRelationColor(World.PlayerTeam);
                    if (shipHovered && !shipSelected)
                    {
                        color = color with { A = 200 };
                    }
                    SelectionHandler.RenderUnitOutline(canvas, ship, color);
                }

                ship.Prototype.Model.Render(canvas, new Transform { Position = this.Transform.Position, Rotation = ship.InterpolatedTransform.Rotation }, new ColorF(1, 1, 1, 1));
                canvas.PopState();
            }
        }

    }

    public override void RenderGroundOverlay(ICanvas canvas, Camera camera, bool selected)
    {
        base.RenderGroundOverlay(canvas, camera, selected);

        if (Link != null || EstablishingLink != null)
        {
            var link = Link! ?? EstablishingLink!;
            canvas.PushState();
            canvas.Fill(new Color(255, 255, 255, 100));
            canvas.DrawLine(Vector2.Zero, (link.InterpolatedTransform.Position - this.InterpolatedTransform.Position).ToVector2());
            canvas.PopState();
        }
    }

    public override void Tick()
    {
        base.Tick();


        if (EstablishingLink != null && EstablishTicksRemaining == 0)
        {
            Link = EstablishingLink;
            EstablishingLink = null;
        }
        else
        {
            EstablishTicksRemaining--;
        }

        foreach (var ship in ships)
        {
            ship.WormholeStation = null;
        }
        ships.Clear();
        foreach (var ship in World.Ships)
        {
            if (ship.Transform.Distance(this.Transform) <= this.Prototype.CollisionRadius)
            {
                ships.Add(ship);
                ship.WormholeStation = this;
            }
        }
    }

    public override void Layout(GUIWindow window)
    {
        if (Link != null)
        {
            window.Text("linked");
        }
        else if (EstablishingLink != null)
        {
            window.ProgressBar(1 - (EstablishTicksRemaining / (float)Prototype.LinkEstablishTime), 100);
        }
        else
        {
            if (window.TextButton("establish link"))
            {
                World.CurrentInteractionContext = new WormholeStationPicker(this); 
            }
        }

        base.Layout(window);
    }

    public void BeginEstablishingLink(WormholeStation link)
    {
        this.EstablishTicksRemaining = Prototype.LinkEstablishTime;
        this.EstablishingLink = link;
    }

    public void WarpShip(Ship ship)
    {
        DoubleVector offset = ship.Transform.Position - this.Transform.Position;
        Transform destination = ship.Transform with { Position = Link!.Transform.Position + offset };
        ship.Teleport(destination);
    }
}

class WormholeStationPrototype : StationPrototype 
{
    public override Type ActorType => typeof(WormholeStation);

    public required int LinkEstablishTime { get; set; } = 600;

    public override void Layout(GUIWindow window)
    {
        using (window.Row())
        {
            window.ModelImage(this.Model, new(64, 64));

            using (window.Column())
            {
                window.Text(this.Title, 24);

                using (window.Row())
                {
                    window.Text("$" + this.Cost + "k");
                }

                if (!string.IsNullOrWhiteSpace(this.Description))
                {
                    window.Text(this.Description);
                }
            }
        }
    }
}

