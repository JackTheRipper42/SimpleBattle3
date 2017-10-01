using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    protected Grid Grid { get; private set; }

    public GridPosition Position { get; private set; }

    protected virtual void Start()
    {
        Move(GridPosition.FromVector3(transform.position));
        Grid = FindObjectOfType<Grid>();
        Grid.Register(this);
    }

    protected void Move(GridPosition position)
    {
        Position = position;
        transform.position = GridPosition.ToVector3(Position);
    }

    protected void Kill()
    {
        Grid.Remove(this);
        Destroy(gameObject);
    }
}