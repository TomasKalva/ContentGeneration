using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.Util
{
    public struct Ray3Int : IEnumerable<Vector3Int>
    {
        public Vector3Int Start { get; }
        public Vector3Int Direction { get; }

        public Ray3Int(Vector3Int start, Vector3Int direction)
        {
            Start = start;
            Direction = direction;
        }

        /// <summary>
        /// Returns ALL points on the ray.
        /// </summary>
        public IEnumerator<Vector3Int> GetEnumerator()
        {
            var curPos = Start;
            while (true)
            {
                yield return curPos;
                curPos += Direction;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
