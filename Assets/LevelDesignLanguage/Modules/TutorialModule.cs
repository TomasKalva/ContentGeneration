using OurFramework.Environment.ShapeGrammar;
using System.Linq;
using Util;

namespace OurFramework.LevelDesignLanguage.CustomModules
{
    class TutorialModule : LDLanguage
    {
        public TutorialModule(LanguageParams parameters) : base(parameters) { }
        
        public void Main()
        {
            LevelStart();
            LevelContinue();
            AddRoofs();
        }

        void LevelStart()
        {
            Env.One(Gr.PrL.CreateNewHouse(), NodesQueries.All, out var area);
            area.Get.Node.AddSymbol(Gr.Sym.LevelStartMarker);
        }

        void LevelContinue()
        {
            Env.Line(Gr.PrL.Town(), NodesQueries.All, 5, out var line);
            //var firstArea = path.AreasList.First();
            //firstArea.AddEnemy(Lib.Enemies.MayanSwordsman());
            /*
            Env.Execute()
            PlC.RandomAreaPlacer(new UniformDistr(1, 3), Lib.Enemies.MayanSwordsman() enemyMaker.GetRandomEnemy(level))
                .Place(pathToShortcut);

            Env.BranchRandomly(Gr.PrL.Town(), 5, out var branching);
            */
        }

        void AddRoofs()
        {
            Env.Execute(new AllGrammar(Gr.PrL.Roofs()));
        }
    }
}
