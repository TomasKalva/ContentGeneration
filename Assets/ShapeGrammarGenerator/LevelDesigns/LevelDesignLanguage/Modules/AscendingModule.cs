using Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions;
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
using static InteractiveObject;

namespace Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage
{
    class AscendingModule : LDLanguage
    {
        public AscendingModule(LanguageParams parameters) : base(parameters) { }


        public void AscendingBranch(Func<int> startingAscensionPrice)
        {
            Env.One(Gr.PrL.Garden(), NodesQueries.All, out var ascending_area);

            int ascensionPrice = startingAscensionPrice();
            int ascensionPriceIncrease = 50;

            var statsIncreases = CharacterStats.StatIncreases;
            Func<StatManipulation<Action<CharacterStats>>, InteractOption<Kiln>> increaseOption = null; // Declare function before calling it recursively
            increaseOption =
                statIncrease => new InteractOption<Kiln>($"{statIncrease.Stat}",
                    (kiln, player) =>
                    {
                        if (player.Pay(ascensionPrice))
                        {
                            statIncrease.Manipulate(player.Stats);
                            ascensionPrice += ascensionPriceIncrease;
                            kiln.IntObj.BurstFire();
                            kiln.Interaction =
                                new InteractionSequence<Kiln>()
                                .Say("Do you desire further ascending?")
                                .Decision($"What ascension are you longing for? ({ascensionPrice} Spirit)",
                                    statsIncreases.Shuffle().Take(3).Select(si => increaseOption(si)).ToArray());
                        }
                    });

            //Env.One(Gr.PrL.Garden(), NodesQueries.LastCreated, out var farmer_area);
            Enumerable.Range(0, 1).ForEach(_ => ascending_area.Get.AddInteractiveObject(
                Lib.InteractiveObjects.Kiln()
                    .SetInteraction(
                        ins => ins
                            .Say("Ascension kiln is delighted to feel your presence.")
                            .Decision($"What ascension are you longing for? ({ascensionPrice} Spirit)",
                                statsIncreases.Shuffle().Take(3).Select(si => increaseOption(si)).ToArray())
                    )
                )
            );

            // Add the same branch to the next level
            State.LC.AddNecessaryEvent($"Ascending branch", 90, _ => M.AscendingModule.AscendingBranch(() => ascensionPrice));
        }
    }
}
