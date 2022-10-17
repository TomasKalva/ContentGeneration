using System;

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
