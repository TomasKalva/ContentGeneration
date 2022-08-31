﻿using ContentGeneration.Assets.UI;
using ContentGeneration.Assets.UI.Model;
using ContentGeneration.Assets.UI.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static ShapeGrammar.AsynchronousEvaluator;

namespace ShapeGrammar
{
    class Area
    {
        LDLanguage L { get; }
        public Node Node { get; }
        public List<AreasConnection> EdgesFrom { get; }
        public List<AreasConnection> EdgesTo { get; }
        public List<InteractiveObjectState> InteractiveObjectStates { get; }
        public List<CharacterState> EnemyStates { get; }

        public Area(Node node, LDLanguage language)
        {
            L = language;
            Node = node;
            EdgesFrom = new List<AreasConnection>();
            EdgesTo = new List<AreasConnection>();
            InteractiveObjectStates = new List<InteractiveObjectState>();
            EnemyStates = new List<CharacterState>();
        }

        public void AddInteractiveObject(InteractiveObjectState interactiveObject)
        {
            InteractiveObjectStates.Add(interactiveObject);
        }

        void AddWaitForPlayerBehavior(World world, Agent enemy)
        {
            var worldGeometry = world.WorldGeometry;
            var gotoPosition = worldGeometry.GridToWorld(Node.LE.CG().BottomLayer().Cubes.GetRandom().Position);
            //L.Lib.InteractiveObjects.AscensionKiln().MakeGeometry().transform.position = gotoPosition; // visualization of waiting spots
            var thisAreaPositions = new HashSet<Vector3Int>(Node.LE.CG().Cubes.Select(c => c.Position));
            enemy.Behaviors.AddBehavior(
                new Wait(
                    _ =>
                    {
                        // Wait until the player spawns
                        if (GameViewModel.ViewModel.PlayerState.Agent == null)
                            return true;

                        var playerGridPosition = Vector3Int.RoundToInt(worldGeometry.WorldToGrid(GameViewModel.ViewModel.PlayerState.Agent.transform.position));
                        return !thisAreaPositions.Contains(playerGridPosition);
                    },
                    _ => gotoPosition
                    )
                );
        }

        public void AddEnemy(CharacterState enemy)
        {
            EnemyStates.Add(enemy);
        }

        void CreateInteractiveObject(World world, InteractiveObjectState ios, Vector3Int position)
        {
            if (!ios.CanBeCreated())
                return;

            ios.MakeGeometry();
            ios.InteractiveObject.transform.position = world.WorldGeometry.GridToWorld(position);

            world.AddInteractiveObject(ios);
        }

        void CreateEnemy(World world, CharacterState enemy, Vector3Int position)
        {
            if (!enemy.CanBeCreated())
                return;

            enemy.MakeGeometry();
            enemy.Agent.transform.position = world.WorldGeometry.GridToWorld(position);
            AddWaitForPlayerBehavior(world, enemy.Agent);

            world.AddEnemy(enemy);
        }

        public IEnumerable<TaskSteps> InstantiateAll(World world)
        {
            //var flooredCubes = new Stack<Cube>(Node.LE.CG().WithFloor().Cubes.Shuffle());

            var grid = L.State.Ldk.grid;

            var paths = L.State.TraversabilityGraph.Connections.Select(areasConnection => areasConnection.Path.LE.CG());
            var pathEnds = paths.SelectMany(path => PathNode.FindPathEndsInFloor(Node.LE.CG().WithFloor(), paths));

            yield return TaskSteps.One();

            var unoccupiedFloor = Node.LE.CG().WithFloor().Cubes.Except(pathEnds);
            var flooredCubes = new Holder<Cube>(unoccupiedFloor);

            var worldGeometry = world.WorldGeometry;

            yield return TaskSteps.One();

            var blockingIos = InteractiveObjectStates.Where(ios => ios.IsBlocking);
            var notBlockingIos = InteractiveObjectStates.Except(blockingIos);

            yield return TaskSteps.One();

            // Place blocking objects
            foreach(var ios in blockingIos)
            {
                var validCube = flooredCubes.TakeRandom(
                    cube => PathNode.IsConnected(
                        flooredCubes.Rest().Except(cube.ToEnumerable()).ToCubeGroup(grid),
                        pathEnds));

                CreateInteractiveObject(world, ios, validCube.Position);
                yield return TaskSteps.One();
            }

            // Place not blocking objects
            foreach(var ios in notBlockingIos)
            {
                if (!flooredCubes.Any())
                {
                    Debug.LogError("Not enough empty cubes");
                    break;
                }

                CreateInteractiveObject(world, ios, flooredCubes.TakeRandom().Position);
                yield return TaskSteps.One();
            }

            foreach (var enemy in EnemyStates)
            {
                if (!flooredCubes.Any())
                {
                    Debug.LogError("Not enough empty cubes");
                    break;
                }

                CreateEnemy(world, enemy, flooredCubes.TakeRandom().Position);
                yield return TaskSteps.One();
            }
        }

        public void Enable()
        {
            EnemyStates.ForEach(enemy =>
            {
                if (enemy.Agent)
                {
                    enemy.Agent.gameObject.SetActive(true);
                }
            });
        }

        public void Disable()
        {
            EnemyStates.ForEach(enemy =>
            {
                if (enemy.Agent)
                {
                    enemy.Agent.gameObject.SetActive(false);
                }
            });
        }
    }

    /// <summary>
    /// Represents collection from which itmes satisfying some condition can be gradually removed at random.
    /// </summary>
    class Holder<T>
    {
        List<T> _heldObjects;

        public bool Any() => _heldObjects.Any();

        public Holder(IEnumerable<T> heldObjects)
        {
            _heldObjects = heldObjects.ToList();
        }

        /// <summary>
        /// Returns random item statisfying the predicate and removes it.
        /// </summary>
        public T TakeRandom(Func<T, bool> predicate = null)
        {
            predicate ??= _ => true;
            var randT = _heldObjects.Where(predicate).ToList().GetRandom();
            _heldObjects.Remove(randT);
            return randT;
        }

        /// <summary>
        /// Returns items that remain.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<T> Rest() => _heldObjects;
    }

    class AreasConnection : IEdge<Area>
    {
        public Node Path { get; }
        public Area From { get; }
        public Area To { get; }

        public AreasConnection(Node path, Area from, Area to)
        {
            Path = path;
            From = from;
            To = to;
            From.EdgesFrom.Add(this);
            To.EdgesTo.Add(this);
        }

        public bool Connects(Area from, Area to)
        {
            return (from == From && to == To) || (from == To && to == From);
        }

        public bool Contains(Area vert)
        {
            return vert == From || vert == To;
        }

        public Area Other(Area vert)
        {
            return vert == From ? To : vert == To ? From : throw new InvalidOperationException($"The vertex {vert} isn't in the edge.");
        }
    }

    class TraversabilityGraph : GraphAlgorithms<Area, AreasConnection, TraversabilityGraph>, IGraph<Area, AreasConnection>
    {
        public List<Area> Areas { get; }
        public List<AreasConnection> Connections { get; }

        public TraversabilityGraph() : base(null)
        {
            Areas = new List<Area>();
            Connections = new List<AreasConnection>();
            graph = this;
        }

        public Area GetArea(Node node)
        {
            var area = TryGetArea(node);
            if (area == null)
            {
                node.Print(new PrintingState()).Show();
                throw new InvalidOperationException($"Area for the node doesn't exist");
            }
            return area;
        }

        public Area TryGetArea(Node node)
        {
            return Areas.Where(area => area.Node == node).FirstOrDefault();
        }

        public bool AreConnected(Area from, Area to)
        {
            return Connections.Any(edge => edge.Connects(from, to));
        }

        public IEnumerable<AreasConnection> EdgesFrom(Area vert)
        {
            return Connections.Where(edge => edge.Contains(vert));
        }
        public IEnumerable<AreasConnection> EdgesTo(Area vert)
        {
            return Connections.Where(edge => edge.To == vert);
        }

        public IEnumerable<Area> Neighbors(Area vert)
        {
            return EdgesFrom(vert).Select(edge => edge.Other(vert));
        }
    }

}
