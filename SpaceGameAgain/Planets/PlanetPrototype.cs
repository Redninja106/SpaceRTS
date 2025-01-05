using SpaceGame.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Planets;
internal class PlanetPrototype : WorldActorPrototype
{
    public override WorldActor Deserialize(BinaryReader reader)
    {
        ulong id = reader.ReadUInt64();
        Transform transform = reader.ReadTransform();

        float radius = reader.ReadSingle();
        Color color = (Color)reader.ReadUInt32();

        ActorReference<Grid> grid = reader.ReadActorReference<Grid>();

        Planet planet = new Planet(this, id, transform, grid)
        {
            Radius = radius,
            Color = color,
        };

        bool hasOrbit = reader.ReadBoolean();
        if (hasOrbit)
        {
            ActorReference<WorldActor> center = reader.ReadActorReference<WorldActor>();
            float phase = reader.ReadSingle();
            float orbitRadius = reader.ReadSingle();

            planet.Orbit = new Orbit(center, orbitRadius, phase);
        }

        return planet;
    }
}
