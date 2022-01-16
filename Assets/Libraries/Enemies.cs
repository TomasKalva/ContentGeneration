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
    public SkinnyWomanAgent skinnyWomanPrefab;
    public DragonManAgent dragonManPrefab;

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
        character.Health = 40f;
        character.Will = 20f;
        character.Posture = 10f;

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
        character.Health = 50f;
        character.Will = 20f;
        character.Posture = 10f;

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

    public Agent SkinnyWoman()
    {
        var skinnyWoman = Instantiate(skinnyWomanPrefab);
        var character = skinnyWoman.CharacterState;

        // properties
        character.Health = 50f;
        character.Will = 20f;
        character.Posture = 10f;

        // inventory
        character.SetItemToSlot(SlotType.Active, new FreeWill());
        character.SetItemToSlot(SlotType.LeftWeapon, libraries.Items.MayanSword());
        character.SetItemToSlot(SlotType.RightWeapon, libraries.Items.MayanSword());

        // behaviors
        var behaviors = skinnyWoman.Behaviors;
        var controller = skinnyWoman.GetComponent<SkinnyWomanController>();

        AddDefaultBehaviors(behaviors);

        behaviors.AddBehavior(new DetectorBehavior(skinnyWoman.RushForward, controller.rushForwardDetector));
        behaviors.AddBehavior(new DetectorBehavior(skinnyWoman.CastFireball, controller.castDetector));

        skinnyWoman.acting.MyReset();

        return skinnyWoman;
    }

    public Agent DragonMan()
    {
        var dragonMan = Instantiate(dragonManPrefab);
        var character = dragonMan.CharacterState;

        // properties
        character.Health = 50f;
        character.Will = 20f;
        character.Posture = 10f;

        // inventory
        character.SetItemToSlot(SlotType.Active, new FreeWill());
        character.SetItemToSlot(SlotType.LeftWeapon, libraries.Items.MayanSword());
        character.SetItemToSlot(SlotType.RightWeapon, libraries.Items.MayanSword());

        // behaviors
        var behaviors = dragonMan.Behaviors;
        var controller = dragonMan.GetComponent<DragonManController>();

        AddDefaultBehaviors(behaviors);

        behaviors.AddBehavior(new DetectorBehavior(dragonMan.Slash, controller.slashDetector));
        behaviors.AddBehavior(new DetectorBehavior(dragonMan.FlapWings, controller.castDetector));
        behaviors.AddBehavior(new DetectorBehavior(dragonMan.SpitFire, controller.spitFireDetector));

        dragonMan.acting.MyReset();

        return dragonMan;
    }
}
