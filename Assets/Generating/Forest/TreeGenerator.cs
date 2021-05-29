using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeGenerator : MonoBehaviour
{

    [SerializeField, Range(2f, 50f)]
    float minHeight;

    [SerializeField, Range(2f, 50f)]
    float maxHeight;

    [SerializeField]
    GameObject trunk;

    public GameObject Generate()
    {
        //var tree =  Instantiate(trunk);
        var tree = (GameObject)UnityEditor.PrefabUtility.InstantiatePrefab(trunk);
        trunk.transform.localScale = new Vector3(1f, Random.Range(minHeight, maxHeight), 1f);

        return tree;

    }
}
