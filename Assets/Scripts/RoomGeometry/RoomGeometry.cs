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
        private static List<List<Vector2>> dummyHoles = new List<List<Vector2>> ();

        public static Mesh[] GetWallMeshes (
            BaseWallData prevWall,
            BaseWallData wall,
            BaseWallData nextWall)
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

            Assert.IsTrue (prevLines.Inner.Cross (startLines.Inner, out innerStart));
            Assert.IsTrue (prevLines.Outer.Cross (startLines.Outer, out outerStart));
            Assert.IsTrue (nextLines.Inner.Cross (endLines.Inner, out innerEnd));
            Assert.IsTrue (nextLines.Outer.Cross (endLines.Outer, out outerEnd));

            var innerPoints = wall.InnerPoints.Value;
            var outerPoints = wall.OuterPoints.Value;

            var finalInnerPoints = new List<SystemVector2> ();
            finalInnerPoints.Add (innerStart.ToSystemVector ());
            finalInnerPoints.AddRange (innerPoints.Where ((_, i) => i > 0 && i < innerPoints.Length - 1));
            finalInnerPoints.Add (innerEnd.ToSystemVector ());

            var finalOuterPoints = new List<SystemVector2> ();
            finalOuterPoints.Add (outerStart.ToSystemVector ());
            finalOuterPoints.AddRange (outerPoints.Where ((_, i) => i > 0 && i < outerPoints.Length - 1));
            finalOuterPoints.Add (outerEnd.ToSystemVector ());

            Mesh innerMesh = PlaneMeshMaker.GetMesh (finalInnerPoints.ToArray (), wall.Height, "inner");
            Mesh outerMesh = PlaneMeshMaker.GetMesh (finalOuterPoints.ToArray (), wall.Height, "outer");
            outerMesh.FlipFaces ();

            float height = wall.Height;
            Mesh sideAMesh = PlaneMeshMaker.GetMesh (
                finalOuterPoints.First ().ToUnityVector3 (height, true),
                finalOuterPoints.First ().ToUnityVector3 (0, true),
                finalInnerPoints.First ().ToUnityVector3 (0, true),
                finalInnerPoints.First ().ToUnityVector3 (height, true),
                "leftside");

            Mesh sideBMesh = PlaneMeshMaker.GetMesh (
                finalOuterPoints.Last ().ToUnityVector3 (height, true),
                finalInnerPoints.Last ().ToUnityVector3 (height, true),
                finalInnerPoints.Last ().ToUnityVector3 (0, true),
                finalOuterPoints.Last ().ToUnityVector3 (0, true),
                "rightside");

            var topVertices = new List<SystemVector2> (finalInnerPoints);
            topVertices.Reverse ();
            topVertices.AddRange (finalOuterPoints);

            List<Vector3> vertices;
            List<int> triangles;
            MeshGenerator.Triangulate (
                topVertices.ConvertAll (x => x.ToUnityVector2 ()),
                dummyHoles,
                false,
                out vertices,
                out triangles);

            var topMesh = MeshGenerator.CreateMesh (
                vertices.ConvertAll (x => new Vector3 (x.x, height, x.z)).ToArray (),
                triangles.ToArray (),
                false,
                "top");

            return new[] {innerMesh, outerMesh, sideAMesh, sideBMesh, topMesh};
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

    public class Opening
    {
        public readonly OpeningContour FrontContour;
        public readonly OpeningContour BackContour;
        public readonly bool HasBackWall;

        public Opening (OpeningContour frontContour, OpeningContour backContour, bool hasBackWall = true)
        {
            FrontContour = frontContour;
            BackContour = backContour;
            HasBackWall = hasBackWall;
        }
    }

    public class Openings : List<Opening>
    {
        public Openings ()
        {
        }

        public Openings ([NotNull] IEnumerable<Opening> collection) : base (collection)
        {
        }

        public Openings (int capacity) : base (capacity)
        {
        }
    }

    public struct Wall
    {
        public readonly Points Points;
        public readonly Vector2 Size;
        public readonly Openings Openings;
        public Vector2 Start => Points.First ();
        public Vector2 End => Points.Last ();

        public Wall (Points points, Openings openings)
        {
            Points = points;
            Openings = openings;
            Size = Points.Last () - Points.First ();
        }
    }

    public class Contour : List<Wall>
    {
    }

    public struct Geometry
    {
        public readonly Contour InnerContour;
        public readonly Contour OuterContour;

        public Geometry (Contour innerContour, Contour outerContour)
        {
            InnerContour = innerContour;
            OuterContour = outerContour;
        }
    }
}