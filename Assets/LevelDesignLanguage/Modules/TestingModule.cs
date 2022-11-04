using ContentGeneration.Assets.UI.Model;
using OurFramework.Environment.ShapeGrammar;
using System;
using System.Linq;
using UnityEngine;
using Util;
using static OurFramework.LevelDesignLanguage.CustomModules.EnvironmentModule;

namespace OurFramework.LevelDesignLanguage.CustomModules
{

    class TestingModule : LDLanguage
    {
        public TestingModule(LanguageParams parameters) : base(parameters) { }

        /// <summary>
        /// Test creating a large environmnet.
        /// </summary>
        public void LargeLevel()
        {
            var grammarState = State.GrammarState;

            var shapeGrammar = new RandomGrammar(Gr.PrL.Town(), 20);
            var randGardenGrammar = new RandomGrammar(Gr.PrL.Garden(), 1);
            var graveyardGrammar = new RandomGrammar(Gr.PrL.Chapels(), 10);
            var graveyardPostprocessGrammar = new AllGrammar(Gr.PrL.ChapelsPostprocess());
            var roofGrammar = new AllGrammar(Gr.PrL.Roofs());

            Env.Execute(shapeGrammar);
            Env.Execute(randGardenGrammar);
            Env.Execute(graveyardGrammar);
            Env.Execute(graveyardPostprocessGrammar);
            Env.Execute(roofGrammar);

            var allAreas = State.TraversableAreas;
            //var objects = Enumerable.Range(0, 100)
            //.Select(_ => Lib.InteractiveObjects.Item(Lib.Items.FreeWill()));
            //.Select(_ => Lib.InteractiveObjects.AscensionKiln());
            //.Select(_ => Lib.InteractiveObjects.InteractiveObject<InteractiveObject>("bush", Lib.InteractiveObjects.Geometry<InteractiveObject>(Lib.Objects.farmer)));
            //objects.ForEach(obj => allAreas.GetRandom().AddInteractiveObject(obj));

            State.TraversableAreas
               .ForEach(
                   area => Enumerable.Range(0, 1)
                       .ForEach(_ => area.AddEnemy(Lib.Enemies.AllAgents().GetRandom()()))
               );
        }

        /// <summary>
        /// Places each existing enemy type into one area and gives it the declared stats.
        /// </summary>
        public void StatsScalingOfEnemies()
        {
            Env.Line(Gr.PrL.Garden(), NodesQueries.All, Lib.Enemies.AllAgents().Count(), out var areas);

            var allEnemiesPlacer = PlC.EvenPlacer(
                Lib.Enemies.AllAgents().Select(chF =>
                {
                    var enemy = chF();
                    var stats = new CharacterStats()
                    {
                        Will = 0,
                        Strength = 5,
                        Endurance = 5,
                        Agility = 10,
                        Posture = 5,
                        Resistances = 5,
                        Versatility = 5
                    };
                    enemy.Stats = stats;

                    return enemy;
                })
            );

            allEnemiesPlacer.Place(areas);
        }

        /// <summary>
        /// Places an area with an npc that gives stat increasing items.
        /// </summary>
        public void LevellingUpItems()
        {
            Env.One(Gr.PrL.Garden(), NodesQueries.LastCreated, out var level_up_area);

            var statIncreaseItems = CharacterStats.StatIncreases.Select(statIncrease =>
                new ItemState()
                {
                    Name = statIncrease.Stat.ToString(),
                    Description = $"Increases {statIncrease.Stat}"
                }
                .OnUse(player => statIncrease.Manipulate(player.Stats))
            ).ToArray();

            level_up_area.Get.AddInteractiveObject(
                Lib.InteractiveObjects.Farmer("Levelling up object")
                    .SetInteraction(
                        ins => ins
                            .Interact("Take levelling up items", (ios, player) => statIncreaseItems.ForEach(item => player.AddItem(item))
                            )
                        )
                    );
        }

        /// <summary>
        /// Places an area with an npc that gives all spells.
        /// </summary>
        public void Spells()
        {
            Env.One(Gr.PrL.Garden(), NodesQueries.All, out var area);

            var spells = new Spells(Lib.Effects, Lib.Selectors, Lib.VFXs);
            var spellItems = new SpellItems(spells, Lib.VFXs);
            Func<ItemState>[] s = spellItems.AllSpellItems()
                .Select<Func<ItemState>, Func<ItemState>>(itemF => () => itemF().SetReplenishable(1)).ToArray();

            area.Get.AddInteractiveObject(
                Lib.InteractiveObjects.Farmer("Spell objects")
                    .SetInteraction(
                        ins => ins
                            .Interact("Take all spells", (ios, player) => s.ForEach(itemF => player.AddItem(itemF()))
                            )
                        )
                    );
        }

        /// <summary>
        /// Places an area with multiple items lying on the ground.
        /// </summary>
        public void ItemsTesting()
        {
            Env.One(Gr.PrL.Garden(), NodesQueries.All, out var area);

            var itemPlacer = PlO.RandomAreaPlacer(new ConstDistrInt(3), () => Lib.InteractiveObjects.Item(Lib.Items.NewItem("Item", "Description")));
            itemPlacer.Place(area);
        }

        /// <summary>
        /// Test grammars.
        /// </summary>
        public void GrammarTesting()
        {
            var grammarState = State.GrammarState;

            //var shapeGrammar = new RandomGrammar(Gr.PrL.TestingProductions(), 20);
            /*var randGardenGrammar = new RandomGrammar(Gr.PrL.Garden(), 1);
            var graveyardGrammar = new RandomGrammar(Gr.PrL.Graveyard(), 10);
            var graveyardPostprocessGrammar = new AllGrammar(Gr.PrL.GraveyardPostprocess());
            var roofGrammar = new AllGrammar(Gr.PrL.Roofs());
            */

            //Env.Execute(shapeGrammar);
            /*Env.Execute(randGardenGrammar);
            Env.Execute(graveyardGrammar);
            Env.Execute(graveyardPostprocessGrammar);
            Env.Execute(roofGrammar);
            */

            //Env.Line(Gr.PrL.Town(), NodesQueries.All, 10, out var _);
            Env.Line(Gr.PrL.Castle(), NodesQueries.All, 10, out var _);
            //Env.Line(Gr.PrL.Castle(), NodesQueries.All, 10, out var _);
            //Env.BranchRandomly(Gr.PrL.Town(), 40, out var _);

            /*Env.Line(Gr.PrL.TestingProductions(), NodesQueries.All, 10, out var _);
            Env.Line(Gr.PrL.TestingProductions(), NodesQueries.All, 10, out var _);
            Env.Line(Gr.PrL.TestingProductions(), NodesQueries.All, 10, out var _);
            Env.Line(Gr.PrL.TestingProductions(), NodesQueries.All, 10, out var _);*/

            Env.BranchRandomly(Gr.PrL.ChapelsDetails(), 2, out var _);
            Env.Execute(new AllGrammar(Gr.PrL.Roofs()/*ChapelsPostprocess()*/));

            var allAreas = State.TraversableAreas;
            //var objects = Enumerable.Range(0, 100)
            //.Select(_ => Lib.InteractiveObjects.Item(Lib.Items.FreeWill()));
            //.Select(_ => Lib.InteractiveObjects.AscensionKiln());
            //.Select(_ => Lib.InteractiveObjects.InteractiveObject<InteractiveObject>("bush", Lib.InteractiveObjects.Geometry<InteractiveObject>(Lib.Objects.farmer)));
            //objects.ForEach(obj => allAreas.GetRandom().AddInteractiveObject(obj));

            /*
            State.TraversableAreas
               .ForEach(
                   area => Enumerable.Range(0, 1)
                       .ForEach(_ => area.AddEnemy(Lib.Enemies.AllAgents().GetRandom()()))
               );*/
        }

        /// <summary>
        /// Test npcs using possible events.
        /// </summary>
        public void StartNonPersistentNpcLines()
        {
            Enumerable.Range(0, 6).ForEach(i => 
                State.LC.AddPossibleEvent($"Npc {i}", 0, 
                    _ =>
                    {
                        Env.One(Gr.PrL.Garden(), NodesQueries.All, out var area);
                        area.AreasList[0].AddInteractiveObject(
                            Lib.InteractiveObjects.Kiln()
                                .SetInteraction(
                                    ins => ins
                                        .Say($"I'm npc {i}")
                                )
                            );
                        }
                    )
                );
        }

        /// <summary>
        /// Test npcs using possible events that persist after being used.
        /// </summary>
        public void StartPersistentNpcLines()
        {
            Enumerable.Range(0, 6).ForEach(i =>
                State.LC.AddPossibleEvent($"Npc {i}", 0,
                    _ =>
                    {
                        Env.One(Gr.PrL.Garden(), NodesQueries.All, out var area);
                        area.AreasList[0].AddInteractiveObject(
                            Lib.InteractiveObjects.Kiln()
                                .SetInteraction(
                                    ins => ins
                                        .Say($"I'm npc {i}")
                                )
                            );
                    }, 
                    true)
                );
        }

        /// <summary>
        /// Creates an environment with a locked door.
        /// </summary>
        public void TestLocking()
        {
            M.LockingModule.LineWithKey(NodesQueries.LastCreated, 1, Gr.PrL.Garden(), out var lockedArea, out var linearPath);
        }

        /// <summary>
        /// Creates sky and sea for the given level. Also places an area with an npc that gives sky changing items.
        /// </summary>
        public void TestSky(int level)
        {
            var envState = new EnvironmentState(
                () =>
                {
                    var env = Lib.Objects.EnvironmentMap();
                    State.World.AddSpecialObject(env.transform);
                    RenderSettings.sun = env.Sun;
                    return env;
                });
            State.World.OnLevelStart += envState.MakeGeometry;

            Env.One(Gr.PrL.Town(), NodesQueries.All, out var area);

            envState.SetParameters(M.EnvironmentModule.GetSkyParameter(level));

            Func<ItemState>[] skyChangingItems = M.EnvironmentModule.SkyParams
                .Select<SkyParameters, Func<ItemState>>((skyParams, i) => () => Lib.Items
                 .NewItem($"Set sky {i}", $"Summon sky of {i}-th level")
                     .OnUse(user => envState.SetParameters(skyParams))).ToArray();

            area.Get.AddInteractiveObject(
                Lib.InteractiveObjects.Farmer("Sky distributor")
                    .SetInteraction(
                        ins => ins
                            .Interact("Take all skies", (ios, player) => skyChangingItems.ForEach(itemF => player.AddItem(itemF()))
                            )
                        )
                    );
        }

        public void NewGrammar()
        {
            Env.Execute(new RandomGrammar(Gr.PrL.NewStart(), 1));
            Env.Execute(new RandomGrammar(Gr.PrL.NewGrammar(), 40));
            Env.Execute(new AllGrammar(Gr.PrL.NewRoofs()));

            //Env.BranchRandomly(/*, NodesQueries.All*/, 40, out var branching);
        }
    }
}
