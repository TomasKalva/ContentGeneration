﻿using OurFramework.Environment.ShapeGrammar;
using System.Linq;
using ContentGeneration.Assets.UI.Model;
using UnityEngine;
using Util;

namespace OurFramework.LevelDesignLanguage.CustomModules
{
    class TutorialModule : LDLanguage
    {
        public TutorialModule(LanguageParams parameters) : base(parameters) { }
        
        public void Main()
        {
            LevelStart();
            LevelUpArea();
            LevelContinue();
            AddRoofs();

            CustomizePlayer();
        }

        void LevelStart()
        {
            Env.One(Gr.PrL.CreateNewHouse(), NodesQueries.All, out var area);
            area.Get.Node.AddSymbol(Gr.Sym.LevelStartMarker);
        }

        void LevelContinue()
        {
            Env.Line(Gr.PrL.Town(), NodesQueries.All, 5, out var line);
            /*
            var firstArea = line.AreasList.First();
            firstArea.AddEnemy(Lib.Enemies.MayanSwordsman());

            
            line.AreasList.ForEach(area =>
            {
                area.AddEnemy(Lib.Enemies.MayanSwordsman());
            });

            line.AreasList.ForEach(area =>
            {
                Enumerable.Range(0, new UniformDistr(1, 4).Sample()).ForEach(
                    _ => area.AddEnemy(Lib.Enemies.MayanSwordsman()));
            });

            var placer = PlC.RandomAreaPlacer(new UniformDistr(1, 2), Lib.Enemies.MayanSwordsman);
            placer.Place(line);
            */
            
            
            var placer = PlC.RandomAreaPlacer(
                new UniformDistr(1, 4), 
                () => Lib.Enemies.MayanSwordsman()
                    .SetLeftWeapon(Lib.Items.Katana())
                    .AddAndEquipItem(Lib.Items.Nails())
                    .AddAndEquipItem(Lib.Items.Nails())
                    .SetStats(
                     new CharacterStats() 
                     { 
                         Will = 10,
                         Endurance = 10,
                         Agility = 25
                     })
                    );
            
            placer.Place(line);

            /*
            Func<ItemState> chocolateBarF = () => Lib.Items.NewItem("Health potion", "Heals over time.")
                .SetStackable()
                .OnUse(Lib.Effects.RegenerateHealth(2f, 5f));
            State.World.PlayerState.AddItem(chocolateBarF());
            var itemPlacer = PlO.RandomAreasPlacer(new UniformDistr(3, 6), () => Lib.InteractiveObjects.Item(chocolateBarF()));
            itemPlacer.Place(line);
            */
        }

        void AddRoofs()
        {
            Env.Execute(new AllGrammar(Gr.PrL.Roofs()));
        }

        void CustomizePlayer()
        {
            var playerState = State.World.PlayerState;
            playerState
                .AddOnDeath(() =>
                {
                    GameObject.Destroy(playerState.Agent.gameObject);
                    State.GC.ResetLevel(5.0f);
                });

            playerState
                .SetStats(new CharacterStats()
                {
                    Agility = 10,
                    Strength = 10,
                })
                .SetRightWeapon(
                    Lib.Items.SculptureClub()
                        .AddUpgradeEffect(
                        user => enemy =>
                        {
                            Lib.Effects.Heal(10)(user);
                        }));

            var chocolateBar = Lib.Items.NewItem("Health potion", "Heals over time.")
                .SetStackable()
                .OnUse(Lib.Effects.RegenerateHealth(2f, 5f));

            // ok for debugging but can be added multiple times
            State.World.PlayerState.AddItem(chocolateBar);
        }

        void LevelUpArea()
        {
            Env.One(Gr.PrL.Garden(), NodesQueries.All, out var kilnArea);

            var kiln = Lib.InteractiveObjects.Kiln()
                    .SetInteraction(ins => ins
                        .Say("I am a talking kiln")
                        .Act("Do you understand?", (kiln, player) => kiln.Interaction.TryMoveNext(kiln))
                        .Decision("Would you like to increase stats?",
                            new InteractOption<Kiln>("Yes", (kiln, player) =>
                            {
                                CharacterStats.StatIncreases.ForEach(statIncrease => statIncrease.Manipulate(player.Stats));
                                kiln.SetInteraction(ins => ins.Say("Stats increased."));
                                kiln.IntObj.BurstFire();
                            }),
                            new InteractOption<Kiln>("No", (kiln, _) => kiln.Interaction.TryMoveNext(kiln))
                        )
                        .Say("Ok, maybe next time")
                    );
            kilnArea.AreasList.First().AddInteractiveObject(kiln);
        }
    }
}
