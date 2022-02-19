using Assets.InteractiveObject;
using ContentGeneration.Assets.UI.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class InteractiveObjects : ScriptableObject
{
    [MenuItem("Assets/Create/InteractiveObjects")]
    public static void CreateMyAsset()
    {
        InteractiveObjects asset = ScriptableObject.CreateInstance<InteractiveObjects>();

        string name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/InteractiveObjects.asset");
        AssetDatabase.CreateAsset(asset, name);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }

    [SerializeField]
    InteractiveObject gravePrefab;

    public InteractiveObject Grave()
    {
        var state = new GraveState() 
        {
            Name = "Grave",
            MessageOnInteract = "Grave chosen",
            InteractionDescription = "Choose Grave"
        };
        var obj = Instantiate(gravePrefab);
        obj.State = state;

        return obj;
    }
}
