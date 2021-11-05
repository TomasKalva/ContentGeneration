using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class QuadBlock : MonoBehaviour
{
    public Dictionary<Vector3Int, QuadPlane> DirectionToPlane { get; private set; }

    public void Init(Vector3Int extents)
    {
        DirectionToPlane = new Dictionary<Vector3Int, QuadPlane>();
        foreach (var dir in ExtensionMethods.Directions())
        {
            var plane = new GameObject().AddComponent<QuadPlane>();
            plane.transform.position += (dir * extents) / 2;
            plane.transform.rotation = Quaternion.LookRotation(-dir);
            plane.Init(extents.Deflate(dir));

            DirectionToPlane.Add(dir, plane);
        }
    }
}

class QuadPlane : MonoBehaviour
{
    static Transform quad;

    public static void InitQuad(Transform q)
    {
        quad = q;
    }

    Transform[,] quads;

    public void Init(Vector2Int extents)
    {
        quads = new Transform[extents.x, extents.y];
        foreach(var pos in new Box2Int(Vector2Int.zero, extents))
        {
            var newQuad = Instantiate(quad, transform);
            newQuad.localPosition = new Vector3(pos.x - extents.x / 2 + 0.5f, pos.y - extents.y / 2 + 0.5f, 0f);
        }
    }
}