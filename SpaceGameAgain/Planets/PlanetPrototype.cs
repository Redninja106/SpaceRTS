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

    public override Actor? Deserialize(BinaryReader reader)
    {
        ulong id = reader.ReadUInt64();
        Transform transform = reader.ReadTransform();

        float radius = reader.ReadSingle();
        Color color = (Color)reader.ReadUInt32();


        Planet planet = new Planet(this, id, transform)
        {
            Radius = radius,
            Color = color,
        };

        bool hasOrbit = reader.ReadBoolean();
        if (hasOrbit)
        {
            ActorReference<Actor> center = reader.ReadActorReference<Actor>();
            float phase = reader.ReadSingle();
            float orbitRadius = reader.ReadSingle();

            planet.Orbit = new Orbit(center, orbitRadius, phase);
        }

        List<ActorReference<Structure>> structures = new();
        int structureCount = reader.ReadInt32();
        for (int i = 0; i < structureCount; i++)
        {
            structures.Add(reader.ReadActorReference<Structure>());
        }

        Dictionary<HexCoordinate, GridCell> cells = new();
        int cellCount = reader.ReadInt32();
        for (int i = 0; i < cellCount; i++)
        {
            HexCoordinate coordinate = reader.ReadHexCoordinate();
            ActorReference<Structure> cell = reader.ReadActorReference<Structure>();
            cells.Add(coordinate, new() { Structure = cell });
        }

        planet.Grid.structures = structures;
        planet.Grid.cells = cells;

        return planet;
    }
}
