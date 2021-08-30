﻿using System;
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

    public static Vector2Int RandomVector2Int(Vector2Int leftBottom, Vector2Int rightTop)
    {
        var x = UnityEngine.Random.Range(leftBottom.x, rightTop.x);
        var y = UnityEngine.Random.Range(leftBottom.y, rightTop.y);
        return new Vector2Int(x, y);
    }

    public static Vector3Int RandomVector3Int(Vector3Int leftBottomBack, Vector3Int rightTopFront)
    {
        var x = UnityEngine.Random.Range(leftBottomBack.x, rightTopFront.x);
        var y = UnityEngine.Random.Range(leftBottomBack.y, rightTopFront.y);
        var z = UnityEngine.Random.Range(leftBottomBack.z, rightTopFront.z);
        return new Vector3Int(x, y, z);
    }

    public static void GetRandomExtents(int M, int m, out int a, out int b)
    {
        int size = UnityEngine.Random.Range(1, m + 1);
        a = UnityEngine.Random.Range(0, M - m);
        b = a + size;
    }

    public static Box3Int RandomBox(Box3Int boundingBox)
    {
        var M = boundingBox.rightTopFront - boundingBox.leftBottomBack;
        var m = M / 2;
        Debug.Log(m);
        GetRandomExtents(M.x, m.x, out var x, out var X);
        GetRandomExtents(M.y, m.y, out var y, out var Y);
        GetRandomExtents(M.z, m.z, out var z, out var Z);

        var b = boundingBox.rightTopFront;
        return new Box3Int(b + new Vector3Int(x, y, z), b + new Vector3Int(X, Y, Z));
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

    /// <summary>
    /// Using Fisher–Yates shuffle.
    /// </summary>
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> enumerable)
    {
        var buffer = enumerable.ToList();
        for (int i = 0; i < buffer.Count; i++)
        {
            int j = UnityEngine.Random.Range(i, buffer.Count);
            yield return buffer[j];

            buffer[j] = buffer[i];
        }
    }

    public static IEnumerable<Vector3Int> HorizontalDirections()
    {
        yield return Vector3Int.forward;
        yield return Vector3Int.right;
        yield return -Vector3Int.forward;
        yield return -Vector3Int.right;
    }

    public static Vector3Int ComponentWise(this Vector3Int v, Func<int, int> f)
    {
        return new Vector3Int(f(v.x), f(v.y), f(v.z));
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
    public static Vector2Int XZ(this Vector3Int v)
    {
        return new Vector2Int(v.x, v.z);
    }

    public static Vector3Int X0Z(this Vector2Int v)
    {
        return new Vector3Int(v.x, 0, v.y);
    }

    public static bool InRect(this Vector3Int v, Vector3Int leftBottomBack, Vector3Int rightTopFront) => v.AtLeast(leftBottomBack) && v.Less(rightTopFront);
    public static bool InRect(this Vector2Int v, Vector2Int leftBottom, Vector2Int rightTop) => v.AtLeast(leftBottom) && v.Less(rightTop);

    public static bool Less(this Vector3Int u, Vector3Int v)
    {
        return u.x < v.x && u.y < v.y && u.z < v.z;
    }

    public static bool AtLeast(this Vector3Int u, Vector3Int v)
    {
        return u.x >= v.x && u.y >= v.y && u.z >= v.z;
    }

    public static bool Less(this Vector2Int u, Vector2Int v)
    {
        return u.x < v.x && u.y < v.y;
    }

    public static bool AtLeast(this Vector2Int u, Vector2Int v)
    {
        return u.x >= v.x && u.y >= v.y;
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