using UnityEngine;

namespace Serialization
{
    public class MissionManagerFactory : IInstanceFactory<MissionManager>
    {
        public MissionManager Create(SerializationInfo serializationInfo)
        {
            return Object.FindObjectOfType<MissionManager>();
        }
    }
}
