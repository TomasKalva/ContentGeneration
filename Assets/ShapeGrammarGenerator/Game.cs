using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Assets.Util;
using ContentGeneration.Assets.UI.Util;
using ContentGeneration.Assets.UI.Model;
using ContentGeneration.Assets.UI;

namespace ShapeGrammar
{

    public class Game : MonoBehaviour
    {
        [SerializeField]
        Transform worldParent;

        [SerializeField]
        GeometricPrimitives GeometricPrimitives;

        [SerializeField]
        protected Libraries libraries;

        MyLanguage GameLanguage;

        World World { get; set; }

        private void Awake()
        {
            libraries.Initialize();
            InitializePlayer();
            InitializeLevelConstructor();
            GoToNextLevel();
        }

        public void InitializePlayer()
        {
            if (Application.isEditor)
            {
                UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));
            }
            //world.AddEnemy(libraries.Enemies.MayanSwordsman(), new Vector3(0, 1, 0));
            //world.AddEnemy(libraries.Enemies.DragonMan(), new Vector3(0, 1, 0));
            //UnityEngine.Random.InitState(42);

            // todo: make this initialization less annoying
            var playerState = new ContentGeneration.Assets.UI.Model.PlayerCharacterState();
            playerState.Spirit = 50;
            var stats = new ContentGeneration.Assets.UI.Model.CharacterStats()
            {
                Will = 5,
                Strength = 5,
                Endurance = 5,
                Agility = 5,
                Posture = 5,
                Resistances = 5,
                Versatility = 5
            };
            playerState.Stats = stats;

            playerState.Inventory.LeftWeapon.Item = libraries.Items.MayanKnife();
            playerState.Inventory.RightWeapon.Item = libraries.Items.Katana();
            playerState.AddAndEquipItem(libraries.Items.FreeWill());
            playerState.AddAndEquipItem(libraries.Items.MayanKnife());

            GameViewModel.ViewModel.PlayerState = playerState;
        }

        public void InitializeLevelConstructor()
        {
            var playerState = GameViewModel.ViewModel.PlayerState;

            World = new World(
                new WorldGeometry(worldParent, 2.8f), 
                playerState
                );



            var ldk = new LevelDevelopmentKit(GeometricPrimitives, worldParent, libraries);

            // Declaration
            {
                ShapeGrammarState grammarState = new ShapeGrammarState(ldk);
                {
                    var levelConstructor = new LevelConstructor();
                    var languageState = new LanguageState(levelConstructor, ldk);
                    languageState.Restart(World, grammarState);
                    var gr = new Grammars(ldk);
                    var sym = new Symbols();
                    ProductionProgram.pr = new Productions(ldk, sym);
                    ProductionProgram.ldk = ldk;

                    GameLanguage = new MyLanguage(new LanguageParams(libraries, gr, languageState));


                    GameLanguage.MyWorldStart();
                }

            }
        }

        void PutPlayerToWorld(PlayerCharacterState playerState, LevelElement entireLevel)
        {
            // Stuff related to player initialization
            {
                // enable disabling enemies in distance
                var spacePartitioning = new SpacePartitioning(GameLanguage.State.TraversabilityGraph);
                playerState.OnUpdate = () =>
                {
                    var playerGridPosition = Vector3Int.RoundToInt(World.WorldGeometry.WorldToGrid(GameViewModel.ViewModel.PlayerState.Agent.transform.position));
                    var playerNode = GameLanguage.State.GrammarState.GetNode(playerGridPosition);
                    spacePartitioning.Update(playerNode);
                };



                Debug.Log(entireLevel.Print(0));

                var goodCubes = entireLevel.CG().WithFloor().Cubes
                    .Where(cube => cube.NeighborsHor().All(neighbor => neighbor.FacesVer(Vector3Int.down).FaceType == FACE_VER.Floor));
                var goodGraveCube = goodCubes.ElementAt(0);
                var graveState = libraries.InteractiveObjects.Grave();
                var grave = graveState.MakeGeometry();
                grave.transform.position = World.WorldGeometry.GridToWorld(goodGraveCube.Position);
                World.AddInteractiveObject(graveState);
                World.Grave = graveState;

                World.InitializePlayer();
            }
        }

        public void GoToNextLevel()
        {
            // Restart game language
            var ldk = GameLanguage.State.Ldk;
            var playerState = GameViewModel.ViewModel.PlayerState;

            World = new World(
                new WorldGeometry(worldParent, 2.8f),
                playerState
                );
            var grammarState = new ShapeGrammarState(ldk);

            GameLanguage.State.Restart(World, grammarState);
            GameViewModel.ViewModel.World = World;


            // Generating the world

            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            GameLanguage.State.LC.Construct();

            ldk.grid.Generate(World.WorldGeometry.WorldScale, World);

            GameLanguage.Instantiate();





            stopwatch.Stop();
            Debug.Log(stopwatch.ElapsedMilliseconds);

            var levelRoot = grammarState.WorldState.Added;
            PutPlayerToWorld(playerState, levelRoot);

            grammarState.Print(new PrintingState()).Show();
            grammarState.Stats.Print();


            Debug.Log("Generating world");

        }

        private void FixedUpdate()
        {
            World.Update(Time.fixedDeltaTime);
        }
    }
}