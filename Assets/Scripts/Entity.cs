﻿using UnityEngine;

public abstract class Entity : MonoBehaviour, ISerializable
{
    public string PrefabName;

    protected MissionManager MissionManager { get; private set; }

    public GridPosition Position { get; private set; }

    protected bool IsLoading { get; private set; }

    protected virtual void Awake()
    {        
    }

    protected virtual void Start()
    {
        Move(GridPosition.FromVector3(transform.position));
        MissionManager = FindObjectOfType<MissionManager>();
        MissionManager.Register(this);
    }

    protected void Move(GridPosition position)
    {
        Position = position;
        transform.position = GridPosition.ToVector3(Position);
    }

    protected void Kill()
    {
        MissionManager.Remove(this);
        Destroy(gameObject);
    }

    protected virtual void Serialize(SerializationInfo serializationInfo)
    {
        serializationInfo.SetValue(EntitySerializationNames.Prefab, PrefabName);
        serializationInfo.SetValue(EntitySerializationNames.PositionX, transform.position.x);
        serializationInfo.SetValue(EntitySerializationNames.PositionY, transform.position.y);
        serializationInfo.SetValue(EntitySerializationNames.PositionZ, transform.position.z);
    }

    protected virtual void Deserialize(SerializationInfo serializationInfo)
    {
        PrefabName = serializationInfo.GetString(EntitySerializationNames.Prefab);
        transform.position = new Vector3(
            serializationInfo.GetSingle(EntitySerializationNames.PositionX),
            serializationInfo.GetSingle(EntitySerializationNames.PositionY),
            serializationInfo.GetSingle(EntitySerializationNames.PositionZ));
    }

    public abstract bool IsObstacle(Side side);

    void ISerializable.Serialize(SerializationInfo serializationInfo)
    {
        Serialize(serializationInfo);
    }

    void ISerializable.Deserialize(SerializationInfo serializationInfo)
    {
        IsLoading = true;
        Deserialize(serializationInfo);
    }

    public static Entity Create(SerializationInfo serializationInfo)
    {
        var prefab = Resources.Load<GameObject>($"Prefabs/{serializationInfo.GetString(EntitySerializationNames.Prefab)}");
        var gameObject = Instantiate(prefab);
        return gameObject.GetComponent<Entity>();
    }

    protected class EntitySerializationNames
    {
        public const string Prefab = "Prefab";
        public const string PositionX = "Position.X";
        public const string PositionY = "Position.Y";
        public const string PositionZ = "Position.Z";
    }
}