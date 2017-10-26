using System;

[Serializable]
public class Structure : ISerializable
{
    private const string MaxHitPointsName = "MaxHitPoints";
    private const string HitPointsName = "HitPoints";

    public float MaxHitPoints;
    public float HitPoints;

    public void Serialize(SerializationInfo serializationInfo)
    {
        serializationInfo.SetValue(MaxHitPointsName, MaxHitPoints);
        serializationInfo.SetValue(HitPointsName, HitPoints);
    }

    public void Deserialize(SerializationInfo serializationInfo)
    {
        MaxHitPoints = serializationInfo.GetSingle(MaxHitPointsName);
        HitPoints = serializationInfo.GetSingle(HitPointsName);
    }
}