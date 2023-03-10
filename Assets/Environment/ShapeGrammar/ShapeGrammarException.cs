using System;

namespace OurFramework.Environment.ShapeGrammar
{
    /// <summary>
    /// Thrown when shape grammar fails.
    /// </summary>
    class ShapeGrammarException : Exception
    {
        public ShapeGrammarException(string message) : base(message)
        {
        }
    }

    class PathNotFoundException : ShapeGrammarException
    {
        public PathNotFoundException(string message) : base(message)
        {
        }
    }

    class NoApplicableProductionException : ShapeGrammarException
    {
        public NoApplicableProductionException(string message) : base(message)
        {
        }
    }
}
