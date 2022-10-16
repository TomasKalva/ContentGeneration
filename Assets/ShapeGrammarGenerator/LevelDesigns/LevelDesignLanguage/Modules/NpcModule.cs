using Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions;
using Assets.Util;
using ContentGeneration.Assets.UI;
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

    class NpcModule : LDLanguage
    {
        public NpcModule(LanguageParams parameters) : base(parameters) { }
        
        public void PutNpcToNewArea(InteractiveObjectState npc)
        {
            Env.One(Gr.PrL.Garden(), NodesQueries.All, out var area);
            area.AreasList[0].AddInteractiveObject(npc);
        }
        
        public void ContinueNpc<InteractiveObjectT>(
            InteractiveObjectState<InteractiveObjectT> npc, 
            Action<InteractiveObjectState<InteractiveObjectT>> initialize,
            LevelConstructionCondition condition) 
            where InteractiveObjectT : InteractiveObject
        {
            State.LC.AddPossibleEvent($"{npc.Name}", 0, _ =>
            {
                Env.One(Gr.PrL.Garden(), NodesQueries.All, out var area);
                area.AreasList[0].AddInteractiveObject(npc);
                initialize(npc);
            }, 
            false,
            condition);
        }

        public void DewStart()
        {
            var npc = Lib.InteractiveObjects.Kiln();
            ContinueNpc(
                npc, 
                npc => npc.SetInteraction(
                        ins => ins
                            .Say($"I'm an npc.")
                            .Decision("Could you go and collect dew for me?",
                                new InteractOption<Kiln>("Yes",
                                    (thisNpc, player) =>
                                    {
                                        thisNpc.SetInteraction(
                                            ins => ins.Say("Very well then. I'll see you later.")
                                            );

                                        ContinueNpc(
                                            thisNpc,
                                            DewEnd,
                                            _ => player.Inventory.HasItems("Dew", 3, out var _)
                                        );
                                    }
                                )
                    )
                ),
                _ => true
            );
        }

        public void DewEnd<InteractiveObjectT>(InteractiveObjectState<InteractiveObjectT> npc) where InteractiveObjectT : InteractiveObject
        {
            npc.SetInteraction(
                ins => ins
                    .Say("You found the dew")
                    .Say("It is wet. Maybe too much.")
                    .Decision("Would you mind sharing some?",
                        new InteractOption<InteractiveObjectT>("Give one dew",
                            (thisNpc, player) =>
                            {
                                if (!player.Inventory.TryPay("Dew", 1, () => Msg.Show("Dew given"), () => Msg.Show("Not enough Dew")))
                                    return;

                                thisNpc.SetInteraction(
                                    ins => ins
                                        .Say("Good dew is a good dew")
                                );
                                player.Inventory.AddItem(
                                    Lib.SpellItems.Refreshment()
                                        .SetName("Dew refreshment")
                                        .SetReplenishable(2)
                                    );
                            }
                        ),
                        new InteractOption<InteractiveObjectT>("Give two dews",
                            (thisNpc, player) =>
                            {
                                if (!player.Inventory.TryPay("Dew", 2, () => Msg.Show("Dew given"), () => Msg.Show("Not enough Dew")))
                                    return;

                                thisNpc.SetInteraction(
                                    ins => ins
                                        .Say("A wonderful dew.")
                                        .Say("Much appreciated.")
                                );
                                player.Inventory.AddItem(
                                    Lib.SpellItems.Replenishment()
                                        .SetName("Dew replenishment")
                                        .SetReplenishable(2)
                                    );
                            }
                        )
                    )
                );
        }
        /*
        public void PutNpcToNewArea(InteractiveObjectState npc)
        {
            Env.One(Gr.PrL.Garden(), NodesQueries.All, out var area);
            area.AreasList[0].AddInteractiveObject(npc);
        }*/

        public void InitializeNpcs()
        {
            DewStart();

            // Npc appears

            // Hello, how are you. I'm the lord of cinder or something. I travel to the desert of language ambiguity.

            // Wow you did something. I didn't think you'd do that. Now take this. I ressume my journey.

            // How could that be. They did the surgery on grape. I'm leaving for blue mountain underneath.
        }
    }
}
