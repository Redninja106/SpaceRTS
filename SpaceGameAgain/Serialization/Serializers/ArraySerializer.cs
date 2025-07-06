using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Serialization.Serializers;
internal class ArraySerializer : Serializer
{
    Type arrayType;
    Type elementType;
    Serializer elementSerializer;

    public ArraySerializer(SerializationContext context, Type arrayType, Type elementType)
    {
        this.arrayType = arrayType;
        this.elementType = elementType;
        this.elementSerializer = context.GetSerializer(elementType);
    }

    public override object Deserialize(BinaryReader reader)
    {
        int length = reader.ReadInt32();
        Array array = Array.CreateInstance(elementType, length);
        for (int i = 0; i < length; i++)
        {
            array.SetValue(elementSerializer.Deserialize(reader), i);
        }
        return array;
    }

    public override void Serialize(BinaryWriter writer, object value)
    {
        Array array = (Array)value;
        writer.Write(array.Length);
        for (int i = 0; i < array.Length; i++)
        {
            elementSerializer.Serialize(writer, array.GetValue(i)!);
        }
    }
}
