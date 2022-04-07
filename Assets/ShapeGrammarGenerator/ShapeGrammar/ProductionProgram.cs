using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShapeGrammar
{
    public class ProductionProgram
    {
        public LevelDevelopmentKit ldk { get; }
        bool Failed { get; set; }

        IEnumerable<Vector3Int> Directions { get; }

        public ProductionProgram(LevelDevelopmentKit ldk)
        {
            this.ldk = ldk;
        }
        /*
        public ProductionProgram GetDirection(PathGuide pathGuide)
        {
            Directions = pathGuide.SelectDirections()
        }*/
    }
}
