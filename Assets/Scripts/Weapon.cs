using System;
using UnityEngine;

[Serializable]
public class Weapon : ISerializable
{
    private const string RangeName = "Range";
    private const string DamageName = "Damage";
    private const string AccuracyName = "Accuracy";

    public int Range;
    public float Damage;
    [Range(0f, 1f)] public float Accuracy = 1f;

    public void Serialize(SerializationInfo serializationInfo)
    {
        serializationInfo.SetValue(RangeName, Range);
        serializationInfo.SetValue(DamageName, Damage);
        serializationInfo.SetValue(AccuracyName, Accuracy);
    }

    public void Deserialize(SerializationInfo serializationInfo)
    {
        Range = serializationInfo.GetInt32(RangeName);
        Damage = serializationInfo.GetSingle(DamageName);
        Accuracy = serializationInfo.GetSingle(AccuracyName);
    }
}