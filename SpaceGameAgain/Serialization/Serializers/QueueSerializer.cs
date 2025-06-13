using System.Collections;
using System.Reflection;

namespace SpaceGame.Serialization.Serializers;

class QueueSerializer(Type queueType, Type elementType) : Serializer
{
    private Serializer elementSerializer = GetSerializer(elementType);

    public override void Serialize(BinaryWriter writer, object value)
    {
        ICollection queue = (ICollection)value;

        writer.Write(queue.Count);
        foreach (var element in queue)
        {
            elementSerializer.Serialize(writer, element);
        }
    }

    public override object Deserialize(BinaryReader reader)
    {
        int count = reader.ReadInt32();
        ICollection result = (ICollection)Activator.CreateInstance(queueType)!;
        MethodInfo enqueue = queueType.GetMethod("Enqueue")!;
        for (int i = 0; i < count; i++)
        {
            object element = elementSerializer.Deserialize(reader);
            enqueue.Invoke(result, [element]);
        }
        return result;
    }
}
