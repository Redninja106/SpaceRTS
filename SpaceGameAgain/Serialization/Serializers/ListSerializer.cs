using System.Collections;

namespace SpaceGame.Serialization.Serializers;

class ListSerializer(SerializationContext context, Type listType, Type elementType) : Serializer
{
    private Serializer elementSerializer = context.GetSerializer(elementType);

    public override void Serialize(BinaryWriter writer, object value)
    {
        IList list = (IList)value;

        writer.Write(list.Count);
        for (int i = 0; i < list.Count; i++)
        {
            elementSerializer.Serialize(writer, list[i]!);
        }
    }

    public override object Deserialize(BinaryReader reader)
    {
        int count = reader.ReadInt32();
        IList result = (IList)Activator.CreateInstance(listType)!;
        for (int i = 0; i < count; i++)
        {
            object element = elementSerializer.Deserialize(reader);
            result.Add(element);
        }
        return result;
    }
}
