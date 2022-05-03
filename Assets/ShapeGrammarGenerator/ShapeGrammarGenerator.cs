using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

        public override void Generate(World world)
        {
            /*if (Application.isEditor)
            {
                UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));
            }*/
            //world.AddEnemy(libraries.Enemies.MayanSwordsman(), new Vector3(0, 1, 0));
            //world.AddEnemy(libraries.Enemies.DragonMan(), new Vector3(0, 1, 0));
            //UnityEngine.Random.InitState(42);

            worldScale = 2.8f;

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

            var goodGraveCube = levelRoot.CG().WithFloor().Cubes
                .Where(cube => cube.NeighborsHor().All(neighbor => neighbor.FacesVer(Vector3Int.down).FaceType == FACE_VER.Floor)).GetRandom();
            world.AddInteractiveObject(interactiveObjects.Grave().MakeGeometry(), GridToWorld(goodGraveCube.Position));


            var hs = new HashSet<Vector3Int>() { new Vector3Int(0, 1, 0) };

            Debug.Log($"hs contains (0, 1, 0): {hs.Contains(new Vector3Int(0, 1, 0))}");
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