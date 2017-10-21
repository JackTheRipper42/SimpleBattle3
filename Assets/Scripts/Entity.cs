using Serialization;
using UnityEngine;

public abstract class Entity : MonoBehaviour, ISerializable
{
    public string PrefabName;

    protected GameManager GameManager { get; private set; }

    public GridPosition Position { get; private set; }

    protected virtual void Awake()
    {
    }

    protected virtual void Start()
    {
        Move(GridPosition.FromVector3(transform.position));
        GameManager = FindObjectOfType<GameManager>();
        GameManager.Register(this);
    }

    protected void Move(GridPosition position)
    {
        Position = position;
        transform.position = GridPosition.ToVector3(Position);
    }

    protected void Kill()
    {
        GameManager.Remove(this);
        Destroy(gameObject);
    }

    public abstract bool IsObstacle(Side side);

    public virtual void Serialize(SerializationInfo serializationInfo)
    {
        serializationInfo.SetValue(EntitySerializationNames.Prefab, PrefabName);
        serializationInfo.SetValue(EntitySerializationNames.PositionX, transform.position.x);
        serializationInfo.SetValue(EntitySerializationNames.PositionY, transform.position.y);
        serializationInfo.SetValue(EntitySerializationNames.PositionZ, transform.position.z);
    }

    public virtual void Deserialize(SerializationInfo serializationInfo)
    {
        PrefabName = serializationInfo.GetString(EntitySerializationNames.Prefab);
        transform.position = new Vector3(
            serializationInfo.GetSingle(EntitySerializationNames.PositionX),
            serializationInfo.GetSingle(EntitySerializationNames.PositionY),
            serializationInfo.GetSingle(EntitySerializationNames.PositionZ));
    }
}