using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public LayerMask GridLayerMask;
    public GameObject SelectionMarker;
    public GameObject DestinationMarkerPrefab;
    public float Speed = 30;
    public Side PlayerSide;

    private Astar _astar;
    private List<Entity> _entities;
    private Ship _selectedShip;
    private List<GameObject> _destinationMarkers;
    private bool _animationRunning;

    protected virtual void Start()
    {
        _astar = new Astar();
        _entities = new List<Entity>();
        _destinationMarkers = new List<GameObject>();
        SelectionMarker.SetActive(false);
        _selectedShip = null;
    }

    protected virtual void Update()
    {
        if (Input.mousePresent && Input.GetMouseButtonUp(0) && !_animationRunning)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, float.MaxValue, GridLayerMask.value))
            {
                var gridPosition = GridPosition.FromVector3(hit.point);

                var clickedEntity = _entities
                    .OfType<Ship>()
                    .FirstOrDefault(entity => entity.Position == gridPosition && entity.Side == PlayerSide);

                if (clickedEntity == null)
                {
                    SelectionMarker.SetActive(false);
                    _selectedShip = null;
                }
                else
                {
                    SelectionMarker.SetActive(true);
                    var newPosition = GridPosition.ToVector3(gridPosition);
                    SelectionMarker.transform.position = newPosition;
                    _selectedShip = clickedEntity;
                }
            }
        }
        if (Input.mousePresent && Input.GetMouseButtonUp(1) && _selectedShip != null && !_animationRunning)
        {
            var ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit, float.MaxValue, GridLayerMask.value))
            {
                var gridPosition = GridPosition.FromVector3(hit.point);

                var clickedEntity = _entities.FirstOrDefault(entity => entity.Position == gridPosition);

                if (clickedEntity == null)
                {
                    if (_selectedShip.CanMove)
                    {
                        var obstacles = _entities
                            .Where(entity => entity.IsObstacle(PlayerSide))
                            .Select(entity => entity.Position)
                            .ToList();
                        var path = _astar.Calculate(_selectedShip.Position, gridPosition, obstacles);

                        if (path != null && path.Count <= _selectedShip.MovementRange + 1)
                        {
                            _animationRunning = true;
                            StartCoroutine(Move(_selectedShip, path));
                        }
                    }
                }
                else
                {
                    if (_selectedShip.CanFire)
                    {
                        var target = clickedEntity as Ship;
                        if (target != null && target.Side != PlayerSide)
                        {
                            var range = _selectedShip.Position.Distance(target.Position);
                            if (range <= _selectedShip.FireRange)
                            {
                                _animationRunning = true;
                                StartCoroutine(Attack(_selectedShip, target));
                            }
                        }
                    }
                }
            }
        }

        foreach (var ship in _entities.OfType<Ship>())
        {
            if (_selectedShip == null || 
                ship.Side == PlayerSide ||
                !_selectedShip.CanFire || 
                _animationRunning)
            {
                ship.DisableTargetMarker();
            }
            else
            {
                var range = _selectedShip.Position.Distance(ship.Position);
                if (range <= _selectedShip.FireRange)
                {
                    ship.EnableTargetMarker();
                }
                else
                {
                    ship.DisableTargetMarker();
                }
            }
        }

        if (_selectedShip != null && _selectedShip.CanMove && !_animationRunning)
        {
            var obstacles = new HashSet<GridPosition>(_entities
                .Where(entity => entity.IsObstacle(PlayerSide))
                .Select(entity => entity.Position));
            var entities = new HashSet<GridPosition>(_entities.Select(entity => entity.Position));
            var destinations = GetDestinations(_selectedShip.Position, obstacles, entities, _selectedShip.MovementRange);

            var markers = new Queue<GameObject>(_destinationMarkers);
            _destinationMarkers.Clear();

            foreach (var destination in destinations)
            {
                var marker = markers.Any() ? markers.Dequeue() : Instantiate(DestinationMarkerPrefab);
                if (marker != null)
                {
                    marker.transform.position = GridPosition.ToVector3(destination);
                    marker.transform.SetParent(transform);
                    _destinationMarkers.Add(marker);
                }
            }
            while (markers.Any())
            {
                Destroy(markers.Dequeue());
            }
        }
        else
        {
            foreach (var destinationMarker in _destinationMarkers)
            {
                Destroy(destinationMarker);
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

    public void EndTurn()
    {
        foreach (var ship in _entities.OfType<Ship>())
        {
            ship.StartTurn();
        }
    }

    private static IEnumerable<GridPosition> GetDestinations(
        GridPosition position,
        HashSet<GridPosition> obstacles,
        HashSet<GridPosition> entities,
        int range)
    {
        var destinations = new List<GridPosition>();

        if (range < 0)
        {
            return destinations;
        }

        if (!entities.Contains(position))
        {
            destinations.Add(position);
        }

        foreach (var neighbor in position.Neighbors)
        {
            if (!obstacles.Contains(neighbor))
            {
                destinations.AddRange(GetDestinations(neighbor, obstacles, entities, range - 1));
            }
        }

        return destinations.Distinct().ToList();
    }

    private IEnumerator Move(Ship ship, IList<GridPosition> path)
    {
        SelectionMarker.SetActive(false);
        ship.ThrusterAudio.Play();
        for (var index = 1; index < path.Count; index++)
        {
            var end = GridPosition.ToVector3(path[index]);

            while ((ship.transform.position - end).sqrMagnitude > 0.01)
            {
                var step = Speed * Time.deltaTime;
                ship.transform.position = Vector3.MoveTowards(ship.transform.position, end, step);
                yield return new WaitForEndOfFrame();
            }
            ship.Move(path[index]);
        }
        ship.ThrusterAudio.Stop();
        SelectionMarker.SetActive(true);
        SelectionMarker.transform.position = GridPosition.ToVector3(ship.Position);
        _animationRunning = false;
    }

    private IEnumerator Attack(Ship ship, Ship target)
    {
        yield return ship.Attack(target);
        _animationRunning = false;
    }
}
