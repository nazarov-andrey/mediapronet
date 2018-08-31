using System.Numerics;
using Geometry;
using UnityEngine;
using UnityEngine.Assertions;
using SystemVector2 = System.Numerics.Vector2;

namespace RoomGeometry
{
    public struct UnwrappedCurve
    {
        public readonly Matrix3x2[] Matrices;
        public readonly SystemVector2[] UnwrappedPoints;

        public UnwrappedCurve (SystemVector2[] points, Matrix3x2 initialMatrix)
        {
            Matrices = new Matrix3x2[points.Length];
            UnwrappedPoints = new SystemVector2[points.Length];

            var matrix = initialMatrix;
            for (int count = points.Length, i = 0; i < count; i++) {
                var originalPoint = points[i];
                var point = SystemVector2.Transform (originalPoint, matrix);
                Matrix3x2 inveseMatrix;
                Assert.IsTrue (Matrix3x2.Invert (matrix, out inveseMatrix));
                Matrices[i] = inveseMatrix;
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

        public SystemVector2 WrapBack (SystemVector2 point)
        {
            var index = 0;
            var minDistance = float.MaxValue;
            for (int unwrappedPointsCount = UnwrappedPoints.Length, j = 0; j < unwrappedPointsCount; j++) {
                var distance = Mathf.Abs (UnwrappedPoints[j].X - point.X);
                if (distance < minDistance) {
                    minDistance = distance;
                    index = j;
                }
            }

            var matrix = Matrices[index];
            point = SystemVector2.Transform (point, matrix);

            return point;
        }
    }
}