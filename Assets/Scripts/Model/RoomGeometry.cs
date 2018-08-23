using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Model.RoomGeometry
{
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
    }

    public struct Wall
    {
        public readonly Vector2 Start;
        public readonly Vector2 End;
        public readonly Vector2 Size;
        public readonly Openings Openings;

        public Wall (Vector2 start, Vector2 end, Openings openings)
        {
            Start = start;
            End = end;
            Openings = openings;
            Size = End - Start;
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