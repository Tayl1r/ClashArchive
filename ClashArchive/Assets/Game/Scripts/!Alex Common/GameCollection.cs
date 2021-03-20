using System;
using System.Collections.Generic;
using UnityEngine;

public class GameCollection<T>
{
    public Action<T> OnChanged;
    public Action<T> OnMemberAdded;
    public Action<T> OnMemberRemoved;
    public List<T> Data { private set; get; }

    public GameCollection()
    {
        Data = new List<T>();
    }

    public bool AddMember(T obj)
    {
        if (Data.Contains(obj))
            return false;
        Data.Add(obj);

        OnMemberAdded?.Invoke(obj);
        OnChanged?.Invoke(obj);
        return true;
    }

    public bool RemoveMember(T obj)
    {
        if (!Data.Contains(obj))
            return false;
        Data.Remove(obj);

        OnMemberRemoved?.Invoke(obj);
        OnChanged?.Invoke(obj);
        return true;
    }

    public T RandomFrom => RandomUtils.RandomFrom(Data);
}
