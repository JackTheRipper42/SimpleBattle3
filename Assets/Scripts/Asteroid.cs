using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Asteroid : Entity
{
    public float Speed = 1;

    protected override void Start()
    {
        base.Start();
        GetComponent<Rigidbody>().angularVelocity = Random.insideUnitSphere * Speed;
    }
}