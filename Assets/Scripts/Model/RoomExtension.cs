﻿using System;
using System.Collections.Generic;
using Geometry;
using Model.RoomGeometry;
using UnityEngine;
using UnityEngine.Assertions;

namespace Model
{
    public static class RoomExtension
    {
        private static Vector3 RelativeOpeningToAbsolute (
            Wall wall,
            Vector2 relative,
            Func<Line> lineGetter,
            Func<Vector2> directionGetter)
        {
            Vector3 absolute =
                lineGetter ().ProjectPoint (wall.Start + wall.Size.normalized * relative.x, directionGetter ());
            absolute.z = relative.y;

            return absolute;
        }

        public static RoomGeometry.Geometry GenerateGeometry (this Room room)
        {
            var innerPoints = new List<Vector2?> ();
            var outerPoints = new List<Vector2?> ();
            var walls = room.Walls;

            for (int count = walls.Count, i = count - 1; i < count * 2 - 1; i++) {
                var wall = walls[i % count];
                var nextWall = walls[(i + 1) % count];
                var wallLines = wall.lines;
                var nextWallLines = nextWall.lines;
                Vector2 cross;

                if (wallLines.Inner.Cross (nextWallLines.Inner, out cross)) {
                    innerPoints.Add (cross);
                    Assert.IsTrue (wallLines.Outer.Cross (nextWallLines.Outer, out cross));
                    outerPoints.Add (cross);
                } else {
                    switch (wall.widthChangeType) {
                        case Wall.WidthChangeType.Type1:
                            innerPoints.Add (null);
                            outerPoints.Add (null);
                            break;
                        case Wall.WidthChangeType.Type2:
                            innerPoints.Add (
                                nextWallLines.Inner.ProjectPoint (nextWall.Start, nextWallLines.InnerNormal));
                            outerPoints.Add (
                                nextWallLines.Outer.ProjectPoint (nextWall.Start, nextWallLines.OuterNormal));
                            break;
                        default:
                            throw new ArgumentOutOfRangeException ();
                    }
                }
            }

            var inner = new Contour ();
            var outer = new Contour ();
            for (int count = innerPoints.Count, i = 0; i < count; i++) {
                var nextIndex = (i + 1) % count;
                var wall = room.Walls[i];
                var wallLines = wall.lines;
                var innerOpenings = new Openings ();
                var outerOpenings = new Openings ();

                Vector2? innerPoint = innerPoints[i];
                Vector2? outerPoint = outerPoints[i];
                if (innerPoint == null) {
                    var startWidth = wall.Width / 2f;
                    innerPoint = wall.Start.TransposePoint (wallLines.InnerNormal, startWidth);
                    outerPoint = wall.Start.TransposePoint (wallLines.OuterNormal, startWidth);
                }

                Vector2? nextInnerPoint = innerPoints[nextIndex];
                Vector2? nextOuterPoint = outerPoints[nextIndex];
                if (nextInnerPoint == null) {
                    var nextWall = room.Walls[nextIndex];
                    var endWidth = wall.Width / 2f;
                    if (wall.widthChangeType == Wall.WidthChangeType.Type2)
                        endWidth = nextWall.Width / 2f;

                    nextInnerPoint = wall.End.TransposePoint (wallLines.InnerNormal, endWidth);
                    nextOuterPoint = wall.End.TransposePoint (wallLines.OuterNormal, endWidth);
                }

                var actualInnerLine = Line.Create (innerPoint.Value, nextInnerPoint.Value);
                var actualOuterLine = Line.Create (outerPoint.Value, nextOuterPoint.Value);

                foreach (var opening in wall.openings) {
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
                                        () => wallLines.OuterNormal)));

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
                                        () => wallLines.InnerNormal)));

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
                                            () => wallLines.OuterNormal))));*/
                }

                inner.Add (new RoomGeometry.Wall (innerPoint.Value, nextInnerPoint.Value, innerOpenings));
                outer.Add (new RoomGeometry.Wall (outerPoint.Value, nextOuterPoint.Value, outerOpenings));
            }

            return new RoomGeometry.Geometry (inner, outer);
        }
    }
}