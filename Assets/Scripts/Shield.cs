using Serialization;
using System;
using UnityEngine;

[Serializable]
public class Shield : ISerializable
{
    private const string MaxHitPointsName = "MaxHitPoints";
    private const string HitPointsName = "HitPoints";
    private const string AbsorptionName = "Absorption";

    public float MaxHitPoints;
    public float HitPoints;
    [Range(0f, 1f)] public float Absorption = 1;

    public void Serialize(SerializationInfo serializationInfo)
    {
        serializationInfo.SetValue(MaxHitPointsName, MaxHitPoints);
        serializationInfo.SetValue(HitPointsName, HitPoints);
        serializationInfo.SetValue(AbsorptionName, Absorption);
    }

    public void Deserialize(SerializationInfo serializationInfo)
    {
        MaxHitPoints = serializationInfo.GetSingle(MaxHitPointsName);
        HitPoints = serializationInfo.GetSingle(HitPointsName);
        Absorption = serializationInfo.GetSingle(AbsorptionName);
    }
}
