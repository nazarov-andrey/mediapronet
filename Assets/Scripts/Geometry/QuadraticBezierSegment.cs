using System.Collections.Generic;
using UnityEngine;

namespace Geometry
{
    public struct QuadraticBezierSegment : ILineSegment
    {
        private readonly Vector2 p0;
        private readonly Vector2 p1;
        private readonly Vector2 p2;

        public QuadraticBezierSegment (Vector2 p0, Vector2 p1, Vector2 p2, int quality)
        {
            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;

            Points = new List<Vector2> ();
            for (int i = 0; i < quality; i++) {
                Points.Add (Calculate ((float) i / quality));
            }
        }

        public Vector2 Calculate (float t)
        {
            return Mathf.Pow (1 - t, 2f) * p0 + 2 * (1 - t) * t * p1 + Mathf.Pow (t, 2f) * p2;
        }

        public List<UnityEngine.Vector2> Points { get; }
    }
}