using OurFramework.Environment.ShapeGrammar;
using System.Linq;
using ContentGeneration.Assets.UI.Model;
using UnityEngine;
using Util;
using System;
using System.Collections.Generic;
using ContentGeneration.Assets.UI;
using OurFramework.Gameplay.RealWorld;

namespace OurFramework.LevelDesignLanguage.CustomModules
{
    class TutorialModule4 : LDLanguage
    {
        public TutorialModule4(LanguageParams parameters) : base(parameters) { }

        public void DeclareGame()
        {
            State.LC.AddNecessaryEvent($"Level Start", 100, level => M.LevelModule.LevelStart(), true);
            State.LC.AddNecessaryEvent("Main", 95, _ => Main(), true);
            State.LC.AddNecessaryEvent("Player initialization", 90, _ => InitializePlayer());
            State.LC.AddNecessaryEvent("Enable death", 90, _ => M.DeathModule.DieClasically(), true);
            State.LC.AddNecessaryEvent("Roofs", -1, _ => M.LevelModule.AddRoofs(), true);
            State.LC.AddNecessaryEvent($"Sky", 0, level => M.EnvironmentModule.CreateSky(level), true);

            State.LC.AddPossibleEvent("Quest", 50, _ => StartQuest());

            //AddNpcEvents();
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

        void AddNpcEvents()
        {
            RandomNPCs().Shuffle().ForEach(npc =>
            {
                State.LC.AddPossibleEvent("Npc", 50, _ =>
                {
                    Env.One(Gr.PrL.Town(), NodesQueries.All, out var npcArea);
                    npcArea.Area.AddInteractiveObject(npc);
                });
            });
        }

        IEnumerable<InteractiveObjectState> RandomNPCs()
        {
            var names = new[] { "Amanda", "Bolton", "Coffey", "Darcie" };
            var places = new[] { "America", "Azeroth", "Moon" };
            var itemFs = Lib.Items.AllHeadItems();

            return
                names.SelectMany(name =>
                places.SelectMany(place =>
                itemFs.Select(itemF =>
                {
                    var item = itemF();
                    int price = 100;
                    return Lib.InteractiveObjects.Farmer()
                            .SetInteraction(ins => ins
                                .Say($"My greetings, I'm {name} and I come from {place}")
                                .Decide($"May I offer you a {item.Name}?",
                                    opt => opt
                                        .SetDescription($"Yes (pay {price} spirit)")
                                        .SetAction(
                                            (npc, player) =>
                                            {
                                                if (player.Pay(price))
                                                {
                                                    player.AddItem(item);
                                                    npc.SetInteraction(ins => ins.Say("I wish you luck on your journey."));
                                                }
                                                else
                                                {
                                                    Msg.Show("Not enough spirit");
                                                }
                                            }),
                                    opt => opt
                                        .SetDescription($"No")
                                        .SetAction(
                                            (npc, player) =>
                                            {
                                                npc.Interaction.TryMoveNext(npc);
                                            })
                                    )
                                .Say("Ok, fine, bye.")
                            );
                })));
        }


        void StartQuest()
        {
            // Create an environment with an item that starts the quest
            Env.One(Gr.PrL.Town(), NodesQueries.All, out var area);
            var questStarter = Lib.Items.NewItem("Dragon Egg", "Crack to unleash a dragon.")
                .SetConsumable()
                .OnUse(ch => {
                    Msg.Show("A dragon has been unleashed.");
                    State.LC.AddPossibleEvent("Dragon quest", 50,
                        level => ContinueQuest());
                });
            area.Area.AddInteractiveObject(Lib.InteractiveObjects.Item(questStarter));
        }

        void ContinueQuest()
        {
            // Create a locked room and a path to its key
            M.LockingModule.LineWithKey(NodesQueries.All, 4, Gr.PrL.Garden(), out var locked, out var keyLine);

            // Place enemies
            var enemyPlacer = PlC.RandomAreaPlacer(new UniformDistr(1, 3), Enemies());
            enemyPlacer.Place(keyLine);


            // Place spell items
            var spellPlacer = PlO.RandomAreasPlacer(new UniformDistr(4, 6),
                () => Lib.InteractiveObjects.Item(
                    Lib.SpellItems.AllSpellsByPower()[1].GetRandom()().SetStackable()));
            spellPlacer.Place(keyLine);

            // Create dragon state so that we can use it when defining sword that defeats it
            var dragon = Lib.Enemies.DragonMan();


            // Deal large damage to dragon using a strong sword
            // The dragon whose reference we use doesn't exist physically in this level yet
            var powerSword = Lib.Items.MayanSword()
                .AddUpgradeEffect(
                user => enemy =>
                {
                    if (enemy == dragon && user.Stats.Will > 0)
                    {
                        Lib.Effects.Damage(new DamageDealt(DamageType.Chaos, 100))(enemy);
                        user.Stats.Will--;
                    }
                })
                .SetName("Dragon Slayer")
                .SetDescription("A powerful weapon whose mission is to slay its chosen dragon. Only those who posses enough Will are fit to carry it and even then it takes its toll.");
            locked.Area.AddInteractiveObject(Lib.InteractiveObjects.Item(powerSword));


            // Continue the quest
            var questContinuer = Lib.Items.NewItem("Dragon Scale", "Caressing dragon scales brings luck.")
                 .SetConsumable()
                 .OnUse(ch =>
                 {
                     Msg.Show("The dragon grows.");
                     State.LC.AddPossibleEvent("Dragon quest 2", 50,
                        level => EndQuest(dragon));// Pass the dragon to the next level
                 });
            locked.Area.AddInteractiveObject(Lib.InteractiveObjects.Item(questContinuer));

        }

        Func<CharacterState>[] Enemies()
        {
            return new Func<CharacterState>[]
               {
                Lib.Enemies.MayanSwordsman,
                Lib.Enemies.MayanThrower,
                Lib.Enemies.SkinnyWoman,
               };
        }

        void EndQuest(CharacterState dragon)
        {
            // Place the powerful dragon to the level
            Env.One(Gr.PrL.Town(), NodesQueries.All, out var area);
            dragon
                .DropItem(
                    () => Lib.InteractiveObjects.Item(
                        Lib.Items.NewItem("Dragons Demise", "The dragon fades, but its Will lives on.")
                            .OnUse(ch => ch.Stats.Will += 5))
                );
            dragon.Stats.Will = 40;
            area.Area.AddEnemy(dragon);

            // Weaken the dragon using an interactive object
            Env.Line(Gr.PrL.Castle(), NodesQueries.All, 4, out var pathToGoblet);
            var goblet = Lib.InteractiveObjects.SpikyGoblet()
                .SetInteraction(ins => ins
                    .Interact("Touch", (goblet, player) =>
                    {
                        dragon.Stats.Will = 10;
                        Msg.Show("Dragon weakened");
                        goblet.SetInteraction(ins => ins.Say("Dragon weakened"));
                    })
                );
            pathToGoblet.LastArea().AddInteractiveObject(goblet);

            // Place enemies
            var enemyPlacer = PlC.RandomAreaPlacer(new UniformDistr(1, 3), Enemies());
            enemyPlacer.Place(pathToGoblet);

            // Place stronger spell items
            var spellPlacer = PlO.RandomAreasPlacer(new UniformDistr(2, 4),
                () => Lib.InteractiveObjects.Item(
                    Lib.SpellItems.AllSpellsByPower()[2].GetRandom()().SetStackable()));
            spellPlacer.Place(pathToGoblet);
        }
    }
}
