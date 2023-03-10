using System;

namespace OurFramework.LevelDesignLanguage
{
    /// <summary>
    /// Thrown if something goes wrong with populating the level.
    /// </summary>
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
