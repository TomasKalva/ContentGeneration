using OurFramework.UI;
using OurFramework.UI.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using OurFramework.Environment.ShapeGrammar;
using OurFramework.Gameplay.RealWorld;
using OurFramework.Gameplay.Data;
using OurFramework.Util;

namespace OurFramework.LevelDesignLanguage.CustomModules
{

    class FactionsModule : LDLanguage
    {
        public FactionsModule(LanguageParams parameters) : base(parameters) { }

        public void InitializeFactions(int factionsCount)
        {
            var branches = Branches();

            var concepts = new FactionConcepts(
                    new List<Func<PathGuide, ProductionList>>()
                    {
                        pathGuide => Gr.PrL.Town(pathGuide),
                        pathGuide => Gr.PrL.Castle(pathGuide),
                        pathGuide => Gr.PrL.Chapels(pathGuide),
                    },
                    new List<Func<CharacterState>>()
                    {
                        Lib.Enemies.MayanSwordsman,
                        Lib.Enemies.MayanThrower,
                        Lib.Enemies.SkinnyWoman,
                        Lib.Enemies.Dog,
                    }
                    .Shuffle().ToList(),
                    Lib.Items.AllBasicWeapons().ToList(),
                    Lib.SpellItems.AllSpellsByPower()
                );

            Enumerable.Range(0, factionsCount).ForEach(_ =>
            {
                var factionConcepts = concepts.TakeSubset(3, 4);
                var faction = new Faction(concepts);

                State.LC.AddNecessaryEvent($"Start Manifestation", 5, level =>
                    {
                        var factionManifestation = faction.GetFactionManifestation();
                        var factionEnvironment = factionManifestation.GetFactionEnvironment();
                        branches.GetRandom()(factionEnvironment, faction.StartingBranchProgress, level);
                    },
                    true,
                    level => faction.ProgressedEnvironmentInMaxLevel < level
                );
            });
        }

        public delegate void FactionEnvironmentConstructor(FactionEnvironment fe, int progress, int level);

        /// <summary>
        /// Returns interactive object that allows player to continue the manifestation.
        /// </summary>
        InteractiveObjectState ProgressOfManifestation(FactionManifestation manifestation, int level)
        {
            Func<string>[] toSay = new Func<string>[3]
            {
                () => "Death comes sparsely and every chance to rest is greatly appreciated.",
                () => "Our members are tired. Most of us don't get many chances to die regularly anymore...",
                () => "Living in death. What was the last time we could? Bringing rest to our bodies feels... refreshing.",
            };
            int progressPrice = 500 * (1 + manifestation.Progress);
            return Lib.InteractiveObjects.Farmer("Progress of Manifestation")
                    .SetInteraction(
                        ins => ins
                            .Say("Our faction is the most pleased with your service.")
                            .Say(toSay.GetRandom()())
                            .Say("Finding a skilled executioner is a long time run.")
                            .Decide($"Are your services still available? (Proceed to progress {manifestation.Progress + 1})",
                            new InteractOption<InteractiveObject>($"Yes ({progressPrice} Spirit)",
                                (ios, player) =>
                                {
                                    if (!player.Pay(progressPrice))
                                    {
                                        Msg.Show("Not enough spirit.");
                                        return;
                                    }

                                    manifestation.ContinueManifestation(State.LC, Branches());
                                    Msg.Show("Progress achieved");
                                    manifestation.Faction.ProgressedEnvironmentInMaxLevel = level + 1;
                                    ios.SetInteraction(
                                        ins => ins
                                            .Say("We are in your debt.")
                                    );
                                }
                            , 0)
                        )
                    );
        }

        public Func<InteractiveObjectState>[] ItemsToPlace(FactionEnvironment fe, int count)
        {
            return Enumerable.Range(0, 3).Select<int, Func<InteractiveObjectState>>(_ => () => Lib.InteractiveObjects.Item(fe.CreateItemFactory()(_))).ToArray();
        }

        public IEnumerable<FactionEnvironmentConstructor> Branches()
        {
            yield return LinearWithKey;
            yield return BranchesWithKey;
            yield return RandomBranches;
            yield return LinearBranch;
        }

        IDistribution<int> EnemiesInAreaCount(int progress)
        {
            var maxEnemies = Math.Min(5, 3 + progress);
            return new UniformDistr(1, maxEnemies);
        }

        void PlaceInEndArea(Area endArea, FactionEnvironment fe, int progress, int level)
        {
            if (progress <= fe.FactionManifestation.Faction.MaxProgress - 1)
            {
                // Place creator of next environment
                endArea.AddInteractiveObject(ProgressOfManifestation(fe.FactionManifestation, level));
            }
            else
            {
                // Manifestation is over => get reward
                endArea.AddInteractiveObject(
                    Lib.InteractiveObjects.Item(
                        Lib.Items.NewItem($"Humanity", "Although the art of dying is forbidden in the majority of natural settlments, some recognized its true benefits. In the world of    there is no greater glory than life in death.")
                            .SetStackable(1)
                            .OnUse(user => user.Spirit += 5000)));
            }
        }

        public void LinearWithKey(FactionEnvironment fe, int progress, int level)
        {
            M.LockingModule.LineWithKey(NodesQueries.LastCreated, 4, fe.GetProductionList(), out var lockedArea, out var linearPath);

            var itemPlacer = PlO.RandomAreasPlacer(new UniformDistr(3, 4), ItemsToPlace(fe, 3));
            itemPlacer.Place(linearPath);
            itemPlacer.Place(lockedArea);
            PlC.ProgressFunctionPlacer(fe.CreateEnemyFactory(), EnemiesInAreaCount(progress)).Place(linearPath);

            PlaceInEndArea(lockedArea.Area, fe, progress, level);
        }

        public void BranchesWithKey(FactionEnvironment fe, int progress, int level)
        {
            M.LockingModule.RandomBranchingWithKeys(4, fe.GetProductionList(), out var lockedArea, out var branches);

            var itemPlacer = PlO.RandomAreasPlacer(new UniformDistr(3, 4), ItemsToPlace(fe, 3));
            itemPlacer.Place(branches);
            itemPlacer.Place(lockedArea);
            PlC.ProgressFunctionPlacer(fe.CreateEnemyFactory(), EnemiesInAreaCount(progress)).Place(branches);

            PlaceInEndArea(lockedArea.Area, fe, progress, level);
        }

        public void RandomBranches(FactionEnvironment fe, int progress, int level)
        {
            Env.BranchRandomly(fe.GetProductionList(), 5, out var randomBranches);

            PlO.RandomAreasPlacer(new UniformDistr(3, 6), ItemsToPlace(fe, 3)).Place(randomBranches);
            PlC.ProgressFunctionPlacer(fe.CreateEnemyFactory(), EnemiesInAreaCount(progress)).Place(randomBranches);

            PlaceInEndArea(randomBranches.AreasList.GetRandom(), fe, progress, level);
        }

        public void LinearBranch(FactionEnvironment fe, int progress, int level)
        {
            Env.Loopback(pathGuide => fe.GetProductionList(pathGuide), Gr.PrL.OneWayConnectBack(), 5, NodesQueries.All(State.GrammarState), NodesQueries.All(State.GrammarState), out var path, out var connectTo);

            PlO.RandomAreasPlacer(new UniformDistr(3, 6), ItemsToPlace(fe, 3)).Place(path);
            PlC.ProgressFunctionPlacer(fe.CreateEnemyFactory(), EnemiesInAreaCount(progress)).Place(path);

            PlaceInEndArea(path.LastArea(), fe, progress, level);
        }
    }

    /// <summary>
    /// Persistent over the entire game.
    /// </summary>
    class Faction
    {
        public FactionConcepts Concepts { get; }

        /// <summary>
        /// Maximal level to which progressed environment event was added.
        /// </summary>
        public int ProgressedEnvironmentInMaxLevel { get; set; }

        /// <summary>
        /// Affinity of player with the faction.
        /// </summary>
        public int Affinity { get; set; }

        public int MaxProgress { get; }

        public int StartingBranchProgress => Math.Min(MaxProgress, Affinity / 4);

        public Faction(FactionConcepts concepts)
        {
            Concepts = concepts;
            Affinity = 0;
            MaxProgress = 3;
            ProgressedEnvironmentInMaxLevel = -1;
        }

        public FactionManifestation GetFactionManifestation()
        {
            return new FactionManifestation(Concepts.TakeSubset(3, 3), this);
        }
    }

    /// <summary>
    /// Sequence of environments.
    /// </summary>
    class FactionManifestation
    {
        FactionConcepts Concepts { get; }
        public Faction Faction { get; }
        /// <summary>
        /// How many environments player already went through.
        /// </summary>
        public int Progress { get; set; }

        public FactionManifestation(FactionConcepts concepts, Faction faction)
        {
            Concepts = concepts;
            Faction = faction;
            Progress = 0;
        }

        public FactionEnvironment GetFactionEnvironment()
        {
            return new FactionEnvironment(Concepts.TakeSubset(2, 1 + Progress), this);
        }

        public void ContinueManifestation(LevelConstructor levelConstructor, IEnumerable<FactionsModule.FactionEnvironmentConstructor> branches)
        {
            Progress++;
            levelConstructor.AddNecessaryEvent($"{nameof(ContinueManifestation)} {Progress}", 10 + Progress, level => branches.GetRandom()(GetFactionEnvironment(), Progress, level));
        }
    }

    /// <summary>
    /// One environment put in one level.
    /// </summary>
    class FactionEnvironment
    {
        FactionConcepts Concepts { get; }
        public FactionManifestation FactionManifestation { get; }


        public FactionEnvironment(FactionConcepts concepts, FactionManifestation factionManifestation)
        {
            Concepts = concepts;
            FactionManifestation = factionManifestation;
        }

        public ProductionList GetProductionList(PathGuide pathGuide = null)
        {
            pathGuide ??= new RandomPathGuide();
            return Concepts.ProductionLists.GetRandom()(pathGuide);
        }

        /// <summary>
        /// Returns a factory that returns the same items.
        /// </summary>
        public ProgressFactory<ItemState> CreateItemFactory()
        {
            var manifestationProgress = FactionManifestation.Progress;

            return _ =>
            {
                return FactionManifestation.Faction.Concepts.Spells[manifestationProgress].GetRandom()().SetStackable(1);
            };
        }

        public CharacterStats GetStats(int manifestationProgress)
        {
            var stats = new CharacterStats()
            {
                Will = 4 * manifestationProgress,
                Strength = 5 + 5 * manifestationProgress,
                Versatility = 5 * manifestationProgress
            };

            var statChanges = CharacterStats.StatChanges;
            var randomisedStats = new Stat[]
            {
                Stat.Endurance,
                Stat.Agility,
                Stat.Posture,
                Stat.Resistances,
            };
            var increasedStats = randomisedStats.Take(manifestationProgress).ToHashSet();

            randomisedStats.Select(stat => statChanges[stat])
                .ForEach(sc => sc.Manipulate(stats, 4 + 4 * manifestationProgress));

            return stats;

        }

        /// <summary>
        /// Returns a factory that returns similar enemies.
        /// </summary>
        public ProgressFactory<CharacterState> CreateEnemyFactory()
        {
            var affinity = FactionManifestation.Faction.Affinity;
            var manifestationProgress = FactionManifestation.Progress;

            // Create items for the enemy

            return progress =>
            {
                var character = Concepts.CharacterStates.GetRandom()();

                character.Stats = GetStats(manifestationProgress);
                character.AddOnDeath(() => GameViewModel.ViewModel.PlayerState.Spirit += character.Health.Maximum);

                // Create weapon for the enemy
                var leftWeaponF = Concepts.Weapons.GetRandom();
                var rightWeaponF = Concepts.Weapons.GetRandom();

                character.Inventory.LeftWeapon.Item = leftWeaponF();
                character.Inventory.RightWeapon.Item = rightWeaponF();

                return character;
            };
        }


        /// <summary>
        /// Returns a factory that returns interactive objects.
        /// </summary>
        public ProgressFactory<InteractiveObjectState> CreateInteractiveObjectFactory()
        {
            var affinity = FactionManifestation.Faction.Affinity;
            var progress = FactionManifestation.Progress;

            // Create interaction of the interactive object

            throw new NotImplementedException();
        }
    }

    class FactionConcepts
    {
        public List<Func<PathGuide, ProductionList>> ProductionLists { get; }
        public List<Func<CharacterState>> CharacterStates { get; }
        public List<Func<WeaponItem>> Weapons { get; }
        public List<List<Func<ItemState>>> Spells { get; }

        public FactionConcepts(
            List<Func<PathGuide, ProductionList>> productionLists,
            List<Func<CharacterState>> characterStates,
            List<Func<WeaponItem>> weapons,
            List<List<Func<ItemState>>> spells)
        {
            ProductionLists = productionLists;
            CharacterStates = characterStates;
            Weapons = weapons;
            Spells = spells;
        }

        public FactionConcepts TakeSubset(int characterStatesCount, int weaponsCount)
        {
            return new FactionConcepts(
                    ProductionLists,
                    CharacterStates.Shuffle().Take(characterStatesCount).ToList(),
                    Weapons.Shuffle().Take(weaponsCount).ToList(),
                    Spells
                );
        }
    }


    delegate ByUser<Effect> EffectByFactionEnvironmentByUser(FactionEnvironment factionEnv);
}
