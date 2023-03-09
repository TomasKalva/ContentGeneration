using OurFramework.Environment.ShapeGrammar;
using System.Linq;
using System.Collections.Generic;
using OurFramework.UI;
using OurFramework.Gameplay.Data;
using OurFramework.Util;
using OurFramework.Game;

namespace OurFramework.LevelDesignLanguage.CustomModules
{
    class TutorialModule3 : LDLanguage
    {
        public TutorialModule3(LanguageParams parameters) : base(parameters) { }

        public void DeclareGame()
        {
            State.LC.AddNecessaryEvent($"Level Start", 100, level => M.LevelModule.LevelStart(), true);
            State.LC.AddNecessaryEvent("Main", 95, _ => Main(), true);
            State.LC.AddNecessaryEvent("Player initialization", 90, _ => InitializePlayer());
            State.LC.AddNecessaryEvent("Enable death", 90, _ => M.DeathModule.DieClasically(), true);
            State.LC.AddNecessaryEvent("Roofs", -1, _ => M.LevelModule.AddRoofs(), true);
            State.LC.AddNecessaryEvent($"Sky", 0, level => M.EnvironmentModule.CreateSky(level), true);

            AddNpcEvents();
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
    }
}
