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
            var language = new LDLanguage(grammarState, ldk, lib);

            ((LevelLanguage<LDLanguage>)language).LevelStart(out var start);
            //((FarmersLanguage < LDLanguage > )language).FarmerBranch(0);

            var level = grammarState.WorldState.Added;
            return level;
        }
    }
}
