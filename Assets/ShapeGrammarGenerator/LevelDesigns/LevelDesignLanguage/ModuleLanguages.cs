﻿using ContentGeneration.Assets.UI.Model;
using System;
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
            var gardenEnemies = new WeightedDistribution<Func<CharacterState>>(
                    (3, Lib.Enemies.Dog),
                    (3, Lib.Enemies.Human),
                    (1, Lib.Enemies.Sculpture)
                );
            /*var gardenEnemies = new List<Func<CharacterState>>()
            {
                Lib.Enemies.Dog,
                Lib.Enemies.Sculpture,
                Lib.Enemies.Human,
            };*/

            Env.AddLine(Gr.PrL.Garden(), 2, out var path_to_farmer);
            Env.AddOne(Gr.PrL.Garden(), out var farmer_area);

            var apples = Enumerable.Range(0, 5).Select(_ => 
                Lib.Items.NewItem("Earthen apple", "An apple produced by the earth itself.")
                    .OnUse(ch => ch.Prop.Spirit += 10 )
                    .SetConsumable()
                );

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
            Env.AddRandom(Gr.PrL.Garden(), 5, out var garden);
            apples.ForEach(apple =>
                farmer_area.AddInteractiveObject(
                        Lib.InteractiveObjects.Item(apple)
                    )
            );

            path_to_farmer.Areas.Concat(garden.Areas)
                .ForEach(
                    area => Enumerable.Range(1, 1 + UnityEngine.Random.Range(0, 3))
                        .ForEach(_ => area.AddEnemy(gardenEnemies.Sample()()))
                );
            //);
        }
    }
}
