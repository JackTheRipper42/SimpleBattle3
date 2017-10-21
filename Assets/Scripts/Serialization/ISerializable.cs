namespace Serialization
{
    public interface ISerializable
    {
        void Serialize(SerializationInfo serializationInfo);
        void Deserialize(SerializationInfo serializationInfo);
    }
}