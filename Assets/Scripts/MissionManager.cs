using Serialization;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MissionManager : MonoBehaviour, ISerializable
{
    private const string SaveFolder = "Saves";
    private const string SaveFile = "Game1";

    public LayerMask GridLayerMask;
    public float Speed = 30;
    public Side PlayerSide;
    public GameObject DestinationMarkerPrefab;
    public GameObject SelectionMarkerPrefab;
    public Button LoadButton;
    public GameObject MainCamera;
    public float ScrollSpeed = 1;

    private Astar _astar;
    private List<Entity> _entities;
    private Ship _selectedShip;
    private List<GameObject> _destinationMarkers;
    private bool _blockUI;
    private bool _endTurn;
    private IdiotAI _ai;
    private GameObject _selectionMarker;

    protected virtual void Start()
    {
        _astar = new Astar();
        _ai = new IdiotAI();
        _entities = new List<Entity>();
        _destinationMarkers = new List<GameObject>();
        _selectionMarker = Instantiate(SelectionMarkerPrefab, transform);
        _selectionMarker.SetActive(false);
        _selectedShip = null;
        _blockUI = false;
        _endTurn = false;
        LoadButton.interactable = File.Exists(Path.Combine(SaveFolder, SaveFile));

        if (GameManager.Instance.LoadedSaveGame != null)
        {
            GameManager.Instance.LoadedSaveGame.GetValue("Mission", new MissionManagerFactory());
        }
    }

    protected virtual void Update()
    {
        var inputScroll = new Vector3(
            Input.GetAxis("Horizontal"),
            0f,
            Input.GetAxis("Vertical"));

        MainCamera.transform.position += ScrollSpeed * Time.deltaTime * inputScroll;

        if (_endTurn)
        {
            _endTurn = false;
            _blockUI = true;
            StartCoroutine(FinishTurn());
        }

        if (Input.mousePresent && Input.GetMouseButtonUp(0) && !_blockUI)
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
        }
        if (Input.mousePresent && Input.GetMouseButtonUp(1) && _selectedShip != null && !_blockUI)
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
                            _blockUI = true;
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
                                _blockUI = true;
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
                _blockUI)
            {
                ship.UI.DisableTargetMarker();
            }
            else
            {
                var range = GridPosition.Distance(_selectedShip.Position, ship.Position);
                if (range <= _selectedShip.Weapon.Range)
                {
                    ship.UI.EnableTargetMarker();
                }
                else
                {
                    ship.UI.DisableTargetMarker();
                }
            }
        }

        if (_selectedShip != null && _selectedShip.CanMove && !_blockUI)
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
        if (_blockUI)
        {
            return;
        }

        _endTurn = true;
    }

    public void Save()
    {
        GameManager.Instance.Save(this);
    }

    public void Load()
    {
        GameManager.Instance.Load();
    }

    public void MainMenu()
    {
        GameManager.Instance.MainMenu();
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
        _selectionMarker.SetActive(false);
        yield return ship.Move(path, Speed);
        _selectionMarker.SetActive(true);
        _selectionMarker.transform.position = GridPosition.ToVector3(ship.Position);
        _blockUI = false;
    }

    private IEnumerator Attack(Ship ship, Ship target)
    {
        yield return ship.Attack(target);
        _blockUI = false;
    }

    private IEnumerator FinishTurn()
    {
        _blockUI = true;
        _selectedShip = null;
        _selectionMarker.SetActive(false);

        foreach (var ship in _entities.OfType<Ship>())
        {
            ship.UI.DisableCanFireMarker();
            ship.UI.DisableCanMoveMarker();
        }

        yield return _ai.CalculateTurn(_entities, _astar, Side.Redfore, Speed);

        foreach (var ship in _entities.OfType<Ship>())
        {
            ship.StartTurn();
        }

        _blockUI = false;
    }

    void ISerializable.Serialize(SerializationInfo serializationInfo)
    {
        serializationInfo.SetValue(MissionManagerSerializationNames.Entities, _entities.Count);
        for (var index = 0; index < _entities.Count; index++)
        {
            var entity = _entities[index];
            serializationInfo.SetValue($"{MissionManagerSerializationNames.EntityPrefix}{index}", entity);
        }
        serializationInfo.SetValue(MissionManagerSerializationNames.CameraPositionX, MainCamera.transform.position.x);
        serializationInfo.SetValue(MissionManagerSerializationNames.CameraPositionY, MainCamera.transform.position.y);
        serializationInfo.SetValue(MissionManagerSerializationNames.CameraPositionZ, MainCamera.transform.position.z);
    }

    void ISerializable.Deserialize(SerializationInfo serializationInfo)
    {
        foreach (var entity in _entities)
        {
            Destroy(entity.gameObject);
        }

        _entities.Clear();
        _selectionMarker.SetActive(false);
        _selectedShip = null;
        _blockUI = false;
        _endTurn = false;

        var factory = new EntityFactory();
        var count = serializationInfo.GetInt32(MissionManagerSerializationNames.Entities);
        for (var index = 0; index < count; index++)
        {
            serializationInfo.GetValue($"{MissionManagerSerializationNames.EntityPrefix}{index}", factory);
        }

        MainCamera.transform.position = new Vector3(
            serializationInfo.GetSingle(MissionManagerSerializationNames.CameraPositionX),
            serializationInfo.GetSingle(MissionManagerSerializationNames.CameraPositionY),
            serializationInfo.GetSingle(MissionManagerSerializationNames.CameraPositionZ));
    }
}
