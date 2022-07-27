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
        ShapeGrammarObjectStyle DefaultHouseStyle;

        [SerializeField]
        ShapeGrammarObjectStyle GardenStyle;

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
            /*if (Application.isEditor)
            {
                UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));
            }*/
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

            GameViewModel.ViewModel.PlayerState = playerState;
        }

        public void InitializeLevelConstructor()
        {
            var playerState = GameViewModel.ViewModel.PlayerState;

            var worldScale = 2.8f;
            var worldGeometry = new WorldGeometry(worldParent, worldScale);
            World = new World(worldGeometry, playerState);



            var ldk = new LevelDevelopmentKit(DefaultHouseStyle, GardenStyle, worldParent, libraries);

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
                    ProductionProgram.StyleRules = ldk.houseStyleRules;

                    GameLanguage = new MyLanguage(new LanguageParams(libraries, gr, languageState));


                    GameLanguage.MyWorldStart();
                }

            }
        }

        public void GoToNextLevel()
        {
            var ldk = GameLanguage.State.Ldk;
            var playerState = GameViewModel.ViewModel.PlayerState;
            var worldScale = 2.8f;
            var worldGeometry = new WorldGeometry(worldParent, worldScale);
            World = new World(worldGeometry, playerState);
            var grammarState = new ShapeGrammarState(ldk);

            GameLanguage.State.Restart(World, grammarState);
            GameViewModel.ViewModel.World = World;

            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            GameLanguage.State.LC.Construct();

            ldk.grid.Generate(worldScale, World);

            GameLanguage.Instantiate();



            // enable disabling enemies in distance
            var spacePartitioning = new SpacePartitioning(GameLanguage.State.TraversabilityGraph);
            playerState.OnUpdate = () =>
            {
                var playerGridPosition = Vector3Int.RoundToInt(GameLanguage.State.Ldk.wg.WorldToGrid(GameViewModel.ViewModel.PlayerState.Agent.transform.position));
                var playerNode = GameLanguage.State.GrammarState.GetNode(playerGridPosition);
                spacePartitioning.Update(playerNode);
            };

            grammarState.Print(new PrintingState()).Show();
            grammarState.Stats.Print();

            var levelRoot = grammarState.WorldState.Added;


            stopwatch.Stop();
            Debug.Log(stopwatch.ElapsedMilliseconds);

            Debug.Log(levelRoot.Print(0));

            Debug.Log("Generating world");


            /*
            var elevator = libraries.InteractiveObjects.Elevator(1 * worldScale, false);
            world.AddObject(elevator.Object, Vector3.zero);
            */

            var goodCubes = levelRoot.CG().WithFloor().Cubes
                .Where(cube => cube.NeighborsHor().All(neighbor => neighbor.FacesVer(Vector3Int.down).FaceType == FACE_VER.Floor));
            var goodGraveCube = goodCubes.ElementAt(0);
            var graveState = libraries.InteractiveObjects.Grave();
            var grave = graveState.MakeGeometry();
            grave.transform.position = worldGeometry.GridToWorld(goodGraveCube.Position);
            World.AddInteractiveObject(graveState);
            World.Grave = graveState;

            //var goodTransporterCube = goodCubes.ElementAt(1);
            //world.AddInteractiveObject(interactiveObjects.Transporter().MakeGeometry(), GridToWorld(goodTransporterCube.Position));

            /*
            var allEnemies = libraries.Enemies.AllAgents();
            var enemyCubes = levelRoot.CG().WithFloor().Cubes.Shuffle().Take(10);
            enemyCubes.ForEach(cube => world.AddEnemy(allEnemies.GetRandom()().MakeGeometry(), GridToWorld(cube.Position)));
            
            
            var itemCubes = levelRoot.CG().WithFloor().Cubes.Shuffle().Take(10);
            itemCubes.ForEach(cube => world.AddItem(libraries.InteractiveObjects.Item(libraries.Items.Scythe()).MakeGeometry(), GridToWorld(cube.Position)));
            */

            /*
            var kilnCube = goodGraveCube.NeighborsHor().GetRandom();
            world.AddInteractiveObject(interactiveObjects.AscensionKiln().MakeGeometry(), GridToWorld(kilnCube.Position));
            */


            World.Created();
        }

        private void FixedUpdate()
        {
            World.Update(Time.fixedDeltaTime);
        }
    }
}