using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ForestGenerator))]
public class ForestGeneratorOnInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();
        if (GUILayout.Button("Generate", GUILayout.Width(100), GUILayout.Height(60)))
        {
            var generator = (ForestGenerator)target;
            generator.Generate();
        }
        GUILayout.Label("generate");
    }
}