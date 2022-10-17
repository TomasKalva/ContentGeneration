using OurFramework.Environment.GridMembers;
using OurFramework.Environment.StylingAreas;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Util;

/// <summary>
/// My random is thread safe, unlike the one from Unity.
/// </summary>
static class MyRandom
{
    static readonly System.Random GlobalRandom = new System.Random();

    [ThreadStatic] static System.Random _threadSafeRandom;

    static System.Random ThreadSafeRandom 
    { 
        get
        {
            if(_threadSafeRandom == null)
            {
                int seed;
                lock (GlobalRandom)
                {
                    seed = GlobalRandom.Next();
                }
                _threadSafeRandom = new System.Random(seed);
            }
            return _threadSafeRandom;
        } 
    }

    public static int Range(int min, int max) => ThreadSafeRandom.Next(min, max);
    public static float Range(float min, float max) => Value * (max - min) + min;
    public static float Value => (float)ThreadSafeRandom.NextDouble();
}

static class ExtensionMethods
{
    // from https://ericlippert.com/2010/06/28/computing-a-cartesian-product-with-linq/
    public static IEnumerable<IEnumerable<T>> CartesianProduct<T>(this IEnumerable<IEnumerable<T>> sequences)
    {// base case: 
        IEnumerable<IEnumerable<T>> result = new[] { Enumerable.Empty<T>() };
        foreach (var sequence in sequences)
        {
            // don't close over the loop variable (fixed in C# 5 BTW)
            var s = sequence;
            // recursive case: use SelectMany to build 
            // the new product out of the old one 
            result =
              from seq in result
              from item in s
              select seq.Append(item);
        }
        return result;
    }

    public static U DoUntilSuccess<T, U>(this IEnumerable<T> enumerable, Func<T, U> operation, Func<U, bool> success)
    {
        foreach(var item in enumerable)
        {
            var result = operation(item);
            if (success(result))
                return result;
        }
        return default;
    }

    public static T GetRandom<T>(this List<T> list)
    {
        var count = list.Count;
        if (count == 0) return default;
        var i = MyRandom.Range(0, list.Count);
        return list[i];
    }

    public static T GetRandom<T>(this IEnumerable<T> enumerable)
    {
        var count = enumerable.Count();
        if (count == 0) return default;
        var i = MyRandom.Range(0, count);
        return enumerable.ElementAt(i);
    }

    public static IEnumerable<T> Evaluate<T>(this IEnumerable<T> enumerable)
    {
        return enumerable.ToList();
    }

    public static IEnumerable<T> SkipLast<T>(this IEnumerable<T> enumerable, int skipCount)
    {
        return enumerable.Reverse().Skip(skipCount).Reverse();
    }

    public static CubeGroup ToCubeGroup(this IEnumerable<Cube> enumerable, Grid<Cube> grid)
    {
        return enumerable.ToList().ToCubeGroup(grid);
    }

    public static CubeGroup ToCubeGroup(this List<Cube> list, Grid<Cube> grid)
    {
        return new CubeGroup(grid, list);
    }

    public static CubeGroup ToCubeGroup(this IEnumerable<CubeGroup> enumerable)
    {
        return new CubeGroup(enumerable.FirstOrDefault().Grid, enumerable.SelectMany(cg => cg.Cubes).ToList());
    }

    public static LevelGroupElement ToLevelGroupElement(this IEnumerable<CubeGroup> enumerable, Grid<Cube> grid)
    {
        return enumerable.Select(g => new LevelGeometryElement(g.Grid, AreaStyles.None(), g)).ToLevelGroupElement(grid);
    }

    public static LevelGroupElement ToLevelGroupElement(this IEnumerable<LevelElement> enumerable, Grid<Cube> grid)
    {
        return enumerable.ToList().ToLevelGroupElement(grid);
    }

    public static LevelGroupElement ToLevelGroupElement(this List<LevelElement> list, Grid<Cube> grid)
    {
        return new LevelGroupElement(grid, AreaStyles.None(), list);
    }

    public static IEnumerable<T> ToEnumerable<T>(this T t)
    {
        return new T[1] { t };
    }

    public static Vector2Int RandomVector2Int(Vector2Int leftBottom, Vector2Int rightTop)
    {
        var x = MyRandom.Range(leftBottom.x, rightTop.x);
        var y = MyRandom.Range(leftBottom.y, rightTop.y);
        return new Vector2Int(x, y);
    }

    public static Vector3Int RandomVector3Int(Vector3Int leftBottomBack, Vector3Int rightTopFront)
    {
        var x = MyRandom.Range(leftBottomBack.x, rightTopFront.x);
        var y = MyRandom.Range(leftBottomBack.y, rightTopFront.y);
        var z = MyRandom.Range(leftBottomBack.z, rightTopFront.z);
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

    public static IEnumerable<Vector3Int> OrthogonalHorizontalDirs(this Vector3Int v)
    {
        return new[] { new Vector3Int(-v.z, v.y, v.x), new Vector3Int(v.z, v.y, -v.x) };
    }

    public static void GetRandomExtents(int M, int m, out int a, out int b)
    {
        int size = MyRandom.Range(1, m + 1);
        a = MyRandom.Range(0, M - m);
        b = a + size;
    }

    public static Box2Int RandomHalfBox(Box2Int boundingBox)
    {
        var M = boundingBox.rightTop - boundingBox.leftBottom;
        var m = M / 2;
        GetRandomExtents(M.x, m.x, out var x, out var X);
        GetRandomExtents(M.y, m.y, out var y, out var Y);

        var b = boundingBox.leftBottom;
        return new Box2Int(b + new Vector2Int(x, y), b + new Vector2Int(X, Y));
    }

    public static Box2Int RandomBox(Box2Int boundingBox)
    {
        var M = boundingBox.rightTop - boundingBox.leftBottom;
        GetRandomExtents(M.x, M.x, out var x, out var X);
        GetRandomExtents(M.y, M.y, out var y, out var Y);

        var b = boundingBox.leftBottom;
        return new Box2Int(b + new Vector2Int(x, y), b + new Vector2Int(X, Y));
    }

    public static Box2Int RandomBox(Vector2Int minExt, Vector2Int maxExt)
    {
        var ext = minExt.ComponentWise(maxExt, (min, max) => MyRandom.Range(min, max));
        return new Box2Int(Vector2Int.zero, ext);
    }

    public static Box2Int RandomBox(int minExt, int maxExt)
    {
        return RandomBox(minExt * Vector2Int.one, maxExt * Vector2Int.one);
    }

    public static Box2Int RandomBox(IDistribution<int> xDistr, IDistribution<int> yDistr) 
    {
        return new Box2Int(Vector2Int.zero, new Vector2Int(xDistr.Sample(), yDistr.Sample()));
    }

    public static Box3Int RandomHalfBox(this Box3Int boundingBox)
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

    public static IEnumerable<Box3Int> BoxSequence(Func<Box3Int> f)
    {
        while (true)
        {
            yield return f();
        }
    }

    public static IEnumerable<Box2Int> BoxSequence(Func<Box2Int> f)
    {
        while (true)
        {
            yield return f();
        }
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
    
    /// <summary>
    /// Foreach with index.
    /// </summary>
    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T, int> action)
    {
        enumerable.Select((item, i) => new { item, i }).ForEach(x => action(x.item, x.i));
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
        float r = MyRandom.Value * total;

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

    public static int IndexOfInterval(this IEnumerable<int> intervals, float value) => IndexOfInterval(intervals.Select(v => (float)v), value);

    /// <summary>
    /// Array intervals should be sorted from lowest to highest.
    /// Less than smallest => 0. More than highest => intervals.Count() + 1.
    /// </summary>
    public static int IndexOfInterval(this IEnumerable<float> intervals, float value)
    {
        int i = 0;
        foreach(var v in intervals)
        {
            if (value < v)
            {
                return i;
            }
            i++;
        }
        return i;
    }

    public static IEnumerable<T> Others<T>(this IEnumerable<T> enumerable, T me) where T : class
    {
        return enumerable.Where(t => t != me);
    }

    public static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
    {
        foreach(var item in enumerable)
        {
            action(item);
        }
    }

    public static IEnumerable<U> IntersectMany<T, U>(this IEnumerable<T> enumerable, Func<T, IEnumerable<U>> selector)
    {
        if (!enumerable.Any())
        {
            throw new InvalidOperationException("Can't create intersection of zero sets!");
        }
        return !enumerable.Skip(1).Any() ?
            selector(enumerable.First()) :
            SetIntersect(selector(enumerable.First()), IntersectMany(enumerable.Skip(1), selector));
    }

    /// <summary>
    /// Iterates over all interior elements and both their neighbors.
    /// </summary>
    public static void ForEach3<T>(this List<T> list, Action<T, T, T> action)
    {
        for(int i = 1; i < list.Count - 1; i++)
        {
            action(list[i - 1], list[i], list[i + 1]);
        }
    }

    /// <summary>
    /// For each consecutive triple (l, t, r) returns t if the triple satisfies the condition.
    /// </summary>
    public static List<T> Where3<T>(this List<T> list, Func<T, T, T, bool> cond)
    {
        var selected = new List<T>();
        list.ForEach3((l, t, r) =>
        { 
            if (cond(l, t, r))
            {
                selected.Add(t);
            }
        });
        return selected;
    }

    public static void ForEach2Cycle<T>(this IEnumerable<T> enumerable, Action<T, T> action)
    {
        var first = enumerable;
        var second = enumerable.Skip(1).Append(enumerable.FirstOrDefault());
        first.Zip(second, (f, s) => (f,s)).ForEach((pair) => action(pair.f, pair.s));
    }

    public static void ForEach2<T>(this IEnumerable<T> enumerable, Action<T, T> action)
    {
        var first = enumerable;
        var second = enumerable.Skip(1);
        first.Zip(second, (f, s) => (f, s)).ForEach((pair) => action(pair.f, pair.s));
    }

    /// <summary>
    /// Selects from pairs of consecutive items.
    /// </summary>
    public static IEnumerable<U> Select2<T, U>(this IEnumerable<T> enumerable, Func<T, T, U> selector)
    {
        var first = enumerable;
        var second = enumerable.Skip(1);
        return first.Zip(second, (f, s) => (f, s)).Select((pair) => selector(pair.f, pair.s));
    }

    public static IEnumerable<U> Select2Distinct<T, U>(this IEnumerable<T> enumerable, Func<T, T, U> selector)
    {
        int k = 0;
        foreach(var second in enumerable)
        {
            foreach(var first in enumerable.Take(k))
            {
                yield return selector(first, second);
            }
            k++;
        }
    }

    public static IEnumerable<T> Interleave<T>(this IEnumerable<T> ie1, IEnumerable<T> ie2)
    {
        return ie1.Zip(ie2, (f, s) => new[] { f, s })
                       .SelectMany(f => f);
    }

    public static IEnumerable<U> SelectNN<T, U>(this IEnumerable<T> enumerable, Func<T, U> selector)
    {
        return enumerable.Select(selector).OfType<U>();
    }

    public static IEnumerable<U> SelectManyNN<T, U>(this IEnumerable<T> enumerable, Func<T, IEnumerable<U>> selector)
    {
        return enumerable.SelectNN(selector).SelectMany(ie => ie).OfType<U>();
    }

    public static void Swap<T>(this List<T> list, int i, int j)
    {
        var tmp = list[i];
        list[i] = list[j];
        list[j] = tmp;
    }

    public static IEnumerable<int> Circle1(int r)
    {
        yield return r;
        if(r != 0)
        {
            yield return -r;
        }
    }

    public static IEnumerable<Vector2Int> Circle2(int r)
    {
        for(int y = -r; y <= r; y++)
        {
            int absY = Mathf.Abs(y);
            foreach(var x in Circle1(r - absY))
            {
                yield return new Vector2Int(x, y);
            }
        }
    }

    public static IEnumerable<Vector3Int> Circle3(int r)
    {
        for (int z = -r; z <= r; z++)
        {
            int absZ = Mathf.Abs(z);
            foreach (var xy in Circle2(r - absZ))
            {
                yield return new Vector3Int(xy.x, xy.y, z);
            }
        }
    }

    public static IEnumerable<Vector3Int> AllVectorsXZ()
    {
        int i = 0;
        while (true)
        {
            foreach (var v in Circle2(i))
            {
                yield return v.X0Z();
            }
            i++;
        }
    }

    public static IEnumerable<Vector3Int> AllVectors3()
    {
        int i = 0;
        while (true)
        {
            foreach(var v in Circle3(i))
            {
                yield return v;
            }
            i++;
        }
    }

    public static IEnumerable<int> Naturals()
    {
        int i = 0;
        while (true)
        {
            yield return i;
            i++;
        }
    }

    /// <summary>
    /// Using Fisher–Yates shuffle.
    /// </summary>
    public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> enumerable)
    {
        var buffer = enumerable.ToList();
        for (int i = 0; i < buffer.Count; i++)
        {
            int j = MyRandom.Range(i, buffer.Count);
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

    public static IEnumerable<Vector3Int> PositiveDirections()
    {
        yield return Vector3Int.forward;
        yield return Vector3Int.right;
        yield return Vector3Int.up;
    }

    public static Vector3Int ComponentWise(this Vector3Int v, Func<int, int> f)
    {
        return new Vector3Int(f(v.x), f(v.y), f(v.z));
    }

    public static Vector3Int ComponentWise(this Vector3Int u, Vector3Int v, Func<int, int, int> f)
    {
        return new Vector3Int(f(u.x, v.x), f(u.y, v.y), f(u.z, v.z));
    }

    public static Vector3Int Div(this Vector3Int u, Vector3Int v)
    {
        return u.ComponentWise(v, (a, b) => a < 0 ? (a + 1) / b - 1 : a / b);
    }

    public static Vector3Int Mod(this Vector3Int u, Vector3Int v)
    {
        return u.ComponentWise(v, (a, b) => a < 0 ? (a % b + b ) % b : a % b);
    }

    public static int Mod(this int u, int v)
    {
        return u < 0 ? (u % v + v) % v : u % v;
    }

    public static T ApplyNTimes<T>(Func<T, T> f, T t, int n) => n == 0 ? t : ApplyNTimes(f, f(t), n - 1); 

    static Dictionary<Vector3Int, string> directionNames;

    public static IEnumerable<Vector3Int> MinkowskiMinus(this IEnumerable<Vector3Int> s1, IEnumerable<Vector3Int> s2)
    {

        return
            (from v1 in s1
             from v2 in s2
             select v1 - v2).Distinct();
    }

    public static IEnumerable<T> SetMinus<T>(this IEnumerable<T> s1, IEnumerable<T> s2)
    {
        var set = new HashSet<T>(s1);
        s2.ForEach(t => set.Remove(t));
        return set;
    }

    public static IEnumerable<T> SetIntersect<T>(this IEnumerable<T> s1, IEnumerable<T> s2)
    {
        var set = new HashSet<T>(s1);
        foreach(var t in s2)
        {
            if (set.Contains(t))
            {
                yield return t;
            }
        }
    }

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

    public static float Sum(this Vector3 v, Func<float, float> f)
    {
        return f(v.x) + f(v.y) + f(v.z);
    }

    public static int AbsSum(this Vector3Int v)
    {
        return v.Sum(c => Mathf.Abs(c));
    }

    public static float AbsSum(this Vector3 v)
    {
        return v.Sum(c => Mathf.Abs(c));
    }

    public static int Dot(this Vector3Int u, Vector3Int v)
    {
        return u.x * v.x + u.y * v.y + u.z * v.z;
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
        return MyRandom.Range(0, 2) == 0 ? 1 : -1;
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


    // Adapted from: http://stackoverflow.com/questions/563198/how-do-you-detect-where-two-line-segments-intersect/1968345#1968345
    public static bool LinesIntersects(Vector2 a0, Vector2 a1, Vector2 b0, Vector2 b1)
    {
        float s1_x, s1_y, s2_x, s2_y;
        s1_x = a1.x - a0.x;
        s1_y = a1.y - a0.y;
        s2_x = b1.x - b0.x;
        s2_y = b1.y - b0.y;

        float s, t;
        s = (-s1_y * (a0.x - b0.x) + s1_x * (a0.y - b0.y)) / (-s2_x * s1_y + s1_x * s2_y);
        t = (s2_x * (a0.y - b0.y) - s2_y * (a0.x - b0.x)) / (-s2_x * s1_y + s1_x * s2_y);

        return s > 0 && s < 1 && t > 0 && t < 1;
    }
}

public static class TransformExtensions
{
    public static Transform FindRecursive(this Transform self, string exactName) => self.FindRecursive(child => child.name == exactName);

    public static Transform FindRecursive(this Transform self, Func<Transform, bool> selector)
    {
        foreach (Transform child in self)
        {
            if (selector(child))
            {
                return child;
            }

            var finding = child.FindRecursive(selector);

            if (finding != null)
            {
                return finding;
            }
        }

        return null;
    }

    public static void SetParent(this Transform self, Transform parent, Vector3 localPosition)
    {
        self.SetParent(parent);
        self.localPosition = localPosition;
    }
}