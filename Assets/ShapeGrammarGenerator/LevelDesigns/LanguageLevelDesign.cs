using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShapeGrammar
{

    public class LanguageLevelDesign : LevelDesign
    {
        Libraries lib;

        public LanguageLevelDesign(LevelDevelopmentKit ldk, Libraries lib) : base(ldk)
        {
            this.lib = lib;
        }

        public override LevelElement CreateLevel()
        {
            var grammarState = new ShapeGrammarState(ldk);
            var pr = new Productions(ldk);
            ProductionProgram.pr = pr;
            ProductionProgram.ldk = ldk;
            ProductionProgram.StyleRules = ldk.houseStyleRules;

            ILDLanguageImpl language = new LDLanguage(grammarState, ldk, lib);

            language.Level();
            //((FarmersLanguage < LDLanguage > )language).FarmerBranch(0);

            grammarState.Print(new PrintingState()).Show();
            grammarState.Stats.Print();

            var level = grammarState.WorldState.Added;
            return level;
        }
    }
}
