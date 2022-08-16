﻿using Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions;
using Assets.Util;
using ContentGeneration.Assets.UI.Model;
using ContentGeneration.Assets.UI.Util;
using ShapeGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions.SelectorLibrary;
using static InteractiveObject;

namespace Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage
{

    class TestingLanguage : LDLanguage
    {
        public TestingLanguage(LanguageParams tools) : base(tools) { }

        public void LargeLevel()
        {
            var grammarState = State.GrammarState;

            var shapeGrammar = new RandomGrammar(Gr.PrL.Town(), 20);
            var randGardenGrammar = new RandomGrammar(Gr.PrL.Garden(), 1);
            var graveyardGrammar = new RandomGrammar(Gr.PrL.Chapels(), 10);
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

        public void StatsScalingOfEnemies()
        {
            Env.Line(Gr.PrL.Garden(), NodesQueries.All, Lib.Enemies.AllAgents().Count(), out var areas);

            var allEnemiesPlacer = PlC.EvenPlacer(
                Lib.Enemies.AllAgents().Select(chF =>
                {
                    var enemy = chF();
                    var stats = new CharacterStats()
                    {
                        Will = 5,
                        Strength = 5,
                        Endurance = 5,
                        Agility = 10,
                        Posture = 5,
                        Resistances = 5,
                        Versatility = 5
                    };
                    enemy.Stats = stats;

                    return enemy;
                })
            );

            allEnemiesPlacer.Place(areas);
        }

        public void LevellingUpItems()
        {
            Env.One(Gr.PrL.Garden(), NodesQueries.LastCreated, out var level_up_area);

            var statIncreaseItems = CharacterStats.StatIncreases().Select(statIncrease =>
                new ItemState()
                {
                    Name = statIncrease.Stat.ToString(),
                    Description = $"Increases {statIncrease.Stat}"
                }
                .OnUse(player => statIncrease.Manipulate(player.Stats))
            ).ToArray();

            level_up_area.Get.AddInteractiveObject(
                Lib.InteractiveObjects.InteractiveObject("Levelling up object", Lib.InteractiveObjects.Geometry<InteractiveObject>(Lib.Objects.farmer))
                    .SetInteraction(
                        new InteractionSequence<InteractiveObject>()
                            .Act("Take levelling up items", (ios, player) => statIncreaseItems.ForEach(item => player.AddItem(item))
                            )
                        )
                    );
        }

        public void Spells()
        {
            Env.One(Gr.PrL.Garden(), NodesQueries.All, out var area);

            var spells = new Spells(Lib.Effects, Lib.Selectors, Lib.VFXs);
            var spellItems = new SpellItems(spells, Lib.VFXs);
            Func<ItemState>[] s = spellItems.AllSpellItems()
                .Select<Func<ItemState>, Func<ItemState>>(itemF => () => itemF().SetReplenishable(1)).ToArray();

            area.Get.AddInteractiveObject(
                Lib.InteractiveObjects.InteractiveObject("Spells object", Lib.InteractiveObjects.Geometry<InteractiveObject>(Lib.Objects.farmer))
                    .SetInteraction(
                        new InteractionSequence<InteractiveObject>()
                            .Act("Take all spells", (ios, player) => s.ForEach(itemF => player.AddItem(itemF()))
                            )
                        )
                    );
        }

        public void GrammarTesting()
        {
            var grammarState = State.GrammarState;

            //var shapeGrammar = new RandomGrammar(Gr.PrL.TestingProductions(), 20);
            /*var randGardenGrammar = new RandomGrammar(Gr.PrL.Garden(), 1);
            var graveyardGrammar = new RandomGrammar(Gr.PrL.Graveyard(), 10);
            var graveyardPostprocessGrammar = new AllGrammar(Gr.PrL.GraveyardPostprocess());
            var roofGrammar = new AllGrammar(Gr.PrL.Roofs());
            */

            //Env.Execute(shapeGrammar);
            /*Env.Execute(randGardenGrammar);
            Env.Execute(graveyardGrammar);
            Env.Execute(graveyardPostprocessGrammar);
            Env.Execute(roofGrammar);
            */

            //Env.Line(Gr.PrL.Town(), NodesQueries.All, 10, out var _);
            Env.Line(Gr.PrL.Chapels(), NodesQueries.All, 40, out var _);
            //Env.Line(Gr.PrL.Castle(), NodesQueries.All, 10, out var _);
            //Env.BranchRandomly(Gr.PrL.Town(), 40, out var _);

            /*Env.Line(Gr.PrL.TestingProductions(), NodesQueries.All, 10, out var _);
            Env.Line(Gr.PrL.TestingProductions(), NodesQueries.All, 10, out var _);
            Env.Line(Gr.PrL.TestingProductions(), NodesQueries.All, 10, out var _);
            Env.Line(Gr.PrL.TestingProductions(), NodesQueries.All, 10, out var _);*/

            //Env.BranchRandomly(Gr.PrL.CastleDetails(), 8, out var _);
            Env.Execute(new AllGrammar(Gr.PrL.GraveyardPostprocess()));

            var allAreas = State.TraversableAreas;
            //var objects = Enumerable.Range(0, 100)
            //.Select(_ => Lib.InteractiveObjects.Item(Lib.Items.FreeWill()));
            //.Select(_ => Lib.InteractiveObjects.AscensionKiln());
            //.Select(_ => Lib.InteractiveObjects.InteractiveObject<InteractiveObject>("bush", Lib.InteractiveObjects.Geometry<InteractiveObject>(Lib.Objects.farmer)));
            //objects.ForEach(obj => allAreas.GetRandom().AddInteractiveObject(obj));

            /*
            State.TraversableAreas
               .ForEach(
                   area => Enumerable.Range(0, 1)
                       .ForEach(_ => area.AddEnemy(Lib.Enemies.AllAgents().GetRandom()()))
               );*/
        }
    }
}