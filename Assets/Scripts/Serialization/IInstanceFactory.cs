using JetBrains.Annotations;

namespace Serialization
{
    public interface IInstanceFactory<T>
        where T : ISerializable
    {
        [NotNull]T Create(SerializationInfo serializationInfo);
    }
}
