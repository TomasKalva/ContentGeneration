using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static ShapeGrammar.Grid;
using System.Linq;
using Assets.Util;

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

        /*
        private void Start()
        {
            // Keep scene view
            if (Application.isEditor)
            {
                UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));
            }
            //UnityEngine.Random.InitState(16);

            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            Debug.Log("Generating world");

            var examples = new Examples(FountainheadStyle);
            examples.JustHouse();

            examples.grid.Generate(2f, parent);

            //Debug.Log(ExtensionMethods.Circle3(2).Count());// ForEach(v => Debug.Log($"{v}\n"));

            stopwatch.Stop();
            Debug.Log(stopwatch.ElapsedMilliseconds);
        }
        */

        public override void Generate(World world)
        {
            //world.AddEnemy(libraries.Enemies.MayanSwordsman(), new Vector3(0, 1, 0));
            //world.AddEnemy(libraries.Enemies.DragonMan(), new Vector3(0, 1, 0));
            //UnityEngine.Random.InitState(17);

            worldScale = 2.8f;

            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            var examples = new Examples(DefaultHouseStyle, GardenStyle);
            var levelRoot = examples.DebugPlatform();
            examples.grid.Generate(worldScale, parent);

            stopwatch.Stop();
            Debug.Log(stopwatch.ElapsedMilliseconds);

            Debug.Log(levelRoot.Print(0));

            Debug.Log("Generating world");

            var goodGraveCube = levelRoot.CubeGroup().WithFloor().Cubes
                .Where(cube => cube.NeighborsHor().All(neighbor => neighbor.FacesVer(Vector3Int.down).FaceType == FACE_VER.Floor)).GetRandom();
            world.AddInteractiveObject(interactiveObjects.Grave(), GridToWorld(goodGraveCube.Position));

            var elevator = libraries.InteractiveObjects.Elevator(10, false);
            world.AddObject(elevator, Vector3.zero);

            /*            
            var allEnemies = libraries.Enemies.AllAgents();
            var enemyCubes = levelRoot.CubeGroup().WithFloor().Cubes.Shuffle().Take(10);
            enemyCubes.ForEach(cube => world.AddEnemy(allEnemies.GetRandom()(), GridToWorld(cube.Position)));
            */

            var itemCubes = levelRoot.CubeGroup().WithFloor().Cubes.Shuffle().Take(10);
            itemCubes.ForEach(cube => world.AddItem(libraries.Items.Physical(libraries.Items.Mace()), GridToWorld(cube.Position)));

            var kilnCube = goodGraveCube.NeighborsHor().GetRandom();
            world.AddInteractiveObject(interactiveObjects.AscensionKiln(), GridToWorld(kilnCube.Position));
            
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

        Vector3 GridToWorld(Vector3 pos)
        {
            return parent.position + worldScale * pos;
        }

    }
}