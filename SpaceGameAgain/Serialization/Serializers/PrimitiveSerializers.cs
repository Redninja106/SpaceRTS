using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Serialization.Serializers;
internal static class PrimitiveSerializers
{
    public static void Register(Dictionary<Type, Serializer> typeSerializers)
    {
        typeSerializers.Add(typeof(int), new Int32Serializer());
        typeSerializers.Add(typeof(ulong), new UInt64Serializer());
        typeSerializers.Add(typeof(float), new SingleSerializer());
        typeSerializers.Add(typeof(double), new DoubleSerializer());
        typeSerializers.Add(typeof(bool), new BooleanSerializer());
    }

    public class Int32Serializer : Serializer
    {
        public override object Deserialize(BinaryReader reader)
        {
            return reader.ReadInt32();
        }

        public override void Serialize(BinaryWriter writer, object value)
        {
            writer.Write((int)value);
        }
    }

    public class UInt64Serializer : Serializer
    {
        public override object Deserialize(BinaryReader reader)
        {
            return reader.ReadUInt64();
        }

        public override void Serialize(BinaryWriter writer, object value)
        {
            writer.Write((ulong)value);
        }
    }


    public class SingleSerializer : Serializer
    {
        public override object Deserialize(BinaryReader reader)
        {
            return reader.ReadSingle();
        }

        public override void Serialize(BinaryWriter writer, object value)
        {
            writer.Write((float)value);
        }
    }

    public class DoubleSerializer : Serializer
    {
        public override object Deserialize(BinaryReader reader)
        {
            return reader.ReadDouble();
        }

        public override void Serialize(BinaryWriter writer, object value)
        {
            writer.Write((double)value);
        }
    }

    public class BooleanSerializer : Serializer
    {
        public override object? Deserialize(BinaryReader reader)
        {
            return reader.ReadBoolean();
        }

        public override void Serialize(BinaryWriter writer, object value)
        {
            writer.Write((bool)value);
        }
    }
}
