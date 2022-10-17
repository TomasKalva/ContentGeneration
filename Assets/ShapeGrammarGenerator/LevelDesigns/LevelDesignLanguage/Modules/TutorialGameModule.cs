using ShapeGrammar;

namespace Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Modules
{

    class TutorialGameModule : LDLanguage
    {
        public TutorialGameModule(LanguageParams parameters) : base(parameters) { }
        
        public void Main()
        {

        }

        public void LevelStart()
        {
            Env.One(Gr.PrL.CreateNewHouse(), NodesQueries.All, out var area);
            area.Get.Node.AddSymbol(Gr.Sym.LevelStartMarker);
        }
    }
}
