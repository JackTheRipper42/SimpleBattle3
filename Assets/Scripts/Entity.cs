using UnityEngine;

public class Entity : MonoBehaviour
{
    protected Grid Grid { get; private set; }

    public GridPosition Position { get; private set; }

    protected virtual void Start()
    {
        Grid = FindObjectOfType<Grid>();
        Grid.Register(this);
        Position = new GridPosition(transform.position);
        transform.position = Position.GetPosition();
    }
}