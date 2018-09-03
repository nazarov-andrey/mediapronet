using UnityEngine;

namespace Geometry
{
    public struct StreightLineSegment : ILineSegment
    {
        public StreightLineSegment (float x0, float y0, float x1, float y1)
            : this (new Vector2 (x0, y0), new Vector2 (x1, y1))
        {
        }

        public StreightLineSegment (Vector2 start, Vector2 end)
        {
            Points = new Points {start, end};
        }

        public Points Points { get; }
    }
}