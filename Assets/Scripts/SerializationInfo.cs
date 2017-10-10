﻿using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SerializationInfo
{
    private readonly Dictionary<string, object> _data;

    public SerializationInfo()
    {
        _data = new Dictionary<string, object>();
    }

    public SerializationInfo(BinaryReader reader)
        :this(reader, true)
    {        
    }

    private SerializationInfo(BinaryReader reader, bool readType)
    {
        _data = new Dictionary<string, object>();
        if (readType)
        {
            var baseType = (Type) reader.ReadByte();
            if (baseType != Type.SerializationInfo)
            {
                throw new InvalidOperationException($"The base type '{baseType}' is invalid.");
            }
        }
        var count = reader.ReadInt32();
        for (var i = 0; i < count; i++)
        {
            var name = reader.ReadString();
            var type = (Type)reader.ReadByte();
            object value;
            switch (type)
            {
                case Type.SerializationInfo:
                    value = new SerializationInfo(reader, false);
                    break;
                case Type.String:
                    value = reader.ReadString();
                    break;
                case Type.Float:
                    value = reader.ReadSingle();
                    break;
                case Type.Int32:
                    value = reader.ReadInt32();
                    break;
                case Type.Boolean:
                    value = reader.ReadBoolean();
                    break;
                default:
                    throw new NotSupportedException($"The type '{type}' is not supported.");
            }
            _data.Add(name, value);
        }
    }

    public void SetValue(string name, float value)
    {
        _data[name] = value;
    }

    public void SetValue(string name, int value)
    {
        _data[name] = value;
    }

    public void SetValue(string name, string value)
    {
        _data[name] = value;
    }

    public void SetValue(string name, GridPosition value)
    {
        var serializationInfo = new SerializationInfo();
        serializationInfo.SetValue("U", value.U);
        serializationInfo.SetValue("V", value.V);
        _data[name] = serializationInfo;
    }

    public void SetValue(string name, Vector3 value)
    {
        var serializationInfo = new SerializationInfo();
        serializationInfo.SetValue("x", value.x);
        serializationInfo.SetValue("y", value.y);
        serializationInfo.SetValue("z", value.z);
        _data[name] = serializationInfo;
    }

    public void SetValue(string name, bool value)
    {
        _data[name] = value;
    }

    public void SetValue(string name, SerializationInfo value)
    {
        _data[name] = value;
    }

    public float GetFloat(string name)
    {
        return (float) _data[name];
    }

    public int GetInt32(string name)
    {
        return (int) _data[name];
    }

    public string GetString(string name)
    {
        return (string) _data[name];
    }

    public GridPosition GetGridPosition(string name)
    {
        var serializationInfo = (SerializationInfo)_data[name];
        var u = serializationInfo.GetInt32("U");
        var v = serializationInfo.GetInt32("V");
        return new GridPosition(u,v);
    }

    public Vector3 GetVector3(string name)
    {
        var serializationInfo = (SerializationInfo)_data[name];
        var x = serializationInfo.GetFloat("x");
        var y = serializationInfo.GetFloat("y");
        var z = serializationInfo.GetFloat("z");
        return new Vector3(x, y, z);
    }

    public bool GetBoolean(string name)
    {
        return (bool) _data[name];
    }

    public SerializationInfo GetValue(string name)
    {
        return (SerializationInfo) _data[name];
    }

    public void Write(BinaryWriter writer)
    {
        writer.Write((byte) Type.SerializationInfo);
        writer.Write(_data.Count);
        foreach (var entry in _data)
        {
            writer.Write(entry.Key);
            var serializationInfo = entry.Value as SerializationInfo;
            if (serializationInfo != null)
            {
                serializationInfo.Write(writer);
            }
            else if (entry.Value is string)
            {
                writer.Write((byte)Type.String);
                writer.Write((string) entry.Value);
            }
            else if (entry.Value is int)
            {
                writer.Write((byte)Type.Int32);
                writer.Write((int) entry.Value);
            }
            else if (entry.Value is float)
            {
                writer.Write((byte)Type.Float);
                writer.Write((float) entry.Value);
            }
            else if (entry.Value is bool)
            {
                writer.Write((byte) Type.Boolean);
                writer.Write((bool) entry.Value);
            }
            else
            {
                throw new NotSupportedException($"The type '{entry.Value}' is not supported.");
            }
        }
    }

    private enum Type
    {
        SerializationInfo,
        String,
        Float,
        Int32,
        Boolean
    }
}