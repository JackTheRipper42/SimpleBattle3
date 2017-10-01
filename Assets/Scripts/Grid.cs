﻿using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Grid : MonoBehaviour
{
    public LayerMask GridLayerMask;
    public GameObject SelectionMarker;
    public GameObject DestinationMarkerPrefab;

    private Astar _astar;
    private List<Entity> _entities;
    private Ship _selectedShip;
    private List<GameObject> _destinationMarkers;

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
                        SelectionMarker.transform.position = GridPosition.ToVector3(gridPosition);
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

        foreach (var ship in _entities.OfType<Ship>())
        {
            if (ship == _selectedShip || _selectedShip == null)
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

        if (_selectedShip != null)
        {
            var obstacles = _entities.Select(entity => entity.Position).ToList();
            var destinations = GetDestinations(_selectedShip.Position, obstacles, _selectedShip.MovementRange);

            var markers = new Queue<GameObject>(_destinationMarkers);
            _destinationMarkers.Clear();

            foreach (var destination in destinations)
            {
                if (destination == _selectedShip.Position)
                {
                    continue;
                }

                var marker = markers.Any() ? markers.Dequeue() : Instantiate(DestinationMarkerPrefab);
                marker.transform.position = GridPosition.ToVector3(destination);
                marker.transform.SetParent(transform);
                _destinationMarkers.Add(marker);
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

    private static ICollection<GridPosition> GetDestinations(
        GridPosition position, 
        ICollection<GridPosition> obstacles,
        int range)
    {
        var destinations = new List<GridPosition>();

        if (range < 0)
        {
            return destinations;
        }

        destinations.Add(position);

        foreach (var neighbor in position.Neighbors)
        {
            if (!obstacles.Contains(neighbor))
            {
                destinations.AddRange(GetDestinations(neighbor, obstacles, range - 1));
            }
        }

        return destinations.Distinct().ToList();
    }
}
