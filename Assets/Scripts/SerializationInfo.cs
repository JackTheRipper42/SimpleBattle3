using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

public class SerializationInfo
{
    private readonly Dictionary<string, object> _data;

    public SerializationInfo()
    {
        _data = new Dictionary<string, object>();
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

    public void SetValue(string name, bool value)
    {
        _data[name] = value;
    }

    public void SetValue<T>(string name, T value)
        where T : ISerializable
    {
        var serializationInfo = new SerializationInfo();
        value.Serialize(serializationInfo);
        _data[name] = serializationInfo;
    }

    public float GetSingle(string name)
    {
        var value = _data[name];
        if (value is float)
        {
            return (float) _data[name];
        }

        return Convert.ToSingle(value);
    }

    public int GetInt32(string name)
    {
        return (int) _data[name];
    }

    public string GetString(string name)
    {
        return (string) _data[name];
    }

    public bool GetBoolean(string name)
    {
        return (bool) _data[name];
    }

    public T GetValue<T>(string name)
        where T : ISerializable, new()
    {
        var value = new T();
        var serializationInfo = (SerializationInfo) _data[name];
        value.Deserialize(serializationInfo);
        return value;
    }

    public T GetValue<T>(string name, Func<SerializationInfo, T> factory)
        where T : ISerializable
    {
        var serializationInfo = (SerializationInfo) _data[name];
        var value = factory(serializationInfo);
        value.Deserialize(serializationInfo);
        return value;
    }

    public static class Serializer
    {
        private static readonly DataContractJsonSerializer JsonSerializer
            = new DataContractJsonSerializer(
                typeof(Entry[]),
                new DataContractJsonSerializerSettings
                {
                    SerializeReadOnlyTypes = true,
                    IgnoreExtensionDataObject = true,
                    EmitTypeInformation = EmitTypeInformation.Never
                });

        public static void Write(Stream stream, SerializationInfo serializationInfo)
        {
            var entries = CreateEntries(serializationInfo);
            JsonSerializer.WriteObject(stream, entries);
        }

        public static SerializationInfo Read(Stream stream)
        {
            var entries = (Entry[])JsonSerializer.ReadObject(stream);
            return Create(entries);
        }

        private static SerializationInfo Create(Entry[] entries)
        {
            var result = new SerializationInfo();
            foreach (var entry in entries)
            {
                if (entry.NestedData != null)
                {
                    var innerSerializationInfo = Create(entry.NestedData);
                    result._data.Add(entry.Name, innerSerializationInfo);
                }
                else
                {
                    result._data.Add(entry.Name, entry.Data);
                }
            }
            return result;
        }

        private static Entry CreateEntry(string name, object data)
        {
            var innerSerializationInfo = data as SerializationInfo;
            if (innerSerializationInfo != null)
            {
                var entries = CreateEntries(innerSerializationInfo);
                return new Entry
                {
                    Name = name,
                    NestedData = entries
                };
            }

            return new Entry
            {
                Name = name,
                Data = data
            };
        }

        private static Entry[] CreateEntries(SerializationInfo serializationInfo)
        {
            var entries = new Entry[serializationInfo._data.Count];
            var index = 0;
            foreach (var keyValuePair in serializationInfo._data)
            {
                entries[index++] = CreateEntry(keyValuePair.Key, keyValuePair.Value);
            }
            return entries;
        }

        [DataContract]
        [KnownType(typeof(Entry[]))]
        private class Entry
        {
            [DataMember(Name = "N", Order = 1, IsRequired = true)] public string Name;
            [DataMember(Name = "D", Order = 2, IsRequired = false, EmitDefaultValue = false)] public object Data;
            [DataMember(Name = "ND", Order = 3, IsRequired = false, EmitDefaultValue = false)] public Entry[] NestedData;
        }
    }
}