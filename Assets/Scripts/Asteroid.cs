using Serialization;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class Asteroid : Entity
{
    public float Speed = 1;

    private bool _isLoading;

    protected override void Start()
    {
        base.Start();

        if (!_isLoading)
        {
            GetComponent<Rigidbody>().angularVelocity = Random.insideUnitSphere * Speed;
            _isLoading = false;
        }
    }

    public override bool IsObstacle(Side side)
    {
        return true;
    }

    public override void Serialize(SerializationInfo serializationInfo)
    {
        base.Serialize(serializationInfo);

        var angularVelocity = GetComponent<Rigidbody>().angularVelocity;
        serializationInfo.SetValue(AsteriodSerializationNames.AngularVelocityX, angularVelocity.x);
        serializationInfo.SetValue(AsteriodSerializationNames.AngularVelocityY, angularVelocity.y);
        serializationInfo.SetValue(AsteriodSerializationNames.AngularVelocityZ, angularVelocity.z);
    }

    public override void Deserialize(SerializationInfo serializationInfo)
    {
        _isLoading = true;

        base.Deserialize(serializationInfo);

        GetComponent<Rigidbody>().angularVelocity = new Vector3(
            serializationInfo.GetSingle(AsteriodSerializationNames.AngularVelocityX),
            serializationInfo.GetSingle(AsteriodSerializationNames.AngularVelocityY),
            serializationInfo.GetSingle(AsteriodSerializationNames.AngularVelocityZ));
    }
}