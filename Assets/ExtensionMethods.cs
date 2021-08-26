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

    public static T GetRandom<T>(this IEnumerable<T> enumerable, Func<T, float> weightF)
    {
        float total = enumerable.Sum(weightF);
        float r = UnityEngine.Random.value * total;

        int i = 0;
        float x = 0f;
        foreach (var item in enumerable)
        {
            x += weightF(item);
            if (r <= x)
            {
                return item;
            }
            i++;
        }
        return default;
    }

    public static IEnumerable<Vector3Int> HorizontalDirections()
    {
        yield return Vector3Int.forward;
        yield return -Vector3Int.forward;
        yield return Vector3Int.right;
        yield return -Vector3Int.right;
    }

    public static int PlusMinusOne()
    {
        return UnityEngine.Random.Range(0, 2) == 0 ? 1 : -1;
    }

    public static Vector3 ProjectDirectionOnPlane(Vector3 direction, Vector3 normal)
    {
        return (direction - Vector3.Dot(direction, normal) * normal).normalized;
    }

    public static Vector2 XZ(this Vector3 v)
    {
        return new Vector2(v.x, v.z);
    }

    public static Vector3 X0Z(this Vector2 v)
    {
        return new Vector3(v.x, 0f, v.y);
    }

    public static bool Less(this Vector3Int u, Vector3Int v)
    {
        return u.x < v.x && u.y < v.y && u.z < v.z;
    }

    public static bool More(this Vector3Int u, Vector3Int v)
    {
        return u.x > v.x && u.y > v.y && u.z > v.z;
    }

    public static bool IsPointInDirection(Vector3 start, Vector3 direction, Vector3 point)
    {
        return Vector3.Dot(direction, point - start) >= 0f;
    }

    public static float PerFixedSecond(float value)
    {
        return value * Time.fixedDeltaTime;
    }
}