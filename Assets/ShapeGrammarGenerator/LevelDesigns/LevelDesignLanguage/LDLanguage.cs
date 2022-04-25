using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShapeGrammar
{
    class LDLanguage : 
        LevelLanguage<LDLanguage>, 
        BrothersLanguage<LDLanguage>
    {
    }

    #region Primitives

    class Area
    {

    }

    #endregion

    interface ILDLanguage<LanguageUserT>
    {

        public LanguageUserT This => (LanguageUserT)this;
    }

    #region Language tools

    interface EnvironmentCreatorLanguage<LanguageUserT> : ILDLanguage<LanguageUserT>
    {
        public LanguageUserT AddLine()
        {

            return This;
        }

        public LanguageUserT AddOne(Grammar grammar, out Area one)
        {
            one = null;

            return This;
        }

        public LanguageUserT LevelEnd()
        {

            return This;
        }
    }

    #endregion

    #region Module languages
    /// <summary>
    /// Conatains declaration of all data members of LDLanguage, so that the
    /// module sub-languages can use them.
    /// </summary>

    interface LevelLanguage<LanguageUserT> : ILDLanguage<LanguageUserT>
    {
        public LanguageUserT LevelStart()
        {

            return This;
        }

        public LanguageUserT LevelPathSegment()
        {

            return This;
        }

        public LanguageUserT LevelEnd()
        {

            return This;
        }
    }

    interface BrothersLanguage<LanguageUserT> : ILDLanguage<LanguageUserT>
    {
        public LanguageUserT ThymeTea()
        {

            return This;
        }

        public LanguageUserT GiftOfHope()
        {

            return This;
        }
    }

    #endregion

}
