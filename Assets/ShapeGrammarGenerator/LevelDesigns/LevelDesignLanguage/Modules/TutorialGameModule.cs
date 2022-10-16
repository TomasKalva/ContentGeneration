﻿using Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage.Factions;
using Assets.Util;
using ContentGeneration.Assets.UI;
using ContentGeneration.Assets.UI.Model;
using ContentGeneration.Assets.UI.Util;
using ShapeGrammar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using static InteractiveObject;

namespace Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage
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