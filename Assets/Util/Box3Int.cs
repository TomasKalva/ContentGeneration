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
