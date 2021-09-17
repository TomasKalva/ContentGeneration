using ContentGeneration.Assets.UI.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Enemies : ScriptableObject
{
    [MenuItem("Assets/Create/Enemies")]
    public static void CreateMyAsset()
    {
        Enemies asset = ScriptableObject.CreateInstance<Enemies>();

        string name = UnityEditor.AssetDatabase.GenerateUniqueAssetPath("Assets/NewScripableObject.asset");
        AssetDatabase.CreateAsset(asset, name);
        AssetDatabase.SaveAssets();

        EditorUtility.FocusProjectWindow();

        Selection.activeObject = asset;
    }

    [SerializeField]
    Libraries libraries;

    public HumanAgent human;
    public SculptureAgent sculpture;
    public BlobAgent blob;
    public MayanAgent mayanPrefab;

    void AddDefaultBehaviors(Behaviors behaviors)
    {
        behaviors.AddBehavior(new TurnToTargetBehavior(10));
        behaviors.AddBehavior(new GoToTargetBehavior(10));
        behaviors.AddBehavior(new WaitForPlayer(10));
        behaviors.AddBehavior(new Awareness(10, new Vector2(3.0f, 5.0f), 5f, 15f));
    }

    public Agent MayanThrower()
    {
        var mayan = Instantiate(mayanPrefab);
        var character = mayan.CharacterState;

        // properties
        character.Health = 100f;

        // inventory
        character.SetItemToSlot(SlotType.Active, new FreeWill());
        character.SetItemToSlot(SlotType.LeftWeapon, libraries.Items.MayanSword());
        character.SetItemToSlot(SlotType.RightWeapon, libraries.Items.MayanKnife());

        // behaviors
        var behaviors = mayan.Behaviors;
        var controller = mayan.GetComponent<MayanController>();

        AddDefaultBehaviors(behaviors);

        behaviors.AddBehavior(new DetectorBehavior(mayan.OverheadAttack, controller.overheadDetector));
        behaviors.AddBehavior(new DetectorBehavior(mayan.Throw, controller.throwDetector));
        behaviors.AddBehavior(new DetectorBehavior(mayan.LeftSwing, controller.swingDetector));

        mayan.acting.MyReset();

        return mayan;
    }

    public Agent MayanSwordsman()
    {
        var mayan = Instantiate(mayanPrefab);
        var character = mayan.CharacterState;

        // properties
        character.Health = 100f;

        // inventory
        character.SetItemToSlot(SlotType.Active, new FreeWill());
        character.SetItemToSlot(SlotType.LeftWeapon, libraries.Items.MayanSword());
        character.SetItemToSlot(SlotType.RightWeapon, libraries.Items.MayanSword());

        // behaviors
        var behaviors = mayan.Behaviors;
        var controller = mayan.GetComponent<MayanController>();

        AddDefaultBehaviors(behaviors);

        behaviors.AddBehavior(new DetectorBehavior(mayan.LongOverheadAttack, controller.overheadDetector));
        behaviors.AddBehavior(new DetectorBehavior(mayan.LeftSwing, controller.swingDetector));
        behaviors.AddBehavior(new DetectorBehavior(mayan.RightSwing, controller.swingDetector));

        mayan.acting.MyReset();

        return mayan;
    }
}
