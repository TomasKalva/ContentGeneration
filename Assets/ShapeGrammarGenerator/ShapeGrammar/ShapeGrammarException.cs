using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.ShapeGrammarGenerator.ShapeGrammar
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
