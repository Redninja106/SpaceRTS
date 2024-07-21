using SpaceGame.Structures;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Ships.Formations;
internal class ClusterFormation : Formation
{
    public static Vector2[] PlaceShips(Ship[] ships)
    {
        Vector2[] result = new Vector2[ships.Length];
        float spacing = .8f;

        int radius = 0;
        IEnumerator<HexCoordinate> currentRing = Array.Empty<HexCoordinate>().Append(HexCoordinate.Zero).GetEnumerator();
        for (int i = 0; i < ships.Length; i++)
        {
            if (!currentRing.MoveNext())
            {
                radius++;
                currentRing = Ring(radius).GetEnumerator();
                Debug.Assert(currentRing.MoveNext());
            }
            result[i] = currentRing.Current.ToCartesian() * spacing + Random.Shared.NextUnitVector2() * Random.Shared.NextSingle() * .25f;
        }

        return result;
    }

    private static IEnumerable<HexCoordinate> Ring(int radius)
    {
        HexCoordinate cursor = new HexCoordinate(radius, 0);
        HexCoordinate delta = HexCoordinate.UnitQ.Rotated(2);
        for (int i = 0; i < 6; i++)
        {
            for (int j = 0; j < radius; j++)
            {
                yield return cursor;
                cursor += delta;
            }
            delta = delta.RotatedRight();
        }
    }
}
