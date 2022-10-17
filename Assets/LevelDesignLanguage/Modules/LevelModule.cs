using OurFramework.Characters.Items.ItemClasses;
using ContentGeneration.Assets.UI.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using Util;
using OurFramework.Environment.ShapeGrammar;

namespace OurFramework.LevelDesignLanguage.CustomModules
{
    class LevelModule : LDLanguage
    {
        public LevelModule(LanguageParams parameters) : base(parameters) { }

        public void LevelStart()
        {
            Env.One(Gr.PrL.CreateNewHouse(), NodesQueries.All, out var area);
            area.Get.Node.AddSymbol(Gr.Sym.LevelStartMarker);
        }

        public List<Func<ProductionList>> MainPathProductionLists() =>
            new()
                    {
                            () => Gr.PrL.Town(),
                            () => Gr.PrL.Castle(),
                            () => Gr.PrL.Chapels(),
                    };

        EnemyMaker BasicEnemyMaker()
        {
            return new EnemyMaker(
                level => new CharacterStats()
                {
                    Will = 0 + 1 * level,
                    Strength = 4 + 3 * level,
                    Versatility = 3 + 5 * level,
                    Endurance = 3 + 5 * level,
                    Agility = 5 + 5 * level,
                    Posture = 1 + 2 * level,
                    Resistances = 0 + level
                },
                new List<Func<WeaponItem>>()
                {
                    Lib.Items.Katana,
                    Lib.Items.MayanSword,
                    Lib.Items.Mace,
                },
                new List<Func<CharacterState>>()
                {
                    Lib.Enemies.MayanSwordsman,
                    Lib.Enemies.MayanThrower,
                    Lib.Enemies.SkinnyWoman,
                },
                Lib.Items.MiscellaneousItems()
                    .Select<Func<ItemState>, Func<InteractiveObjectState>>(item => () => Lib.InteractiveObjects.Item(item())).ToList()
            );
        }

        public void MainPath(int level)
        {
            var enemyMaker = BasicEnemyMaker();

            // Place first part of the main path
            Env.Line(MainPathProductionLists().GetRandom()(), NodesQueries.All, 6, out var pathToShortcut);
            PlC.RandomAreaPlacer(new UniformDistr(1, 3), enemyMaker.GetRandomEnemy(level))
                .Place(pathToShortcut);

            // Create a shortcut
            var shortcutArea = pathToShortcut.LastArea();
            var first = pathToShortcut.AreasList.First();
            Env.MoveFromTo(pathGuide => Gr.PrL.GuidedGarden(pathGuide), Gr.PrL.ConnectBack(), 2, shortcutArea.Node.ToEnumerable(), first.Node.ToEnumerable(), out var shortcut);

            // Lock the shortcut
            var shortcutKey = M.LockingModule.CreateLockItems(State.UniqueNameGenerator.UniqueName("Shortcut key"), 1, "Unlocks a shortcut", out var unlock).First();
            shortcut.AreasList[0].AddInteractiveObject(Lib.InteractiveObjects.Item(shortcutKey));
            M.LockingModule.LockArea(shortcut.AreasList[1], unlock);

            // Create second part of the main path
            Env.Line(Gr.PrL.Town(), _ => shortcutArea.Node.ToEnumerable(), 5, out var pathToEnd);
            PlC.RandomAreaPlacer(new UniformDistr(1, 4), enemyMaker.GetRandomEnemy(level))
                .Place(pathToEnd);

            // Place transporter to the next level
            var end = pathToEnd.LastArea();
            end.AddInteractiveObject(
                Lib.InteractiveObjects.Transporter(State.GC)
                );
        }

        /// <summary>
        /// Adds possible event that can end level right at the begining. Can be used at most once.
        /// </summary>
        public void AddOptionalEnd()
        {
            bool usedOptionalEnd = false;
            State.LC.AddPossibleEvent($"Optional end", 99, level =>
            {
                Env.One(Gr.PrL.LevelEnd(), NodesQueries.All, out var area);
                area.Get.AddInteractiveObject(
                    Lib.InteractiveObjects.Transporter(State.GC, () => usedOptionalEnd = true)
                    );
            }, true, _ => !usedOptionalEnd);

        }

        public void LevelEnd()
        {
            Env.One(Gr.PrL.LevelEnd(), NodesQueries.All, out var area);
            area.Get.AddInteractiveObject(
                Lib.InteractiveObjects.Transporter(State.GC)
                );

        }

        public void AddRoofs()
        {
            Env.Execute(new AllGrammar(Gr.PrL.Roofs()));
        }

    }
}
