using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public struct Box3Int : IEnumerable<Vector3Int>
{
    public Vector3Int leftBottomBack;
    public Vector3Int rightTopFront;

    public Box3Int(Vector3Int leftBottomBack, Vector3Int rightTopFront)
    {
        this.leftBottomBack = leftBottomBack;
        this.rightTopFront = rightTopFront;
    }

    public Box3Int Padding(Vector3Int border)
    {
        return new Box3Int(leftBottomBack + border, rightTopFront - border);
    }

    public IEnumerator<Vector3Int> GetEnumerator()
    {
        for (int i = leftBottomBack.x; i < rightTopFront.x; i++)
        {
            for (int j = leftBottomBack.y; j < rightTopFront.y; j++)
            {
                for (int k = leftBottomBack.z; k < rightTopFront.z; k++)
                {
                    yield return new Vector3Int(i, j, k);
                }
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

public struct Box2Int : IEnumerable<Vector2Int>
{
    public Vector2Int leftBottom;
    public Vector2Int rightTop;

    public Box2Int(Vector2Int leftBottom, Vector2Int rightTop)
    {
        this.leftBottom = leftBottom;
        this.rightTop = rightTop;
    }

    public Box2Int Padding(Vector2Int border)
    {
        return new Box2Int(leftBottom + border, rightTop - border);
    }

    public IEnumerator<Vector2Int> GetEnumerator()
    {
        for (int x = leftBottom.x; x < rightTop.x; x++)
        {
            for (int y = leftBottom.y; y < rightTop.y; y++)
            {
                yield return new Vector2Int(x, y);
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
