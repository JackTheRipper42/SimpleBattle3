using System;

[Serializable]
public class Structure : ISerializable
{
    public float MaxHitPoints;
    public float HitPoints;

    public void Serialize(SerializationInfo serializationInfo)
    {
        serializationInfo.SetValue(SerializationNames.MaxHitPoints, MaxHitPoints);
        serializationInfo.SetValue(SerializationNames.HitPoints, HitPoints);
    }

    public void Deserialize(SerializationInfo serializationInfo)
    {
        MaxHitPoints = serializationInfo.GetSingle(SerializationNames.MaxHitPoints);
        HitPoints = serializationInfo.GetSingle(SerializationNames.HitPoints);
    }

    protected class SerializationNames
    {
        public const string MaxHitPoints = "MaxHitPoints";
        public const string HitPoints = "HitPoints";
    }
}