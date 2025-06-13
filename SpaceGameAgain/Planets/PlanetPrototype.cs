using SpaceGame.Extensions;
using SpaceGame.Rendering;
using SpaceGame.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Planets;
internal class PlanetPrototype : Prototype
{
    public override Type ActorType => typeof(Planet);

    public BackgroundMaterial Material { get; set; }

    //public override Actor Deserialize(BinaryReader reader)
    //{
    //    ulong id = reader.ReadUInt64();
    //    Transform transform = reader.ReadTransform();

    //    float radius = reader.ReadSingle();
    //    Color color = (Color)reader.ReadUInt32();
    //    float soiRad = reader.ReadSingle();

    //    ActorReference<Grid> grid = reader.ReadActorReference<Grid>();

    //    bool hasOrbit = reader.ReadBoolean();
    //    Orbit? orbit = null;
    //    if (hasOrbit)
    //    {
    //        ActorReference<Actor> center = reader.ReadActorReference<Actor>();
    //        float phase = reader.ReadSingle();
    //        float orbitRadius = reader.ReadSingle();

    //        orbit = new Orbit(center, orbitRadius, phase);
    //    }

    //    Planet planet = new Planet(this, id, transform, orbit, grid)
    //    {
    //        Radius = radius,
    //        Color = color,
    //    };
    //    planet.SphereOfInfluence.Radius = soiRad;

    //    return planet;
    //}
}
