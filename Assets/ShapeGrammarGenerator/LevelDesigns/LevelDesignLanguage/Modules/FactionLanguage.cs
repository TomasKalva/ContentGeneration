using Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions;
using Assets.Util;
using ContentGeneration.Assets.UI.Model;
using ContentGeneration.Assets.UI.Util;
using ShapeGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions.SelectorLibrary;
using static InteractiveObject;

namespace Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage
{

    class FactionsLanguage : LDLanguage
    {
        public FactionsLanguage(LanguageParams tools) : base(tools) { }

        public void InitializeFactions(int factionsCount)
        {
            var branches = Branches();

            var selectorLibrary = Lib.Selectors;
            var effectsLibrary = Lib.Effects;


            var spells = new Spells(Lib.Effects, Lib.Selectors, Lib.VFXs);
            var spellItems = new SpellItems(spells, Lib.VFXs);
            Func<ItemState>[] s = spellItems.AllSpellItems()
                .Select<Func<ItemState>, Func<ItemState>>(itemF => () => itemF().SetReplenishable(1)).ToArray();

            var concepts = new FactionConcepts(
                    new List<Func<ProductionList>>()
                    {
                            Gr.PrL.Town,
                            Gr.PrL.Castle,
                            Gr.PrL.Chapels,
                    },
                    new List<Func<CharacterState>>()
                    {
                        Lib.Enemies.MayanSwordsman,
                        Lib.Enemies.MayanThrower,
                        Lib.Enemies.SkinnyWoman,
                        Lib.Enemies.Dog,
                    }
                    .Shuffle().ToList(),
                    Lib.Items.AllWeapons().ToList(),
                    new List<List<Func<ItemState>>>()
                    {
                        new List<Func<ItemState>>()
                        {
                            spellItems.FireBolt,
                            spellItems.Replenishment,
                        },
                        new List<Func<ItemState>>()
                        {
                            spellItems.FlameBolt,
                            spellItems.Fireball,
                            spellItems.Cloud,
                            spellItems.PillarsOfHeaven,
                            spellItems.ConsecratedGround,
                            spellItems.Refreshment,
                        },
                        new List<Func<ItemState>>()
                        {
                            spellItems.ChaosBolt,
                            spellItems.Firefall,
                            spellItems.SquareOfChaos,
                            spellItems.FlameOfHeaven,
                            spellItems.HeavenlyFlameCloud,
                            spellItems.Triangle,
                        },
                        new List<Func<ItemState>>()
                        {
                            spellItems.CircleOfChaos,
                            spellItems.FlamesOfHeaven,
                            spellItems.Inferno,
                            spellItems.WaveOfChaos,
                        },
                    }
                );

            Enumerable.Range(0, factionsCount).ForEach(_ =>
            {
                var factionConcepts = concepts.TakeSubset(3, 4);
                var faction = new Faction(concepts);

                State.LC.AddEvent(
                    new LevelConstructionEvent(5, () =>
                    {
                        var factionManifestation = faction.GetFactionManifestation();
                        var factionEnvironment = factionManifestation.GetFactionEnvironment();
                        branches.GetRandom()(factionEnvironment, faction.StartingBranchProgress);
                        return false;
                    })
                );
            });
        }

        public delegate void FactionEnvironmentConstructor(FactionEnvironment fe, int progress);

        /// <summary>
        /// Returns interactive object that allows player to continue the manifestation.
        /// </summary>
        InteractiveObjectState ProgressOfManifestation(FactionManifestation manifestation)
        {
            return Lib.InteractiveObjects.InteractiveObject("Progress of Manifestation", Lib.InteractiveObjects.Geometry<InteractiveObject>(Lib.Objects.farmer))
                    .SetInteraction(
                        new InteractionSequence<InteractiveObject>()
                            .Decision($"Progress this manifestation ({manifestation.Progress + 1})",
                            new InteractOption<InteractiveObject>("Yes",
                                (ios, player) =>
                                {
                                    manifestation.ContinueManifestation(State.LC, Branches());
                                    Msg.Show("Progress achieved");
                                    ios.SetInteraction(
                                        new InteractionSequence<InteractiveObject>()
                                            .Say("Thank you for your attention.")
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
            return new UniformIntDistr(1, maxEnemies);
        }

        void PlaceInEndArea(Area endArea, FactionEnvironment fe, int progress)
        {
            if (progress <= fe.FactionManifestation.Faction.MaxProgress - 1)
            {
                // Place creator of next environment
                endArea.AddInteractiveObject(ProgressOfManifestation(fe.FactionManifestation));
            }
            else
            {
                // Manifestation is over => get reward
                endArea.AddInteractiveObject(
                    Lib.InteractiveObjects.Item(
                        Lib.Items.NewItem($"Spirit of Lord", "This spirit belonged to some lord.")
                            .OnUse(user => user.Spirit += 5000)));
            }
        }

        public void LinearWithKey(FactionEnvironment fe, int progress)
        {
            L.PatternLanguage.BranchWithKey(NodesQueries.LastCreated, 4, Gr.PrL.Town(), out var lockedArea, out var linearPath);

            var itemPlacer = PlO.RandomAreasPlacer(new UniformDistr(3, 4), ItemsToPlace(fe, 3));
            itemPlacer.Place(linearPath);
            itemPlacer.Place(lockedArea);
            PlC.ProgressFunctionPlacer(fe.CreateEnemyFactory(), EnemiesInAreaCount(progress)).Place(linearPath);

            PlaceInEndArea(lockedArea.Get, fe, progress);
            //lockedArea.Get.AddInteractiveObject(ProgressOfManifestation(fe.FactionManifestation));
        }

        public void BranchesWithKey(FactionEnvironment fe, int progress)
        {
            L.PatternLanguage.RandomBranchingWithKeys(4, Gr.PrL.Town(), out var lockedArea, out var branches);

            var itemPlacer = PlO.RandomAreasPlacer(new UniformDistr(3, 4), ItemsToPlace(fe, 3));
            itemPlacer.Place(branches);
            itemPlacer.Place(lockedArea);
            PlC.ProgressFunctionPlacer(fe.CreateEnemyFactory(), EnemiesInAreaCount(progress)).Place(branches);

            PlaceInEndArea(lockedArea.Get, fe, progress);
            //lockedArea.Get.AddInteractiveObject(ProgressOfManifestation(fe.FactionManifestation));
        }

        public void RandomBranches(FactionEnvironment fe, int progress)
        {
            Env.BranchRandomly(fe.ProductionList(), 5, out var randomBranches);

            //PlO.ProgressFunctionPlacer(fe.CreateInteractiveObjectFactory(), new UniformIntDistr(1, 4)).Place(path);
            PlO.RandomAreasPlacer(new UniformDistr(3, 6), ItemsToPlace(fe, 3)).Place(randomBranches);
            PlC.ProgressFunctionPlacer(fe.CreateEnemyFactory(), EnemiesInAreaCount(progress)).Place(randomBranches);

            PlaceInEndArea(randomBranches.AreasList.GetRandom(), fe, progress);
            //path.AreasList.GetRandom().AddInteractiveObject(ProgressOfManifestation(fe.FactionManifestation));
        }

        public void LinearBranch(FactionEnvironment fe, int progress)
        {
            Env.Line(fe.ProductionList(), NodesQueries.All, 5, out var path);

            //PlO.ProgressFunctionPlacer(fe.CreateInteractiveObjectFactory(), new UniformIntDistr(1, 4)).Place(path);
            PlO.RandomAreasPlacer(new UniformDistr(3, 6), ItemsToPlace(fe, 3)).Place(path);
            PlC.ProgressFunctionPlacer(fe.CreateEnemyFactory(), EnemiesInAreaCount(progress)).Place(path);

            PlaceInEndArea(path.LastArea(), fe, progress);
            //path.LastArea().AddInteractiveObject(ProgressOfManifestation(fe.FactionManifestation));
        }
    }
}
