using System.Collections.Generic;
using UnityEngine;

namespace Geometry
{
    public struct StreightLineSegment : ILineSegment
    {
        public StreightLineSegment (Vector2 start, Vector2 end)
        {
            Points = new List<Vector2> {start, end};
        }

        public List<Vector2> Points { get; }
    }
}