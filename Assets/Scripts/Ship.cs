using UnityEngine;

public class Ship : Entity
{
    public int MovementRange = 4;
    public int FireRange = 1;
    public GameObject TargetMarker;

    protected override void Start()
    {
        base.Start();
        DisableTargetMarker();
    }

    public new void Move(GridPosition position)
    {
        base.Move(position);
    }

    public new void Kill()
    {
        base.Kill();
    }

    public void EnableTargetMarker()
    {
        TargetMarker.SetActive(true);
    }

    public void DisableTargetMarker()
    {
        TargetMarker.SetActive(false);
    }
}