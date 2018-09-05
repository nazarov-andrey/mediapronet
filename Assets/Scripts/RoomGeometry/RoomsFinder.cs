using System;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Model;
using UnityEngine;

namespace RoomGeometry
{
    public static class RoomsFinder
    {
        class WallUseCounter
        {
            public readonly WallData Wall;
            public int Counter;
            public bool Deadlock;
            public bool CheckedOnce;

            public WallUseCounter (WallData wall)
            {
                Wall = wall;
                Deadlock = false;
                Counter = 0;
                CheckedOnce = false;
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
            var result = new T[array.Length + 1];
            Array.Copy (array, result, array.Length);
            result[result.Length - 1] = value;

            return result;
        }

        private static Tuple<WallUseCounter[], bool> Find (
            WallUseCounter wallUseCounter,
            bool useReversedWall,
            PointWallsMap allPointWallsMap,
            WallUseCounter[] room,
            List<WallData[]> rooms,
            int depth = 0)
        {
            wallUseCounter.CheckedOnce = true;

            string indent = new string (' ', depth * 2);
/*            Debug.Log (
                $"{indent}Wall: {wallUseCounter.Wall}; useReversedWall: {useReversedWall}; room: {string.Join (",", Array.ConvertAll (room, x => x.Wall.ToString ()).ToArray ())}");*/

            if (room.Length > 0 && wallUseCounter == room[0]) {
/*                Debug.Log (
                    $"{indent}Found room {string.Join (",", Array.ConvertAll (room, x => x.Wall.ToString ()).ToArray ())}");*/
                Array.ForEach (room, x => x.Counter++);
                rooms.Add (room.Select (x => x.Wall).ToArray ());
                return new Tuple<WallUseCounter[], bool> (room, true);
            }

            room = AppendToArray (room, wallUseCounter);

            var wall = wallUseCounter.Wall;
            if (useReversedWall)
                wall = wall.Reverse ();

            var end = wall
                .Points
                .Last ();

            var wallVector = wall.GetInverseVector ();
            var allNextWalls = allPointWallsMap[end];
            var nextWalls = allNextWalls
                .Where (
                    x => x != wallUseCounter && !x.Deadlock && x.Counter < 2)
                .Select (x => GetNextWallData (wall, wallVector, x))
                .OrderBy (x => x.Item3)
                .ToList ();

            if (nextWalls.Count == 0) {
/*                Debug.Log (
                    $"{indent} Deadlock {string.Join (",", Array.ConvertAll (room, x => x.Wall.ToString ()))}");*/
                wallUseCounter.Deadlock = true;
                return new Tuple<WallUseCounter[], bool> (room, false);
            }

            while (nextWalls.Count > 0) {
                var neatestWall = nextWalls[0];
                var solveResult = Find (
                    neatestWall.Item1,
                    neatestWall.Item2,
                    allPointWallsMap,
                    room,
                    rooms,
                    depth + 1);

/*                Debug.Log (
                    $"{indent}roomFound {string.Join (",", Array.ConvertAll (solveResult.Item1, x => x.Wall.ToString ()))}; {solveResult.Item2}");*/
                nextWalls.RemoveAt (0);

                if (!solveResult.Item2) {
                    room = solveResult.Item1;
/*                    Debug.Log (
                        $"{indent}room {string.Join (",", Array.ConvertAll (room, x => x.Wall.ToString ()))}");*/
                    continue;
                }

                for (int count = nextWalls.Count, i = 0; i < count; i++) {
                    Find (
                        nextWalls[i].Item1,
                        nextWalls[i].Item2,
                        allPointWallsMap,
                        new WallUseCounter[0],
                        rooms,
                        depth + 1);
                }

                return new Tuple<WallUseCounter[], bool> (room, true);
            }

            return new Tuple<WallUseCounter[], bool> (room, false);
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

            var allRooms = new List<WallData[]> ();
            var rooms = new List<WallData[]> ();
            do {
                var startWall = wallCounterMap
                    .Values
                    .FirstOrDefault (x => !x.CheckedOnce);

                if (startWall == null)
                    break;

                rooms.Clear ();
                Find (
                    startWall,
                    false,
                    pointWallsMap,
                    new WallUseCounter[0],
                    rooms);

                /**
                 * Remove 'superroom', ie room which includes all rooms in this group.
                 * Such room include only 'shared' walls, ie walls with UseCount == 2.
                 */
                if (rooms.Count > 1) {
                    var max = rooms[0].Length;
                    var maxIndex = 0;
                    for (int count = rooms.Count, j = 1; j < count; j++) {
                        if (rooms[j].All (x => wallCounterMap[x].Counter == 2)) {
                            rooms.RemoveAt (j);
                            break;
                        }
                    }
                }

                allRooms.AddRange (rooms);
            } while (true);

            wallCounterMap.Clear ();
            pointWallsMap.Clear ();
            rooms.Clear ();
            return allRooms.ToArray ();
        }
    }
}