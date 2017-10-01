using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public LayerMask GridLayerMask;
    public Transform Marker;

    private List<Entity> _entities;

    protected virtual void Start()
    {
        _entities = new List<Entity>();
    }

    protected virtual void Update()
    {
        var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, float.MaxValue, GridLayerMask.value))
        {
            var gridPosition = GridPosition.FromVector3(hit.point);
            var newPosition = GridPosition.ToVector3(gridPosition);
            Marker.position = newPosition;
        }

        var astar = new Astar();
        var start = _entities.First();
        var end = _entities.Last();
        var obstacles = _entities.Except(new[] {start, end}).Select(entity => entity.Position).ToList();

        var path = astar.Calculate(start.Position, end.Position, obstacles);

        for (var i = 1; i < path.Count; i++)
        {
            Debug.DrawLine(GridPosition.ToVector3(path[i -1]), GridPosition.ToVector3(path[i]), Color.green);
        }
    }

    public void Register(Entity entity)
    {
        _entities.Add(entity);
    }

    public void Remove(Entity entity)
    {
        _entities.Remove(entity);
    }
}
