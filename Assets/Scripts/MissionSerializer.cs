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
        serializationInfo.SetValue(MissionSerializationNames.Entities, _missionManager.Entities.Count);
        for (var index = 0; index < _missionManager.Entities.Count; index++)
        {
            var entity = _missionManager.Entities[index];
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
        _missionManager.ResetState();

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

    protected class MissionSerializationNames
    {
        public const string Entities = "Entities";
        public const string EntityPrefix = "Entity";
        public const string CameraPositionX = "Camera.Position.X";
        public const string CameraPositionY = "Camera.Position.Y";
        public const string CameraPositionZ = "Camera.Position.Z";
    }
}