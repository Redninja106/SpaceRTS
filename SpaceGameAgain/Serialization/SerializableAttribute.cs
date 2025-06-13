namespace SpaceGame.Serialization;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
class SerializableAttribute : Attribute
{
    public bool AllFields { get; set; }
    public bool Abstract { get; set; }
}
