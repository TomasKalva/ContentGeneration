using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

static class ExtensionMethods
{
    public static T GetRandom<T>(this List<T> list)
    {
        var i = UnityEngine.Random.Range(0, list.Count);
        return list[i];
    }

    public static T GetRandom<T>(this IEnumerable<T> enumerable)
    {
        var i = UnityEngine.Random.Range(0, enumerable.Count());
        return enumerable.ElementAt(i);
    }

    public static T ArgMax<T>(this IEnumerable<T> enumerable, Func<T, float> f)
    {
        var best = enumerable.FirstOrDefault();
        float max = float.MinValue;
        foreach (var item in enumerable)
        {
            var val = f(item);
            if(val > max)
            {
                best = item;
                max = val;
            }
        }
        return best;
    }

    public static T ArgMin<T>(this IEnumerable<T> enumerable, Func<T, float> f)
    {
        return enumerable.ArgMax(a => -f(a));
    }

    public static int PlusMinusOne()
    {
        return UnityEngine.Random.Range(0, 2) == 0 ? 1 : -1;
    }

    public static Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal)
    {
        return (direction - Vector3.Dot(direction, normal) * normal).normalized;
    }

    public static bool PointInDirection(Vector3 start, Vector3 direction, Vector3 point)
    {
        return Vector3.Dot(direction, point - start) >= 0f;
    }

    public static float PerFixedSecond(float value)
    {
        return value * Time.fixedDeltaTime;
    }
}