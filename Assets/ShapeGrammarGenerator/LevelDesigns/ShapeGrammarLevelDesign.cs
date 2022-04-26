using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShapeGrammar
{

    public class ShapeGrammarLevelDesign : LevelDesign
    {
        public ShapeGrammarLevelDesign(LevelDevelopmentKit ldk) : base(ldk)
        {
        }



        public override LevelElement CreateLevel()
        {
            var pr = new Productions(ldk, new Symbols());
            ProductionProgram.pr = pr;
            ProductionProgram.ldk = ldk;
            ProductionProgram.StyleRules = ldk.houseStyleRules;

            var grammarState = new ShapeGrammarState(ldk);
            var prL = new ProductionLists(ldk, pr);

            var guideRandomly = new RandomPathGuide();
            var guideToPoint = new PointPathGuide(grammarState, state => new Vector3Int(0, 0, 50));
            var guideBack = new PointPathGuide(grammarState, 
                state =>
                {
                    // todo: fix null reference exception
                    var returnToNodes = state.WithSymbols(pr.sym.ReturnToMarker);
                    var currentNodesCenter = state.LastCreated.Select(node => node.LE).ToLevelGroupElement(ldk.grid).CG().Center();
                    var targetPoint = returnToNodes.SelectMany(n => n.LE.Cubes()).ArgMin(cube => (cube.Position - currentNodesCenter).AbsSum()).Position;
                    return Vector3Int.RoundToInt(targetPoint);
                });

            var targetedLowGarden = prL.GuidedGarden(guideBack);

            //var shapeGrammar = new CustomGrammarEvaluator(productionList, 20, null, state => state.LastCreated);


            var gardenGrammar =
                new GrammarSequence()
                    .SetStartHandler(
                        state =>state
                            .LastCreated.SelectMany(node => node.AllDerivedFrom()).Distinct()
                            .ForEach(parent => parent.AddSymbol(pr.sym.ReturnToMarker))
                    )
                    .AppendLinear(
                        new ProductionList(pr.GardenFromCourtyard()),
                        1, pr.sym.Courtyard
                    )
                    .AppendLinear(
                        prL.Garden(),
                        5, pr.sym.Courtyard
                    )
                    .AppendLinear(
                        targetedLowGarden,
                        5, pr.sym.Courtyard
                    )
                    .AppendStartEnd(
                        pr.sym,
                        prL.ConnectBack(),
                        state => state.LastCreated,
                        state => state.WithSymbols(pr.sym.ReturnToMarker)
                    )
                    .SetEndHandler(
                        state => state.Root.AllDerived().ForEach(parent => parent.RemoveSymbolByName(pr.sym.ReturnToMarker))
                    );

            var shapeGrammar = new RandomGrammar(prL.TestingProductions(), 20);
            var randGardenGrammar = new RandomGrammar(prL.Garden(), 1);

            var newNodes = grammarState.ApplyProduction(pr.CreateNewHouse());
            randGardenGrammar.Evaluate(grammarState);
            /*var graveyardGrammar = new RandomGrammar(prL.Graveyard(pr), 10);
            var graveyardPostprocessGrammar = new AllGrammar(prL.GraveyardPostprocess(pr));
            var roofGrammar = new AllGrammar(prL.Roofs(pr));

            var newNodes = grammarState.ApplyProduction(pr.CreateNewHouse());
            shapeGrammar.Evaluate(grammarState);

            gardenGrammar.Evaluate(grammarState);
            

            //graveyardGrammar.Evaluate(grammarState);
            //graveyardPostprocessGrammar.Evaluate(grammarState);

            roofGrammar.Evaluate(grammarState);
            */

            grammarState.Print(new PrintingState()).Show();
            grammarState.Stats.Print();
            //grammarState.ShowVerticallyTaken(ldk.houseStyleRules);

            var level = grammarState.WorldState.Added;
            return level;
        }
    }
}
