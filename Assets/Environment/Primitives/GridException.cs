using System;

namespace OurFramework.Environment.GridMembers
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
