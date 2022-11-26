using OurFramework.Environment.ShapeGrammar;

namespace OurFramework.LevelDesignLanguage.CustomModules
{
    class MainModule : LDLanguage
    {
        public MainModule(LanguageParams parameters) : base(parameters) { }

        /// <summary>
        /// Called when the game is started.
        /// </summary>
        public void DeclareGame()
        {
            DeclareDebugGame();
            //State.LC.AddNecessaryEvent($"Tutorial module", 100, level => M.TutorialModule.Main());
            //M.TutorialModule.DeclareGame();
            //DeclareEnvironmentForPrettyPictures();
            //State.LC.AddNecessaryEvent($"Level Start", 100, level => M.LevelModule.LevelStart(), true);
            //State.LC.AddNecessaryEvent($"New Grammar", 50, level => M.TestingModule.NewGrammar());
        }

        void DeclareDebugGame()
        {
            State.LC.AddNecessaryEvent($"Level Start", 100, level => M.LevelModule.LevelStart(), true);
            State.LC.AddNecessaryEvent($"Level End", 99, level => M.LevelModule.LevelEnd(), true);
            
            
            State.LC.AddNecessaryEvent($"Death", 99, _ =>
            {
                State.World.PlayerState.AddItem(Lib.Items.VibrantMemory());
                State.World.PlayerState.AddItem(Lib.Items.VibrantMemory());
                //L.DeathLanguage.EnableClassicalDeath();
                M.DeathModule.DieIfNotProtected();
                //M.DeathModule.EndRunAfterDeaths(2);
                //M.DeathModule.DropSpiritBloodstainOnDeath();
                M.DeathModule.DropRunEndingBloodstainOnDeath();
                //M.DeathModule.EndRunIfOutOfSmile();
            }, true);
            
            //M.LevelModule.AddOptionalEnd();
            
            M.FactionsModule.InitializeFactions(2);
            

            State.LC.AddNecessaryEvent($"Main path", 98, level => M.LevelModule.MainPath(level), true);

            State.LC.AddNecessaryEvent($"Sky", 0, level => M.EnvironmentModule.CreateSky(level), true);
            /*
            State.LC.AddNecessaryEvent($"Add Details", 0, level => M.DetailsModule.AddDetails(level), true);

            State.LC.AddNecessaryEvent($"Out of depth encounter", 80, level => M.OutOfDepthEncountersModule.DifficultEncounter(level), true);

            M.OutOfDepthEncountersModule.AddLightMaceEncounter();



            State.LC.AddNecessaryEvent($"Roofs", -1, level => M.LevelModule.AddRoofs(), true);
            

            M.AscendingModule.AddAscendingEvents(M.AscendingModule.AscendingKiln(ad => 100 + 50 * ad));
            */
            //State.LC.AddNecessaryEvent("Ascending", 80, _ =>);


            //State.LC.AddNecessaryEvent("Testing enemies", 5, _ => L.TestingLanguage.StatsScalingOfEnemies());


            //State.LC.AddNecessaryEvent($"Environment", 99, level => M.TestingModule.TestSky(level), true);


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

        void DeclareEnvironmentForPrettyPictures()
        {
            State.LC.AddNecessaryEvent($"Level Start", 100, level => M.LevelModule.LevelStart(), true);
            State.LC.AddNecessaryEvent("Testing enemies", 5, _ => 
            {
                Env.Line(Gr.PrL.Town(), NodesQueries.All, 20, out var _);
                Env.Line(Gr.PrL.Castle(), NodesQueries.All, 20, out var _);
                Env.Line(Gr.PrL.Chapels(), NodesQueries.All, 20, out var _);
            });

            State.LC.AddNecessaryEvent($"Environment", 90, level => M.TestingModule.TestSky(level), true);


            State.LC.AddNecessaryEvent($"Roofs", -1, level => M.LevelModule.AddRoofs(), true);
        }
    }
}
