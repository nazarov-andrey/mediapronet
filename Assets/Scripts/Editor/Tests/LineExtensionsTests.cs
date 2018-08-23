using Geometry;
using NUnit.Framework;
using UnityEngine;

namespace Tests
{
    public class LineExtensionsTests
    {
        private Line CreateLine (Vector2 a, Vector2 b)
        {
            return Line.Create (a, b);
        }

        [Test]
        public void IsVertical_VerticalPassed_ReturnsTrue ()
        {
            var line = CreateLine (new Vector2 (0f, 0f), new Vector2 (0f, 5f));
            Assert.IsTrue (line.IsVertical ());
        }

        [Test]
        public void IsVertical_NotVerticalPassed_ReturnsFalse ()
        {
            var line = CreateLine (new Vector2 (0f, 0f), new Vector2 (2f, 2f));
            Assert.IsFalse (line.IsVertical ());
        }

        [Test]
        public void IsHorizontal_HorizontalPassed_ReturnsTrue ()
        {
            var line = CreateLine (new Vector2 (-1f, 4f), new Vector2 (5f, 4f));
            Assert.IsTrue (line.IsHorizontal ());
        }

        [Test]
        public void IsHorizontal_NotHorizontalPassed_ReturnsFalse ()
        {
            var line = CreateLine (new Vector2 (0f, 0f), new Vector2 (2f, 2f));
            Assert.IsFalse (line.IsHorizontal ());
        }

        [Test]
        public void IsParallelWith_ParallelPassed_ReturnsTrue ()
        {
            var lineA = CreateLine (new Vector2 (1f, -1f), new Vector2 (-3f, 3f));
            var lineB = CreateLine (new Vector2 (-1f, 0f), new Vector2 (1f, -2f));
            Assert.IsTrue (lineA.IsParallelWith (lineB));
            Assert.IsTrue (lineB.IsParallelWith (lineA));

            lineA = CreateLine (new Vector2 (-1f, 0f), new Vector2 (3f, 0f));
            lineB = CreateLine (new Vector2 (-2f, 3f), new Vector2 (5f, 3f));
            Assert.IsTrue (lineA.IsParallelWith (lineB));
            Assert.IsTrue (lineB.IsParallelWith (lineA));

            lineA = CreateLine (new Vector2 (0f, 0f), new Vector2 (0f, 5f));
            lineB = CreateLine (new Vector2 (2f, 3f), new Vector2 (2f, -10f));
            Assert.IsTrue (lineA.IsParallelWith (lineB));
            Assert.IsTrue (lineB.IsParallelWith (lineA));
        }

        [Test]
        public void IsParallelWith_NotParallelPassed_ReturnsFalse ()
        {
            var lineA = CreateLine (new Vector2 (1f, -1f), new Vector2 (-3f, 3f));
            var lineB = CreateLine (new Vector2 (-1f, -1f), new Vector2 (1f, -2f));
            Assert.IsFalse (lineA.IsParallelWith (lineB));
            Assert.IsFalse (lineB.IsParallelWith (lineA));
        }

        [Test]
        public void IsPerpendicularWith_PerpendicularPassed_ReturnsTrue ()
        {
            var lineA = CreateLine (new Vector2 (0f, 0f), new Vector2 (0f, 3f));
            var lineB = CreateLine (new Vector2 (0f, 0f), new Vector2 (-1f, 0f));
            Assert.IsTrue (lineA.IsPerpendicularWith (lineB));
            Assert.IsTrue (lineB.IsPerpendicularWith (lineA));

            lineA = CreateLine (new Vector2 (0f, 0f), new Vector2 (1f, 1f));
            lineB = CreateLine (new Vector2 (0f, 0f), new Vector2 (-1f, 1f));
            Assert.IsTrue (lineA.IsPerpendicularWith (lineB));
            Assert.IsTrue (lineB.IsPerpendicularWith (lineA));
        }

        [Test]
        public void IsPerpendicularWith_NotPerpendicularPassed_ReturnsFalse ()
        {
            var lineA = CreateLine (new Vector2 (0f, 0f), new Vector2 (2f, 1f));
            var lineB = CreateLine (new Vector2 (-1f, 1f), new Vector2 (3f, 0f));
            Assert.IsFalse (lineA.IsPerpendicularWith (lineB));
            Assert.IsFalse (lineB.IsPerpendicularWith (lineA));
        }

        [Test]
        public void DistanceTo_PointPassed_CorrectDistanceReturned ()
        {
            var line = CreateLine (new Vector2 (-1f, -1f), new Vector2 (1f, 1f));
            Assert.IsTrue (Mathf.Approximately (Mathf.Sqrt (2), line.DistanceTo (new Vector2 (-1f, 1f))));

            line = CreateLine (new Vector2 (0f, 0f), new Vector2 (0f, 1f));
            Assert.IsTrue (Mathf.Approximately (3f, line.DistanceTo (new Vector2 (3f, 3f))));

            line = CreateLine (new Vector2 (0f, 0f), new Vector2 (3f, 0f));
            Assert.IsTrue (Mathf.Approximately (3f, line.DistanceTo (new Vector2 (3f, 3f))));
        }

        private void PerformCrossTest (
            Vector2 p11,
            Vector2 p12,
            Vector2 p21,
            Vector2 p22,
            bool expectedCross,
            Vector2? expectedCrossPoint = null)
        {
            var lineA = CreateLine (p11, p12);
            var lineB = CreateLine (p21, p22);
            Vector2 cross;

            Assert.AreEqual (lineA.Cross (lineB, out cross), expectedCross);
            if (expectedCrossPoint.HasValue)
                Assert.AreEqual (cross, expectedCrossPoint.Value);
            
            Assert.AreEqual (lineB.Cross (lineA, out cross), expectedCross);
            if (expectedCrossPoint.HasValue)
                Assert.AreEqual (cross, expectedCrossPoint.Value);            
        }

        [Test]
        public void Cross_LinePassed_CorrectCrossPointReturned ()
        {
            //Vertical with not vertical nor horizontal
            PerformCrossTest (
                new Vector2 (0f, 0f),
                new Vector2 (0f, 1f),
                new Vector2 (-1f, 2f),
                new Vector2 (1f, 0f),
                true,
                new Vector2 (0f, 1f));

            //Vertical with horizontal 
            PerformCrossTest (
                new Vector2 (0f, 0f),
                new Vector2 (0f, 1f),
                new Vector2 (-1f, 0f),
                new Vector2 (1f, 0f),
                true,
                new Vector2 (0f, 0f));

            //Horizontal with not vertical nor horizontal
            PerformCrossTest (
                new Vector2 (-1f, 0),
                new Vector2 (1f, 0f),
                new Vector2 (1f, -1f),
                new Vector2 (0f, 1f),
                true,
                new Vector2 (0.5f, 0f));
            
            //Not vertical nor horizontal with not vertical nor horizontal
            PerformCrossTest (
                new Vector2 (-1f, -1),
                new Vector2 (1f, 2f),
                new Vector2 (5f, 2f),
                new Vector2 (3f, 5f),
                true,
                new Vector2 (3f, 5f));
            
            PerformCrossTest (
                new Vector2 (0f, 0f),
                new Vector2 (1f, 0f),
                new Vector2 (0f, -1f),
                new Vector2 (5f, -1f),
                false);
            
            PerformCrossTest (
                new Vector2 (0f, 0f),
                new Vector2 (0f, 1f),
                new Vector2 (1f, -1f),
                new Vector2 (1f, 5f),
                false);
            
            PerformCrossTest (
                new Vector2 (0f, 0f),
                new Vector2 (1f, 1f),
                new Vector2 (0f, 1f),
                new Vector2 (1f, 2f),
                false);            
        }
    }
}