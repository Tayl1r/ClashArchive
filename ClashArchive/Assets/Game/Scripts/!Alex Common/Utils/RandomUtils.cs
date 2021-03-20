using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class RandomUtils
{
    public static T RandomFrom<T>(List<T> objects)
    {
        return objects[Random.Range(0, objects.Count)];
    }

    public static T RandomFrom<T>(T[] objects)
    {
        return objects[Random.Range(0, objects.Length)];
    }

    public static T RandomFrom<T>(HashSet<T> hashSet)
    {
        return hashSet.ElementAt(Random.Range(0, hashSet.Count));
    }

    public static void Shuffle<T>(this IList<T> list)
    {
        var n = list.Count;
        while (n > 1)
        {
            n--;
            var k = Random.Range(0, list.Count);
            var value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }

    public static Vector3 RandomPointInBounds(Bounds bounds)
    {
        return new Vector3(
            Random.Range(bounds.min.x, bounds.max.x),
            Random.Range(bounds.min.y, bounds.max.y),
            Random.Range(bounds.min.z, bounds.max.z)
        );
    }
}