﻿using SpaceGame.Structures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame;
internal static class BinaryReaderExtensions
{
    public static Transform ReadTransform(this BinaryReader reader)
    {
        return new Transform()
        {
            Position = reader.ReadFixedVector2(),
            Rotation = reader.ReadSingle(),
            Scale = reader.ReadVector2(),
        };
    }

    public static Fixed32 ReadFixed32(this BinaryReader reader)
    {
        return new(reader.ReadInt32());
    }

    public static Vector2 ReadVector2(this BinaryReader reader)
    {
        return new(reader.ReadSingle(), reader.ReadSingle());
    }

    public static FixedVector2 ReadFixedVector2(this BinaryReader reader)
    {
        return new(reader.ReadFixed32(), reader.ReadFixed32());
    }

    public static HexCoordinate ReadHexCoordinate(this BinaryReader reader)
    {
        return new(reader.ReadInt32(), reader.ReadInt32());
    }

    public static ActorReference<TActor> ReadActorReference<TActor>(this BinaryReader reader)
        where TActor : WorldActor
    {
        ulong id = reader.ReadUInt64();
        return ActorReference<TActor>.Create(id);
    }
}
