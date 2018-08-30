using UnityEngine;

namespace Geometry
{
    public struct StreightLineSegment : ILineSegment
    {
        public StreightLineSegment (Vector2 start, Vector2 end)
        {
            Points = new Points {start, end};
        }

        public Points Points { get; }
    }
}