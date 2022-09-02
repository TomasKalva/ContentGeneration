using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.ShapeGrammarGenerator.Primitives
{
    class GridException : Exception
    {
    }

    class GroupEmptyException : GridException
    {

    }

    class NoValidMovesException : GridException
    {

    }
}
