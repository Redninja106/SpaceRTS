using System.Collections;

namespace SpaceGame.Serialization.Serializers;

class DictionarySerializer : Serializer
{
    private Type dictionaryType;

    private Serializer keySerializer;

    private Serializer valueSerializer;

    public DictionarySerializer(Type dictionaryType, Type keyType, Type valueType)
    {
        this.dictionaryType = dictionaryType;

        keySerializer = GetSerializer(keyType);
        valueSerializer = GetSerializer(valueType);
    }

    public override object Deserialize(BinaryReader reader)
    {
        int count = reader.ReadInt32();
        IDictionary dictionary = (IDictionary)Activator.CreateInstance(dictionaryType)!;

        for (int i = 0; i < count; i++)
        {
            object key = keySerializer.Deserialize(reader);
            object value = valueSerializer.Deserialize(reader);
            dictionary.Add(key, value);
        }

        return dictionary;
    }

    public override void Serialize(BinaryWriter writer, object value)
    {
        IDictionary dictionary = (IDictionary)value;

        writer.Write(dictionary.Count);
        foreach (var (key, val) in dictionary.Keys.OfType<object>().Zip(dictionary.Values.OfType<object>()))
        {
            keySerializer.Serialize(writer, key);
            valueSerializer.Serialize(writer, val);
        }
    }
}
