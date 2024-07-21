﻿using SpaceGame;
using SpaceGame.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Planets;
internal class Planet : Actor
{
    public float Radius { get; init; } = 26;
    public Color Color { get; init; }

    public Orbit? Orbit 
    { 
        get 
        {
            return orbit;
        }
        set
        {
            value?.Update(this, 0);
            SphereOfInfluence.lastPosition = this.Transform.Position;
            this.orbit = value;
        }
    }

    public Grid Grid { get; }

    public SphereOfInfluence SphereOfInfluence { get; }

    private Orbit? orbit;

    public Planet()
    {
        Grid = new(this);
        SphereOfInfluence = new(this);
    }

    public override void Update()
    {
        Grid.Update();
    }

    public override void Render(ICanvas canvas)
    {
        canvas.Fill(Color);
        canvas.DrawCircle(0, 0, Radius);
        
        // Grid.Render(canvas);
        SphereOfInfluence.Render(canvas);
    }

    public void UpdateOrbit()
    {
        Orbit?.Update(this, Time.DeltaTime);
    }
}
