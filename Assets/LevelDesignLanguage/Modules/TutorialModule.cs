using OurFramework.Environment.ShapeGrammar;
using System.Linq;
using ContentGeneration.Assets.UI.Model;
using UnityEngine;
using Util;
using System;

namespace OurFramework.LevelDesignLanguage.CustomModules
{
    class TutorialModule : LDLanguage
    {
        public TutorialModule(LanguageParams parameters) : base(parameters) { }
        
        public void DeclareGame()
        {
            State.LC.AddNecessaryEvent($"Level Start", 100, level => M.LevelModule.LevelStart(), true);
            State.LC.AddNecessaryEvent("Main", 95, _ => Main(), true);
            State.LC.AddNecessaryEvent("Player initialization", 90, _ => InitializePlayer());
            State.LC.AddNecessaryEvent("Enable death", 90, _ => M.DeathModule.DieClasically(), true);
            State.LC.AddNecessaryEvent("Roofs", -1, _ => M.LevelModule.AddRoofs(), true);
            //State.LC.AddNecessaryEvent("End", 99, _ => M.LevelModule.LevelEnd(), true);
        }

        public void Main()
        {
            LevelUpArea();
            LevelContinue();
        }

        void LevelContinue()
        {
            Env.Line(Gr.PrL.Town(), NodesQueries.All, 5, out var line);
            
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

            var itemPlacer = PlO.RandomAreasPlacer(new UniformDistr(3, 6), () => Lib.InteractiveObjects.Item(HealthPotion()));
            itemPlacer.Place(line);


            var end = line.LastArea();
            end.AddInteractiveObject(
                Lib.InteractiveObjects.Transporter(State.GC)
                );
        }

        void InitializePlayer()
        {
            var playerState = State.World.PlayerState;
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
        }

        public ItemState HealthPotion()
        {
            return Lib.Items.NewItem("Health potion", "Heals over time.")
                .OnUse(Lib.Effects.RegenerateHealth(2f, 5f))
                .SetStackable();
        }

        void LevelUpArea()
        {
            Env.One(Gr.PrL.Garden(), NodesQueries.All, out var kilnArea);

            var kiln = Lib.InteractiveObjects.Kiln()
                    .SetInteraction(ins => ins
                        .Say("I am a levelling up kiln.")
                        .Interact("Do you want to level up?", 
                            (kiln, player) =>
                            {
                                CharacterStats.StatIncreases.ForEach(statIncrease => statIncrease.Manipulate(player.Stats));
                                kiln.SetInteraction(ins => ins.Say("Levelled up."));
                                kiln.IntObj.BurstFire();
                            })
                    );
            kilnArea.AreasList.First().AddInteractiveObject(kiln);
        }
    }
}
