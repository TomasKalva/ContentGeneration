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
        public LevelConstructor LC { get; }

        public LanguageState(ShapeGrammarState grammarState)
        {
            GrammarState = grammarState;
            TraversabilityGraph = new TraversabilityGraph();
            LC = new LevelConstructor();
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

        public void MoveToNextLevel()
        {
            LC.Construct();
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
        public ObjectPlacement<CharacterState> PlC { get; }
        public ObjectPlacement<InteractiveObjectState> PlO { get; }
        public MsgPrinter Msg { get; }

        public LDLanguage(LanguageParams languageParams)
        {
            Ldk = languageParams.Ldk;
            Lib = languageParams.Lib;
            Gr = languageParams.Gr;
            
            State = languageParams.LanguageState;

            Env = new Environments(this);
            PlC = new ObjectPlacement<CharacterState>((area, enemy) => area.AddEnemy(enemy));
            PlO = new ObjectPlacement<InteractiveObjectState>((area, io) => area.AddInteractiveObject(io));
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


    class SpacePartitioning
    {
        HashSet<Area> activeAreas;
        TraversabilityGraph traversabilityGraph;
        bool initialized = false; // initialize during first update so that UI can get reference to this enemy

        public SpacePartitioning(TraversabilityGraph traversabilityGraph)
        {
            this.traversabilityGraph = traversabilityGraph;
        }

        public void Initialize()
        {
            traversabilityGraph.Areas.ForEach(area => area.Disable());
            activeAreas = new HashSet<Area>();
        }

        public void Update(Node activeNode)
        {
            if (!initialized)
            {
                Initialize();
                initialized = true;
            }

            if(activeNode == null)
            {
                return;
            }

            var activeArea = traversabilityGraph.GetArea(activeNode);
            var currentAreas = traversabilityGraph.Neighbors(activeArea).Append(activeArea);

            // remove no longer active
            var notActive = activeAreas.Where(area => traversabilityGraph.Distance(area, activeArea, int.MaxValue) > 1).ToList();
            notActive.ForEach(area => area.Disable());
            notActive.ForEach(area => activeAreas.Remove(area));

            // add newly active
            var newActive = currentAreas.Except(activeAreas);
            newActive.ForEach(area => area.Enable());
            newActive.ForEach(area => activeAreas.Add(area));
        }
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
        public PatternLanguage PatternLanguage { get; private set; }
        public TestingLanguage TestingLanguage { get; private set; }

        Languages()
        {
        }

        void Init(LanguageParams tools)
        {
            LevelLanguage = new LevelLanguage(tools);
            BrothersLanguage = new BrothersLanguage(tools);
            FarmersLanguage = new FarmersLanguage(tools);
            PatternLanguage = new PatternLanguage(tools);
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

    #endregion
}
