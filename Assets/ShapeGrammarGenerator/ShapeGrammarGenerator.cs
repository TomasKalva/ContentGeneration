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
            worldScale = 2.8f;

            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            var examples = new Examples(DefaultHouseStyle, GardenStyle);
            var levelRoot = examples.CurveDesign();
            examples.grid.Generate(worldScale, parent);

            stopwatch.Stop();
            Debug.Log(stopwatch.ElapsedMilliseconds);

            Debug.Log(levelRoot.Print(0));
            /*
            Debug.Log("Generating world");
            var l = new List<int>() { 1, 2, 3 };
            l.Select2Distinct((a, b) =>
            {
                return new { a, b };
            }).ForEach(pair => Debug.Log($"{pair.a}, {pair.b}"));*/

            Debug.Log("Generating world");

            var goodBonfirePosition = GridToWorld(levelRoot.CubeGroup().WithFloor().Cubes
                .Where(cube => cube.NeighborsHor().All(neighbor => neighbor.FacesVer(Vector3Int.down).FaceType == FACE_VER.Floor)).GetRandom().Position);
            world.AddInteractiveObject(interactiveObjects.bonfire, goodBonfirePosition);

            var allEnemies = libraries.Enemies.AllAgents();
            var enemyCubes = levelRoot.CubeGroup().WithFloor().Cubes.Shuffle().Take(10);
            enemyCubes.ForEach(cube => world.AddEnemy(allEnemies.GetRandom()(), GridToWorld(cube.Position)));
        }

        Vector3 GridToWorld(Vector3 pos)
        {
            return parent.position + worldScale * pos;
        }

    }
}