using UnityEngine;

public class Ship : Entity
{
    public int MovementRange = 4;
    public int FireRange = 1;
    public Side Side;
    public AudioSource FireAudio;
    public AudioSource ThrusterAudio;
    public GameObject UIPrefab;

    private ShipUI _shipUI;

    public bool CanMove { get; private set; }

    public bool CanFire { get; private set; }

    protected virtual void Awake()
    {
        var uiObject = Instantiate(UIPrefab, transform);
        _shipUI = uiObject.GetComponent<ShipUI>();
    }

    protected override void Start()
    {
        base.Start();
        StartTurn();
    }

    public new void Move(GridPosition position)
    {
        base.Move(position);
        CanMove = false;
        _shipUI.DisableCanMoveMarker();
    }

    public void Attack(Ship target)
    {
        target.Kill();
        CanFire = false;
        _shipUI.DisableCanFireMarker();
    }

    public void EnableTargetMarker()
    {
        _shipUI.EnableTargetMarker();
    }

    public void DisableTargetMarker()
    {
        _shipUI.DisableTargetMarker();
    }

    public void StartTurn()
    {
        CanFire = true;
        CanMove = true;
        if (GameManager.PlayerSide == Side)
        {
            _shipUI.EnableCanFireMarker();
            _shipUI.EnableCanMoveMarker();
        }
    }

    public override bool IsObstacle(Side side)
    {
        return side != Side;
    }
}