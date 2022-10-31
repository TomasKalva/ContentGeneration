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
            State.LC.AddNecessaryEvent("Start", 100, _ => Main());
            //State.LC.AddNecessaryEvent("Player initialization", 90, _ => InitializePlayer());
            State.LC.AddNecessaryEvent("End", 99, _ => M.LevelModule.LevelEnd());// throws sequence contains no elements
        }

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

            var itemPlacer = PlO.RandomAreasPlacer(new UniformDistr(3, 6), () => Lib.InteractiveObjects.Item(HealthPotion()));
            itemPlacer.Place(line);
            
        }

        void AddRoofs()
        {
            Env.Execute(new AllGrammar(Gr.PrL.Roofs()));
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

        void CustomizePlayer()
        {
            var playerState = State.World.PlayerState;
            playerState
                .AddOnDeath(() =>
                {
                    GameObject.Destroy(playerState.Agent.gameObject);
                    State.GC.ResetLevel(5.0f);
                });

            /*playerState
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
                        }));*/
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
