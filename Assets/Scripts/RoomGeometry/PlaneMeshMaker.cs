using System;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using UnityEngine;
using SystemVector2 = System.Numerics.Vector2;
using SystemVector3 = System.Numerics.Vector3;

namespace RoomGeometry
{
    public static class PlaneMeshMaker
    {
        class HoleData
        {
            public readonly List<Vector2> Hole;
            public float Min;
            public float Max;

            public HoleData (List<Vector2> hole, float min, float max)
            {
                Hole = hole;
                Min = min;
                Max = max;
            }
        }

        enum PointType
        {
            Regular,
            PointStart,
            PointEnd
        }

        class Point
        {
            public class Comparer : IComparer<Point>
            {
                public int Compare (Point x, Point y)
                {
                    return Comparer<float>.Default.Compare (x.X, y.X);
                }
            }

            public PointType PointType;
            public readonly float X;

            public Point (PointType pointType, float x)
            {
                PointType = pointType;
                X = x;
            }

            public override string ToString ()
            {
                return $"{nameof (PointType)}: {PointType}, {nameof (X)}: {X}";
            }
        }

        private static HoleData GetHoleData (List<Vector2> hole)
        {
            var sorted = hole
                .OrderBy (x => x.x)
                .ToArray ();

            return new HoleData (hole, sorted.First ().x, sorted.Last ().x);
        }

        public static Mesh GetMesh (Vector3 a, Vector3 b, Vector3 c, Vector3 d, string name = null)
        {
            var vertices = new[] {a, b, c, d};
            var trianles = new[] {0, 3, 2, 0, 2, 1};

            return MeshGenerator.CreateMesh (vertices, trianles, false, name);
        }

        public static Mesh Triangulate (
            List<Vector2> sourceVertices,
            List<List<Vector2>> holes,
            UnfoldedCurve unfoldedCurve,
            string name = null)
        {
            List<int> trianles;
            List<Vector3> vertices;

            MeshGenerator.Triangulate (
                sourceVertices,
                holes,
                false,
                out vertices,
                out trianles);

            for (int count = vertices.Count, i = 0; i < count; i++) {
                var vertex = vertices[i];
                var point = unfoldedCurve.Unfold (new SystemVector2 (vertex.x, vertex.y));
                vertices[i] = new Vector3 (point.X, vertex.z, point.Y);
            }

            return MeshGenerator.CreateMesh (vertices.ToArray (), trianles.ToArray (), false, name);
        }

        public static Mesh GetMesh (
            Vector2[] contour,
            Vector2[][] sourceHoles,
            float height,
            string name = null)
        {
            return GetMesh (
                Array.ConvertAll (contour, x => x.ToSystemVector2 ()),
                sourceHoles == null
                    ? null
                    : Array.ConvertAll (sourceHoles, x => Array.ConvertAll (x, y => y.ToSystemVector2 ())),
                height,
                name);
        }

        public static Mesh GetMesh (
            SystemVector2[] contour,
            SystemVector2[][] sourceHoles,
            float height,
            string name = null)
        {
            return GetMesh (
                new UnfoldedCurve (contour),
                sourceHoles,
                height,
                name);
        }

        public static Mesh GetMesh (
            UnfoldedCurve unfoldedCurve,
            SystemVector2[][] sourceHoles,
            float height,
            string name = null)
        {
            using (new ProfileBlock ("GetMesh")) {
                var unwrappedPoints = unfoldedCurve.UnfoldedPoints.ToList ();
                var sourceVerticesBottomLine = unwrappedPoints.ConvertAll (x => x.ToUnityVector2 ());
                var width = unwrappedPoints.Last ().X;
                var holes = sourceHoles
                    ?.ToList ()
                    .ConvertAll (
                        x => x
                            .ToList ()
                            .ConvertAll (y => y.ToUnityVector2 ()));

                if (holes == null || holes.Count == 0) {
                    var sourceVerticesTopLine = sourceVerticesBottomLine
                        .Select (x => x + new Vector2 (0f, height))
                        .Reverse ()
                        .ToList ();

                    var sourceVertices = new List<Vector2> (sourceVerticesBottomLine);
                    sourceVertices.AddRange (sourceVerticesTopLine);

                    return Triangulate (sourceVertices, null, unfoldedCurve, name);
                }

                if (unwrappedPoints.Count == 2) {
                    var sourceVertices = new List<Vector2>
                    {
                        Vector2.zero,
                        new Vector2 (0f, height),
                        new Vector2 (width, height),
                        new Vector2 (width, 0f)
                    };

                    return Triangulate (sourceVertices, holes, unfoldedCurve, name);
                }

                var sortedHoles = holes
                    .Select (GetHoleData)
                    .OrderBy (x => x.Min)
                    .ToArray ();
                var xs = new List<Point> (
                    sourceVerticesBottomLine.ConvertAll (x => new Point (PointType.Regular, x.x)));

                foreach (var hole in sortedHoles) {
                    xs.Add (new Point (PointType.PointStart, hole.Min));
                    xs.Add (new Point (PointType.PointEnd, hole.Max));
                }

                xs.Sort ((x, y) => Comparer<float>.Default.Compare (x.X, y.X));
                var chunks = new List<List<float>> {new List<float> ()};
                var startedCount = 0;
                foreach (var x in xs) {
                    chunks
                        .Last ()
                        .Add (x.X);
                    switch (x.PointType) {
                        case PointType.Regular:
                            break;
                        case PointType.PointStart:
                            if (++startedCount == 1)
                                chunks.Add (new List<float> {x.X});
                            break;
                        case PointType.PointEnd:
                            if (--startedCount == 0)
                                chunks.Add (new List<float> {x.X});
                            break;
                        default:
                            throw new ArgumentOutOfRangeException ();
                    }
                }

                var meshes = new List<Mesh> ();
                foreach (var chunk in chunks) {
                    if (chunk.Count == 1)
                        continue;

                    var chunkBottomLine = new List<Vector2> (chunk.ConvertAll (x => new Vector2 (x, 0f)));
                    var chunkHoles = new List<List<Vector2>> ();
                    foreach (var x in chunk) {
                        var holesData = Array.FindAll (sortedHoles, y => Mathf.Approximately (x, y.Min));
                        if (holesData.Length > 0 && !Mathf.Approximately (x, chunk.Last ()))
                            chunkHoles.AddRange (holesData.Select (y => y.Hole));
                    }

                    var chunkTopLine = chunkBottomLine
                        .Select (x => new Vector2 (x.x, height))
                        .Reverse ()
                        .ToArray ();
                    var chunkVertices = new List<Vector2> (chunkBottomLine);
                    chunkVertices.AddRange (chunkTopLine);

                    var chunkMesh = Triangulate (chunkVertices, chunkHoles, unfoldedCurve);
                    meshes.Add (chunkMesh);

                    //                MeshGenerator.CreateGameObject ("qqq", chunkMesh);
                }

                //            throw new NotImplementedException ();

                var mesh = new Mesh ();
                mesh.name = name ?? "";
                mesh.Clear ();
                mesh.CombineMeshes (
                    meshes.ConvertAll (x => new CombineInstance {mesh = x}).ToArray (),
                    true,
                    false);

                return mesh;
            }
        }
    }
}