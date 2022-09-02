using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.ShapeGrammarGenerator.Primitives
{
    class GridException : Exception
    {
        public GridException(string message) : base(message)
        {
        }
    }

    class GroupEmptyException : GridException
    {
        public GroupEmptyException(string message) : base(message)
        {
        }
    }

    class NoValidMovesException : GridException
    {
        public NoValidMovesException(string message) : base(message)
        {
        }
    }
}
