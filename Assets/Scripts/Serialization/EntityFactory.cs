using UnityEngine;
using Object = UnityEngine.Object;

namespace Serialization
{
    public class EntityFactory : IInstanceFactory<Entity>
    {
        public Entity Create(SerializationInfo serializationInfo)
        {
            var prefab = Resources.Load<GameObject>($"Prefabs/{serializationInfo.GetString(EntitySerializationNames.Prefab)}");
            var gameObject = Object.Instantiate(prefab);
            return gameObject.GetComponent<Entity>();
        }
    }
}
