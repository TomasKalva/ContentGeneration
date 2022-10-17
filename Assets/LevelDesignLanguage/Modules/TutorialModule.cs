using OurFramework.Environment.ShapeGrammar;

namespace OurFramework.LevelDesignLanguage.CustomModules
{
    class TutorialModule : LDLanguage
    {
        public TutorialModule(LanguageParams parameters) : base(parameters) { }
        
        public void Main()
        {
            LevelStart();
        }

        void LevelStart()
        {
            Env.One(Gr.PrL.CreateNewHouse(), NodesQueries.All, out var area);
            area.Get.Node.AddSymbol(Gr.Sym.LevelStartMarker);
        }
    }
}
