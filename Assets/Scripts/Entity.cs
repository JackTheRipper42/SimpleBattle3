using UnityEngine;

public abstract class Entity : MonoBehaviour, ISerializable
{
#pragma warning disable 649
    // ReSharper disable ConvertToConstant.Local
    // ReSharper disable FieldCanBeMadeReadOnly.Local
    [SerializeField] private string _prefabName;
    // ReSharper restore FieldCanBeMadeReadOnly.Local
    // ReSharper restore ConvertToConstant.Local
#pragma warning restore 649

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
        serializationInfo.SetValue("Prefab", _prefabName);
        serializationInfo.SetValue("Position", transform.position);
    }

    public virtual void Deserialize(SerializationInfo serializationInfo)
    {
        _prefabName = serializationInfo.GetString("Prefab");
        transform.position = serializationInfo.GetVector3("Position");
    }

    public static GameObject Create(SerializationInfo serializationInfo)
    {
        var prefab = Resources.Load<GameObject>($"Prefabs/{serializationInfo.GetString("Prefab")}");
        var gameObject = Instantiate(prefab);
        var entity = gameObject.GetComponent<Entity>();
        entity.Deserialize(serializationInfo);
        return gameObject;
    }
}