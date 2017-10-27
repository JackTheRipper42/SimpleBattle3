using System;
using UnityEngine;

[Serializable]
public class Shield : ISerializable
{
    public float MaxHitPoints;
    public float HitPoints;
    [Range(0f, 1f)] public float Absorption = 1;

    public void Serialize(SerializationInfo serializationInfo)
    {
        serializationInfo.SetValue(SerializationNames.MaxHitPoints, MaxHitPoints);
        serializationInfo.SetValue(SerializationNames.HitPoints, HitPoints);
        serializationInfo.SetValue(SerializationNames.Absorption, Absorption);
    }

    public void Deserialize(SerializationInfo serializationInfo)
    {
        MaxHitPoints = serializationInfo.GetSingle(SerializationNames.MaxHitPoints);
        HitPoints = serializationInfo.GetSingle(SerializationNames.HitPoints);
        Absorption = serializationInfo.GetSingle(SerializationNames.Absorption);
    }

    protected class SerializationNames
    {
        public const string MaxHitPoints = "MaxHitPoints";
        public const string HitPoints = "HitPoints";
        public const string Absorption = "Absorption";
    }
}
