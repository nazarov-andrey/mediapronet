namespace Model
{
    public static class RoomExtension
    {
/*        private static Vector3 RelativeOpeningToAbsolute (
            IWall wall,
            Vector2 relative,
            Func<Line> lineGetter,
            Func<Vector2> directionGetter)
        {
            Vector3 absolute =
                lineGetter ().ProjectPoint (wall.Start + wall.Size.normalized * relative.x, directionGetter ());
            absolute.z = relative.y;

            return absolute;
        }*/

/*        public static RoomGeometry.Geometry GenerateGeometry (this Room room)
        {
            var innerPoints = new List<Tuple<Vector2?, List<Vector2>>> ();
            var outerPoints = new List<Tuple<Vector2?, List<Vector2>>> ();
            var walls = room.Walls;

            for (int count = walls.Count, i = count - 1; i < count * 2 - 1; i++) {
                var wall = walls[i % count];
                var nextWall = walls[(i + 1) % count];
                var wallLines = wall.EndLines;
                var nextWallLines = nextWall.StartLines;
                Vector2 cross;

                Vector2? innerPoint = null, outerPoint = null;

                if (wallLines.Inner.Cross (nextWallLines.Inner, out cross)) {
                    innerPoint = cross;
                    Assert.IsTrue (wallLines.Outer.Cross (nextWallLines.Outer, out cross));
                    outerPoint = cross;
                } else {
                    switch (wall.WidthChangeType) {
                        case WidthChangeType.Type1:
                            break;
                        case WidthChangeType.Type2:
                            innerPoint = nextWallLines.Inner.ProjectPoint (nextWall.Start, nextWallLines.InnerNormal);
                            outerPoint = nextWallLines.Outer.ProjectPoint (nextWall.Start, nextWallLines.OuterNormal);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException ();
                    }
                }

                var innerWallPoints = wall.InnerLine;
                var outerWallPoints = wall.OuterLine;

                List<Vector2> innerMiddlePoints = null, outerMiddlePoints = null;

                if (innerWallPoints.Count > 2)
                    innerMiddlePoints =
                        innerWallPoints
                            .GetRange (1, innerWallPoints.Count - 2);

                if (outerWallPoints.Count > 2)
                    outerMiddlePoints =
                        outerWallPoints
                            .GetRange (1, outerMiddlePoints.Count - 2);

                innerPoints.Add (new Tuple<Vector2?, List<Vector2>> (innerPoint, innerMiddlePoints));
                outerPoints.Add (new Tuple<Vector2?, List<Vector2>> (outerPoint, outerMiddlePoints));
            }

            var inner = new Contour ();
            var outer = new Contour ();
            for (int count = innerPoints.Count, i = 0; i < count; i++) {
                var nextIndex = (i + 1) % count;
                var wall = room.Walls[i];
                var startWallLines = wall.StartLines;
                var endWallLines = wall.EndLines;
                var innerOpenings = new RoomGeometry.Openings ();
                var outerOpenings = new RoomGeometry.Openings ();

                Vector2? innerPoint = innerPoints[i].Item1;
                Vector2? outerPoint = outerPoints[i].Item1;
                if (innerPoint == null) {
                    var startWidth = wall.Width / 2f;
                    innerPoint = wall.Start.TransposePoint (startWallLines.InnerNormal, startWidth);
                    outerPoint = wall.Start.TransposePoint (startWallLines.OuterNormal, startWidth);
                }

                Vector2? nextInnerPoint = innerPoints[nextIndex].Item1;
                Vector2? nextOuterPoint = outerPoints[nextIndex].Item1;
                if (nextInnerPoint == null) {
                    var nextWall = room.Walls[nextIndex];
                    var endWidth = wall.Width / 2f;
                    if (wall.WidthChangeType == WidthChangeType.Type2)
                        endWidth = nextWall.Width / 2f;

                    nextInnerPoint = wall.End.TransposePoint (endWallLines.InnerNormal, endWidth);
                    nextOuterPoint = wall.End.TransposePoint (endWallLines.OuterNormal, endWidth);
                }

                var actualInnerLine = Line.Create (innerPoint.Value, nextInnerPoint.Value);
                var actualOuterLine = Line.Create (outerPoint.Value, nextOuterPoint.Value);

                foreach (var opening in wall.Openings) {
                    OpeningContour frontContour, backContour;
                    Opening innerOpening = null, outerOpening = null;

                    if (opening.OpeningType.HasFlag (OpeningType.Outer)) {
                        frontContour = new OpeningContour (
                            opening.Conoutour.ConvertAll (
                                x =>
                                    RelativeOpeningToAbsolute (
                                        wall,
                                        x,
                                        () => actualOuterLine,
                                        () => startWallLines.OuterNormal)));

                        backContour = null;
                        if (opening.OpeningType != OpeningType.Through) {
                            var wallNormal = actualOuterLine.GetNormalVector () * -1;
                            backContour = new OpeningContour (
                                frontContour.ConvertAll (x => x.TransposePoint (wallNormal, opening.Depth)));
                        }

                        outerOpening = new Opening (frontContour, backContour);
                        outerOpenings.Add (outerOpening);
                    }

                    if (opening.OpeningType.HasFlag (OpeningType.Inner)) {
                        frontContour = new OpeningContour (
                            opening.Conoutour.ConvertAll (
                                x =>
                                    RelativeOpeningToAbsolute (
                                        wall,
                                        x,
                                        () => actualInnerLine,
                                        () => startWallLines.InnerNormal)));

                        bool HasBackWall = opening.OpeningType != OpeningType.Through;
                        if (HasBackWall) {
                            var wallNormal = actualInnerLine.GetNormalVector ();
                            backContour = new OpeningContour (
                                frontContour.ConvertAll (x => x.TransposePoint (wallNormal, opening.Depth)));
                        } else {
                            Assert.IsNotNull (outerOpening);
                            backContour = outerOpening.FrontContour;
                        }

                        innerOpening = new Opening (frontContour, backContour, HasBackWall);
                        innerOpenings.Add (innerOpening);
                    }

/*                    if (opening.OpeningType.HasFlag(OpeningType.Inner))
                        innerOpenings.Add (
                            new Opening (
                                opening.OpeningType,
                                opening.Depth,
                                opening.Conoutour.ConvertAll (
                                    x =>
                                        RelativeOpeningToAbsolute (
                                            wall,
                                            x,
                                            () => actualInnerLine,
                                            () => wallLines.InnerNormal))));
                    
                    if (opening.OpeningType.HasFlag(OpeningType.Outer))
                        outerOpenings.Add (
                            new Opening (
                                opening.OpeningType,
                                opening.Depth,
                                opening.Conoutour.ConvertAll (
                                    x =>
                                        RelativeOpeningToAbsolute (
                                            wall,
                                            x,
                                            () => actualOuterLine,
                                            () => wallLines.OuterNormal))));#1#
                }

                inner.Add (new Wall (new Points {innerPoint.Value, nextInnerPoint.Value}, innerOpenings));
                outer.Add (new Wall (new Points {outerPoint.Value, nextOuterPoint.Value}, outerOpenings));
            }

            return new RoomGeometry.Geometry (inner, outer);
        }*/
    }
}