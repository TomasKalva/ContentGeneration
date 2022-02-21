using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Resizer))]
public class ResizerOnInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();
        if (GUILayout.Button("Resize", GUILayout.Width(60), GUILayout.Height(40)))
        {
            var resizer = (Resizer)target;
            resizer.Resize();
        }
    }
}
