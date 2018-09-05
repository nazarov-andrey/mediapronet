using System;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Model;
using UnityEngine;
using UnityEngine.Assertions;

namespace RoomGeometry
{
    public static class RoomsFinder
    {
        class WallUseCounter
        {
            public readonly WallData Wall;
            public int Counter;
            public bool Deadlock;

            public WallUseCounter (WallData wall)
            {
                Wall = wall;
                Deadlock = false;
                Counter = 0;
            }

            public override string ToString ()
            {
                return
                    $"[WallUseCounter {nameof (Wall)}: {Wall}, {nameof (Counter)}: {Counter}, {nameof (Deadlock)}: {Deadlock}]";
            }
        }

        class PointWallsMap : Dictionary<Vector2, WallUseCounter[]>
        {
        }

        private static Tuple<WallUseCounter, bool, float> GetNextWallData (
            WallData wall,
            Vector2 wallVector,
            WallUseCounter anotherWallUseCounter)
        {
            var anotherWall = anotherWallUseCounter.Wall;
            var useReversedWall = wall.End != anotherWall.Start;
            if (useReversedWall)
                anotherWall = anotherWall.Reverse ();

            var wallAngle = wallVector.GetWallAngle (anotherWall);
            return new Tuple<WallUseCounter, bool, float> (
                anotherWallUseCounter,
                useReversedWall,
                wallAngle);
        }

        private static T[] AppendToArray<T> (T[] array, T value)
        {
            var length = array?.Length + 1 ?? 1;
            var result = new T[length];
            if (array == null) {
                result[0] = value;
            } else {
                Array.Copy (array, result, array.Length);
                result[result.Length - 1] = value;
            }

            return result;
        }

        private static string RoomToString (WallData[] room)
        {
            return string.Join (",", Array.ConvertAll (room, x => x.ToString ()));
        }

        private static WallData[] FindRoom (
            WallUseCounter wallCounter,
            bool useReversedWall,
            PointWallsMap pointWallsMap,
            ref WallUseCounter[] path,
            int depth = 0)
        {
            var indent = new string (' ', depth * 2);
//            Debug.Log ($"{indent}Wall {wallCounter}");
            Assert.IsTrue (depth < 10);

            if (path != null && path.Length > 0 && wallCounter == path[0]) {
                Array.ForEach (path, x => x.Counter++);
                var room = path.Select (x => x.Wall).ToArray ();
//                Debug.Log ($"{indent}Room found {RoomToString (room)}");
                return room;
            }

            var wall = wallCounter.Wall;
            if (useReversedWall)
                wall = wall.Reverse ();

            var end = wall
                .Points
                .Last ();

            var wallVector = wall.GetInverseVector ();
            var allNextWalls = pointWallsMap[end];
            var nextWalls = allNextWalls
                .Where (
                    x => x != wallCounter && !x.Deadlock && x.Counter < 2)
                .Select (x => GetNextWallData (wall, wallVector, x))
                .OrderBy (x => x.Item3)
                .ToList ();

            path = AppendToArray (path, wallCounter);
            foreach (var nextWall in nextWalls) {
                var room = FindRoom (
                    nextWall.Item1,
                    nextWall.Item2,
                    pointWallsMap,
                    ref path,
                    depth + 1);

                if (room != null)
                    return room;
            }

//            Debug.Log ($"{indent}Wall {wallCounter.Wall} deadlocked");
            wallCounter.Deadlock = true;
            return null;
        }

        public static WallData[][] FindRooms (Walls walls)
        {
            var pointWallsMap = new PointWallsMap ();
            var uniqueWalls = new HashSet<WallData> ();
            foreach (var point in walls.Points) {
                foreach (var wall in walls.GetPointWalls (point)) {
                    uniqueWalls.Add (wall);
                }
            }

            var wallCounterMap = new Dictionary<WallData, WallUseCounter> ();
            foreach (var wall in uniqueWalls) {
                wallCounterMap.Add (wall, new WallUseCounter (wall));
            }

            uniqueWalls.Clear ();

            foreach (var point in walls.Points) {
                pointWallsMap.Add (
                    point,
                    walls
                        .GetPointWalls (point)
                        .ConvertAll (x => wallCounterMap[x])
                        .ToArray ());
            }

            var rooms = new List<WallData[]> ();
            var vector2Comparer = new Vector2Comparer ();
            var orderedWallCounters = wallCounterMap
                .Values
                .OrderBy (x => vector2Comparer.GetMin (x.Wall.Start, x.Wall.End), vector2Comparer)
                .ToArray ();

            var i = 0;
            do {
                Assert.IsTrue (++i < 10);
                var startWall = orderedWallCounters
                    .FirstOrDefault (x => x.Counter == 0);

                if (startWall == null)
                    break;

                WallUseCounter[] path = null;
                var room = FindRoom (startWall, false, pointWallsMap, ref path);
                Assert.IsNotNull (room);
                rooms.Add (room);
            } while (true);

            /**
             * Remove 'superroom', ie room which includes all rooms in this group.
             * Such room include only 'shared' walls, ie walls with UseCount == 2.
             */
/*            if (rooms.Count > 1) {
                var sorterRooms = rooms
                    .OrderByDescending (x => x.Length)
                    .ToArray ();
                for (int j = 0; j < sorterRooms.Length; j++) {
                    if (rooms[j].All (x => wallCounterMap[x].Counter == 2)) {
                        rooms.RemoveAt (j);
                        break;
                    }
                }
            }*/

            return rooms.ToArray ();
        }
    }
}