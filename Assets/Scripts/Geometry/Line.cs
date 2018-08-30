using UnityEngine;
using SystemVector = System.Numerics.Vector2;

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

        public float Calculate (float x)
        {
            if (float.IsInfinity (k))
                return Random.Range (-10f, 10f);

            return k * x + b;
        }

        public static Line Create (Vector2 pointA, Vector2 pointB)
        {
            var a = pointA.y - pointB.y;
            var b = pointB.x - pointA.x;
            var c = pointA.x * pointB.y - pointB.x * pointA.y;

            return new Line (a, b, c);
        }
        
        public static Line Create (SystemVector pointA, SystemVector pointB)
        {
            var a = pointA.Y - pointB.Y;
            var b = pointB.X - pointA.X;
            var c = pointA.X * pointB.Y - pointB.X * pointA.Y;

            return new Line (a, b, c);
        }

        public override string ToString ()
        {
            return
                $"[Line ({nameof (A)}: {A}, {nameof (B)}: {B}, {nameof (C)}: {C}, {nameof (k)}: {k}, {nameof (b)}: {b}]";
        }
    }
}