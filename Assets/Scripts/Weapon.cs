using System;
using UnityEngine;

[Serializable]
public class Weapon : ISerializable
{
    public int Range = 1;
    public float Damage;
    [Range(0f, 1f)] public float Accuracy = 1f;
    public int SalveRounds = 1;

    public void Serialize(SerializationInfo serializationInfo)
    {
        serializationInfo.SetValue(SerializationNames.Range, Range);
        serializationInfo.SetValue(SerializationNames.Damage, Damage);
        serializationInfo.SetValue(SerializationNames.Accuracy, Accuracy);
        serializationInfo.SetValue(SerializationNames.SalveRounds, SalveRounds);
    }

    public void Deserialize(SerializationInfo serializationInfo)
    {
        Range = serializationInfo.GetInt32(SerializationNames.Range);
        Damage = serializationInfo.GetSingle(SerializationNames.Damage);
        Accuracy = serializationInfo.GetSingle(SerializationNames.Accuracy);
        SalveRounds = serializationInfo.GetInt32(SerializationNames.SalveRounds);
    }

    protected class SerializationNames
    {
        public const string Range = "Range";
        public const string Damage = "Damage";
        public const string Accuracy = "Accuracy";
        public const string SalveRounds = "SalveRounds";
    }
}