using System.Reflection;

namespace SpaceGame.Serialization.Serializers;

struct FieldSerializer
{
    public Type ObjectType { get; }
    private List<(FieldInfo, Serializer)> fieldSerializers = [];

    public FieldSerializer(Type objectType)
    {
        ObjectType = objectType;

        foreach (var field in GetFields(objectType))
        {
            bool isNullable = field.GetCustomAttribute<System.Runtime.CompilerServices.NullableAttribute>() != null;
            fieldSerializers.Add((field, Serializer.GetSerializer(field.FieldType, isNullable)));
        }
    }

    private static IEnumerable<FieldInfo> GetFields(Type objectType)
    {
        SerializableAttribute attribute = objectType.GetCustomAttribute<SerializableAttribute>() ?? throw new($"{objectType} is not serializable!");
        Type type = objectType;

        while (type != typeof(object))
        {
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly))
            {
                if (attribute.AllFields || field.GetCustomAttribute<SerializeAttribute>() != null)
                {
                    yield return field;
                }
            }

            type = type.BaseType!;
        }
    }

    public readonly void Deserialize(BinaryReader reader, object value)
    {
        foreach (var (field, serializer) in fieldSerializers)
        {
            object? fieldValue = serializer.Deserialize(reader);
            field.SetValue(value, fieldValue);
        }
    }

    public readonly void Serialize(BinaryWriter writer, object value)
    {
        foreach (var (field, serializer) in fieldSerializers)
        {
            object? fieldValue = field.GetValue(value);
            serializer.Serialize(writer, fieldValue!);
        }
    }
}
