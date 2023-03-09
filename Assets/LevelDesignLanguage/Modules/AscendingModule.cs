using OurFramework.Environment.ShapeGrammar;
using OurFramework.Gameplay.Data;
using OurFramework.Gameplay.RealWorld;
using OurFramework.Util;
using System;
using System.Linq;

namespace OurFramework.LevelDesignLanguage.CustomModules
{
    class AscendingModule : LDLanguage
    {
        public AscendingModule(LanguageParams parameters) : base(parameters) { }

        /// <summary>
        /// Returns a kiln that let's the player increase the stats. The price of ascending depends on number of already done ascendings by ascensionPriceF.
        /// </summary>
        public InteractiveObjectState<Kiln> AscendingKiln(Func<int, int> ascensionPriceF)
        {
            int numberOfAscendingsDone = 0;
            int ascensionPrice = ascensionPriceF(numberOfAscendingsDone);
            var statsIncreases = CharacterStats.StatIncreases;
            // Declare function before calling it recursively
            Func<StatManipulation<Action<CharacterStats>>, InteractOption<Kiln>> increaseOption = null; 
            increaseOption =
                statIncrease => new InteractOption<Kiln>($"{statIncrease.Stat}",
                    (kiln, player) =>
                    {
                        if (player.Pay(ascensionPrice))
                        {
                            statIncrease.Manipulate(player.Stats);
                            numberOfAscendingsDone++;
                            ascensionPrice = ascensionPriceF(numberOfAscendingsDone);
                            kiln.IntObj.BurstFire();
                            kiln.Interaction =
                                new InteractionSequence<Kiln>()
                                .Say("Do you desire further ascending?")
                                .Decide($"What ascension are you longing for? ({ascensionPrice} Spirit)",
                                    statsIncreases.Shuffle().Take(3).Select(si => increaseOption(si)).ToArray());
                        }
                    });

            return Lib.InteractiveObjects.Kiln()
                    .SetInteraction(
                        ins => ins
                            .Say("Ascension kiln is delighted to feel your presence.")
                            .Decide($"What ascension are you longing for? ({ascensionPrice} Spirit)",
                                statsIncreases.Shuffle().Take(3).Select(si => increaseOption(si)).ToArray())
                    );
        }

        /// <summary>
        /// Creates an area with the ascending kiln.
        /// </summary>
        public void AddAscendingEvents(InteractiveObjectState<Kiln> kiln)
        {
            // Add the same branch to the next level
            State.LC.AddNecessaryEvent($"Ascending branch", 
                90, _ =>
                {
                    Env.One(Gr.PrL.Garden(), NodesQueries.All, out var ascendingArea);
                    ascendingArea.Area.AddInteractiveObject(kiln);
                }, true);
        }
    }
}
