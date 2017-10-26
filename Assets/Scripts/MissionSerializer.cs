using Serialization;
using UnityEngine;

public class MissionSerializer : MonoBehaviour, ISerializable
{
    private MissionManager _missionManager;

    protected virtual void Start()
    {
        _missionManager = FindObjectOfType<MissionManager>();
    }

    void ISerializable.Serialize(SerializationInfo serializationInfo)
    {
        var entities = _missionManager.GetEntities();
        serializationInfo.SetValue(MissionSerializationNames.Entities, entities.Count);
        for (var index = 0; index < entities.Count; index++)
        {
            var entity = entities[index];
            serializationInfo.SetValue($"{MissionSerializationNames.EntityPrefix}{index}", entity);
        }
        serializationInfo.SetValue(
            MissionSerializationNames.CameraPositionX,
            _missionManager.MainCamera.transform.position.x);
        serializationInfo.SetValue(
            MissionSerializationNames.CameraPositionY,
            _missionManager.MainCamera.transform.position.y);
        serializationInfo.SetValue(
            MissionSerializationNames.CameraPositionZ,
            _missionManager.MainCamera.transform.position.z);
    }

    void ISerializable.Deserialize(SerializationInfo serializationInfo)
    {
        _missionManager.Reset();

        var count = serializationInfo.GetInt32(MissionSerializationNames.Entities);
        for (var index = 0; index < count; index++)
        {
            serializationInfo.GetValue($"{MissionSerializationNames.EntityPrefix}{index}", Entity.Create);
        }

        _missionManager.MainCamera.transform.position = new Vector3(
            serializationInfo.GetSingle(MissionSerializationNames.CameraPositionX),
            serializationInfo.GetSingle(MissionSerializationNames.CameraPositionY),
            serializationInfo.GetSingle(MissionSerializationNames.CameraPositionZ));
    }
}