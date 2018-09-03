using System;
using System.Numerics;
using Geometry;
using UnityEngine;
using SystemVector2 = System.Numerics.Vector2;
using Vector2 = UnityEngine.Vector2;

namespace Model
{
    public class WallPointNormals
    {
        public readonly Vector2 Inner;
        public readonly Vector2 Outer;

        public WallPointNormals (Vector2 inner)
        {
            Inner = inner.normalized;
            Outer = Inner * -1f;
        }
    }

    public class WallSegmentLines
    {
        public readonly Line Inner;
        public readonly Line Outer;

        public WallSegmentLines (Line inner, Line outer)
        {
            Inner = inner;
            Outer = outer;
        }

        public override string ToString ()
        {
            return $"[WallSegmentLines {nameof (Inner)}: {Inner}, {nameof (Outer)}: {Outer}]";
        }
    }

    [Flags]
    public enum OpeningType
    {
        Inner = 1,
        Outer = 2,
        Through = Inner | Outer 
    }
    
    public class OpeningData
    {
        public readonly OpeningType Type;
        public readonly Vector2[] Points;

        public OpeningData (OpeningType type, Vector2[] points)
        {
            Type = type;
            Points = points;
        }
    }

    public class WallData
    {
        public readonly Vector2[] Points;
        public readonly float Width;
        public readonly float Height;
        public readonly OpeningData[] Openings;
        public readonly WidthChangeType WidthChangeType;

        public readonly Lazy<Vector2[]> InnerPoints;
        public readonly Lazy<Vector2[]> OuterPoints;
        public readonly Lazy<float> StartAngle;
        public readonly Lazy<float> EndAngle;
        public readonly Lazy<WallPointNormals[]> Normals;
        public readonly Lazy<WallSegmentLines[]> Lines;

        public WallData (
            Vector2[] points,
            float width,
            float height,
            OpeningData[] openings,
            WidthChangeType widthChangeType)
        {
            Points = points;
            Width = width;
            Height = height;
            Openings = openings;
            WidthChangeType = widthChangeType;

            StartAngle = new Lazy<float> (GetStartAngle);
            EndAngle = new Lazy<float> (GetEndAngle);
            Normals = new Lazy<WallPointNormals[]> (GetNormals);
            Lines = new Lazy<WallSegmentLines[]> (GetLines);
            InnerPoints = new Lazy<Vector2[]> (GetInnerPoints);
            OuterPoints = new Lazy<Vector2[]> (GetOuterPoints);
        }

        private Vector2[] GetInnerPoints ()
        {
            var normals = Normals.Value;
            var innerPoints = new Vector2[Points.Length];
            for (int i = 0; i < Points.Length; i++) {
                innerPoints[i] = Points[i] + normals[i].Inner * Width / 2f;
            }

            return innerPoints;
        }

        private Vector2[] GetOuterPoints ()
        {
            var normals = Normals.Value;
            var outerPoints = new Vector2[Points.Length];
            for (int i = 0; i < Points.Length; i++) {
                outerPoints[i] = Points[i] + normals[i].Outer * Width / 2f;
            }

            return outerPoints;
        }

        private WallSegmentLines[] GetLines ()
        {
            var innerPoints = InnerPoints.Value;
            var outerPoints = OuterPoints.Value;
            var lines = new WallSegmentLines[Points.Length - 1];
            for (int i = 0; i < innerPoints.Length - 1; i++) {
                lines[i] = new WallSegmentLines (
                    Line.Create (innerPoints[i + 1], innerPoints[i]),
                    Line.Create (outerPoints[i + 1], outerPoints[i]));
            }

            return lines;
        }

        private WallPointNormals[] GetNormals ()
        {
            var lines = new Line[Points.Length - 1];
            for (int i = 0; i < Points.Length - 1; i++) {
                lines[i] = Line.Create (Points[i + 1], Points[i]);
            }

            var normals = new WallPointNormals[Points.Length];
            normals[0] = new WallPointNormals (lines[0].GetNormalVector ());
            normals[normals.Length - 1] = new WallPointNormals (lines[lines.Length - 1].GetNormalVector ());

            for (int i = 0; i < Points.Length - 2; i++) {
                normals[i + 1] = new WallPointNormals (lines[i].GetNormalVector () + lines[i + 1].GetNormalVector ());
            }

            return normals;
        }

        private float GetStartAngle ()
        {
            var startLine = Line.Create (Points[1], Points[0]);
            return Mathf.Atan2 (-startLine.A, startLine.B);
        }

        private float GetEndAngle ()
        {
            var endLine = Line.Create (Points[Points.Length - 1], Points[Points.Length - 2]);
            return Mathf.Atan2 (-endLine.A, endLine.B);
        }

        public WallData Transform (Matrix3x2 matrix)
        {
            return new WallData (
                Array.ConvertAll (
                    Points,
                    x => SystemVector2.Transform (x.ToSystemVector2 (), matrix).ToUnityVector2 ()),
                Width,
                Height,
                Openings,
                WidthChangeType);
        }

        public static WallData CreateStreight (
            Vector2 start,
            Vector2 end,
            float width,
            float height,
            OpeningData[] openings = null,
            WidthChangeType widthChangeType = WidthChangeType.Type1)
        {
            return new WallData (
                new[] {start, end},
                width,
                height,
                openings,
                widthChangeType);
        }

        public static WallData CreateCurved (
            Vector2 p0,
            Vector2 p1,
            Vector2 p2,
            float width,
            float height,
            OpeningData[] openings = null,
            WidthChangeType widthChangeType = WidthChangeType.Type1,
            int quality = 50)
        {
            var bezierSegment = new QuadraticBezierSegment (p0, p1, p2, quality);
            return new WallData (
                bezierSegment.Points.ToArray (),
                width,
                height,
                openings,
                widthChangeType);
        }
    }

    public enum WidthChangeType
    {
        Type1,
        Type2
    }
}