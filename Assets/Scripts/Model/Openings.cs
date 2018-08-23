using System;
using System.Collections.Generic;
using Geometry;
using UnityEngine;

namespace Model
{
    [Flags]
    public enum OpeningType
    {
        Inner = 1,
        Outer = 2,
        Through = Inner | Outer
    }

    public interface IOpening
    {
        List<Vector2> Conoutour { get; }
        OpeningType OpeningType { get; }
        float Depth { get; }
    }

    public struct SquareOpening : IOpening
    {
        public List<Vector2> Conoutour { get; }
        public OpeningType OpeningType { get; }
        public float Depth { get; }

        public SquareOpening (
            OpeningType openingType,
            float depth,
            Vector2 position,
            Vector2 size)
        {
            OpeningType = openingType;
            Depth = depth;
            Conoutour = new List<Vector2>
            {
                position,
                position + new Vector2 (0f, size.y),
                position + size,
                position + new Vector2 (size.x, 0f)
            };
        }
    }

    public struct SegmentsOpening : IOpening
    {
        public List<Vector2> Conoutour { get; }
        public OpeningType OpeningType { get; }
        public float Depth { get; }

        public SegmentsOpening (
            OpeningType openingType,
            float depth = float.MaxValue,
            params ILineSegment[] segments)
        {
            OpeningType = openingType;
            Depth = depth;
            Conoutour = new List<Vector2> ();
            foreach (var segment in segments) {
                var points = segment.Points;
                Conoutour.AddRange (points.GetRange (0, points.Count - 1));
            }
        }
    }
}