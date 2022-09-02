using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.ShapeGrammarGenerator.ShapeGrammar
{
    class PathNotFoundException : Exception
    {
        public PathNotFoundException(string message) : base(message)
        {
        }
    }
}
