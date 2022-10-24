using OurFramework.Environment.ShapeGrammar;

namespace OurFramework.LevelDesignLanguage.CustomModules
{
    class TutorialModule1 : LDLanguage
    {
        public TutorialModule1(LanguageParams parameters) : base(parameters) { }

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
        }

        void AddRoofs()
        {
            Env.Execute(new AllGrammar(Gr.PrL.Roofs()));
        }
    }
}
