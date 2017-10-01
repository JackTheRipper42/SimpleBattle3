using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public LayerMask GridLayerMask;
    public GameObject Marker;

    private Astar _astar;
    private List<Entity> _entities;
    private Ship _selectedShip;
    
    protected virtual void Start()
    {
        _astar = new Astar();
        _entities = new List<Entity>();
        Marker.SetActive(false);
        _selectedShip = null;
    }

    protected virtual void Update()
    {
        if (Input.mousePresent && Input.GetMouseButtonUp(0))
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, float.MaxValue, GridLayerMask.value))
            {
                var gridPosition = GridPosition.FromVector3(hit.point);

                var clickedEntity =
                    _entities.OfType<Ship>().FirstOrDefault(entity => entity.Position == gridPosition);

                if (clickedEntity == null)
                {
                    Marker.SetActive(false);
                    _selectedShip = null;
                }
                else
                {
                    Marker.SetActive(true);
                    var newPosition = GridPosition.ToVector3(gridPosition);
                    Marker.transform.position = newPosition;
                    _selectedShip = clickedEntity;
                }
            }
        }
        if (Input.mousePresent && Input.GetMouseButtonUp(1) && _selectedShip != null)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, float.MaxValue, GridLayerMask.value))
            {
                var gridPosition = GridPosition.FromVector3(hit.point);

                var clickedEntity =
                    _entities.FirstOrDefault(entity => entity.Position == gridPosition);

                if (clickedEntity == null)
                {
                    var obstacles = _entities
                        .Where(entity => entity != _selectedShip)
                        .Select(entity => entity.Position)
                        .ToList();
                    var path = _astar.Calculate(_selectedShip.Position, gridPosition, obstacles);

                    if (path != null && path.Count <= _selectedShip.MovementRange + 1)
                    {
                        _selectedShip.Move(gridPosition);
                        Marker.transform.position = GridPosition.ToVector3(gridPosition);
                    }
                }
                else
                {
                    var target = clickedEntity as Ship;
                    if (target != null && target != _selectedShip)
                    {
                        var range = _selectedShip.Position.Distance(target.Position);
                        if (range <= _selectedShip.FireRange)
                        {
                            target.Kill();
                        }
                    }
                }
            }
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
