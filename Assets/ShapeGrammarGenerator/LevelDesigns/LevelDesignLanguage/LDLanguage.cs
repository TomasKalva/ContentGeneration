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
    class LanguageState
    {
        public ShapeGrammarState GrammarState { get; }
        public IEnumerable<Area> TraversableAreas => TraversabilityGraph.Areas;
        public TraversabilityGraph TraversabilityGraph { get; }

        public LanguageState(ShapeGrammarState grammarState)
        {
            GrammarState = grammarState;
            TraversabilityGraph = new TraversabilityGraph();
        }

        public void AddAreas(List<Area> areas)
        {
            TraversabilityGraph.Areas.AddRange(areas);
        }

        public void AddConnections(List<Node> connections)
        {
            var areaConnections = connections.Select(nCon =>
            {
                var pred = nCon.DerivedFrom;
                if (pred.Count < 2) 
                {
                    throw new InvalidOperationException($"The connection node doesn't have enough parents: expected 2, actual {pred.Count}");
                }
                var from = TraversabilityGraph.GetArea(pred[0]);
                var to = TraversabilityGraph.GetArea(pred[1]);
                return new AreasConnection(nCon, from, to);
            });
            TraversabilityGraph.Connections.AddRange(areaConnections);
        }
    }

    class TraversabilityGraph : IGraph<Area, AreasConnection>
    {
        public List<Area> Areas { get; }
        public List<AreasConnection> Connections { get; }

        public TraversabilityGraph()
        {
            Areas = new List<Area>();
            Connections = new List<AreasConnection>();
        }

        public Area GetArea(Node node)
        {
            var area = Areas.Where(area => area.Node == node).FirstOrDefault();
            if(area == null)
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

        public IEnumerable<Area> Neighbors(Area vert)
        {
            return EdgesFrom(vert).Select(edge => edge.Other(vert));
        }
    }

    abstract class LDLanguage
    {
        public LanguageState State { get; }

        public LevelDevelopmentKit Ldk { get; }
        public Libraries Lib { get; }
        public Grammars Gr { get; }
        public Languages L { get; }
        public Environments Env { get; }
        public MsgPrinter Msg { get; }

        public LDLanguage(LanguageParams languageParams)
        {
            Ldk = languageParams.Ldk;
            Lib = languageParams.Lib;
            Gr = languageParams.Gr;

            State = languageParams.LanguageState;

            Env = new Environments(this);
            //Env = languageParams.Env;
            Msg = new MsgPrinter();

            L = Languages.Get(languageParams);
        }

        public void Instantiate()
        {
            State.TraversableAreas.ForEach(area => area.InstantiateAll(Ldk.gg));
        }
    }

    class LanguageParams
    {
        public LanguageState LanguageState { get; }

        public LevelDevelopmentKit Ldk { get; }
        public Libraries Lib { get; }
        public Grammars Gr { get; }

        public LanguageParams(LevelDevelopmentKit ldk, Libraries lib, Grammars gr, LanguageState languageState)
        {
            Ldk = ldk;
            Lib = lib;
            Gr = gr;
            LanguageState = languageState;
        }
    }

    #region Primitives

    class Area
    {
        LDLanguage L { get; }
        public Node Node { get; }
        public List<InteractiveObjectState> InteractiveObjectStates { get; }
        public List<CharacterState> EnemyStates { get; }

        public Area(Node node, LDLanguage language)
        {
            L = language;
            Node = node;
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
            var gotoPosition = L.Ldk.gg.GridToWorld(Node.LE.CG().WithFloor().Cubes.GetRandom().Position);
            //L.Lib.InteractiveObjects.AscensionKiln().MakeGeometry().transform.position = gotoPosition; // visualization of waiting spots
            var thisAreaPositions = new HashSet<Vector3Int>(Node.LE.CG().Cubes.Select(c => c.Position));
            enemy.Behaviors.AddBehavior(
                new Wait(
                    _ => 
                    {
                        var playerGridPosition = Vector3Int.RoundToInt(L.Ldk.gg.WorldToGrid(GameViewModel.ViewModel.PlayerState.Agent.transform.position));
                        return !thisAreaPositions.Contains(playerGridPosition); 
                    },
                    _ => gotoPosition
                    )
                );
            EnemyStates.Add(enemy);
        }

        public virtual void InstantiateAll(GeneratorGeometry gg)
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
                // todo: Create tool for placement of real assets, so that magic numbers aren't needed
                ios.InteractiveObject.transform.position = gg.GridToWorld(flooredCubes.Pop().Position);
            }

            foreach (var enemy in EnemyStates)
            {
                if (!flooredCubes.Any())
                {
                    Debug.LogError("Not enough empty cubes");
                    break;
                }

                enemy.MakeGeometry();
                // todo: Create tool for placement of real assets, so that magic numbers aren't needed
                enemy.Agent.transform.position = gg.GridToWorld(flooredCubes.Pop().Position);
            }
        }

        public void Enable()
        {
            EnemyStates.ForEach(enemy => enemy.Agent.enabled = true);
        }

        public void Disable()
        {
            EnemyStates.ForEach(enemy => enemy.Agent.enabled = false);
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

    class SpacePartitioning
    {
        IEnumerable<Area> activeAreas;
        TraversabilityGraph traversabilityGraph;
        GraphAlgorithms<Area, Edge<Area>, IGraph<Area, Edge<Area>>> areasConnnectionsAlg;

        public SpacePartitioning(TraversabilityGraph traversabilityGraph)
        {
            this.traversabilityGraph = traversabilityGraph;
            Initialize();
        }

        public void Initialize()
        {
            traversabilityGraph.Areas.ForEach(area => area.Disable());
            activeAreas = traversabilityGraph.Areas;
        }

        public void Update(Node activeNode)
        {
            if(activeNode == null)
            {
                return;
            }

            var activeArea = traversabilityGraph.GetArea(activeNode);
            activeAreas.ForEach(area => area.Disable());

            // current neighbors
            var currActive = traversabilityGraph.Neighbors(activeArea).Append(activeArea);
            // remove no longer active
            var notActive = activeAreas.Where(area => areasConnnectionsAlg.Distance(area, activeArea, int.MaxValue) > 2).ToList();
            // add newly active
            var newActive = currActive.Except(notActive);

            activeAreas = newActive;
            activeAreas.ForEach(area => area.Enable());
        }
    }

    class LinearPath
    {
        public List<Area> Areas { get; }

        public LinearPath(List<Area> areas)
        {
            Areas = areas;
        }

        public Area LastArea() => Areas.LastOrDefault();
    }

    class Branching
    {
        public List<Area> Areas { get; }

        public Branching(List<Area> areas)
        {
            Areas = areas;
        }
    }

    class Enemy
    {

    }

    class Item
    {

    }

    public class GeometryMaker<T> where T : MonoBehaviour
    {
        T geometry;
        Func<T> GeometryF { get; }

        public GeometryMaker(Func<T> geometryF)
        {
            GeometryF = geometryF;
        }

        public T CreateGeometry()
        {
            geometry = GeometryF();
            return geometry;
        }
    }

    #endregion

    class Languages
    {
        /// <summary>
        /// Singleton to prevent cycles when creating languages.
        /// </summary>
        static Languages _get;
        public static Languages Get(LanguageParams tools)
        {
            if (_get == null)
            {
                _get = new Languages();
                _get.Init(tools);
            }
            return _get;
        }

        public LevelLanguage LevelLanguage { get; private set; }
        public BrothersLanguage BrothersLanguage { get; private set; }
        public FarmersLanguage FarmersLanguage { get; private set; }
        public TestingLanguage TestingLanguage { get; private set; }

        Languages()
        {
        }

        void Init(LanguageParams tools)
        {
            LevelLanguage = new LevelLanguage(tools);
            BrothersLanguage = new BrothersLanguage(tools);
            FarmersLanguage = new FarmersLanguage(tools);
            TestingLanguage = new TestingLanguage(tools);
        }
    }

    #region Language tools

    class Environments
    {
        LDLanguage L { get; }
        LanguageState LanguageState { get; }
        Symbols Sym { get; }

        public Environments(LDLanguage language)
        {
            L = language;
            LanguageState = L.State;
            Sym = L.Gr.Sym;
        }

        IEnumerable<Area> GenerateAndTakeTraversable(Grammar grammar)
        {
            var newNodes = grammar.Evaluate(LanguageState.GrammarState);
            var traversable = newNodes
                                .Where(node => node.HasSymbols(Sym.FullFloorMarker))
                                .Select(node => new Area(node, L))
                                .ToList();
            LanguageState.AddAreas(traversable);
            var connections = newNodes
                                .Where(node => node.HasSymbols(Sym.ConnectionMarker))
                                .ToList();
            LanguageState.AddConnections(connections);
            return traversable;
        }

        public void Execute(Grammar grammar)
        {
            var traversable = GenerateAndTakeTraversable(grammar);
        }

        public void Line(ProductionList productions, NodesQuery startNodesQuery, int count, out LinearPath linearPath)
        {
            var linearGrammar = new CustomGrammar(productions, count, startNodesQuery, state => state.LastCreated);
            var traversable = GenerateAndTakeTraversable(linearGrammar);
            linearPath = new LinearPath(traversable.ToList());
        }

        public void One(ProductionList productions, NodesQuery startNodesQuery, out Area one)
        {
            var grammar = new CustomGrammar(productions, 1, startNodesQuery);
            one = GenerateAndTakeTraversable(grammar).First();
        }

        public void ExtendRandomly(ProductionList productions, NodesQuery startNodesQuery, int count, out Branching branching)
        {
            var grammar = new CustomGrammar(productions, count, startNodesQuery, NodesQueries.Extend(NodesQueries.LastCreated));
            var traversableNodes = GenerateAndTakeTraversable(grammar);
            branching = new Branching(traversableNodes.ToList());
        }
    }
    class MsgPrinter
    {
        public void Show(string message)
        {
            GameViewModel.ViewModel.Message = message;
        }
    }

    class Grammars
    {
        public ProductionLists PrL { get; }
        public Productions Pr { get; }
        public Symbols Sym { get; }

        public Grammars(LevelDevelopmentKit ldk)
        {
            Sym = new Symbols();
            Pr = new Productions(ldk, Sym);
            PrL = new ProductionLists(ldk, Pr);
        }
    }
    /*
    interface IProductions : ILDLanguage
    {
        protected ProductionLists PrL => new ProductionLists(Ldk);
        protected Productions Pr => new Productions(Ldk);

        public ProductionList CreateNewHouse() => PrL.CreateNewHouse(Pr);
        public ProductionList Garden() => PrL.Garden(Pr);
        public ProductionList GuidedGarden(PathGuide guide) => PrL.GuidedGarden(Pr, guide);
        public ProductionList Graveyard() => PrL.Graveyard(Pr);
        public ProductionList GraveyardPostprocess() => PrL.GraveyardPostprocess(Pr);
        public ProductionList ConnectBack() => PrL.ConnectBack(Pr);
        public ProductionList Roofs() => PrL.Roofs(Pr);
    }*/

    /*
    class InteractiveObjects
    {
        public GeometryMaker Geometry(Transform prefab)
        {
            return new GeometryMaker(() => GameObject.Instantiate(prefab));
        }

        public InteractiveObjectState NewInteractiveObject(string name, GeometryMaker geometryMaker)
        {
            var newInteractiveObject = new InteractiveObjectState()
            {
                Name = name,
            };
            return newInteractiveObject;
        }
    }*/

    #endregion
}
