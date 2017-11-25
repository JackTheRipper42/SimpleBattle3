using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class MissionManager : MonoBehaviour
{
    public float Speed = 30;
    public float SalvoFlightTime = 0.35f;
    public Side PlayerSide;
    public GameObject DestinationMarkerPrefab;
    public GameObject SelectionMarkerPrefab;
    public GameObject MainCamera;

    private Astar _astar;
    private LivingEntity _selectedEntity;
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

        foreach (var livingEntity in Entities.OfType<LivingEntity>())
        {
            if (_selectedEntity == null ||
                livingEntity.Side == PlayerSide ||
                BlockUI)
            {
                livingEntity.UI.EnableTargetMarker(false);
                livingEntity.UI.EnableBoardMarker(false);
            }
            else
            {
                var range = GridPosition.Distance(_selectedEntity.Position, livingEntity.Position);
                livingEntity.UI.EnableTargetMarker(_selectedEntity.CanAttack && range <= _selectedEntity.Weapon.Range);
                livingEntity.UI.EnableBoardMarker(_selectedEntity.CanBoard && range == 1);
            }
        }

        if (_selectedEntity != null && _selectedEntity.CanMove && !BlockUI)
        {
            var obstacles = new HashSet<GridPosition>(Entities
                .Where(entity => entity.IsObstacle(PlayerSide))
                .Select(entity => entity.Position));
            var entities = new HashSet<GridPosition>(Entities.Select(entity => entity.Position));
            var destinations = _astar.GetDestinations(_selectedEntity.Position, obstacles, entities, _selectedEntity.MovementRange);

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
        _selectedEntity = null;
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

        if (entity == _selectedEntity)
        {
            _selectionMarker.SetActive(false);
            _selectedEntity = null;
        }

        if (Entities.OfType<LivingEntity>().All(livingEntity => livingEntity.Side != PlayerSide))
        {
            GameManager.Instance.GameOver();
        }
        if (Entities.OfType<LivingEntity>().All(livingEntity => livingEntity.Side == PlayerSide))
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
            .OfType<LivingEntity>()
            .FirstOrDefault(entity => entity.Position == gridPosition && entity.Side == PlayerSide);

        if (clickedEntity == null)
        {
            _selectionMarker.SetActive(false);
            _selectedEntity = null;
        }
        else
        {
            _selectionMarker.SetActive(true);
            var newPosition = GridPosition.ToVector3(gridPosition);
            _selectionMarker.transform.position = newPosition;
            _selectedEntity = clickedEntity;
        }
    }

    public void Interact(GridPosition gridPosition)
    {
        if (_selectedEntity == null)
        {
            return;
        }

        var clickedEntity = Entities.FirstOrDefault(entity => entity.Position == gridPosition);

        if (clickedEntity == null)
        {
            if (_selectedEntity.CanMove)
            {
                var obstacles = Entities
                    .Where(entity => entity.IsObstacle(PlayerSide))
                    .Select(entity => entity.Position)
                    .ToList();
                var path = _astar.Calculate(_selectedEntity.Position, gridPosition, obstacles);

                if (path != null && path.Count <= _selectedEntity.MovementRange + 1)
                {
                    BlockUI = true;
                    StartCoroutine(Move(_selectedEntity, path));
                }
            }
        }
        else
        {
            if (_selectedEntity.CanAttack)
            {
                var target = clickedEntity as LivingEntity;
                if (target != null && target.Side != PlayerSide)
                {
                    var range = GridPosition.Distance(_selectedEntity.Position, target.Position);
                    if (range <= _selectedEntity.Weapon.Range)
                    {
                        BlockUI = true;
                        StartCoroutine(Attack(_selectedEntity, target));
                    }
                }
            }
        }
    }

    private IEnumerator Move(LivingEntity livingEntity, IList<GridPosition> path)
    {
        _selectionMarker.SetActive(false);
        yield return livingEntity.Move(path, Speed);
        _selectionMarker.SetActive(true);
        _selectionMarker.transform.position = GridPosition.ToVector3(livingEntity.Position);
        BlockUI = false;
    }

    private IEnumerator Attack(LivingEntity livingEntity, LivingEntity target)
    {
        yield return livingEntity.Attack(target, SalvoFlightTime);
        BlockUI = false;
    }

    private IEnumerator FinishTurn()
    {
        BlockUI = true;
        _selectedEntity = null;
        _selectionMarker.SetActive(false);

        foreach (var livingEntity in Entities.OfType<LivingEntity>())
        {
            livingEntity.UI.EnableCanFireMarker(false);
            livingEntity.UI.EnableCanMoveMarker(false);
            livingEntity.UI.EnableCanBoardMarker(false);
        }

        yield return _ai.CalculateTurn(Entities, _astar, Side.Redfore, Speed, SalvoFlightTime);

        foreach (var livingEntity in Entities.OfType<LivingEntity>())
        {
            livingEntity.StartTurn();
        }

        BlockUI = false;
    }
}
