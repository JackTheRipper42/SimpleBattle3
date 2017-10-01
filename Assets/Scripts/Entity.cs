using UnityEngine;

public class Entity : MonoBehaviour
{
    protected Grid Grid { get; private set; }

    public GridPosition Position { get; private set; }

    protected virtual void Start()
    {
        Grid = FindObjectOfType<Grid>();
        Grid.Register(this);
        Position = GridPosition.FromVector3(transform.position);
        transform.position = GridPosition.ToVector3(Position);
    }
}