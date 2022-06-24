using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Assets.Util;
using ContentGeneration.Assets.UI.Util;

namespace ShapeGrammar
{
    public class ShapeGrammarGenerator : WorldGenerator
    {
        [SerializeField]
        Transform parent;

        [SerializeField]
        ShapeGrammarObjectStyle DefaultHouseStyle;

        [SerializeField]
        ShapeGrammarObjectStyle GardenStyle;

        float worldScale;

        public override void Generate(World world)
        {
            /*if (Application.isEditor)
            {
                UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));
            }*/
            //world.AddEnemy(libraries.Enemies.MayanSwordsman(), new Vector3(0, 1, 0));
            //world.AddEnemy(libraries.Enemies.DragonMan(), new Vector3(0, 1, 0));
            UnityEngine.Random.InitState(42);

            worldScale = 2.8f;

            // todo: make this initialization less annoying
            var playerState = new ContentGeneration.Assets.UI.Model.PlayerCharacterState();
            var prop = new ContentGeneration.Assets.UI.Model.CharacterProperties()
            {
                Health = 100,
                Spirit = 100,
                Will = 50,
            };
            playerState.Prop = prop;
            prop.Character = playerState;
            GameViewModel.ViewModel.PlayerState = playerState;

            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            var examples = new Examples(DefaultHouseStyle, GardenStyle, parent, libraries);
            var levelRoot = examples.LanguageDesign(libraries);
            examples.grid.Generate(worldScale, parent);

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
            world.AddInteractiveObject(interactiveObjects.Grave().MakeGeometry(), GridToWorld(goodGraveCube.Position));

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

        }
        /*
         For profiling

        bool generated = false;

        private void Update()
        {
            if (generated) return;
            generated = true;

            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            Debug.Log("Generating world", this);

            UnityEngine.Profiling.Profiler.BeginSample("Generating world");
            var examples = new Examples(DefaultHouseStyle, GardenStyle);
            var levelRoot = examples.CurveDesign();


            stopwatch.Stop();
            Debug.Log(stopwatch.ElapsedMilliseconds);
        }
        */

        public Vector3 GridToWorld(Vector3 pos)
        {
            return parent.position + worldScale * pos;
        }

        public override void DestroyWorld()
        {
            for (int i = parent.childCount; i > 0; --i)
            {
                GameObject.Destroy(parent.GetChild(0).gameObject);
            }
        }
    }

    public class GeneratorGeometry
    {
        Transform worldParent;
        float worldScale;

        public GeneratorGeometry(Transform worldParent, float worldScale)
        {
            this.worldParent = worldParent;
            this.worldScale = worldScale;
        }

        public Vector3 GridToWorld(Vector3 gridPos)
        {
            return worldParent.position + worldScale * gridPos;
        }

        public Vector3 WorldToGrid(Vector3 worldPos)
        {
            return (worldPos - worldParent.position) / worldScale;
        }
    }
}