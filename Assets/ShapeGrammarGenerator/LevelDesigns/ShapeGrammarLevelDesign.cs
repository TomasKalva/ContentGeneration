using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShapeGrammar
{

    public class ShapeGrammarLevelDesign : LevelDesign
    {
        public ShapeGrammarLevelDesign(LevelDevelopmentKit ldk) : base(ldk)
        {
        }

        public override LevelElement CreateLevel()
        {
            var pr = new Productions(ldk);
            var productionList = new List<Production>()
            {
                pr.CreateNewHouse(),
                pr.ExtrudeCourtyard(),
                //pr.ExtrudeRoof()
            };
            var shapeGrammar = new ShapeGrammar(productionList, ldk);
            shapeGrammar.DoProductions(15);
            var level = shapeGrammar.ShapeGrammarState.WorldState.Added;


            return level;
        }
    }
}
