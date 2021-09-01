using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Modules : ScriptableObject
{
    [MenuItem("Assets/Create/Modules")]
    public static void CreateMyAsset()
    {
        Modules asset = ScriptableObject.CreateInstance<Modules>();

        string name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/Modules.asset");
        AssetDatabase.CreateAsset(asset, name);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }

    [SerializeField]
    Module room;

    [SerializeField]
    Module bridge;

    [SerializeField]
    Module stairs;

    [SerializeField]
    Module empty;

    public Module RoomModule()
    {
        return Instantiate(room);
    }

    public Module BridgeModule()
    {
        return Instantiate(bridge);
    }

    public Module StairsModule()
    {
        return Instantiate(stairs);
    }

    public Module EmptyModule()
    {
        return Instantiate(empty);
    }
}
