using Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions;
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
    Libraries Lib;

    public SculptureAgent sculpturePrefab;
    public MayanAgent mayanPrefab;
    public SkinnyWomanAgent skinnyWomanPrefab;
    public DragonManAgent dragonManPrefab;
    public DogAgent dogPrefab;

    public IEnumerable<Func<CharacterState>> AllAgents () => new List<Func<CharacterState>>()
    {
        Sculpture,
        MayanThrower,
        MayanSwordsman,
        SkinnyWoman,
        DragonMan,
        Dog,
    };



    Behaviors AddDefaultBehaviors(Behaviors behaviors)
    {
        /*
        behaviors;
        behaviors;
        //behaviors.AddBehavior(new WaitForPlayer(10));
        behaviors.AddBehavior(new Awareness(10, new Vector2(3.0f, 5.0f), 5f, 10f));*/
        return behaviors
            .AddBehavior(new TurnToTargetBehavior(10))
            .AddBehavior(new GoToTargetBehavior(10))
            .AddBehavior(new Awareness(10, new Vector2(3.0f, 5.0f), 5f, 10f));
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
        sculpture.GeometryMaker = AgentGeometry(sculpturePrefab,
                 (agent, behaviors) => 
                    AddDefaultBehaviors(behaviors)
                        .AddBehavior(new DetectorBehavior(agent.WideAttack, agent.leftWideDetector, agent.rightWideDownDetector))
                        .AddBehavior(new DetectorBehavior(agent.OverheadAttack, agent.overheadDetector))
                        .AddBehavior(new DetectorBehavior(agent.DoubleSwipe, agent.doubleSwipeLeftDetector, agent.doubleSwipeRightDetector))
                        .AddBehavior(new DetectorBehavior(agent.GroundSlam, agent.groundSlamDetector)));
        
        // properties
        sculpture.Health = 40f;
        sculpture.Stamina = 20f;
        sculpture.Poise = 10f;

        // inventory
        sculpture.AddAndEquipItem( new FreeWill());
        sculpture.Inventory.LeftWeapon.Item = Lib.Items.SculptureClub();
        sculpture.Inventory.RightWeapon.Item = Lib.Items.SculptureClub();

        return sculpture;
    }
    
    public CharacterState MayanThrower()
    {
        var mayan = new CharacterState();
        mayan.GeometryMaker = AgentGeometry(mayanPrefab,
            (agent, behaviors) => 
                AddDefaultBehaviors(behaviors)
                    .AddBehavior(new DetectorBehavior(agent.LeftSwing, agent.swingDetector))

                    .AddBehavior(new DetectorBehavior(() => 
                        agent.Throw(Lib.Spells.Bolt(Lib.VFXs.Lightning, Color.yellow, Lib.VFXs.LightningTexture, 1.0f, 10f, new DamageDealt(DamageType.Chaos, 10f))), 
                        agent.throwDetector)
                    )
                );

        // properties
        mayan.Health = 40f;
        mayan.Stamina = 20f;
        mayan.Poise = 10f;

        // inventory
        mayan.AddAndEquipItem(new FreeWill());
        mayan.Inventory.LeftWeapon.Item = Lib.Items.MayanSword();
        mayan.Inventory.RightWeapon.Item = Lib.Items.MayanKnife();

        return mayan;
    }
    
    /// <summary>
    /// Is the same as mayan thrower for now.
    /// </summary>
    public CharacterState MayanSwordsman()
    {
        var mayan = new CharacterState();
        mayan.GeometryMaker = AgentGeometry(mayanPrefab,
            (agent, behaviors) => 
                AddDefaultBehaviors(behaviors)
                    .AddBehavior(new DetectorBehavior(agent.OverheadAttack, agent.overheadDetector))
                    .AddBehavior(new DetectorBehavior(agent.LeftSwing, agent.swingDetector)));
        
        // properties
        mayan.Health = 50f;
        mayan.Stamina = 20f;
        mayan.Poise = 10f;

        // inventory
        mayan.AddAndEquipItem(new FreeWill());
        mayan.Inventory.LeftWeapon.Item = Lib.Items.MayanSword();
        mayan.Inventory.RightWeapon.Item = Lib.Items.MayanKnife();

        return mayan;
    }
    
    public CharacterState SkinnyWoman()
    {
        var skinnyWoman = new CharacterState();
        skinnyWoman.GeometryMaker = AgentGeometry(skinnyWomanPrefab,
            (agent, behaviors) => 
                AddDefaultBehaviors(behaviors)
                    .AddBehavior(new DetectorBehavior(agent.RushForward, agent.rushForwardDetector))
                    .AddBehavior(new DetectorBehavior(() => 
                        agent.CastFireball(
                            Lib.Spells.Bolt(Lib.VFXs.Fireball, Color.yellow, Lib.VFXs.FireTexture, 0.5f, 10f, new DamageDealt(DamageType.Chaos, 10f), false)), 
                        agent.castDetector))
                );

        // properties
        skinnyWoman.Health = 50f;
        skinnyWoman.Stamina = 20f;
        skinnyWoman.Poise = 10f;

        // inventory
        skinnyWoman.AddAndEquipItem(new FreeWill());
        skinnyWoman.Inventory.LeftWeapon.Item = Lib.Items.MayanSword();
        skinnyWoman.Inventory.RightWeapon.Item = Lib.Items.MayanSword();

        return skinnyWoman;
    }
    
    public CharacterState DragonMan()
    {
        var dragonMan = new CharacterState();
        dragonMan.GeometryMaker = AgentGeometry(dragonManPrefab,
            (agent, behaviors) => 
                AddDefaultBehaviors(behaviors)
                    .AddBehavior(new DetectorBehavior(agent.Slash, agent.slashDetector))

                    .AddBehavior(new DetectorBehavior(() => 
                        agent.FlapWings(
                            Lib.Spells.Cloud(Lib.VFXs.MovingCloud, Color.white, Lib.VFXs.WindTexture, 1.5f, 7f, 700f, new DamageDealt(DamageType.Divine, 2f))), 
                        agent.castDetector))

                    .AddBehavior(new DetectorBehavior(() =>
                        agent.SpitFire(
                            (position, direction) => user => user.World.CreateOccurence(
                                Lib.Selectors.GeometricSelector(Lib.VFXs.Fireball, 4f, Lib.Selectors.Initializator()
                                    .ConstPosition(position)
                                    .SetVelocity(user => direction, 6f)
                                    .RotatePitch(-90f)
                                    .Scale(1.5f)
                                    )(new SelectorArgs(Color.yellow, Lib.VFXs.FireTexture))(user),
                                Lib.Effects.Damage(new DamageDealt(DamageType.Chaos, 6f))
                            )
                        ), 
                        agent.spitFireDetector))
                    );
        
        // properties
        dragonMan.Health = 40f;
        dragonMan.Stamina = 20f;
        dragonMan.Poise = 10f;

        // inventory
        dragonMan.AddAndEquipItem(new FreeWill());
        dragonMan.Inventory.LeftWeapon.Item = Lib.Items.MayanSword();
        dragonMan.Inventory.RightWeapon.Item = Lib.Items.MayanSword();

        return dragonMan;
    }
    
    public CharacterState Dog()
    {
        var dog = new CharacterState();
        dog.GeometryMaker = AgentGeometry(dogPrefab,
            (agent, behaviors) => 
                AddDefaultBehaviors(behaviors)
                    .AddBehavior(new DetectorBehavior(agent.DashForward, agent.dashForwardDetector))
                    .AddBehavior(new DetectorBehavior(agent.LeftSlash, agent.slashDetector))
                    .AddBehavior(new DetectorBehavior(agent.RightSlash, agent.slashDetector)));

        // properties
        dog.Health = 50f;
        dog.Stamina = 20f;
        dog.Poise = 10f;

        // inventory
        dog.AddAndEquipItem(new FreeWill());
        dog.Inventory.LeftWeapon.Item = Lib.Items.MayanSword();

        return dog;
    }
    
    public GeometryMaker<AgentT> AgentGeometry<AgentT>(AgentT prefab, Action<AgentT, Behaviors> initializeBehaviors) where AgentT : Agent
    {
        return new GeometryMaker<AgentT>(
            () =>
            {
                var newObj = Instantiate(prefab);
                var agent = newObj.GetComponentInChildren<AgentT>();
                if (agent == null)
                {
                    throw new InvalidOperationException("The object doesn't have Agent component");
                }
                var behaviors = agent.Behaviors;
                initializeBehaviors(agent, behaviors);
                return agent;
            }
        );
    }
}
