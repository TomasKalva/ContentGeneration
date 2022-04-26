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
            var gr = new Grammars(ldk);
            var env = new Environments(grammarState);
            ProductionProgram.pr = new Productions(ldk, new Symbols());
            ProductionProgram.ldk = ldk;
            ProductionProgram.StyleRules = ldk.houseStyleRules;

            MyLanguage language = new MyLanguage(new LanguageParams(ldk, lib, gr, grammarState, env));

            language.MyLevel();
            //((FarmersLanguage < LDLanguage > )language).FarmerBranch(0);

            grammarState.Print(new PrintingState()).Show();
            grammarState.Stats.Print();

            var level = grammarState.WorldState.Added;
            return level;
        }
    }
}
