using SpaceGame.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;
internal static class BinaryWriterExtensions
{
    public static void Write(this BinaryWriter writer, Transform transform)
    {
        writer.Write(transform.Position);
        writer.Write(transform.Rotation);
        writer.Write(transform.Scale);
    }

    public static void Write(this BinaryWriter writer, Fixed32 value)
    {
        writer.Write(value.bits);
    }

    public static void Write(this BinaryWriter writer, FixedVector2 vec)
    {
        writer.Write(vec.X);
        writer.Write(vec.Y);
    }

    public static void Write(this BinaryWriter writer, Vector2 vec)
    {
        writer.Write(vec.X);
        writer.Write(vec.Y);
    }

    public static void Write(this BinaryWriter writer, HexCoordinate coordinate)
    {
        writer.Write(coordinate.Q);
        writer.Write(coordinate.R);
    }

    public static void Write<TActor>(this BinaryWriter writer, ActorReference<TActor> actor)
        where TActor : WorldActor
    {
        writer.Write(actor.ID);
    }
}
