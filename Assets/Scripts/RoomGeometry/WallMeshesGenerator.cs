using System;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Model;
using UnityEngine;
using UnityEngine.Assertions;
using SystemVector2 = System.Numerics.Vector2;

namespace RoomGeometry
{
    public static class WallMeshesGenerator
    {
        struct OpeningPoint2D
        {
            public readonly Vector2 Position;
            public readonly Vector2 Normal;

            public OpeningPoint2D (Vector2 position, Vector2 normal)
            {
                Position = position;
                Normal = normal;
            }
        }

        struct OpeningPoint3D
        {
            public readonly Vector3 Position;
            public readonly Vector3 Normal;

            public OpeningPoint3D (Vector3 position, Vector3 normal)
            {
                Position = position;
                Normal = normal;
            }
        }

        struct Hole2DProjection
        {
            public readonly OpeningPoint2D[] Points;
            public readonly OpeningData Opening;

            public Hole2DProjection (OpeningPoint2D[] points, OpeningData opening)
            {
                Points = points;
                Opening = opening;
            }
        }

        struct Hole3DProjection
        {
            public readonly OpeningPoint3D[] Points;
            public readonly OpeningData Opening;

            public Hole3DProjection (OpeningPoint3D[] points, OpeningData opening)
            {
                Points = points;
                Opening = opening;
            }
        }

        private static List<List<Vector2>> dummyHolesList = new List<List<Vector2>> ();

        private static OpeningPoint2D ProjectOpeningPoint (
            Vector2 openingPoint,
            WallData wall,
            UnfoldedCurve unfoldedWall,
            UnfoldedCurve unfoldedWallSide,
            Func<WallPointNormals, Vector2> normalGetter)
        {
/*            Debug.Log (
                $"unwrappedWall {string.Join (",", Array.ConvertAll (unwrappedWall.UnwrappedPoints, x => x.ToString ()))}");

            Debug.Log (
                $"unwrappedWallSide {string.Join (",", Array.ConvertAll (unwrappedWallSide.UnwrappedPoints, x => x.ToString ()))}");*/

            var wallPoints = wall.Points;
            for (int i = 1; i < wallPoints.Length; i++) {
                if (openingPoint.x > unfoldedWall.UnfoldedPoints[i].X)
                    continue;

                var normals = wall.Normals.Value;
                var normal = normalGetter (normals[i]);
                var prevNormal = normalGetter (normals[i - 1]);
                var averageNormal = normal + prevNormal;
                var unwrapped = unfoldedWall
                    .Unfold (new Vector2 (openingPoint.x, 0f))
                    .ToUnityVector2 ();

//                var unwrappedDebug = new Vector3 (unwrapped.x, openingPoint.y, unwrapped.y);

                var unwrappedOnSide = unwrapped.TransposePoint (averageNormal, wall.Width / 2f);
                var wrappedOnSide = unfoldedWallSide.Fold (unwrappedOnSide);
                var result = new Vector2 (wrappedOnSide.x, openingPoint.y);

//                var unwrappedOnSideDebug = new Vector3 (unwrappedOnSide.x, openingPoint.y, unwrappedOnSide.y);

//                Debug.DrawLine(unwrappedDebug, unwrappedOnSideDebug, color, float.MaxValue);
//                Debug.DrawLine(unwrappedDebug, unwrappedOnSideDebug + new Vector3(normal.x, 0f, normal.y), Color.yellow, float.MaxValue);
/*
                Debug.Log (
                    $"openingPoint {openingPoint} unwrapped {unwrapped} unwrappedOnSide {unwrappedOnSide} wrappedOnSide {wrappedOnSide} result {result}");
*/

                return new OpeningPoint2D (result, -averageNormal);
            }

            throw new NotImplementedException ();
        }

        private static OpeningPoint3D GetHole3DPoint (UnfoldedCurve curve, OpeningPoint2D point2D)
        {
            var unwrapped = curve.Unfold (new Vector2 (point2D.Position.x, 0f));
            var result = new Vector3 (unwrapped.X, point2D.Position.y, unwrapped.Y);

            return new OpeningPoint3D (
                result,
                new Vector3 (point2D.Normal.x, 0f, point2D.Normal.y));
        }

        private static IEnumerable<Mesh> ProcessJambs (
            Hole3DProjection[] hole3DProjections,
            Hole3DProjection[] oppositeHole3DProjections,
            Predicate<OpeningData> isThroughPredicate,
            string name,
            bool flipFaces = false)
        {
            var jambMeshes = new List<Mesh> ();
            for (int i = 0; i < hole3DProjections.Length; i++) {
                var main3DHole = hole3DProjections[i];
                var opening = main3DHole.Opening;
                var jambVertices = new List<Vector3> ();

                if (isThroughPredicate (opening)) {
                    var opposite3DHole = oppositeHole3DProjections
                        .First (x => x.Opening == main3DHole.Opening);
                    jambVertices
                        .AddRange (
                            opposite3DHole
                                .Points
                                .Select (x => x.Position)
                                .ToArray ());
                } else {
                    var jambBackPoints = main3DHole
                        .Points
                        .Select (x => x.Position.TransposePoint (x.Normal, opening.Depth))
                        .ToArray ();
                    jambVertices.AddRange (jambBackPoints);

                    var jambCurvePoints = jambBackPoints
                        .Select (x => new Vector2 (x.x, x.z))
                        .OrderBy (x => x.x)
                        .ToArray ();

                    var jambCurve = new UnfoldedCurve (jambCurvePoints);
                    var jamb2DVertices = jambBackPoints
                        .Select (x => new Vector2 (jambCurve.Fold (new Vector2 (x.x, x.z)).x, x.y))
                        .ToList ();

                    var mesh = PlaneMeshMaker.Triangulate (jamb2DVertices, null, jambCurve, $"{name} back {i}");
                    if (flipFaces)
                        mesh.FlipFaces ();
                    jambMeshes.Add (mesh);
                }

                jambVertices.AddRange (
                    main3DHole
                        .Points
                        .Select (x => x.Position)
                        .ToArray ());

                var jambTriangles = new List<int> ();
                var verticesHalfLength = main3DHole.Points.Length;
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
                    flipFaces,
                    $"{name} {i}");
                jambMeshes.Add (jambMesh);
            }

            return jambMeshes;
        }

        private static Hole2DProjection[] OpeningsToHole2DProjections (
            WallData wall,
            UnfoldedCurve mainCurve,
            UnfoldedCurve sideCurve,
            Func<OpeningData, bool> predicate,
            Func<WallPointNormals, Vector2> normalGetter)
        {
            return wall
                .Openings
                ?.Where (predicate)
                .Select (
                    x => new Hole2DProjection (
                        x
                            .Points
                            .Select (
                                y => ProjectOpeningPoint (
                                    y,
                                    wall,
                                    mainCurve,
                                    sideCurve,
                                    normalGetter))
                            .ToArray (),
                        x))
                .ToArray ();
        }

        private static Hole3DProjection[] Hole2DProjectionsTo3D (
            Hole2DProjection[] projections2D,
            UnfoldedCurve curve)
        {
            return projections2D
                ?.Select (
                    x => new Hole3DProjection (
                        x
                            .Points
                            .Select (y => GetHole3DPoint (curve, y))
                            .ToArray (),
                        x.Opening
                    ))
                .ToArray ();
        }

        public static Mesh[] GetWallMeshes (
            WallData prevInnerWall,
            WallData prevOuterWall,
            WallData wall,
            WallData nextInnerWall,
            WallData nextOuterWall)
        {
/*            Debug.Log (
                $"pi: {prevInnerWall}; po: {prevOuterWall}; w: {wall}; ni: {nextInnerWall}; no: {nextOuterWall}");*/

            var prevInnerLine = prevInnerWall
                ?.Lines
                .Value
                .Last ()
                .Inner;

            var prevOuterLine = prevOuterWall
                ?.Lines
                .Value
                .Last ()
                .Outer;

            var nextInnerLine = nextInnerWall
                ?.Lines
                .Value
                .First ()
                .Inner;

            var nextOuterLine = nextOuterWall
                ?.Lines
                .Value
                .First ()
                .Outer;

            var startLines = wall
                .Lines
                .Value
                .First ();

            var endLines = wall
                .Lines
                .Value
                .Last ();

            Vector2 innerStart, innerEnd, outerStart, outerEnd;

            var innerPoints = wall.InnerPoints.Value;
            var outerPoints = wall.OuterPoints.Value;

            if (!(prevInnerLine.HasValue && startLines.Inner.Cross (prevInnerLine.Value, out innerStart)))
                innerStart = innerPoints.First ();

            if (!(prevOuterLine.HasValue && startLines.Outer.Cross (prevOuterLine.Value, out outerStart)))
                outerStart = outerPoints.First ();

            if (!(nextInnerLine.HasValue && endLines.Inner.Cross (nextInnerLine.Value, out innerEnd))) {
                switch (wall.WidthChangeType) {
                    case WidthChangeType.Type1:
                        innerEnd = innerPoints.Last ();
                        break;
                    case WidthChangeType.Type2:
                        Assert.IsNotNull (nextInnerWall);
                        innerEnd = nextInnerWall
                            .InnerPoints
                            .Value
                            .First ();

                        break;
                    default:
                        throw new ArgumentOutOfRangeException ();
                }
            }

            if (!(nextOuterLine.HasValue && endLines.Outer.Cross (nextOuterLine.Value, out outerEnd))) {
                switch (wall.WidthChangeType) {
                    case WidthChangeType.Type1:
                        outerEnd = outerPoints.Last ();
                        break;
                    case WidthChangeType.Type2:
                        Assert.IsNotNull (nextOuterWall);
                        outerEnd = nextOuterWall
                            .OuterPoints
                            .Value
                            .First ();

                        break;
                    default:
                        throw new ArgumentOutOfRangeException ();
                }
            }

/*            if (nextLines != null && nextLines.Inner.Cross (endLines.Inner, out innerEnd)) {
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
            }*/

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
            var unwrappedWall = new UnfoldedCurve (wall.Points);
            var outerUnwrappedWall = new UnfoldedCurve (finalOuterPoints);
            var innerUnwrappedWall = new UnfoldedCurve (finalInnerPoints);

            var outer2DHoles =
                OpeningsToHole2DProjections (
                    wall,
                    unwrappedWall,
                    outerUnwrappedWall,
                    x => x.Type.HasFlag (OpeningType.Outer),
                    x => x.Outer);

            var inner2DHoles =
                OpeningsToHole2DProjections (
                    wall,
                    unwrappedWall,
                    innerUnwrappedWall,
                    x => x.Type.HasFlag (OpeningType.Inner),
                    x => x.Inner);

            var outer3DHoles = Hole2DProjectionsTo3D (outer2DHoles, outerUnwrappedWall);
            var inner3DHoles = Hole2DProjectionsTo3D (inner2DHoles, innerUnwrappedWall);

            bool flipInnerJambs = wall.StartAngle.Value < Mathf.PI / 2f;
            List<Mesh> jambMeshes = null;
            if (outer3DHoles != null) {
                jambMeshes = new List<Mesh> ();
                jambMeshes.AddRange (
                    ProcessJambs (
                        outer3DHoles,
                        inner3DHoles,
                        x => x.Type.HasFlag (OpeningType.Through),
                        "Outer Jamb",
                        !flipInnerJambs));
            }

            if (inner3DHoles != null) {
                jambMeshes = jambMeshes ?? new List<Mesh> ();
                jambMeshes.AddRange (
                    ProcessJambs (
                        inner3DHoles
                            .Where (x => !x.Opening.Type.HasFlag (OpeningType.Through))
                            .ToArray (),
                        null,
                        x => false,
                        "Inner Jamb",
                        flipInnerJambs));
            }

            Mesh innerMesh = PlaneMeshMaker.GetMesh (
                finalInnerPoints.ToArray (),
                inner2DHoles?.Select (x => x.Points.Select (y => y.Position).ToArray ()).ToArray (),
                wall.Height,
                "inner");
            Mesh outerMesh = PlaneMeshMaker.GetMesh (
                finalOuterPoints.ToArray (),
                outer2DHoles?.Select (x => x.Points.Select (y => y.Position).ToArray ()).ToArray (),
                wall.Height,
                "outer");
            outerMesh.FlipFaces ();

            float height = wall.Height;
            Mesh sideAMesh = PlaneMeshMaker.GetMesh (
                "leftside",
                finalInnerPoints.First ().ToVector3 (0, true),
                wall.Start.ToVector3 (0, true),
                finalOuterPoints.First ().ToVector3 (0, true),
                finalOuterPoints.First ().ToVector3 (height, true),
                wall.Start.ToVector3 (height, true),
                finalInnerPoints.First ().ToVector3 (height, true));

            Mesh sideBMesh = PlaneMeshMaker.GetMesh (
                "rightside",
                finalOuterPoints.Last ().ToVector3 (0, true),
                wall.End.ToVector3 (0, true),
                finalInnerPoints.Last ().ToVector3 (0, true),
                finalInnerPoints.Last ().ToVector3 (height, true),
                wall.End.ToVector3 (height, true),
                finalOuterPoints.Last ().ToVector3 (height, true));

            var topVertices = new List<Vector2> (finalInnerPoints);
            topVertices.Reverse ();
            topVertices.Add (wall.Start);
            topVertices.AddRange (finalOuterPoints);
            topVertices.Add (wall.End);

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
}