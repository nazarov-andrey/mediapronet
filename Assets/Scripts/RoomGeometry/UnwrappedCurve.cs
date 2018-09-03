using System;
using System.Numerics;
using Geometry;
using UnityEngine;
using UnityEngine.Assertions;
using SystemVector2 = System.Numerics.Vector2;
using Vector2 = UnityEngine.Vector2;

namespace RoomGeometry
{
    public struct UnwrappedCurve
    {
        public readonly Matrix3x2[] Matrices;
        public readonly Matrix3x2[] InverseMatrices;
        public readonly SystemVector2[] OriginalPoints;
        public readonly SystemVector2[] UnwrappedPoints;

        public UnwrappedCurve (Vector2[] points)
            : this (
                Array.ConvertAll (points, x => x.ToSystemVector2 ()))
        {
        }

        public UnwrappedCurve (SystemVector2[] points)
        {
            Matrices = new Matrix3x2[points.Length];
            InverseMatrices = new Matrix3x2[points.Length];
            OriginalPoints = new SystemVector2[points.Length];
            UnwrappedPoints = new SystemVector2[points.Length];

            var matrix = Matrix3x2.CreateTranslation (-points[0]);
            OriginalPoints = points;

            for (int count = points.Length, i = 0; i < count; i++) {
                var originalPoint = points[i];
                var point = SystemVector2.Transform (originalPoint, matrix);
                Matrix3x2 inveseMatrix;
                Assert.IsTrue (Matrix3x2.Invert (matrix, out inveseMatrix));
                Matrices[i] = matrix;
                InverseMatrices[i] = inveseMatrix;
                UnwrappedPoints[i] = point;

                if (i < count - 1) {
                    var originalNextPoint = points[i + 1];
                    var nextPoint = SystemVector2.Transform (originalNextPoint, matrix);
                    var line = Line.Create (point, nextPoint);
                    var angle = -Mathf.Atan2 (-line.A, line.B);

                    matrix *= Matrix3x2.CreateRotation (angle, point);
                }
            }
        }

        private int FindNearestPointIndex (SystemVector2[] points, SystemVector2 point)
        {
            int count = points.Length;
            for (int i = 0; i < count - 1; i++) {
                if (points[i].X < point.X)
                    continue;

                return i + 1;
            }

            return count - 1;

/*            var index = 0;
            var minDistance = float.MaxValue;
            for (int count = points.Length, i = 0; i < count; i++) {
                var distance = Mathf.Abs (points[i].X - point.X);
                if (distance < minDistance) {
                    minDistance = distance;
                    index = i;
                }
            }

            return index;*/
        }

        public SystemVector2 Wrap (SystemVector2 point)
        {
            var index = FindNearestPointIndex (OriginalPoints, point);
            var matrix = Matrices[index];
            var result = SystemVector2.Transform (point, matrix);
            
            Debug.Log ($"--->>>Wrap {point} index {index} matrix {matrix} result {result}");

            return result;
        }

        public Vector2 Wrap (Vector2 point)
        {
            return Wrap (point.ToSystemVector2 ()).ToUnityVector2 ();
        }

        public SystemVector2 WrapBack (Vector2 point)
        {
            return WrapBack (point.ToSystemVector2 ());
        }

        public SystemVector2 WrapBack (SystemVector2 point)
        {
            var index = FindNearestPointIndex (UnwrappedPoints, point);
            var matrix = InverseMatrices[index];
            point = SystemVector2.Transform (point, matrix);

            return point;
        }
    }
}