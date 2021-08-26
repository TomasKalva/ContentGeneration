using ContentGeneration.Assets.UI.Model;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class GridWorldGenerator : WorldGenerator
{
    [SerializeField]
    Transform parent;

    [SerializeField]
    Module bridge;

    [SerializeField]
    Module stairs;

    [SerializeField]
    Module room;

    [SerializeField]
    int depth;

    [SerializeField]
    int height;
    
    [SerializeField]
    int width;

    [SerializeField]
    Vector3 extents;

    [SerializeField]
    int buildingsCount;

    Module[,,] moduleGrid;

    public override void Generate(World world)
    {
        // Destroy old grid
        /*UnityEditor.EditorApplication.delayCall += () =>
        {
            foreach (Transform child in parent)
            {
                DestroyImmediate(child.gameObject);
            }
        };*/

        moduleGrid = new Module[width, height, depth];
        Debug.Log("Generating grid world");


        AddBuildings(buildingsCount);

        /*for (int i = 0; i < width; i++)
        {
            for (int j = 0; j < height; j++)
            {
                for (int k = 0; k < depth; k++)
                {
                    var module = Instantiate(room);
                    module.transform.SetParent(parent);
                    module.transform.position = Vector3.Scale(new Vector3(i, j, k), extents);
                }
            }
        }*/
        //world.AddItem(items.blueIchorEssence, new Vector3(0, 0, -54));


        //world.AddInteractiveObject(interactiveObjects.bonfire, new Vector3(0, 0, -54));
    }

    void AddBuildings(int n)
    {
        for (int i = 0; i < n; i++)
        {
            AddBuilding();
        }
    }

    void GetExtents(int M, int m, out int a, out int b)
    {
        int size = Random.Range(1, m + 1);
        a = Random.Range(0, M - m);
        b = a + size;
    }

    void AddBuilding()
    {
        GetExtents(width, 4, out var minX, out var maxX);
        GetExtents(depth, 6, out var minZ, out var maxZ);
        int minY = 0;
        int maxY = Random.Range(1, 5);
        for(int i=minX; i < maxX; i++)
        {
            for(int j = minY; j < maxY; j++)
            {
                for(int k = minZ; k < maxZ; k++)
                {
                    var module = Instantiate(room);
                    module.transform.SetParent(parent);
                    module.transform.position = Vector3.Scale(new Vector3(i, j, k), extents);
                }
            }
        }
    }

    void AddBridges()
    {

    }

    public void DestroyAllChildren()
    {
        for (int i = parent.childCount; i > 0; --i)
        {
            DestroyImmediate(parent.GetChild(0).gameObject);
        }
    }
}

[CustomEditor(typeof(GridWorldGenerator))]
public class GridWorldGeneratorOnInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();
        if (GUILayout.Button("Generate", GUILayout.Width(100), GUILayout.Height(60)))
        {
            var generator = (GridWorldGenerator)target;

            generator.DestroyAllChildren();
            generator.Generate(null);
        }
        GUILayout.Label("generate");
    }
}