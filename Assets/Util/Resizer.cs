using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Resizer : MonoBehaviour
{
    public Vector3 newSize;

    public void Resize()
    {
        var collider = GetComponent<Collider>();
        if(collider == null)
        {
            Debug.LogError($"Not collider component exists on {gameObject.name}");
            return;
        }

        var localScale = transform.localScale.ComponentWise(Mathf.Abs);
        if(localScale.Any(a => a == 0f))
        {
            Debug.LogWarning($"Scale component is zero on {gameObject.name}");
        }

        var bounds = 2f * collider.bounds.extents;
        var newGlobalScale = newSize.ComponentWise(bounds, (a, b) => a/ b).ComponentWise(localScale, (a, b) => a * b);
        transform.localScale = newGlobalScale;

    }
}

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
