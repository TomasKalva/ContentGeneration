using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ExamplesTesting))]
public class ExamplesTestingOnInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();
        var et = (ExamplesTesting)target;
        et.Runner.Init();

        foreach(var ex in et.Runner.Examples.AllExamples())
        {
            if (GUILayout.Button(ex.Method.Name, GUILayout.Width(300), GUILayout.Height(40)))
            {
                et.Runner.Run(ex);
            }
        }
    }
}