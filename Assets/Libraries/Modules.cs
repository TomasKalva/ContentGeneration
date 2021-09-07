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

    Module RoomModule()
    {
        return Instantiate(room);
    }

    Module EmptyModule()
    {
        return Instantiate(empty);
    }

    public Module EmptyModule(Area area)
    {
        var emptyModule = RoomModule();
        emptyModule.Init();
        emptyModule.AddProperty(new AreaModuleProperty(area));
        var topologyProp = new TopologyProperty();
        topologyProp.SetAllReachable();
        topologyProp.SetAllDisconnected();
        emptyModule.AddProperty(topologyProp);
        return emptyModule;
    }

    public Module RoomModule(Area area)
    {
        var roomModule = RoomModule();
        roomModule.Init();
        roomModule.AddProperty(new AreaModuleProperty(area));
        var topologyProp = new TopologyProperty();
        topologyProp.SetAllReachable();
        topologyProp.SetAllConnected();
        roomModule.AddProperty(topologyProp);
        return roomModule;
    }
}
