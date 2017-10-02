using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    protected GameManager GameManager { get; private set; }

    public GridPosition Position { get; private set; }

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
}