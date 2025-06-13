using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Serialization.Serializers;
internal class ByteArraySerializer : Serializer
{
    public override object Deserialize(BinaryReader reader)
    {
        int count = reader.ReadInt32();
        return reader.ReadBytes(count);
    }

    public override void Serialize(BinaryWriter writer, object value)
    {
        byte[] array = (byte[])value;
        writer.Write(array.Length);
        writer.Write(array);
    }
}
