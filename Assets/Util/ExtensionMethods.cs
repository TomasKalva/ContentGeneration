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
        var count = list.Count;
        if (count == 0) return default;
        var i = UnityEngine.Random.Range(0, list.Count);
        return list[i];
    }

    public static T GetRandom<T>(this IEnumerable<T> enumerable)
    {
        var count = enumerable.Count();
        if (count == 0) return default;
        var i = UnityEngine.Random.Range(0, count);
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

    public static Vector3Int ToVector3Int(this Vector3 v)
    {
        return new Vector3Int((int)v.x, (int)v.y, (int)v.z);
    }

    public static Vector3Int OrthogonalHorizontalDir(this Vector3Int v)
    {
        return new Vector3Int(-v.z, v.y, v.x);
    }

    public static void GetRandomExtents(int M, int m, out int a, out int b)
    {
        int size = UnityEngine.Random.Range(1, m + 1);
        a = UnityEngine.Random.Range(0, M - m);
        b = a + size;
    }

    public static Box2Int RandomBox(Box2Int boundingBox)
    {
        var M = boundingBox.rightTop - boundingBox.leftBottom;
        var m = M / 2;
        GetRandomExtents(M.x, m.x, out var x, out var X);
        GetRandomExtents(M.y, m.y, out var y, out var Y);

        var b = boundingBox.leftBottom;
        return new Box2Int(b + new Vector2Int(x, y), b + new Vector2Int(X, Y));
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

    public static void TryAdd<K, V>(this Dictionary<K,V> dict, K key, V value)
    {
        if (dict.ContainsKey(key))
        {
            dict[key] = value;
        }
        else
        {
            dict.Add(key, value);
        }
    }

    public static void TryAddEx<K, V>(this Dictionary<K, V> dict, K key, V value, string exceptionMsg)
    {
        if (dict.ContainsKey(key))
        {
            dict[key] = value;
        }
        else
        {
            throw new InvalidOperationException(exceptionMsg);
        }
    }

    /// <summary>
    /// Foreach that works only on non-null items.
    /// </summary>
    public static void ForEachNN<T>(this T[,,] array, Action<T> action)
    {
        foreach(var item in array)
        {
            if(item != null)
            {
                action(item);
            }
        }
    }

    public static Box3Int BoundingBox<T>(this T[,,] array)
    {
        var arraySize = new Vector3Int(array.GetLength(0), array.GetLength(1), array.GetLength(2));
        return new Box3Int(Vector3Int.zero, arraySize);
    }

    public static Vector3Int Zip(this Vector3Int u, Vector3Int v, Func<int,int,int> zipF)
    {
        return new Vector3Int(zipF(u.x, v.x), zipF(u.y, v.y), zipF(u.z, v.z));
    }

    public static Box3Int IntersectBoxes(this Box3Int a, Box3Int b)
    {
        var lower = a.leftBottomBack.Zip(b.leftBottomBack, Math.Max);
        var upper = a.rightTopFront.Zip(b.rightTopFront, Math.Min);
        return new Box3Int(lower, upper);
    }

    public static List<T> GetBoxItems<T>(this T[,,] array, Box3Int box)
    {
        var list = new List<T>();
        var arrayBox = array.BoundingBox();
        var intersBox = box.IntersectBoxes(arrayBox);
        foreach(var i in intersBox)
        {
            list.Add(array[i.x, i.y, i.z]);
        }
        return list;
    }

    public static V Get<K, V>(this Dictionary<K, V> dict, K key, V defaultValue)
    {
        if (dict.TryGetValue(key, out var val))
        {
            return val;
        }
        else
        {
            return defaultValue;
        }
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

    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        foreach(var item in enumerable)
        {
            action(item);
        }
    }

    public static IEnumerable<U> SelectNN<T, U>(this IEnumerable<T> enumerable, Func<T, U> selector)
    {
        return enumerable.Select(selector).OfType<U>();
    }

    public static IEnumerable<U> SelectManyNN<T, U>(this IEnumerable<T> enumerable, Func<T, IEnumerable<U>> selector)
    {
        return enumerable.SelectNN(selector).SelectMany(ie => ie).OfType<U>();
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

    public static IEnumerable<Vector3Int> VerticalDirections()
    {
        yield return Vector3Int.up;
        yield return -Vector3Int.up;
    }

    public static IEnumerable<Vector3Int> HorizontalDiagonals()
    {
        yield return Vector3Int.left + Vector3Int.back;
        yield return Vector3Int.left + Vector3Int.forward;
        yield return Vector3Int.right + Vector3Int.back;
        yield return Vector3Int.right + Vector3Int.forward;
    }

    public static IEnumerable<Vector3Int> Directions()
    {
        yield return Vector3Int.forward;
        yield return Vector3Int.right;
        yield return Vector3Int.up;
        yield return -Vector3Int.forward;
        yield return -Vector3Int.right;
        yield return -Vector3Int.up;
    }

    public static Vector3Int ComponentWise(this Vector3Int v, Func<int, int> f)
    {
        return new Vector3Int(f(v.x), f(v.y), f(v.z));
    }

    public static Vector3Int ComponentWise(this Vector3Int u, Vector3Int v, Func<int, int, int> f)
    {
        return new Vector3Int(f(u.x, v.x), f(u.y, v.y), f(u.z, v.z));
    }

    static Dictionary<Vector3Int, string> directionNames;

    public static string Name(this Vector3Int direction)
    {
        if(directionNames == null)
        {
            directionNames = new Dictionary<Vector3Int, string>();
            directionNames.Add(Vector3Int.forward, "Forward");
            directionNames.Add(-Vector3Int.forward, "Back");
            directionNames.Add(Vector3Int.right, "Right");
            directionNames.Add(-Vector3Int.right, "Left");
            directionNames.Add(Vector3Int.up, "Up");
            directionNames.Add(-Vector3Int.up, "Down");
        }

        if(directionNames.TryGetValue(direction, out var name))
        {
            return name;
        }
        else
        {
            return "Unnamed";
        }
    }

    public static Vector2Int Deflate(this Vector3Int v, Vector3Int direction)
    {
        if (direction == Vector3Int.forward || direction == Vector3Int.back)
        {
            return v.XY();
        }
        if (direction == Vector3Int.left || direction == Vector3Int.right)
        {
            return v.YZ();
        }
        if (direction == Vector3Int.up || direction == Vector3Int.down)
        {
            return v.XZ();
        }
        throw new InvalidOperationException("This vector is not canonical");
    }

    public static Vector2Int ComponentWise(this Vector2Int u, Vector2Int v, Func<int, int, int> f)
    {
        return new Vector2Int(f(u.x, v.x), f(u.y, v.y));
    }

    public static int Sum(this Vector3Int v, Func<int, int> f)
    {
        return f(v.x) + f(v.y) + f(v.z);
    }

    public static Vector3 ComponentWise(this Vector3 v, Func<float, float> f)
    {
        return new Vector3(f(v.x), f(v.y), f(v.z));
    }

    public static Vector3 ComponentWise(this Vector3 u, Vector3 v, Func<float, float, float> f)
    {
        return new Vector3(f(u.x, v.x), f(u.y, v.y), f(u.z, v.z));
    }

    public static Vector3 Divide(this Vector3 u, Vector3 v)
    {
        return u.ComponentWise(v, (a, b) => a / b);
    }

    public static Vector3Int Mult(this Vector3Int u, Vector3Int v)
    {
        return u.ComponentWise(v, (a, b) => a * b);
    }

    public static Vector3Int Invert(this Vector3Int v)
    {
        return v.ComponentWise(a => a == 0 ? 1 : 0);
    }

    public static bool Any(this Vector3 v, Func<float, bool> p)
    {
        return p(v.x) || p(v.y) || p(v.z);
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
    public static Vector2Int XZ(this Vector3Int v) => new Vector2Int(v.x, v.z);
    public static Vector2Int XY(this Vector3Int v) => new Vector2Int(v.x, v.y);
    public static Vector2Int YZ(this Vector3Int v) => new Vector2Int(v.y, v.z);

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