using ContentGeneration.Assets.UI;
using ContentGeneration.Assets.UI.Model;
using ContentGeneration.Assets.UI.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

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

        public void AddEnemy(CharacterState enemy)
        {
            // define behavior that makes enemies only go after player, if he's in their area
            var gotoPosition = L.State.Ldk.wg.GridToWorld(Node.LE.CG().WithFloor().Cubes.GetRandom().Position);
            //L.Lib.InteractiveObjects.AscensionKiln().MakeGeometry().transform.position = gotoPosition; // visualization of waiting spots
            var thisAreaPositions = new HashSet<Vector3Int>(Node.LE.CG().Cubes.Select(c => c.Position));
            enemy.Behaviors.AddBehavior(
                new Wait(
                    _ =>
                    {
                        var playerGridPosition = Vector3Int.RoundToInt(L.State.Ldk.wg.WorldToGrid(GameViewModel.ViewModel.PlayerState.Agent.transform.position));
                        return !thisAreaPositions.Contains(playerGridPosition);
                    },
                    _ => gotoPosition
                    )
                );
            EnemyStates.Add(enemy);
        }

        public virtual void InstantiateAll(WorldGeometry gg, World world)
        {
            var flooredCubes = new Stack<Cube>(Node.LE.CG().WithFloor().Cubes.Shuffle());

            foreach (var ios in InteractiveObjectStates)
            {
                if (!flooredCubes.Any())
                {
                    Debug.LogError("Not enough empty cubes");
                    break;
                }
                ios.MakeGeometry();
                ios.InteractiveObject.transform.position = gg.GridToWorld(flooredCubes.Pop().Position);

                world.AddInteractiveObject(ios);
            }

            foreach (var enemy in EnemyStates)
            {
                if (!flooredCubes.Any())
                {
                    Debug.LogError("Not enough empty cubes");
                    break;
                }
                enemy.MakeGeometry();
                enemy.Agent.transform.position = gg.GridToWorld(flooredCubes.Pop().Position);

                world.AddEnemy(enemy);
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
            var area = Areas.Where(area => area.Node == node).FirstOrDefault();
            if (area == null)
            {
                node.Print(new PrintingState()).Show();
                throw new InvalidOperationException($"Area for the node doesn't exist");
            }
            return area;
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
