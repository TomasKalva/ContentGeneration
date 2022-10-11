
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
using Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Modules;
using Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage;

namespace ShapeGrammar
{

    public sealed class Game : MonoBehaviour
    {
        [SerializeField]
        Transform worldParent;

        [SerializeField]
        GeometricPrimitives GeometricPrimitives;

        [SerializeField]
        LoadingScreen LoadingScreen;

        [SerializeField]
        Libraries libraries;

        MyLanguage GameLanguage;

        public AsynchronousEvaluator AsyncEvaluator { get; private set; }

        private void Awake()
        {
            AsyncEvaluator = new AsynchronousEvaluator(10);
            AsyncEvaluator.SetTasks(StartGame());
        }

        private IEnumerable<TaskSteps> StartGame()
        {
            yield return TaskSteps.Multiple(StartScreenTransition());

            libraries.Initialize();
            yield return TaskSteps.One();

            InitializePlayer();
            yield return TaskSteps.One();

            yield return TaskSteps.Multiple(InitializeLevelConstructor());
            yield return TaskSteps.Multiple(GoToNextLevel());

            yield return TaskSteps.Multiple(EndScreenTransition());
        }

        public void InitializePlayer()
        {
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
            //libraries.Items.AllWristItems().ForEach(wi => playerState.AddItem(wi()));
            //playerState.Inventory.Head.Item = libraries.Items.Eggs();

            libraries.Items.AllSkinItems().ForEach(hi => playerState.AddItem(hi()));
            libraries.Items.AllHeadItems().ForEach(wi => playerState.AddItem(wi()));

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

        public IEnumerable<TaskSteps> InitializeLevelConstructor()
        {
            var ldk = new LevelDevelopmentKit(GeometricPrimitives, worldParent, libraries);

            yield return TaskSteps.One();

            // Declaration
            {
                //ShapeGrammarState grammarState = new ShapeGrammarState(ldk);
                {
                    var levelConstructor = new LevelConstructor();
                    var languageState = new LanguageState(levelConstructor, ldk, CreateWorld);
                    languageState.Restart();

                    yield return TaskSteps.One();

                    var gr = new Grammars(ldk);
                    var sym = new Symbols();
                    ProductionProgram.Pr = new Productions(ldk, sym);
                    ProductionProgram.Ldk = ldk;

                    yield return TaskSteps.One();

                    // Initialize module languages
                    var languages = new Languages();
                    var languageParams = new LanguageParams(libraries, gr, languageState, languages);
                    languages.Initialize(languageParams);

                    // Initialize main language
                    GameLanguage = new MyLanguage(languageParams);


                    GameLanguage.MyWorldStart();

                    yield return TaskSteps.One();
                }

            }
        }

        IEnumerable<TaskSteps> PutPlayerToWorld(PlayerCharacterState playerState, LevelElement entireLevel)
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

                yield return TaskSteps.One();

                Debug.Log(entireLevel.Print(0));

                var startArea = GameLanguage.State.TraversableAreas.Where(area => area.Node.HasSymbols(GameLanguage.Gr.Sym.LevelStartMarker)).First();

                var goodCubes = startArea.Node.LE.CG().WithFloor().OpSub().ExtrudeHor(false).OpNew().Cubes;
                //.Cubes.Where(cube => cube.NeighborsHor().All(neighbor => neighbor.FacesVer(Vector3Int.down).FaceType == FACE_VER.Floor));
                var goodGraveCube = goodCubes.GetRandom();
                var graveState = libraries.InteractiveObjects.Grave();
                var grave = graveState.MakeGeometry();
                grave.transform.position = world.WorldGeometry.GridToWorld(goodGraveCube.Position);
                world.AddInteractiveObject(graveState);
                world.Grave = graveState;

                //yield return TaskSteps.One();

                world.InitializePlayer();

                yield return TaskSteps.One();
            }
        }

        IEnumerable<TaskSteps> ResetLevel(PlayerCharacterState playerState, LevelGroupElement levelRoot)
        {
            yield return TaskSteps.Multiple(StartScreenTransition());

            GameLanguage.State.World.Reset();
            yield return TaskSteps.One();
            yield return TaskSteps.Multiple(GameLanguage.State.InstantiateAreas());
            yield return TaskSteps.Multiple(PutPlayerToWorld(playerState, levelRoot));

            yield return TaskSteps.Multiple(EndScreenTransition());
        }

        public IEnumerable<TaskSteps> GoToNextLevel()
        {
            // Generating the world

            // show transition animation

            yield return TaskSteps.Multiple(GameLanguage.GenerateWorld());

            GameViewModel.ViewModel.World = GameLanguage.State.World;

            var grammarState = GameLanguage.State.GrammarState;

            grammarState.Print(new PrintingState()).Show();
            grammarState.Stats.Print();

            yield return TaskSteps.One();

            // Put player to the world

            var playerState = GameViewModel.ViewModel.PlayerState;
            var levelRoot = grammarState.WorldState.Added;
            yield return TaskSteps.Multiple(PutPlayerToWorld(playerState, levelRoot));

            yield return TaskSteps.One();

            GameLanguage.State.World.OnGameStart();

            yield return TaskSteps.One();

            // Restart level after player dies
            playerState
                .ClearOnDeath()
                .AddOnDeath(() =>
                {
                    AsyncEvaluator.SetTasks(ResetLevel(playerState, levelRoot));
                })
                .ClearOnRest()
                .AddOnRest(() =>
                {
                    AsyncEvaluator.SetTasks(ResetLevel(playerState, levelRoot));
                });

            // hide transition animation
        }

        private void FixedUpdate()
        {
            if (AsyncEvaluator.Evaluate())
            {
                var World = GameLanguage.State.World;
                World.Update(Time.fixedDeltaTime);
            }
        }

        public IEnumerable<TaskSteps> StartScreenTransition()
        {
            GameViewModel.ViewModel.Visible = false;
            LoadingScreen.StartLoading();

            float transitionTime = 0.3f;
            var sw = new Stopwatch();
            sw.Start();
            while(sw.ElapsedMilliseconds < transitionTime * 1000)
            {
                LoadingScreen.SetOpacity(sw.ElapsedMilliseconds * 0.001f / transitionTime);
                yield return TaskSteps.One();
            }
            LoadingScreen.SetOpacity(1f);
        }

        public IEnumerable<TaskSteps> EndScreenTransition()
        {
            float transitionTime = 1.0f;
            var sw = new Stopwatch();
            sw.Start();
            while (sw.ElapsedMilliseconds < transitionTime * 1000)
            {
                LoadingScreen.SetOpacity(1f - sw.ElapsedMilliseconds * 0.001f / transitionTime);
                yield return TaskSteps.One();
            }
            LoadingScreen.SetOpacity(0f);

            GameViewModel.ViewModel.Visible = true;
            LoadingScreen.EndLoading();
            yield return TaskSteps.One();
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
            public static TaskSteps One() => new(Enumerable.Empty<TaskSteps>());
            public static TaskSteps Multiple(IEnumerable<TaskSteps> subtasks) => new(subtasks);

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

        public AsynchronousEvaluator(int msPerFrame)
        {
            _stopwatch = new Stopwatch();
            _msPerFrame = msPerFrame;
        }

        public void SetTasks(TaskSteps taskSteps)
        {
            _taskQueue = taskSteps.GetTasks().GetEnumerator();
        }

        public void SetTasks(IEnumerable<TaskSteps> taskSteps)
        {
            _taskQueue = TaskSteps.Multiple(taskSteps).GetTasks().GetEnumerator();
        }

        /// <summary>
        /// Returns true if finished.
        /// </summary>
        public bool Evaluate()
        {
            if (_taskQueue == null)
                return true;

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