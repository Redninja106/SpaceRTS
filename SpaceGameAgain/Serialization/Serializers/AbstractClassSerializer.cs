using System.Reflection;

namespace SpaceGame.Serialization.Serializers;

class AbstractClassSerializer : Serializer
{
    private Dictionary<string, Serializer> subclasses;
    private Type abstractClass;

    public AbstractClassSerializer(Type abstractClass)
    {
        this.abstractClass = abstractClass;

        subclasses = [];
        foreach (var subclass in Assembly.GetExecutingAssembly().DefinedTypes)
        {
            if (subclass.IsSubclassOf(this.abstractClass) && subclass.GetCustomAttribute<SerializableAttribute>() != null)
            {
                subclasses.Add(subclass.Name, GetSerializer(subclass));
            }
        }
    }

    public override object Deserialize(BinaryReader reader)
    {
        Serializer subclassSerializer = subclasses[reader.ReadString()];
        return subclassSerializer.Deserialize(reader);
    }

    public override void Serialize(BinaryWriter writer, object value)
    {
        string subclassName = value.GetType().Name;
        Serializer subclassSerializer = subclasses[subclassName];
        writer.Write(subclassName);
        subclassSerializer.Serialize(writer, value);
    }
}
