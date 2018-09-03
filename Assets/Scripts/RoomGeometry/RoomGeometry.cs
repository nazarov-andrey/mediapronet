using System;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using JetBrains.Annotations;
using Model;
using UnityEngine;
using UnityEngine.Assertions;
using SystemVector2 = System.Numerics.Vector2;

namespace RoomGeometry
{
    public static class WallGeometry
    {
        private static List<List<Vector2>> dummyHolesList = new List<List<Vector2>> ();

        private static Vector2 ProjectOpeningPoint (
            Vector2 openingPoint,
            WallData wall,
            UnwrappedCurve unwrappedWall,
            UnwrappedCurve unwrappedWallSide,
            Func<WallPointNormals, Vector2> normalsGetter)
        {
/*            Debug.Log (
                $"unwrappedWall {string.Join (",", Array.ConvertAll (unwrappedWall.UnwrappedPoints, x => x.ToString ()))}");

            Debug.Log (
                $"unwrappedWallSide {string.Join (",", Array.ConvertAll (unwrappedWallSide.UnwrappedPoints, x => x.ToString ()))}");*/

            var wallPoints = wall.Points;
            for (int i = 1; i < wallPoints.Length; i++) {
                if (openingPoint.x > unwrappedWall.UnwrappedPoints[i].X)
                    continue;

                var normals = wall.Normals.Value;
                var normal = normalsGetter (normals[i]);
                var prevNormal = normalsGetter (normals[i - 1]);
                var averageNormal = normal + prevNormal;
                var unwrapped = unwrappedWall
                    .Unwrap (new Vector2 (openingPoint.x, 0f))
                    .ToUnityVector2 ();

//                var unwrappedDebug = new Vector3 (unwrapped.x, openingPoint.y, unwrapped.y);

                var unwrappedOnSide = unwrapped.TransposePoint (averageNormal, wall.Width / 2f);
                var wrappedOnSide = unwrappedWallSide.Wrap (unwrappedOnSide);
                var result = new Vector2 (wrappedOnSide.x, openingPoint.y);

//                var unwrappedOnSideDebug = new Vector3 (unwrappedOnSide.x, openingPoint.y, unwrappedOnSide.y);
                
/*                Debug.DrawLine(unwrappedDebug, unwrappedOnSideDebug, color, float.MaxValue);
                Debug.DrawLine(unwrappedDebug, unwrappedOnSideDebug + new Vector3(normal.x, 0f, normal.y), Color.yellow, float.MaxValue);*/
/*
                Debug.Log (
                    $"openingPoint {openingPoint} unwrapped {unwrapped} unwrappedOnSide {unwrappedOnSide} wrappedOnSide {wrappedOnSide} result {result}");
*/

                return result;
            }

            throw new NotImplementedException ();
        }

        private static Vector3 GetAbsoluteHolePosition (UnwrappedCurve curve, Vector2 relativePosition)
        {
            var unwrapped = curve.Unwrap (new Vector2 (relativePosition.x, 0f));
            var result = new Vector3 (unwrapped.X, relativePosition.y, unwrapped.Y);

            return result;
        }

        public static Mesh[] GetWallMeshes (
            WallData prevWall,
            WallData wall,
            WallData nextWall)
        {
//            var matrix = Matrix3x2.CreateRotation (sourceWall.StartAngle.Value, sourceWall.Points[0]);

/*            var prevWall = sourcePrevWall.Transform (matrix);
            var wall = sourceNextWall.Transform (matrix);
            var nextWall = sourceNextWall.Transform (matrix);*/

            var prevLines = prevWall.Lines.Value.Last ();
            var startLines = wall.Lines.Value.First ();
            var endLines = wall.Lines.Value.Last ();
            var nextLines = nextWall.Lines.Value.First ();

            Vector2 innerStart, innerEnd, outerStart, outerEnd;

            var innerPoints = wall.InnerPoints.Value;
            var outerPoints = wall.OuterPoints.Value;


            if (prevLines.Inner.Cross (startLines.Inner, out innerStart)) {
                Assert.IsTrue (prevLines.Outer.Cross (startLines.Outer, out outerStart));
            } else {
                innerStart = innerPoints.First ();
                outerStart = outerPoints.First ();
            }

            if (nextLines.Inner.Cross (endLines.Inner, out innerEnd)) {
                nextLines.Outer.Cross (endLines.Outer, out outerEnd);
            } else {
                switch (wall.WidthChangeType) {
                    case WidthChangeType.Type1:
                        innerEnd = innerPoints.Last ();
                        outerEnd = outerPoints.Last ();
                        break;
                    case WidthChangeType.Type2:
                        innerEnd = nextWall
                            .InnerPoints
                            .Value
                            .First ();

                        outerEnd = nextWall
                            .OuterPoints
                            .Value
                            .First ();

                        break;
                    default:
                        throw new ArgumentOutOfRangeException ();
                }
            }

            var finalInnerPointsList = new List<Vector2> ();
            finalInnerPointsList.Add (innerStart);
            finalInnerPointsList.AddRange (innerPoints.Where ((_, i) => i > 0 && i < innerPoints.Length - 1));
            finalInnerPointsList.Add (innerEnd);

            var finalOuterPointsList = new List<Vector2> ();
            finalOuterPointsList.Add (outerStart);
            finalOuterPointsList.AddRange (outerPoints.Where ((_, i) => i > 0 && i < outerPoints.Length - 1));
            finalOuterPointsList.Add (outerEnd);

            var finalInnerPoints = finalInnerPointsList.ToArray ();
            var finalOuterPoints = finalOuterPointsList.ToArray ();

            var unwrappedWall = new UnwrappedCurve (wall.Points);
            var outerUnwrappedWall = new UnwrappedCurve (finalOuterPoints);
            var innerUnwrappedWall = new UnwrappedCurve (finalInnerPoints);

            var outerHoles = wall
                .Openings
                ?.Select (
                    x => x
                        .Points
                        .Select (
                            y => ProjectOpeningPoint (
                                y,
                                wall,
                                unwrappedWall,
                                outerUnwrappedWall,
                                z => z.Outer))
                        .ToArray ())
                .ToArray ();

            var innerHoles = wall
                .Openings
                ?.Select (
                    x => x
                        .Points
                        .Select (
                            y => ProjectOpeningPoint (
                                y,
                                wall,
                                unwrappedWall,
                                innerUnwrappedWall,
                                z => z.Inner))
                        .ToArray ())
                .ToArray ();

            var outerUnwrappedHoles = outerHoles
                ?.Select (
                    x => x
                        .Select (y => GetAbsoluteHolePosition (outerUnwrappedWall, y))
                        .ToArray ())
                .ToArray ();

            var innerUnwrappedHoles = innerHoles
                ?.Select (
                    x => x
                        .Select (y => GetAbsoluteHolePosition (innerUnwrappedWall, y))
                        .ToArray ())
                .ToArray ();

            List<Mesh> jambMeshes = null;
            if (outerUnwrappedHoles != null) {
                jambMeshes = new List<Mesh> ();
                for (int i = 0; i < outerUnwrappedHoles.Length; i++) {
                    var jambVertices = new List<Vector3> ();
                    jambVertices.AddRange (innerUnwrappedHoles[i]);
                    jambVertices.AddRange (outerUnwrappedHoles[i]);

                    var jambTriangles = new List<int> ();
                    var verticesHalfLength = outerUnwrappedHoles[i].Length;
                    for (int j = 0; j < verticesHalfLength; j++) {
                        var inner = j;
                        var outer = verticesHalfLength + inner;
                        var nextInner = (j + 1) % verticesHalfLength;
                        var nextOuter = verticesHalfLength + nextInner;

                        jambTriangles.AddRange (
                            new[]
                            {
                                inner, nextInner, outer, nextInner, nextOuter, outer
                            });
                    }

                    var jambMesh = MeshGenerator.CreateMesh (
                        jambVertices.ToArray (),
                        jambTriangles.ToArray (),
                        false,
                        $"Jamb{i}");
                    jambMeshes.Add (jambMesh);
                }
            }


            Mesh innerMesh = PlaneMeshMaker.GetMesh (
                finalInnerPoints.ToArray (),
                innerHoles,
                wall.Height,
                "inner");
            Mesh outerMesh = PlaneMeshMaker.GetMesh (
                finalOuterPoints.ToArray (),
                outerHoles,
                wall.Height,
                "outer");
            outerMesh.FlipFaces ();

            float height = wall.Height;
            Mesh sideAMesh = PlaneMeshMaker.GetMesh (
                finalOuterPoints.First ().ToVector3 (height, true),
                finalOuterPoints.First ().ToVector3 (0, true),
                finalInnerPoints.First ().ToVector3 (0, true),
                finalInnerPoints.First ().ToVector3 (height, true),
                "leftside");

            Mesh sideBMesh = PlaneMeshMaker.GetMesh (
                finalOuterPoints.Last ().ToVector3 (height, true),
                finalInnerPoints.Last ().ToVector3 (height, true),
                finalInnerPoints.Last ().ToVector3 (0, true),
                finalOuterPoints.Last ().ToVector3 (0, true),
                "rightside");

            var topVertices = new List<Vector2> (finalInnerPoints);
            topVertices.Reverse ();
            topVertices.AddRange (finalOuterPoints);

            List<Vector3> vertices;
            List<int> triangles;
            MeshGenerator.Triangulate (
                topVertices,
                dummyHolesList,
                false,
                out vertices,
                out triangles);

            var topMesh = MeshGenerator.CreateMesh (
                vertices.ConvertAll (x => new Vector3 (x.x, height, x.z)).ToArray (),
                triangles.ToArray (),
                false,
                "top");

            var result = new List<Mesh> {innerMesh, outerMesh, sideAMesh, sideBMesh, topMesh};
            if (jambMeshes != null)
                result.AddRange (jambMeshes);

            return result.ToArray ();
        }
    }

    public class OpeningContour : List<Vector3>
    {
        public OpeningContour ()
        {
        }

        public OpeningContour ([NotNull] IEnumerable<Vector3> collection) : base (collection)
        {
        }

        public OpeningContour (int capacity) : base (capacity)
        {
        }
    }
}