using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public float Speed = 30;
    public Side PlayerSide;
    public GameObject DestinationMarkerPrefab;
    public GameObject SelectionMarkerPrefab;
    public GameObject MainCamera;

    private Astar _astar;
    private Ship _selectedShip;
    private List<GameObject> _destinationMarkers;
    private bool _endTurn;
    private IdiotAI _ai;
    private GameObject _selectionMarker;

    public bool BlockUI { get; private set; }

    public List<Entity> Entities { get; private set; }

    protected virtual void Start()
    {
        _astar = new Astar();
        _ai = new IdiotAI();
        Entities = new List<Entity>();
        _destinationMarkers = new List<GameObject>();
        _selectionMarker = Instantiate(SelectionMarkerPrefab, transform);

        ResetState();
    }

    protected virtual void Update()
    {
        if (_endTurn)
        {
            _endTurn = false;
            BlockUI = true;
            StartCoroutine(FinishTurn());
        }

        foreach (var ship in Entities.OfType<Ship>())
        {
            if (_selectedShip == null ||
                ship.Side == PlayerSide ||
                BlockUI)
            {
                ship.UI.EnableTargetMarker(false);
                ship.UI.EnableBoardMarker(false);
            }
            else
            {
                var range = GridPosition.Distance(_selectedShip.Position, ship.Position);
                ship.UI.EnableTargetMarker(_selectedShip.CanFire && range <= _selectedShip.Weapon.Range);
                ship.UI.EnableBoardMarker(_selectedShip.CanBoard && range == 1);
            }
        }

        if (_selectedShip != null && _selectedShip.CanMove && !BlockUI)
        {
            var obstacles = new HashSet<GridPosition>(Entities
                .Where(entity => entity.IsObstacle(PlayerSide))
                .Select(entity => entity.Position));
            var entities = new HashSet<GridPosition>(Entities.Select(entity => entity.Position));
            var destinations = _astar.GetDestinations(_selectedShip.Position, obstacles, entities, _selectedShip.MovementRange);

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

    public void ResetState()
    {
        foreach (var entity in Entities)
        {
            Destroy(entity.gameObject);
        }
        Entities.Clear();
        _selectionMarker.SetActive(false);
        _selectedShip = null;
        BlockUI = false;
        _endTurn = false;
    }

    public void Register(Entity entity)
    {
        Entities.Add(entity);
    }

    public void Remove(Entity entity)
    {
        Entities.Remove(entity);

        if (entity == _selectedShip)
        {
            _selectionMarker.SetActive(false);
            _selectedShip = null;
        }

        if (Entities.OfType<Ship>().All(ship => ship.Side != PlayerSide))
        {
            GameManager.Instance.GameOver();
        }
        if (Entities.OfType<Ship>().All(ship => ship.Side == PlayerSide))
        {
            GameManager.Instance.Success();
        }
    }

    public void EndTurn()
    {
        _endTurn = true;
    }

    public void MoveCamera(Vector3 movement)
    {
        MainCamera.transform.position += movement;
    }

    public void Select(GridPosition gridPosition)
    {
        var clickedEntity = Entities
            .OfType<Ship>()
            .FirstOrDefault(entity => entity.Position == gridPosition && entity.Side == PlayerSide);

        if (clickedEntity == null)
        {
            _selectionMarker.SetActive(false);
            _selectedShip = null;
        }
        else
        {
            _selectionMarker.SetActive(true);
            var newPosition = GridPosition.ToVector3(gridPosition);
            _selectionMarker.transform.position = newPosition;
            _selectedShip = clickedEntity;
        }
    }

    public void Interact(GridPosition gridPosition)
    {
        if (_selectedShip == null)
        {
            return;
        }

        var clickedEntity = Entities.FirstOrDefault(entity => entity.Position == gridPosition);

        if (clickedEntity == null)
        {
            if (_selectedShip.CanMove)
            {
                var obstacles = Entities
                    .Where(entity => entity.IsObstacle(PlayerSide))
                    .Select(entity => entity.Position)
                    .ToList();
                var path = _astar.Calculate(_selectedShip.Position, gridPosition, obstacles);

                if (path != null && path.Count <= _selectedShip.MovementRange + 1)
                {
                    BlockUI = true;
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
                    var range = GridPosition.Distance(_selectedShip.Position, target.Position);
                    if (range <= _selectedShip.Weapon.Range)
                    {
                        BlockUI = true;
                        StartCoroutine(Attack(_selectedShip, target));
                    }
                }
            }
        }
    }

    private IEnumerator Move(Ship ship, IList<GridPosition> path)
    {
        _selectionMarker.SetActive(false);
        yield return ship.Move(path, Speed);
        _selectionMarker.SetActive(true);
        _selectionMarker.transform.position = GridPosition.ToVector3(ship.Position);
        BlockUI = false;
    }

    private IEnumerator Attack(Ship ship, Ship target)
    {
        yield return ship.Attack(target);
        BlockUI = false;
    }

    private IEnumerator FinishTurn()
    {
        BlockUI = true;
        _selectedShip = null;
        _selectionMarker.SetActive(false);

        foreach (var ship in Entities.OfType<Ship>())
        {
            ship.UI.EnableCanFireMarker(false);
            ship.UI.EnableCanMoveMarker(false);
            ship.UI.EnableCanBoardMarker(false);
        }

        yield return _ai.CalculateTurn(Entities, _astar, Side.Redfore, Speed);

        foreach (var ship in Entities.OfType<Ship>())
        {
            ship.StartTurn();
        }

        BlockUI = false;
    }
}
