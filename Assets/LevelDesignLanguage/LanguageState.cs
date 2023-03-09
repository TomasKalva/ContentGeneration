using OurFramework.UI;
using OurFramework.Environment.ShapeCreation;
using OurFramework.Environment.ShapeGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using static OurFramework.Game.Game;
using OurFramework.Game;
using static OurFramework.Game.AsynchronousEvaluator;

namespace OurFramework.LevelDesignLanguage
{
    class LanguageState
    {
        public LevelConstructor LC { get; }
        public GameControl GC { get; }

        public ShapeGrammarState GrammarState { get; set; }
        public IEnumerable<Area> TraversableAreas => TraversabilityGraph.Areas;
        public TraversabilityGraph TraversabilityGraph { get; set; }
        public World World { get; set; }
        public LevelDevelopmentKit Ldk { get; set; }
        public UniqueNameGenerator UniqueNameGenerator { get; }

        Func<World> CreateWorld { get; }

        public LanguageState(LevelConstructor levelConstructor, LevelDevelopmentKit ldk, GameControl gameControl, Func<World> createWorld)
        {
            TraversabilityGraph = new TraversabilityGraph();
            LC = levelConstructor;
            GC = gameControl;
            Ldk = ldk;
            UniqueNameGenerator = new UniqueNameGenerator();
            CreateWorld = createWorld;
        }

        public void Restart()
        {
            World?.Destroy();
            Ldk.grid.Clear();

            GrammarState = new ShapeGrammarState(Ldk);
            TraversabilityGraph = new TraversabilityGraph();
            World = CreateWorld();
        }

        public void CalculateObjectsPositions()
        {
            foreach (var area in TraversableAreas)
            {
                area.CalculatePositions(World);
            }
        }

        public IEnumerable<TaskSteps> InstantiateAreas()
        {
            foreach (var area in TraversableAreas)
            {
                yield return TaskSteps.Multiple(area.InstantiateAll(World));
            }
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
}
