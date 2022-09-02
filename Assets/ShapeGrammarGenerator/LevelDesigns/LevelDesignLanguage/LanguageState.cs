using Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions;
using ContentGeneration.Assets.UI;
using ShapeGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static ShapeGrammar.AsynchronousEvaluator;

namespace Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage
{
    class LanguageState
    {
        public LevelConstructor LC { get; }

        public ShapeGrammarState GrammarState { get; set; }
        public IEnumerable<Area> TraversableAreas => TraversabilityGraph.Areas;
        public TraversabilityGraph TraversabilityGraph { get; set; }
        public World World { get; set; }
        public LevelDevelopmentKit Ldk { get; set; }
        public UniqueNameGenerator UniqueNameGenerator { get; }

        Func<World> CreateWorld { get; }

        public LanguageState(LevelConstructor levelConstructor, LevelDevelopmentKit ldk, Func<World> createWorld)
        {
            TraversabilityGraph = new TraversabilityGraph();
            LC = levelConstructor;
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

        public IEnumerable<TaskSteps> InstantiateAreas()
        {
            foreach (var area in TraversableAreas)
            {
                yield return TaskSteps.Multiple(area.InstantiateAll(World));
            }
            //TraversableAreas.ForEach(area => area.InstantiateAll(World));
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
