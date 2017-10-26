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

    protected override void Serialize(SerializationInfo serializationInfo)
    {
        base.Serialize(serializationInfo);

        var angularVelocity = GetComponent<Rigidbody>().angularVelocity;
        serializationInfo.SetValue(AsteroidSerializationNames.AngularVelocityX, angularVelocity.x);
        serializationInfo.SetValue(AsteroidSerializationNames.AngularVelocityY, angularVelocity.y);
        serializationInfo.SetValue(AsteroidSerializationNames.AngularVelocityZ, angularVelocity.z);
    }

    protected override void Deserialize(SerializationInfo serializationInfo)
    {
        _isLoading = true;

        base.Deserialize(serializationInfo);

        GetComponent<Rigidbody>().angularVelocity = new Vector3(
            serializationInfo.GetSingle(AsteroidSerializationNames.AngularVelocityX),
            serializationInfo.GetSingle(AsteroidSerializationNames.AngularVelocityY),
            serializationInfo.GetSingle(AsteroidSerializationNames.AngularVelocityZ));
    }

    protected class AsteroidSerializationNames : EntitySerializationNames
    {
        public const string AngularVelocityX = "AngularVelocity.X";
        public const string AngularVelocityY = "AngularVelocity.Y";
        public const string AngularVelocityZ = "AngularVelocity.Z";
    }
}