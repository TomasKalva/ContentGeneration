using System;

namespace OurFramework.Environment.GridMembers
{
    /// <summary>
    /// Thrown when an operation with contents of grid is invalid.
    /// </summary>
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
