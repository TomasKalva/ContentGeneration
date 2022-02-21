using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ForestGenerator : MonoBehaviour
{
    [SerializeField]
    TreeGenerator treeGen;

    [SerializeField]
    GameObject parent;

    [SerializeField, Range(1, 100)]
    int treesCount;

    public void Generate()
    {
        if(treeGen != null)
        {
            for(int i = 0; i < treesCount; i++)
            {
                var tree = treeGen.Generate();
                if(parent != null)
                {
                    tree.transform.SetParent(parent.transform);
                }


                var areaExtents = gameObject.transform.localScale;
                var treePos = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)) / 2f;
                tree.transform.position = gameObject.transform.position + (Vector3)(gameObject.transform.localToWorldMatrix * treePos);
                tree.transform.Rotate(new Vector3(Random.Range(0f, 15f), Random.Range(0f, 360f)));
            }
        }
    }

    void OnDrawGizmos()
    {
        // Draw a yellow plane at the transform's position
        Gizmos.color = Color.yellow;
        Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.lossyScale);
        Gizmos.DrawCube(Vector3.zero, new Vector3(1f, 0.1f, 1f));
    }
}