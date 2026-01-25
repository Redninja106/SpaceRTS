using Newtonsoft.Json;
using SpaceGame.Generation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SpaceGame.Data.Converters;
internal class RandomPrototypeConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return objectType.IsGenericType && objectType.GetGenericTypeDefinition() == typeof(RandomPrototype<>);
    }

    public override object? ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        Type elementType = objectType.GetGenericArguments()[0];
        Type weightedObjectType = objectType.GetNestedType(nameof(RandomPrototype<object>.WeightedObject))!.MakeGenericType(elementType);
        
        if (reader.TokenType == JsonToken.StartArray)
        {
            reader.Read();

            List<object> choices = [];
            while (reader.TokenType != JsonToken.EndArray)
            {
                object choice;
                if (reader.TokenType == JsonToken.String)
                {
                    string name = (string)reader.Value!;
                    reader.Read();
                    Prototype p = Prototypes.Get(name);
                    choice = Activator.CreateInstance(weightedObjectType, p)!;
                }
                else
                {
                    choice = serializer.Deserialize(reader, weightedObjectType)!;
                    reader.Read();
                }
                choices.Add(choice);
            }

            Array arr = Array.CreateInstance(weightedObjectType, choices.Count);
            choices.CopyTo((object[])arr);
            return Activator.CreateInstance(objectType, arr);
        }

        if (reader.TokenType == JsonToken.String)
        {
            Prototype p = serializer.Deserialize<Prototype>(reader)!;
            object choice = Activator.CreateInstance(weightedObjectType, p)!;
            return Activator.CreateInstance(objectType, [choice]);
        }

        object singleObject = serializer.Deserialize(reader, elementType)!;
        return Activator.CreateInstance(objectType, [Activator.CreateInstance(weightedObjectType, singleObject)]);
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
