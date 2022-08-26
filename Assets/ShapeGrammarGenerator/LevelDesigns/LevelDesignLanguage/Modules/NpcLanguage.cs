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

    class NpcLanguage : LDLanguage
    {
        public NpcLanguage(LanguageParams tools) : base(tools) { }

        public void StartNonPersistentNpcLines()
        {
            Enumerable.Range(0, 6).ForEach(i => 
                State.LC.AddPossibleEvent($"Npc {i}", 0, 
                    _ =>
                    {
                        Env.One(Gr.PrL.Garden(), NodesQueries.All, out var area);
                        area.AreasList[0].AddInteractiveObject(
                            Lib.InteractiveObjects.InteractiveObject<Kiln>($"Npc {i}", Lib.InteractiveObjects.Geometry<Kiln>(Lib.InteractiveObjects.ascensionKilnPrefab.transform))
                                .SetInteraction(
                                    ins => ins
                                        .Say($"I'm npc {i}")
                                )
                            );
                        }
                    )
                );
        }

        public void StartPersistentNpcLines()
        {
            // Npc appears

            Enumerable.Range(0, 6).ForEach(i =>
                State.LC.AddPossibleEvent($"Npc {i}", 0,
                    _ =>
                    {
                        Env.One(Gr.PrL.Garden(), NodesQueries.All, out var area);
                        area.AreasList[0].AddInteractiveObject(
                            Lib.InteractiveObjects.InteractiveObject<Kiln>($"Npc {i}", Lib.InteractiveObjects.Geometry<Kiln>(Lib.InteractiveObjects.ascensionKilnPrefab.transform))
                                .SetInteraction(
                                    ins => ins
                                        .Say($"I'm npc {i}")
                                )
                            );
                    }, 
                    true)
                );

            // Hello, how are you. I'm the lord of cinder or something. I travel to the desert of language ambiguity.

            // Wow you did something. I didn't think you'd do that. Now take this. I ressume my journey.

            // How could that be. They did the surgery on grape. I'm leaving for blue mountain underneath.

        }

        public void TestLocking()
        {

            L.PatternLanguage.BranchWithKey(NodesQueries.LastCreated, 1, Gr.PrL.Garden(), out var lockedArea, out var linearPath);

        }

    }
}
