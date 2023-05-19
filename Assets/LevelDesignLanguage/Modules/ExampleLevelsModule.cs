using OurFramework.Environment.ShapeGrammar;
using OurFramework.Gameplay.State;
using OurFramework.Libraries;
using OurFramework.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static OurFramework.LevelDesignLanguage.CustomModules.EnvironmentModule;

namespace OurFramework.LevelDesignLanguage.CustomModules
{

    class ExampleLevelsModule : LDLanguage
    {
        public ExampleLevelsModule(LanguageParams parameters) : base(parameters) { }

        /// <summary>
        /// Paper example A.
        /// </summary>
        public void Simple()
        {
            State.LC.AddNecessaryEvent($"Simple level", 100, level => SimpleMain(), true);
        }

        public void SimpleMain()
        {
            LevelStart();
            LevelContinue();
            AddRoofs();
        }

        void LevelStart()
        {
            Env.One(Gr.PrL.CreateNewHouse(), NodesQueries.All, out var area);
            area.Area.Node.AddSymbol(Gr.Sym.LevelStartMarker);
        }

        void LevelContinue()
        {
            Env.Line(Gr.PrL.Town(), NodesQueries.All, 5, out var line);
        }

        void AddRoofs()
        {
            Env.Execute(new AllGrammar(Gr.PrL.Roofs()));
        }

        /// <summary>
        /// Paper example B.
        /// </summary>
        public void LongerLevel()
        {
            State.LC.AddNecessaryEvent($"Level Start", 100, level => M.LevelModule.LevelStart(), true);

            State.LC.AddNecessaryEvent($"Longer level", 100, level => M.LevelModule.MainPath(0), true);

            State.LC.AddNecessaryEvent($"Roofs", -1, level => M.LevelModule.AddRoofs(), true);

            State.LC.AddNecessaryEvent($"Sky", 0, level => M.EnvironmentModule.CreateSky(level), true);
        }

        /// <summary>
        /// Paper example C.
        /// </summary>
        public void ComplexLevel()
        {
            State.LC.AddNecessaryEvent($"Level Start", 100, level => M.LevelModule.LevelStart(), true);

            State.LC.AddNecessaryEvent($"Main path", 98, level => M.LevelModule.MainPath(level), true);

            State.LC.AddNecessaryEvent($"Sky", 0, level => M.EnvironmentModule.CreateSky(level), true);

            State.LC.AddNecessaryEvent($"Roofs", -1, level => M.LevelModule.AddRoofs(), true);

            M.FactionsModule.InitializeFactions(2);

            M.AscendingModule.AddAscendingEvents(M.AscendingModule.AscendingKiln(ad => 100 + 50 * ad));



            State.LC.AddNecessaryEvent($"Add Details", 0, level => M.DetailsModule.AddDetails(level), true);

            State.LC.AddNecessaryEvent($"Out of depth encounter", 80, level => M.OutOfDepthEncountersModule.DifficultEncounter(level), true);

            M.OutOfDepthEncountersModule.AddLightMaceEncounter();
        }
    }
}
