using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Module))]
public class ModuleOnInspector : Editor
{
    public override void OnInspectorGUI()
    {
        base.DrawDefaultInspector();

        var module = (Module)target;
        var areaProp = module.GetProperty<AreaModuleProperty>();
        if (areaProp == null)
        {
            return;
        }
        var designer = areaProp.Area.Designer;


        var style = new GUIStyle();
        style.fontSize = 20;

        GUILayout.Label($"Area: {module.GetProperty<AreaModuleProperty>().Area.Name}");
        GUILayout.Label("Rules", style);
        foreach (var rule in designer.UsedRules(module))
        {
            GUILayout.Label($"{rule.RulesClass.Name}: {rule.Name}");
        }
    }
}