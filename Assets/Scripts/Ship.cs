using UnityEngine;

public class Ship : Entity
{
    public int MovementRange = 4;
    public int FireRange = 1;
    public Side Side;
    public GameObject TargetMarker;
    public AudioSource FireAudio;
    public AudioSource ThrusterAudio;

    public bool CanMove { get; private set; }

    public bool CanFire { get; private set; }

    protected override void Start()
    {
        base.Start();
        DisableTargetMarker();
        CanMove = true;
        CanFire = true;
    }

    public new void Move(GridPosition position)
    {
        base.Move(position);
        CanMove = false;
    }

    public void Attack(Ship target)
    {
        target.Kill();
        CanFire = false;
    }

    public void EnableTargetMarker()
    {
        TargetMarker.SetActive(true);
    }

    public void DisableTargetMarker()
    {
        TargetMarker.SetActive(false);
    }

    public void StartTurn()
    {
        CanFire = true;
        CanMove = true;
    }

    public override bool IsObstacle(Side side)
    {
        return side != Side;
    }
}