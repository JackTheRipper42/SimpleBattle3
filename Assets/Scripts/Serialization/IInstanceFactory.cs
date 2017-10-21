namespace Serialization
{
    public interface IInstanceFactory<T>
        where T : ISerializable
    {
        T Create(SerializationInfo serializationInfo);
    }
}
