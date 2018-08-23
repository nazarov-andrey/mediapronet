using System;
using UnityEngine;

namespace Geometry
{
    public static class LineExtensions
    {
        public static bool IsVertical (this Line line)
        {
            return Mathf.Approximately (line.B, 0f);
        }

        public static bool IsHorizontal (this Line line)
        {
            return Mathf.Approximately (line.A, 0f);
        }

        public static bool IsParallelWith (this Line lineA, Line lineB)
        {
            return float.IsInfinity (lineA.k) && float.IsInfinity (lineB.k) || Mathf.Approximately (lineA.k, lineB.k);
        }

        public static bool IsPerpendicularWith (this Line lineA, Line lineB)
        {
            return float.IsInfinity (lineA.k) && Mathf.Approximately (lineB.k, 0f) ||
                   float.IsInfinity (lineB.k) && Mathf.Approximately (lineA.k, 0f) ||
                   Mathf.Approximately (lineA.k, -1 / lineB.k);
        }

        public static float DistanceTo (this Line line, Vector2 point)
        {
            return Mathf.Abs (line.A * point.x + line.B * point.y + line.C) /
                   Mathf.Sqrt (Mathf.Pow (line.A, 2f) + Mathf.Pow (line.B, 2f));
        }

        public static bool Cross (this Line line1, Line line2, out Vector2 cross)
        {
            cross = default (Vector2);

            if (line1.IsParallelWith (line2))
                return false;

            var x = -(line1.C * line2.B - line2.C * line1.B) / (line1.A * line2.B - line2.A * line1.B);
            var y = -(line1.A * line2.C - line2.A * line1.C) / (line1.A * line2.B - line2.A * line1.B);
            cross = new Vector2 (x, y);

            return true;
        }

        public static Vector2 ProjectPoint (this Line line, Vector2 point, Vector2 direction)
        {
            var projectLine = Line.Create (point, point + direction);
            Vector2 cross;
            if (!line.Cross (projectLine, out cross))
                throw new ArgumentException ();

            return cross;
        }

        public static Vector2 GetNormalVector (this Line line)
        {
            return new Vector2 (line.A, line.B);
        }
    }
}