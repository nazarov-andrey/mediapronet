using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Geometry;
using UnityEngine;
using UnityEngine.Assertions;
using SystemVector2 = System.Numerics.Vector2;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

namespace RoomGeometry
{
    public static class PlaneMeshMaker
    {
        private static bool logEnabled = false;

        private static void Log (string message)
        {
            if (logEnabled)
                Debug.Log (message);
        }

        public static Mesh GetMesh (Vector3 a, Vector3 b, Vector3 c, Vector3 d, string name = null)
        {
            var vertices = new[] {a, b, c, d};
            var trianles = new[] {0, 3, 2, 0, 2, 1};

            return MeshGenerator.CreateMesh (vertices, trianles, false, name);
        }

        public static Mesh GetMesh (SystemVector2[] points, float height, string name = null)
        {
            var matrix = Matrix3x2.CreateTranslation (-points[0]);
            var matrices = new List<Matrix3x2> ();
            var unwrappedPoints = new List<SystemVector2> ();
            for (int count = points.Length, i = 0; i < count; i++) {
                var originalPoint = points[i];
                var point = SystemVector2.Transform (originalPoint, matrix);
                Matrix3x2 inveseMatrix;
                Assert.IsTrue (Matrix3x2.Invert (matrix, out inveseMatrix));
                matrices.Add (inveseMatrix);
                Log (
                    $"adding unwrapped point originalPoint {originalPoint} point {point} points {string.Join (",", Array.ConvertAll (points, x => SystemVector2.Transform (x, matrix)).ToArray ())}");
                unwrappedPoints.Add (point);

                if (i < count - 1) {
                    var originalNextPoint = points[i + 1];
                    var nextPoint = SystemVector2.Transform (originalNextPoint, matrix);
                    var line = Line.Create (point, nextPoint);
                    Log (
                        $"line nextPoint: {nextPoint} originalNextPoint: {originalNextPoint} originalPoint: {originalPoint} point: {point} {line}");
                    var angle = -Mathf.Atan2 (-line.A, line.B);

                    Log ($"angle {angle * Mathf.Rad2Deg}");

                    matrix *= Matrix3x2.CreateRotation (angle, point);
                }
            }

            Log (
                $"unwrappedPoints {string.Join (",", unwrappedPoints.ConvertAll (x => x.ToString ()).ToArray ())}");

            var sourceVertices = unwrappedPoints.ConvertAll (x => x.ToUnityVector2 ());
            sourceVertices.AddRange (
                unwrappedPoints
                    .Select (x => x.ToUnityVector2 () + new Vector2 (0f, height))
                    .Reverse ()
                    .ToList ());

            List<int> trianles;
            List<Vector3> vertices;

            MeshGenerator.Triangulate (
                sourceVertices,
                new List<List<Vector2>> (),
                false,
                out vertices,
                out trianles);

            for (int count = vertices.Count, i = 0; i < count; i++) {
                var vertex = vertices[i];
                var point = new SystemVector2 (vertex.x, vertex.y);
                var index = unwrappedPoints.FindIndex (x => Mathf.Approximately (x.X, point.X));
                matrix = matrices[index];
                point = SystemVector2.Transform (point, matrix);

                vertices[i] = new Vector3 (point.X, vertex.z, point.Y);
            }

            return MeshGenerator.CreateMesh (vertices.ToArray (), trianles.ToArray (), false, name);
        }
    }
}