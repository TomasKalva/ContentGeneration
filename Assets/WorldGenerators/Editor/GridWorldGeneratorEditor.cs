using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GridWorldGenerator))]
public class GridWorldGeneratorOnInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();
        if (GUILayout.Button("Generate", GUILayout.Width(100), GUILayout.Height(60)))
        {
            var generator = (GridWorldGenerator)target;

            generator.DestroyWorld();
            generator.Generate(null);
        }
        if (GUILayout.Button("Delete", GUILayout.Width(60), GUILayout.Height(30)))
        {
            var generator = (GridWorldGenerator)target;

            generator.DestroyWorld();
        }
        GUILayout.Label("generate");
    }
}
