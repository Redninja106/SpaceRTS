using SpaceGame;
using SpaceGame.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Planets;
internal class Planet : Actor
{
    public float Radius { get; set; } = 26;
    public Color Color { get; set; }

    public Orbit? Orbit 
    { 
        get 
        {
            return orbit;
        }
        set
        {
            if (value != null)
            {
                value?.Tick(this, 0);
                SphereOfInfluence.lastPosition = this.Transform.Position;
            }
            this.orbit = value;
        }
    }

    public Grid Grid { get; private set; }

    public SphereOfInfluence SphereOfInfluence { get; }

    private Orbit? orbit;

    public Planet()
    {
        SphereOfInfluence = new(this);
        Grid = new(this);
        World.Grids.Add(Grid);
    }

    public override void Update(float tickProgress)
    {
        base.Update(tickProgress);
        Grid.Update(tickProgress);
    }

    public override void Tick()
    {
        base.Tick();
        Grid.Tick();
    }

    public override void Render(ICanvas canvas)
    {
        canvas.Fill(Color);
        canvas.DrawCircle(0, 0, Radius);
        
        // Grid.Render(canvas);
        SphereOfInfluence.Render(canvas);
    }

    public void TickOrbit()
    {
        Orbit?.Tick(this, Program.TickDelta);
    }

    public override void Load(Stream stream)
    {
        base.Load(stream);
        Radius = stream.ReadValue<float>();
        Color = stream.ReadValue<Color>();
        SphereOfInfluence.Load(stream);
        bool hasOrbit = stream.ReadValue<bool>();
        if (hasOrbit)
        {
            orbit = new();
            orbit.Load(stream);
        }

        Grid.Load(stream);
    }

    public override void Save(Stream stream)
    {
        base.Save(stream);
        stream.WriteValue(Radius);
        stream.WriteValue(Color);
        
        SphereOfInfluence.Save(stream);

        bool hasOrbit = orbit != null;
        stream.WriteValue(hasOrbit);
        Orbit?.Save(stream);
        
        Grid.Save(stream);
    }
}
