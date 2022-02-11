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
        ShapeGrammarObjectStyle FountainheadStyle;

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

            var examples = new Examples(FountainheadStyle);
            var levelRoot = examples.CurveDesign();
            examples.grid.Generate(2f, parent);

            Debug.Log("Generating world");

            var goodBonfirePosition = parent.position + 2 * levelRoot.CubeGroup().WithFloor().Cubes.GetRandom().Position;
            world.AddInteractiveObject(interactiveObjects.bonfire, goodBonfirePosition);
        }


    }
}