
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Assets.Util;
using ContentGeneration.Assets.UI.Util;
using ContentGeneration.Assets.UI.Model;
using ContentGeneration.Assets.UI;
using System.Diagnostics;
using Debug = UnityEngine.Debug;
using static ShapeGrammar.AsynchronousEvaluator;

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

        AsynchronousEvaluator AsyncEvaluator;

        private void Awake()
        {
            libraries.Initialize();
            InitializePlayer();
            InitializeLevelConstructor();
            GoToNextLevel();

            AsyncEvaluator = new AsynchronousEvaluator(TaskSteps.Multiple(AsynchronousEvaluator.TestMethod()), 2);
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
                Will = 0,
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
            libraries.Items.AllWristItems().ForEach(wi => playerState.AddItem(wi()));
            playerState.Inventory.Head.Item = libraries.Items.Eggs();

            libraries.Items.AllHeartItems().ForEach(hi => playerState.AddItem(hi()));
            //libraries.Items.AllHeadItems().ForEach(wi => playerState.AddItem(wi()));

            GameViewModel.ViewModel.PlayerState = playerState;
        }

        public World CreateWorld()
        {
            var playerState = GameViewModel.ViewModel.PlayerState;

            return new World(
                new WorldGeometry(worldParent, 2.8f),
                playerState
                );
        }

        public void InitializeLevelConstructor()
        {
            var ldk = new LevelDevelopmentKit(GeometricPrimitives, worldParent, libraries);

            // Declaration
            {
                //ShapeGrammarState grammarState = new ShapeGrammarState(ldk);
                {
                    var levelConstructor = new LevelConstructor();
                    var languageState = new LanguageState(levelConstructor, ldk, CreateWorld);
                    languageState.Restart();
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
                var world = GameLanguage.State.World;
                var spacePartitioning = new SpacePartitioning(GameLanguage.State.TraversabilityGraph);
                playerState.OnUpdate = () =>
                {
                    var playerGridPosition = Vector3Int.RoundToInt(world.WorldGeometry.WorldToGrid(GameViewModel.ViewModel.PlayerState.Agent.transform.position));
                    var playerNode = GameLanguage.State.GrammarState.GetNode(playerGridPosition);
                    spacePartitioning.Update(playerNode);
                };



                Debug.Log(entireLevel.Print(0));

                var goodCubes = entireLevel.CG().WithFloor().Cubes
                    .Where(cube => cube.NeighborsHor().All(neighbor => neighbor.FacesVer(Vector3Int.down).FaceType == FACE_VER.Floor));
                var goodGraveCube = goodCubes.ElementAt(0);
                var graveState = libraries.InteractiveObjects.Grave();
                var grave = graveState.MakeGeometry();
                grave.transform.position = world.WorldGeometry.GridToWorld(goodGraveCube.Position);
                world.AddInteractiveObject(graveState);
                world.Grave = graveState;

                world.InitializePlayer();
            }
        }

        public void GoToNextLevel()
        {
            // Generating the world

            // show transition animation

            GameLanguage.GenerateWorld();

            GameViewModel.ViewModel.World = GameLanguage.State.World;

            var grammarState = GameLanguage.State.GrammarState;

            grammarState.Print(new PrintingState()).Show();
            grammarState.Stats.Print();

            // Put player to the world

            var playerState = GameViewModel.ViewModel.PlayerState;
            var levelRoot = grammarState.WorldState.Added;
            PutPlayerToWorld(playerState, levelRoot);

            // Restart level after player dies
            playerState
                .ClearOnDeath()
                .AddOnDeath(() =>
                {
                    GameLanguage.State.World.Reset();
                    GameLanguage.State.InstantiateAreas();
                    PutPlayerToWorld(playerState, levelRoot);
                })
                .ClearOnRest()
                .AddOnRest(() =>
                {
                    GameLanguage.State.World.Reset();
                    GameLanguage.State.InstantiateAreas();
                    PutPlayerToWorld(playerState, levelRoot);
                });

            // hide transition animation
        }

        private void FixedUpdate()
        {
            var World = GameLanguage.State.World;
            World.Update(Time.fixedDeltaTime);
            AsyncEvaluator.Evaluate();
        }
    }

    /// <summary>
    /// Takes function and evaluates it for fixed amount of time per each frame.
    /// The function has to return IEnumerable<TaskStep> which marks pause points in the algorithm.
    /// </summary>
    public class AsynchronousEvaluator
    {
        public static IEnumerable<TaskSteps> TestMethod()
        {
            Debug.Log("Starting TestMethod");
            yield return TaskSteps.One();
            Debug.Log("Done first step");
            yield return TaskSteps.One();
            yield return TaskSteps.Multiple(TestMethod2());
            Debug.Log("Finished");
            yield return TaskSteps.One();
            Debug.Log("Is this written?");
        }

        public static IEnumerable<TaskSteps> TestMethod2()
        {
            Debug.Log("Starting TestMethod2");
            yield return TaskSteps.One();
            for (int i = 0; i < 10_000; i++)
            {
                int x = 0;
                for(int j = 0; j < 1000; j++)
                {
                    x = 2 * x + 3;
                }
                Debug.Log($"Iteration {i}, x is {x}");
                yield return TaskSteps.One();
            }
            yield return TaskSteps.Multiple(TestMethod3());
        }

        public static IEnumerable<TaskSteps> TestMethod3()
        {
            Debug.Log("Starting TestMethod3");
            yield return TaskSteps.One();
            Debug.Log("Done first step");
            yield return TaskSteps.One();
        }

        /// <summary>
        /// Yield returned by the evaluating function.
        /// </summary>
        public class TaskSteps 
        {
            public static TaskSteps One() => new TaskSteps(Enumerable.Empty<TaskSteps>());
            public static TaskSteps Multiple(IEnumerable<TaskSteps> subtasks) => new TaskSteps(subtasks);

            IEnumerable<TaskSteps> Subtasks { get; }

            TaskSteps(IEnumerable<TaskSteps> subtasks)
            {
                Subtasks = subtasks;
            }

            public IEnumerable<TaskSteps> GetTasks()
            {
                yield return this;
                foreach(var subtask in Subtasks.SelectMany(st => st.GetTasks()))
                {
                    yield return subtask;
                }
            }
        }

        IEnumerator<TaskSteps> _taskQueue;
        Stopwatch _stopwatch;
        int _msPerFrame;

        public AsynchronousEvaluator(TaskSteps taskSteps, int msPerFrame)
        {
            _taskQueue = taskSteps.GetTasks().GetEnumerator();
            _stopwatch = new Stopwatch();
            _msPerFrame = msPerFrame;
        }

        /// <summary>
        /// Returns true if finished.
        /// </summary>
        public bool Evaluate()
        {
            _stopwatch.Restart();
            while(_stopwatch.ElapsedMilliseconds < _msPerFrame)
            {
                if (!_taskQueue.MoveNext())
                {
                    return true;
                }
            }
            return false;
        }
    }
}