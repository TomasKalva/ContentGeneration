using ContentGeneration.Assets.UI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShapeGrammar
{
    class MyLanguage : LDLanguage
    {
        public MyLanguage(LanguageParams tools) : base(tools) { }

        public void MyWorldStart()
        {
            State.LC.AddEvent(100, () =>
            {
                L.LevelLanguage.LevelStart(out var startArea);
                return false;
            });

            State.LC.AddEvent(5, () =>
            {
                L.FarmersLanguage.FarmerBranch(0);
                return false;
            });

            State.LC.AddEvent(5, () =>
            {
                L.PatternLanguage.LockedDoor(NodesQueries.LastCreated, 4, Gr.PrL.TestingProductions());
                return false;
            });

            State.LC.AddEvent(0, () =>
            {
                L.LevelLanguage.LevelEnd();
                return false;
            });
            //L.TestingLanguage.LargeLevel();
        }
    }

    class LevelLanguage : LDLanguage
    {
        public LevelLanguage(LanguageParams tools) : base(tools) { }

        public void LevelStart(out Area area)
        {
            Env.One(Gr.PrL.CreateNewHouse(), NodesQueries.All, out area);
            area.Node.AddSymbol(Gr.Sym.LevelStartMarker);
        }

        public void LevelPathSegment()
        {

        }

        public void LevelEnd()
        {
            Env.One(Gr.PrL.LevelEnd(), NodesQueries.All, out var area);
            area.AddInteractiveObject(Lib.InteractiveObjects.Item(Lib.Items.NewItem("Done", "You have finished")));
        }


    }

    class PatternLanguage : LDLanguage
    {
        public PatternLanguage(LanguageParams tools) : base(tools) { }

        public void LockedDoor(NodesQuery startNodesQuery, int keyBranchLength, ProductionList keyBranchPr)
        {
            var branchNodes = startNodesQuery(State.GrammarState);
            Env.Line(keyBranchPr, startNodesQuery, keyBranchLength, out var keyBranch);
            keyBranch.LastArea().AddInteractiveObject(Lib.InteractiveObjects.Item(Lib.Items.NewItem("Key", "You have key now")));

            Env.One(Gr.PrL.BlockedByDoor(), _ => branchNodes, out var locked);
            locked.AddInteractiveObject(Lib.InteractiveObjects.Item(Lib.Items.NewItem("Unlocked", "The door are unlocked now")));
        }
    }

    class TestingLanguage : LDLanguage
    {
        public TestingLanguage(LanguageParams tools) : base(tools) { }

        public void LargeLevel()
        {
            var grammarState = State.GrammarState;

            var shapeGrammar = new RandomGrammar(Gr.PrL.TestingProductions(), 20);
            var randGardenGrammar = new RandomGrammar(Gr.PrL.Garden(), 1);
            var graveyardGrammar = new RandomGrammar(Gr.PrL.Graveyard(), 10);
            var graveyardPostprocessGrammar = new AllGrammar(Gr.PrL.GraveyardPostprocess());
            var roofGrammar = new AllGrammar(Gr.PrL.Roofs());

            Env.Execute(shapeGrammar);
            Env.Execute(randGardenGrammar);
            Env.Execute(graveyardGrammar);
            Env.Execute(graveyardPostprocessGrammar);
            Env.Execute(roofGrammar);

            var allAreas = State.TraversableAreas;
            //var objects = Enumerable.Range(0, 100)
            //.Select(_ => Lib.InteractiveObjects.Item(Lib.Items.FreeWill()));
            //.Select(_ => Lib.InteractiveObjects.AscensionKiln());
            //.Select(_ => Lib.InteractiveObjects.InteractiveObject<InteractiveObject>("bush", Lib.InteractiveObjects.Geometry<InteractiveObject>(Lib.Objects.farmer)));
            //objects.ForEach(obj => allAreas.GetRandom().AddInteractiveObject(obj));

            State.TraversableAreas
               .ForEach(
                   area => Enumerable.Range(0, 1)
                       .ForEach(_ => area.AddEnemy(Lib.Enemies.AllAgents().GetRandom()()))
               );
        }
    }

    class BrothersLanguage : LDLanguage
    {
        public BrothersLanguage(LanguageParams tools) : base(tools) { }

        public void ThymeTea()
        {

        }

        public void GiftOfHope()
        {

        }
    }

    class FarmersLanguage : LDLanguage
    {
        public FarmersLanguage(LanguageParams tools) : base(tools) { }

        public void FarmerBranch(int progress)
        {
            Env.Line(Gr.PrL.Garden(), NodesQueries.LastCreated, 2, out var path_to_farmer);

            Env.One(Gr.PrL.Garden(), NodesQueries.LastCreated, out var farmer_area);
            farmer_area.AddInteractiveObject(
                Lib.InteractiveObjects.InteractiveObject("Farmer", Lib.InteractiveObjects.Geometry<InteractiveObject>(Lib.Objects.farmer))
                    .SetInteraction(
                        new InteractionSequence<InteractiveObject>()
                            .Say("My name is Ted")
                            .Say("I a farmer")
                            .Say("I desire nourishment")
                            .Act("Confirm your understanding", (ios, _) => ios.Interaction.TryMoveNext(ios))
                            .Decision("Would you mind sharing some apples?",
                            new InteractOption<InteractiveObject>("Give apples",
                                (farmer, player) =>
                                {
                                    if (player.Inventory.HasItems("Earthen apple", 3, out var desiredApples))
                                    {
                                        player.Inventory.RemoveItems(desiredApples);
                                        Msg.Show("Apples given");

                                        // moves farmer to another state
                                        farmer.SetInteraction(
                                            new InteractionSequence<InteractiveObject>()
                                                .Say("Thanks for the apples, mate")
                                        );

                                        player.Prop.Spirit += 10 * (1 + progress);
                                        //Levels().Next().AddPossibleBranch(FarmerBranch(progress + 1);
                                    }
                                    else
                                    {
                                        Msg.Show("Not enough apples");
                                    }
                                }
                            , 0)
                        )
                    )
                );

            Env.ExtendRandomly(Gr.PrL.Garden(), NodesQueries.LastCreated, 5, out var garden);
            var apples = Enumerable.Range(0, 5).Select(_ =>
                Lib.Items.NewItem("Earthen apple", "An apple produced by the earth itself.")
                    .OnUse(ch => ch.Prop.Spirit += 10)
                    .SetConsumable()
                )
                .Select(itemState => Lib.InteractiveObjects.Item(itemState));
            var applePlacer = PlO.EvenPlacer(apples);
            applePlacer.Place(garden.Areas);

            var enemyPlacer = PlC.RandomPlacer(
                        new RandomIntDistr(1, 1),
                        (3, Lib.Enemies.Dog),
                        (3, Lib.Enemies.Human),
                        (1, Lib.Enemies.Sculpture));
            enemyPlacer.Place(path_to_farmer.Areas.Concat(garden.Areas));
        }
    }
}
