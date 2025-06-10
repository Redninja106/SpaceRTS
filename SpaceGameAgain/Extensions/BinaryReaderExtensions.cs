using SpaceGame.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Extensions;
internal static class BinaryReaderExtensions
{
    public static Transform ReadTransform(this BinaryReader reader)
    {
        return new Transform()
        {
            Position = reader.ReadDoubleVector(),
            Rotation = reader.ReadSingle(),
            Scale = reader.ReadVector2(),
        };
    }

    public static Vector2 ReadVector2(this BinaryReader reader)
    {
        return new(reader.ReadSingle(), reader.ReadSingle());
    }

    public static DoubleVector ReadDoubleVector(this BinaryReader reader)
    {
        return new(reader.ReadDouble(), reader.ReadDouble());
    }

    public static HexCoordinate ReadHexCoordinate(this BinaryReader reader)
    {
        int q = reader.ReadInt32();
        int r = reader.ReadInt32();
        return new(q, r);
    }

    public static ActorReference<TActor> ReadActorReference<TActor>(this BinaryReader reader)
        where TActor : WorldActor
    {
        ulong id = reader.ReadUInt64();
        return ActorReference<TActor>.Create(id);
    }
}
