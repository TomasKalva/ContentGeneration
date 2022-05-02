using ContentGeneration.Assets.UI.Model;
using ShapeGrammar;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Enemies : ScriptableObject
{
#if UNITY_EDITOR
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
#endif

    [SerializeField]
    Libraries libraries;

    public HumanAgent humanPrefab;
    public SculptureAgent sculpturePrefab;
    public BlobAgent blob;
    public MayanAgent mayanPrefab;
    public SkinnyWomanAgent skinnyWomanPrefab;
    public DragonManAgent dragonManPrefab;
    public DogAgent dogPrefab;

    public IEnumerable<Func<Agent>> AllAgents () => new List<Func<Agent>>()
    {
        Human,
        Sculpture,
        MayanThrower,
        MayanSwordsman,
        SkinnyWoman,
        DragonMan,
        Dog,
    };

    void AddDefaultBehaviors(Behaviors behaviors)
    {
        behaviors.AddBehavior(new TurnToTargetBehavior(10));
        behaviors.AddBehavior(new GoToTargetBehavior(10));
        behaviors.AddBehavior(new WaitForPlayer(10));
        behaviors.AddBehavior(new Awareness(10, new Vector2(3.0f, 5.0f), 5f, 15f));
    }

    public Agent Human()
    {
        var human = Instantiate(humanPrefab);
        var character = human.CharacterState;

        // properties
        character.Health = 40f;
        character.Will = 20f;
        character.Posture = 10f;

        // inventory
        character.SetItemToSlot(SlotType.Active, new FreeWill());
        character.SetItemToSlot(SlotType.LeftWeapon, libraries.Items.MayanKnife());
        character.SetItemToSlot(SlotType.RightWeapon, libraries.Items.Scythe());

        // behaviors
        var behaviors = human.Behaviors;
        var controller = human.GetComponent<HumanController>();

        AddDefaultBehaviors(behaviors);

        behaviors.AddBehavior(new DetectorBehavior(human.Attack, controller.attackArea));

        human.acting.MyReset();

        return human;
    }

    public Agent Sculpture()
    {
        var sculpture = Instantiate(sculpturePrefab);
        var character = sculpture.CharacterState;

        // properties
        character.Health = 40f;
        character.Will = 20f;
        character.Posture = 10f;

        // inventory
        character.SetItemToSlot(SlotType.Active, new FreeWill());
        character.SetItemToSlot(SlotType.LeftWeapon, libraries.Items.SculptureClub());
        character.SetItemToSlot(SlotType.RightWeapon, libraries.Items.SculptureClub());

        // behaviors
        var behaviors = sculpture.Behaviors;
        var controller = sculpture.GetComponent<SculptureController>();

        AddDefaultBehaviors(behaviors);

        behaviors.AddBehavior(new DetectorBehavior(sculpture.WideAttack, controller.leftWideDetector, controller.rightWideDownDetector));
        behaviors.AddBehavior(new DetectorBehavior(sculpture.OverheadAttack, controller.overheadDetector));
        behaviors.AddBehavior(new DetectorBehavior(sculpture.DoubleSwipe, controller.doubleSwipeLeftDetector, controller.doubleSwipeRightDetector));
        behaviors.AddBehavior(new DetectorBehavior(sculpture.GroundSlam, controller.groundSlamDetector));

        sculpture.acting.MyReset();

        return sculpture;
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
        character.SetItemToSlot(SlotType.RightWeapon, libraries.Items.MayanKnife());

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
        character.Health = 40f;
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

    public Agent Dog()
    {
        var dog = Instantiate(dogPrefab);
        var character = dog.CharacterState;

        // properties
        character.Health = 50f;
        character.Will = 20f;
        character.Posture = 10f;

        // inventory
        character.SetItemToSlot(SlotType.Active, new FreeWill());
        character.SetItemToSlot(SlotType.LeftWeapon, libraries.Items.MayanSword());
        //character.SetItemToSlot(SlotType.RightWeapon, libraries.Items.MayanSword());

        // behaviors
        var behaviors = dog.Behaviors;
        var controller = dog.GetComponent<DogController>();

        AddDefaultBehaviors(behaviors);

        behaviors.AddBehavior(new DetectorBehavior(dog.DashForward, controller.dashForwardDetector));
        behaviors.AddBehavior(new DetectorBehavior(dog.LeftSlash, controller.slashDetector));
        behaviors.AddBehavior(new DetectorBehavior(dog.RightSlash, controller.slashDetector));

        dog.acting.MyReset();

        return dog;
    }

    public GeometryMaker<AgentT> Geometry<AgentT>(AgentT prefab) where AgentT : Agent
    {
        return new GeometryMaker<AgentT>(
            () =>
            {
                var newObj = Instantiate(prefab);
                var comp = newObj.GetComponentInChildren<AgentT>();
                if (comp == null)
                {
                    throw new InvalidOperationException("The object doesn't have Agent component");
                }
                return comp;
            }
        );
    }
}
