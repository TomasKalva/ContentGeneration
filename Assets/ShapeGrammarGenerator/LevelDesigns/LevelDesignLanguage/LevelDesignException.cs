using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.ShapeGrammarGenerator.LevelDesigns.LevelDesignLanguage
{
    class LevelDesignException : Exception
    {
        public LevelDesignException(string message) : base(message)
        {
        }
    }

    class PlacementException : LevelDesignException
    {
        public PlacementException(string message) : base(message)
        {
        }
    }
}
