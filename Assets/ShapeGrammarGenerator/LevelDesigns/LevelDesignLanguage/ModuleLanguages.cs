﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShapeGrammar
{
    class LevelLanguage : LDLanguage
    {
        public LevelLanguage(LanguageParams tools) : base(tools) { }

        public void LevelStart(out Area area)
        {
            Env.AddOne(Gr.PrL.CreateNewHouse(), out area);
        }

        public void LevelPathSegment()
        {

        }

        public void LevelEnd()
        {

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
            //Env.AddLine(Gr.PrL.Garden(), 2, out var path_to_farmer);
            Env.AddOne(Gr.PrL.Garden(), out var farmer_area);
            var apples = Enumerable.Range(0, 5).Select(_ => 
                Lib.Items.NewItem("Earthen apple", "An apple produced by the earth itself.")
                    .OnUse(ch =>
                    {
                        ch.Properties.Spirit += 10;
                        // consume the item
                        //ch.Inventory.DropItem()
                    })
                
                );
            var applesGiven = false;
            farmer_area.AddInteractiveObject(
                Lib.InteractiveObjects.NewInteractiveObject("Farmer", Lib.InteractiveObjects.Geometry<InteractiveObject>(Lib.Objects.farmer))
                    .Description("I desire nourishment.")
                    .Interact(
                        (farmer, player) =>
                        {
                            if (!applesGiven && player.Inventory.HasItems("Earthen apple", 3, out var apples))
                            {
                                Msg.Say("Apples given");
                                farmer.Description("Thanks for the apples, mate");
                                applesGiven = true;

                                player.Properties.Spirit += 10 * (1 + progress);
                                //Levels().Next().AddPossibleBranch(FarmerBranch(progress + 1);
                            }
                        }
                    )
                );
            //Env.AddRandom(Gr.PrL.Garden(), 5, out var garden);
            //garden.Areas.ForEach(area =>
            apples.ForEach(apple =>
                farmer_area.AddInteractiveObject(
                        Lib.InteractiveObjects.Item(apple)
                    )
            );
            //);
        }
    }
}
