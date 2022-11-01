using OurFramework.LevelDesignLanguage.CustomModules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OurFramework.LevelDesignLanguage
{
    class Modules
    {
        public LevelModule LevelModule { get; private set; }
        public LockingModule LockingModule { get; private set; }
        public TestingModule TestingModule { get; private set; }
        public FactionsModule FactionsModule { get; private set; }
        public AscendingModule AscendingModule { get; private set; }
        public OutOfDepthEncountersModule OutOfDepthEncountersModule { get; private set; }
        public DetailsModule DetailsModule { get; private set; }
        public EnvironmentModule EnvironmentModule { get; private set; }
        public DeathModule DeathModule { get; private set; }
        public TutorialModule TutorialModule { get; private set; }


        public Modules()
        {
        }

        /// <summary>
        /// This class is referenced by LanguageParams and also requires them for initialization so
        /// languages can't be initialized in constructor.
        /// </summary>
        public void Initialize(LanguageParams languageParams)
        {
            LevelModule = new LevelModule(languageParams);
            LockingModule = new LockingModule(languageParams);
            TestingModule = new TestingModule(languageParams);
            FactionsModule = new FactionsModule(languageParams);
            AscendingModule = new AscendingModule(languageParams);
            OutOfDepthEncountersModule = new OutOfDepthEncountersModule(languageParams);
            DetailsModule = new DetailsModule(languageParams);
            EnvironmentModule = new EnvironmentModule(languageParams);
            DeathModule = new DeathModule(languageParams);
            TutorialModule = new TutorialModule(languageParams);
        }
    }
}
