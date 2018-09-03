using UnityEngine;

namespace Geometry
{
    public struct QuadraticBezierSegment : ILineSegment
    {
        private readonly Vector2 p0;
        private readonly Vector2 p1;
        private readonly Vector2 p2;

        public QuadraticBezierSegment (float x0, float y0, float x1, float y1, float x2, float y2, int quality)
            : this (new Vector2 (x0, y0), new Vector2 (x1, y1), new Vector2 (x2, y2), quality)
        {
        }

        public QuadraticBezierSegment (Vector2 p0, Vector2 p1, Vector2 p2, int quality)
        {
            quality = Mathf.Max (3, quality);

            this.p0 = p0;
            this.p1 = p1;
            this.p2 = p2;

            Points = new Points ();
            for (int i = 0; i < quality; i++) {
                Points.Add (Calculate ((float) i / (quality - 1)));
            }
        }

        public Vector2 Calculate (float t)
        {
            return Mathf.Pow (1 - t, 2f) * p0 + 2 * (1 - t) * t * p1 + Mathf.Pow (t, 2f) * p2;
        }

        public Points Points { get; }
    }
}