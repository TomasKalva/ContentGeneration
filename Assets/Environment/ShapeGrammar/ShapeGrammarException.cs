using System;

namespace OurFramework.Environment.ShapeGrammar
{
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
