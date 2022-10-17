namespace Assets.LevelDesignLanguage.CustomModules
{
    class MainModule : LDLanguage
    {
        public MainModule(LanguageParams parameters) : base(parameters) { }

        public void StartWorld()
        {
            StartDebugWorld();
        }

        void StartDebugWorld()
        {
            State.LC.AddNecessaryEvent($"Level Start", 100, level => M.LevelModule.LevelStart(), true);

            State.LC.AddNecessaryEvent($"Death", 99, _ =>
            {
                //L.DeathLanguage.EnableClassicalDeath();
                M.DeathModule.DieIfNotProtected();
                //L.DeathLanguage.EndRunAfterDeaths(2);
                //L.DeathLanguage.DropSpiritBloodstainOnDeath();
                //L.DeathLanguage.DropRunEndingBloodstainOnDeath();
                M.DeathModule.EndRunIfOutOfSmile();
            }, true);

            State.LC.AddNecessaryEvent($"Level End", 99, level => M.LevelModule.LevelEnd(), true);

            //L.LevelLanguage.AddOptionalEnd();

            M.FactionsModule.InitializeFactions(2);


            State.LC.AddNecessaryEvent($"Main path", 98, level => M.LevelModule.MainPath(level), true);

            State.LC.AddNecessaryEvent($"Add Details", 0, level => M.DetailsModule.AddDetails(level), true);

            State.LC.AddNecessaryEvent($"Out of depth encounter", 80, level => M.OutOfDepthEncountersModule.DifficultEncounter(level), true);

            State.LC.AddNecessaryEvent($"Environment", 0, level => M.EnvironmentModule.CreateSky(level), true);


            State.LC.AddNecessaryEvent($"Roofs", -1, level => M.LevelModule.Roofs(), true);

            State.LC.AddNecessaryEvent("Ascending", 80, _ => M.AscendingModule.AscendingBranch(() => 100));


            //State.LC.AddNecessaryEvent("Testing enemies", 5, _ => L.TestingLanguage.StatsScalingOfEnemies());


            //State.LC.AddNecessaryEvent($"Environment", 0, level => L.EnvironmentLanguage.TestSky(level), true);

            /*
            State.LC.AddNecessaryEvent("Farmer branch", 5, level => L.FarmersLanguage.FarmerBranch(0));
            */


            /*
            State.LC.AddNecessaryEvent(5, () =>
            {
                //L.PatternLanguage.BranchWithKey(NodesQueries.LastCreated, 4, Gr.PrL.TestingProductions());
                L.PatternLanguage.RandomBranchingWithKeys(6, Gr.PrL.TestingProductions(), out var locked, out var branches);
                return false;
            });
            */

            //State.LC.AddNecessaryEvent("Ascending", 90, _ => L.AscendingLanguage.AscendingBranch(() => 100));
            /*
            State.LC.AddNecessaryEvent(
                new LevelConstructionEvent(10, () =>
                {
                    L.TestingLanguage.LevellingUpItems();
                    return false;
                })
            );
            */





            //State.LC.AddNecessaryEvent("Testing spells", 90, _ => L.TestingLanguage.Spells());

            //State.LC.AddNecessaryEvent("Testing spells", 90, _ => L.TestingLanguage.ItemsTesting());


            //State.LC.AddNecessaryEvent("Testing Grammars", 90, _ => L.TestingLanguage.GrammarTesting());
            //State.LC.AddNecessaryEvent("Testing Locking", 90, _ => L.TestingLanguage.TestLocking());
            //L.NpcLanguage.InitializeNpcs();
            //State.LC.AddNecessaryEvent("Testing Locking", 90, _ => L.TestingLanguage.NpcLine());
        }
    }
}
