using System.Collections.Generic;

namespace Model
{
    public class Apartment
    {
/*        struct WallLines
        {
            public readonly Line Inner;
            public readonly Line Outer;
            public readonly Vector2 InnerNormal;
            public readonly Vector2 OuterNormal;

            public WallLines (Wall wall)
            {
                var vector = wall.End - wall.Start;
                InnerNormal = vector
                                  .GetNormalVector ()
                                  .normalized *
                              wall.Width /
                              2f;
                OuterNormal = InnerNormal * -1f;

                Inner = Line.Create (wall.End + InnerNormal, wall.Start + InnerNormal);
                Outer = Line.Create (wall.End + OuterNormal, wall.Start + OuterNormal);
            }
        }

        public struct Hole
        {
            public readonly Vector2 InnerPivot;
            public readonly Vector2 OuterPivot;
            public readonly float PivotY;
            public readonly Vector2 Size;

            public Hole (Vector2 innerPivot, Vector2 outerPivot, float pivotY, Vector2 size)
            {
                InnerPivot = innerPivot;
                OuterPivot = outerPivot;
                PivotY = pivotY;
                Size = size;
            }

            public override string ToString ()
            {
                return
                    $"[Hole ({nameof (InnerPivot)}: {InnerPivot}, {nameof (OuterPivot)}: {OuterPivot}, {nameof (PivotY)}: {PivotY}, {nameof (Size)}: {Size})]";
            }
        }

        public class Holes : List<Hole>
        {
        }

        public struct Contours
        {
            public readonly List<Vector2> Inner;
            public readonly List<Vector2> Outer;
            public readonly List<Holes> Holes;

            public Contours (List<Vector2> inner, List<Vector2> outer, List<Holes> holes)
            {
                Inner = inner;
                Outer = outer;
                Holes = holes;
            }
        }*/

        public Rooms Rooms;

        public Apartment (params Room[] rooms)
        {
            Rooms = new Rooms (rooms);
        }


/*        public Contours CalculateContours ()
        {
            var innerContour = new List<Vector2> ();
            var outerContour = new List<Vector2> ();
            List<WallLines> wallsLines = Walls.ConvertAll (x => new WallLines (x));

            for (int count = wallsLines.Count, i = count - 1; i < count * 2 - 1; i++) {
                var wallLines = wallsLines[i % count];
                var nextWallLines = wallsLines[(i + 1) % count];
                Vector2 cross;

                Assert.IsTrue (wallLines.Inner.Cross (nextWallLines.Inner, out cross));
                innerContour.Add (cross);
                Assert.IsTrue (wallLines.Outer.Cross (nextWallLines.Outer, out cross));
                outerContour.Add (cross);
            }

            var allHoles = new List<Holes> ();
            for (int i = 0, wallsCount = Walls.Count; i < wallsCount; i++) {
                var wall = Walls[i];
                var openings = wall.Openings;
                var wallLines = wallsLines[i];
                var normalizedWallVector = wall.Vector.normalized;
                var holes = new Holes ();
                for (int j = 0, openingsCount = openings.Count; j < openingsCount; j++) {
                    var opening = openings[j];
                    var pivot = wall.Start + normalizedWallVector * opening.Pivot.x;
                    var innerPivot = pivot + wallLines.InnerNormal;
                    var outerPivot = pivot + wallLines.OuterNormal;

                    holes.Add (new Hole (innerPivot, outerPivot, opening.Pivot.y, opening.Size));
                }

                allHoles.Add (holes);
            }

            return new Contours (innerContour, outerContour, allHoles);
        }*/
    }
}