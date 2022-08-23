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

    public SculptureAgent sculpturePrefab;
    public MayanAgent mayanPrefab;
    public SkinnyWomanAgent skinnyWomanPrefab;
    public DragonManAgent dragonManPrefab;
    public DogAgent dogPrefab;

    public IEnumerable<Func<CharacterState>> AllAgents () => new List<Func<CharacterState>>()
    {
        //Sculpture,
        MayanThrower,
        //MayanSwordsman,
        //SkinnyWoman,
        /*DragonMan,
        Dog,*/
    };



    void AddDefaultBehaviors(Behaviors behaviors)
    {
        behaviors.AddBehavior(new TurnToTargetBehavior(10));
        behaviors.AddBehavior(new GoToTargetBehavior(10));
        //behaviors.AddBehavior(new WaitForPlayer(10));
        behaviors.AddBehavior(new Awareness(10, new Vector2(3.0f, 5.0f), 5f, 10f));
    }

    /*
    public CharacterState Human()
    {
        var human = new CharacterState();
        human.GeometryMaker = Geometry<Agent>(humanPrefab);

        // properties
        human.Health = 40f;
        human.Stamina = 20f;
        human.Poise = 10f;

        // inventory
        human.AddAndEquipItem(new FreeWill());
        human.Inventory.LeftWeapon.Item = libraries.Items.MayanKnife();
        human.Inventory.RightWeapon.Item = libraries.Items.Scythe();

        // behaviors
        var behaviors = human.Behaviors;
        AddDefaultBehaviors(behaviors);

        return human;
    }*/
    
    public CharacterState Sculpture()
    {
        var sculpture = new CharacterState();
        sculpture.GeometryMaker = Geometry<Agent>(sculpturePrefab);

        // properties
        sculpture.Health = 40f;
        sculpture.Stamina = 20f;
        sculpture.Poise = 10f;

        // inventory
        sculpture.AddAndEquipItem( new FreeWill());
        sculpture.Inventory.LeftWeapon.Item = libraries.Items.SculptureClub();
        sculpture.Inventory.RightWeapon.Item = libraries.Items.SculptureClub();

        // behaviors
        var behaviors = sculpture.Behaviors;

        AddDefaultBehaviors(behaviors);

        return sculpture;
    }
    
    public CharacterState MayanThrower()
    {
        var mayan = new CharacterState();
        mayan.GeometryMaker = Geometry<Agent>(mayanPrefab);

        // properties
        mayan.Health = 40f;
        mayan.Stamina = 20f;
        mayan.Poise = 10f;

        // inventory
        mayan.AddAndEquipItem(new FreeWill());
        mayan.Inventory.LeftWeapon.Item = libraries.Items.MayanSword();
        mayan.Inventory.RightWeapon.Item = libraries.Items.MayanKnife();

        // behaviors
        var behaviors = mayan.Behaviors;

        AddDefaultBehaviors(behaviors);

        return mayan;
    }
    
    /// <summary>
    /// Is the same as mayan thrower for now.
    /// </summary>
    public CharacterState MayanSwordsman()
    {
        var mayan = new CharacterState();
        mayan.GeometryMaker = Geometry<Agent>(mayanPrefab);

        // properties
        mayan.Health = 50f;
        mayan.Stamina = 20f;
        mayan.Poise = 10f;

        // inventory
        mayan.AddAndEquipItem(new FreeWill());
        mayan.Inventory.LeftWeapon.Item = libraries.Items.MayanSword();
        mayan.Inventory.RightWeapon.Item = libraries.Items.MayanKnife();

        // behaviors
        var behaviors = mayan.Behaviors;

        AddDefaultBehaviors(behaviors);
        /*
         * todo: define behavior definition strategy, that will allow creating arbitrary links between controllers and behavior
        behaviors.AddBehavior(new DetectorBehavior(mayan.LongOverheadAttack, controller.overheadDetector));
        behaviors.AddBehavior(new DetectorBehavior(mayan.LeftSwing, controller.swingDetector));
        behaviors.AddBehavior(new DetectorBehavior(mayan.RightSwing, controller.swingDetector));
        */

        return mayan;
    }
    
    public CharacterState SkinnyWoman()
    {
        var skinnyWoman = new CharacterState();
        skinnyWoman.GeometryMaker = Geometry<Agent>(skinnyWomanPrefab);

        // properties
        skinnyWoman.Health = 50f;
        skinnyWoman.Stamina = 20f;
        skinnyWoman.Poise = 10f;

        // inventory
        skinnyWoman.AddAndEquipItem(new FreeWill());
        skinnyWoman.Inventory.LeftWeapon.Item = libraries.Items.MayanSword();
        skinnyWoman.Inventory.RightWeapon.Item = libraries.Items.MayanSword();

        // behaviors
        var behaviors = skinnyWoman.Behaviors;

        AddDefaultBehaviors(behaviors);

        return skinnyWoman;
    }
    
    public CharacterState DragonMan()
    {
        var dragonMan = new CharacterState();
        dragonMan.GeometryMaker = Geometry<Agent>(dragonManPrefab);

        // properties
        dragonMan.Health = 40f;
        dragonMan.Stamina = 20f;
        dragonMan.Poise = 10f;

        // inventory
        dragonMan.AddAndEquipItem(new FreeWill());
        dragonMan.Inventory.LeftWeapon.Item = libraries.Items.MayanSword();
        dragonMan.Inventory.RightWeapon.Item = libraries.Items.MayanSword();

        // behaviors
        var behaviors = dragonMan.Behaviors;

        AddDefaultBehaviors(behaviors);

        return dragonMan;
    }
    
    public CharacterState Dog()
    {
        var dog = new CharacterState();
        dog.GeometryMaker = Geometry<Agent>(dogPrefab);

        // properties
        dog.Health = 50f;
        dog.Stamina = 20f;
        dog.Poise = 10f;

        // inventory
        dog.AddAndEquipItem(new FreeWill());
        dog.Inventory.LeftWeapon.Item = libraries.Items.MayanSword();
        //character.SetItemToSlot(SlotType.RightWeapon, libraries.Items.MayanSword());

        // behaviors
        var behaviors = dog.Behaviors;

        AddDefaultBehaviors(behaviors);

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
