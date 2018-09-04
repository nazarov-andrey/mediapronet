using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
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
        }

        class PointWallsMap : Dictionary<Vector2, WallUseCounter[]>
        {
        }

        private static Vector2 GetWallVector (WallData wall)
        {
            return wall.Points.Last () - wall.Points.First ();
        }

        private static float GetVectorWallAngle (Vector2 vector, WallData wall)
        {
            var angle = Vector2.SignedAngle (vector, GetWallVector (wall));
            if (angle < 0)
                angle += 360f;
            return angle;
        }

        private static Tuple<WallUseCounter, bool, float> GetNextWallData (
            WallData wall,
            Vector2 wallVector,
            WallUseCounter anotherWallUseCounter)
        {
            var anotherWall = anotherWallUseCounter.Wall;
            var useReversedWall = wall.Points.Last () != anotherWall.Points.First ();
            if (useReversedWall)
                anotherWall = anotherWall.Reverse ();

            var wallAngle = GetVectorWallAngle (wallVector, anotherWall);
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

        private static Tuple<WallUseCounter[], bool> Solve (
            WallUseCounter wallUseCounter,
            bool useReversedWall,
            PointWallsMap allPointWallsMap,
            WallUseCounter[] room,
            List<WallData[]> rooms,
            int depth = 0)
        {
            string indent = new string (' ', depth * 2);
            Debug.Log ($"{indent}Wall: {wallUseCounter.Wall}; useReversedWall: {useReversedWall}; room: {string.Join (",", Array.ConvertAll (room, x => x.Wall.ToString ()).ToArray ())}");

            if (room.Length > 0 && wallUseCounter == room[0]) {
                Debug.Log (
                    $"{indent}Found room {string.Join (",", Array.ConvertAll (room, x => x.Wall.ToString ()).ToArray ())}");
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

            var wallVector = -GetWallVector (wall);
            var allNextWalls = allPointWallsMap[end];
            var nextWalls = allNextWalls
                .Where (
                    x => x != wallUseCounter && !x.Deadlock && x.Counter < 2)
                .Select (x => GetNextWallData (wall, wallVector, x))
                .OrderBy (x => x.Item3)
                .ToList ();

            if (nextWalls.Count == 0) {
                Debug.Log (
                    $"{indent} Deadlock {string.Join (",", Array.ConvertAll (room, x => x.Wall.ToString ()))}");
                wallUseCounter.Deadlock = true;
                return new Tuple<WallUseCounter[], bool> (room, false);
            }
            
            while (nextWalls.Count > 0) {
                var neatestWall = nextWalls[0];
                var solveResult = Solve (
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
                    Debug.Log (
                        $"{indent}room {string.Join (",", Array.ConvertAll (room, x => x.Wall.ToString ()))}");
                    continue;
                }

                for (int count = nextWalls.Count, i = 0; i < count; i++) {
                    Solve (
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

        public static void FindRooms (Walls walls)
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

            foreach (var point in walls.Points) {
                pointWallsMap.Add (
                    point,
                    walls
                        .GetPointWalls (point)
                        .ConvertAll (x => wallCounterMap[x])
                        .ToArray ());
            }

            uniqueWalls.Clear ();
            wallCounterMap.Clear ();

            var rooms = new List<WallData[]> ();
            using (new ProfileBlock ("Find Rooms")) {
                Solve (
                    pointWallsMap.First ().Value.First (),
                    false,
                    pointWallsMap,
                    new WallUseCounter[0],
                    rooms);
            }

            foreach (var room in rooms) {
                Debug.Log (
                    $"room {string.Join (",", Array.ConvertAll (room, x => $"[wall {x.Points.First ()} {x.Points.Last ()}]"))}");
            }
        }
    }
}