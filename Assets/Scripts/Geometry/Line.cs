using UnityEngine;

namespace Geometry
{
    public struct Line
    {
        public readonly float A;
        public readonly float B;
        public readonly float C;

        public readonly float k;
        public readonly float b;

        public Line (float a, float b, float c)
        {
            A = a;
            B = b;
            C = c;
            
            k = -a / b;
            this.b = -c / b;
        }

        public static Line Create (Vector2 pointA, Vector2 pointB)
        {
            var a = pointA.y - pointB.y;
            var b = pointB.x - pointA.x;
            var c = pointA.x * pointB.y - pointB.x * pointA.y;

            return new Line (a, b, c);
        }

        public override string ToString ()
        {
            return
                $"[Line ({nameof (A)}: {A}, {nameof (B)}: {B}, {nameof (C)}: {C}, {nameof (k)}: {k}, {nameof (b)}: {b}]";
        }
    }
}