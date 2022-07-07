using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Assets.Util;
using ContentGeneration.Assets.UI.Util;

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

        float worldScale;

        public void Generate()
        {
            /*if (Application.isEditor)
            {
                UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));
            }*/
            //world.AddEnemy(libraries.Enemies.MayanSwordsman(), new Vector3(0, 1, 0));
            //world.AddEnemy(libraries.Enemies.DragonMan(), new Vector3(0, 1, 0));
            UnityEngine.Random.InitState(42);

            var worldGeometry = new WorldGeometry(worldParent, 2.8f);
            var world = new World(worldGeometry);


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

            var examples = new Examples(DefaultHouseStyle, GardenStyle, world.ArchitectureParent, libraries);
            var levelRoot = examples.LanguageDesign(libraries, world);
            examples.grid.Generate(worldScale, world);

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
            world.AddInteractiveObject(graveState);
            world.Grave = graveState;

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


            world.Created();

        }
    }
}