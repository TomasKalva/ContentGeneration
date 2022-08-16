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
using static Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions.SelectorLibrary;
using static InteractiveObject;

namespace Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage
{
    class AscendingLanguage : LDLanguage
    {
        public AscendingLanguage(LanguageParams tools) : base(tools) { }


        public void AscendingBranch(Func<int> startingAscensionPrice)
        {
            Env.One(Gr.PrL.Garden(), NodesQueries.All, out var ascending_area);

            var statsIncreases = CharacterStats.StatIncreases();

            int ascensionPrice = startingAscensionPrice();

            Func<StatManipulation<Action<CharacterStats>>, InteractOption<Kiln>> increaseOption = null; // Declare function before calling it recursively
            increaseOption =
                statIncrease => new InteractOption<Kiln>($"{statIncrease.Stat}",
                    (kiln, player) =>
                    {
                        if (player.Pay(ascensionPrice))
                        {
                            statIncrease.Manipulate(player.Stats);
                            ascensionPrice += 50;
                            kiln.IntObj.BurstFire();
                            kiln.Interaction =
                                new InteractionSequence<Kiln>()
                                .Say("Do you desire further ascending?")
                                .Decision($"What ascension are you longing for? ({ascensionPrice} Spirit)",
                                    statsIncreases.Shuffle().Take(3).Select(si => increaseOption(si)).ToArray());
                        }
                    });

            Env.One(Gr.PrL.Garden(), NodesQueries.LastCreated, out var farmer_area);
            farmer_area.Get.AddInteractiveObject(
                Lib.InteractiveObjects.InteractiveObject("Farmer", Lib.InteractiveObjects.Geometry<Kiln>(Lib.InteractiveObjects.ascensionKilnPrefab.transform))
                    .SetInteraction(
                        new InteractionSequence<Kiln>()
                            .Say("Ascension kiln is glad to feel you")
                            .Decision($"What ascension are you longing for? ({ascensionPrice} Spirit)",
                                statsIncreases.Shuffle().Take(3).Select(si => increaseOption(si)).ToArray())
                    )
                );

            // Add the same branch to the next level
            State.LC.AddEvent(
                new LevelConstructionEvent(90,
                () =>
                {
                    L.AscendingLanguage.AscendingBranch(() => ascensionPrice);
                    return true;
                }));
        }
    }
}
