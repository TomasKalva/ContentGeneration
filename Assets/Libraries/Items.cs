using ContentGeneration.Assets.UI.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Items : ScriptableObject
{
#if UNITY_EDITOR
    [MenuItem("Assets/Create/Items")]
    public static void CreateMyAsset()
    {
        Items asset = ScriptableObject.CreateInstance<Items>();

        string name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/Items.asset");
        AssetDatabase.CreateAsset(asset, name);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }
#endif

    [SerializeField]
    Weapons weapons;

    [SerializeField]
    InteractiveObject physicalItemPrefab;

    public ItemState BlueIchorEssence() => new BlueIchorEssence();
    public ItemState RedIchorEssence() => new RedIchorEssence();
    public ItemState FreeWill() => new FreeWill();

    public ItemState SculptureClub() => new WeaponItem("Sculpture Club", "Made of idk stone", weapons.SculptureClub().transform);
    public ItemState MayanKnife() => new WeaponItem("Mayan Knife", "Every Mayan has one", weapons.MayanKnife().transform);
    //public ItemState Fireball() => new WeaponItem("Fireball", "It's a fireball", weapons.Fireball().transform);
    public ItemState MayanSword() => new WeaponItem("Mayan Sword", "Like a knife but bigger", weapons.MayanSword().transform);
    public ItemState Scythe() => new WeaponItem("Scythe", "Harvesting tool", weapons.Scythe().transform);
    public ItemState Mace() => new WeaponItem("Mace", "Mace", weapons.Mace().transform);
    public ItemState Katana() => new WeaponItem("Katana", "Katana", weapons.Katana().transform);

    public InteractiveObject Physical(ItemState itemState)
    {
        var physicalItem = Instantiate(physicalItemPrefab);
        var physicalItemState = new PhysicalItemState() 
        { 
            Name = itemState.Name, 
            MessageOnInteract = "Item picked up", 
            InteractionDescription = "Pick up item", 
            InteractiveObject = physicalItem 
        };
        physicalItemState.Item = itemState;
        return physicalItem;
    }
}
