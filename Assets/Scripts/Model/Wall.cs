using System;
using System.Collections.Generic;
using Geometry;
using UnityEngine;

namespace Model
{
    public class Wall
    {
        public enum WidthChangeType
        {
            Type1,
            Type2
        }

        public class Openings : List<IOpening>
        {
        }

        public struct Lines
        {
            public readonly Line Inner;
            public readonly Line Outer;
            public readonly Vector2 InnerNormal;
            public readonly Vector2 OuterNormal;

            public Lines (Wall wall, Tuple<float, float> widths = null)
            {
                var vector = wall.End - wall.Start;
                InnerNormal = vector
                    .GetNormalVector ()
                    .normalized;
                OuterNormal = InnerNormal * -1f;

                float halfStartWidth, halfEndWidth;
                halfStartWidth = halfEndWidth = wall.Width / 2f;
                if (widths != null) {
                    halfStartWidth = widths.Item1;
                    halfEndWidth = widths.Item2;
                }

                Inner = Line.Create (wall.End + InnerNormal * halfStartWidth, wall.Start + InnerNormal * halfEndWidth);
                Outer = Line.Create (wall.End + OuterNormal * halfStartWidth, wall.Start + OuterNormal * halfEndWidth);
            }
        }

        public readonly Vector2 Start;
        public readonly Vector2 End;
        public readonly float Width;
        public readonly Lines lines;
        public readonly Openings openings;
        public readonly Vector2 Size;
        public readonly WidthChangeType widthChangeType;

        public Wall (
            Vector2 start,
            Vector2 end,
            float width,
            Openings openings,
            WidthChangeType widthChangeType = WidthChangeType.Type1)
        {
            Start = start;
            End = end;
            Width = width;
            this.openings = openings;
            lines = new Lines (this);
            Size = end - start;
            this.widthChangeType = widthChangeType;
        }
    }
}