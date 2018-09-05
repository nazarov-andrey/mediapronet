using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Geometry;
using Model;
using RoomGeometry;
using UnityEngine;
using SystemVector2 = System.Numerics.Vector2;
using Vector2 = UnityEngine.Vector2;

public class MeshMaker : MonoBehaviour
{
    [SerializeField]
    private Material material;

    [SerializeField]
    private float wallHeight = 2.2f;

    [SerializeField]
    private float wallWidth = 0.1f;

    private List<List<Vector2>> dummyHoles = new List<List<Vector2>> ();

    private Vector2[] CreateBezierHoleShape ()
    {
        var q = 10;
        var s1 = new QuadraticBezierSegment (0.5f, 0.25f, 0.25f, 0.625f, 0.5f, 1f, q);
        var s2 = new QuadraticBezierSegment (0.5f, 1f, 0.625f, 1.25f, 0.5f, 1.5f, q);
        var s3 = new QuadraticBezierSegment (0.5f, 1.5f, 0.875f, 1.375f, 1f, 1f, q);
        var s4 = new QuadraticBezierSegment (1f, 1f, 1.125f, 0.375f, 0.75f, 0.25f, q);
        var s5 = new QuadraticBezierSegment (0.75f, 0.25f, 0.625f, 0.125f, 0.5f, 0.25f, q);

        var result = new List<Vector2> (s1.Points);
        result.AddRange (s2.Points);
        result.AddRange (s3.Points);
        result.AddRange (s4.Points);
        result.AddRange (s5.Points);

        return result.ToArray ();
    }

    private Vector2[] CreateRectHoleShape ()
    {
        return new[]
            {new Vector2 (1.5f, 0.25f), new Vector2 (1.5f, 1.5f), new Vector2 (2f, 1.5f), new Vector2 (2f, 0.25f)};
    }

    private void PlaneGenerationTest ()
    {
        var q = 50;
        var countourSegment = new QuadraticBezierSegment (
            new Vector2 (4f, 1f),
            new Vector2 (2.5f, 0f),
            new Vector2 (1f, 1f),
            q);

        //Flat plane

/*        var mesh = PlaneMeshMaker.GetMesh (
            new StreightLineSegment (
                    Vector2.zero,
                    new Vector2 (5f, 0f))
                .Points
                .Select (x => x.ToSystemVector2 ())
                .ToArray (),
//            null,
            new[]
            {
                CreateHoleShape ()
            },
            2f);*/

        //Curved plane
        var mesh = PlaneMeshMaker.GetMesh (
            countourSegment.Points.ToArray (),
//            null,  //No holes
            new[]
            {
                CreateBezierHoleShape ()
            },
            2f);

        MeshGenerator.CreateGameObject ("qqq", mesh);
    }

    private void StreightAndCurvedWallsTest ()
    {
        var nextWall = WallData.CreateStreight (new Vector2 (0f, 0f), new Vector2 (0f, 2f), 0.2f, 2f);
        var prevWall = WallData.CreateStreight (new Vector2 (3f, 2f), new Vector2 (3f, 0f), 0.1f, 2f);

        var curvedWall1 = WallData.CreateCurved (
            new Vector2 (3f, 0f),
            new Vector2 (1.5f, -1.5f),
            new Vector2 (0f, 0f),
            0.15f,
            2f,
            new[]
            {
                new OpeningData (OpeningType.Outer, CreateBezierHoleShape (), 0.05f),
                new OpeningData (OpeningType.Inner, CreateRectHoleShape ())
            });

        var curvedWall2 = curvedWall1.Reverse ();

/*        var curvedWall2 = WallData.CreateCurved (
            new Vector2 (3f, 0f),
            new Vector2 (1.5f, -1.5f),
            new Vector2 (0f, 0f),
            0.15f,
            2f,
            new[]
            {
                new OpeningData (OpeningType.Through, CreateBezierHoleShape (), 0.05f),
                new OpeningData (OpeningType.Through, CreateRectHoleShape ())
            });*/

        var matrix = Matrix3x2.CreateTranslation (0f, 3f);
        var streightWall = WallData.CreateStreight (
            new Vector2 (3f, 0f),
            new Vector2 (0f, 0f),
            0.3f,
            2f,
            new[]
            {
                new OpeningData (OpeningType.Through, CreateBezierHoleShape ()),
                new OpeningData (OpeningType.Through, CreateRectHoleShape ())
            });

        CreateWallMeshes (curvedWall1, null, null);
//        SingleWallTest (streightWall.Transform (matrix), nextWall.Transform (matrix), prevWall.Transform (matrix));
        CreateWallMeshes (streightWall.Transform (matrix), nextWall.Transform (matrix), null);

        matrix = Matrix3x2.CreateTranslation (0f, -3f);
        CreateWallMeshes (
            curvedWall2.Transform (matrix),
            prevWall.Reverse ().Transform (matrix),
            nextWall.Reverse ().Transform (matrix));
    }

    private void CreateWallMeshes (
        WallData wall,
        WallData nextWall,
        WallData prevWall,
        Transform parent = null)
    {
        var meshes = WallMeshesGenerator.GetWallMeshes (
            prevWall,
            prevWall,
            wall,
            nextWall,
            nextWall);

        var wallTransform = new GameObject ("Wall").transform;
        foreach (var mesh in meshes) {
            MeshGenerator
                .CreateGameObject (mesh.name, mesh)
                .transform
                .SetParent (wallTransform, false);
        }

        wallTransform.SetParent (parent);
    }

    private void CreateWallMeshes (
        WallData wall,
        WallData nextInnerWall,
        WallData nextOuterWall,
        WallData prevInnerWall,
        WallData prevOuterWall,
        Transform parent = null)
    {
        var meshes = WallMeshesGenerator.GetWallMeshes (
            prevInnerWall,
            prevOuterWall,
            wall,
            nextInnerWall,
            nextOuterWall);

        var wallTransform = new GameObject (wall.ToString()).transform;
        foreach (var mesh in meshes) {
            MeshGenerator
                .CreateGameObject (mesh.name, mesh)
                .transform
                .SetParent (wallTransform, false);
        }

        wallTransform.SetParent (parent);
    }

    private void WallsTest ()
    {
        var walls = new Walls ();
        walls.AddWallData (WallData.CreateCurved (-2f, -2f, -4f, -0f, -2, 2, 0.3f, 2f));
        walls.AddWallData (
            WallData.CreateStreight (-2f, 2f, 0f, 2f, 0.4f, 2f, null, WidthChangeType.Type2));
        walls.AddWallData (WallData.CreateStreight (0f, 2f, 2f, 2f, 0.1f, 1f));
        walls.AddWallData (WallData.CreateStreight (2f, 2f, 2f, -2f, 0.2f, 2f));
        walls.AddWallData (WallData.CreateStreight (2f, -2f, -2f, -2f, 0.5f, 2f));

//        var walls = new Dictionary<Vector2, List<WallData>> ();


/*        var walls = new[]
        {
//            BaseWallData.CreateStreight (new Vector2 (-2, -2), new Vector2 (-2, 2), 0.2f, 2f),
            WallData.CreateCurved (new Vector2 (-2, -2), new Vector2 (-4, -0), new Vector2 (-2, 2), 0.3f, 2f),
            WallData.CreateStreight (new Vector2 (-2, 2), new Vector2 (0, 2), 0.4f, 2f, null, WidthChangeType.Type2),
            WallData.CreateStreight (new Vector2 (0, 2), new Vector2 (2, 2), 0.1f, 1f),
            WallData.CreateStreight (new Vector2 (2, 2), new Vector2 (2, -2), 0.2f, 2f),
            WallData.CreateStreight (new Vector2 (2, -2), new Vector2 (-2, -2), 0.5f, 2f)
        };*/

/*        var walls = new[]
        {
            WallData.CreateStreight (new Vector2 (0, -2), new Vector2 (-2, 0), 0.2f, 2f),
            WallData.CreateStreight (new Vector2 (-2, 0), new Vector2 (0, 2), 0.2f, 2f),
            WallData.CreateStreight (new Vector2 (0, 2), new Vector2 (2, 0), 0.2f, 2f),
            WallData.CreateStreight (new Vector2 (2, 0), new Vector2 (0, -2), 0.2f, 2f)
        };*/

/*        var wallsCount = walls.Count;
        for (int i = 0; i < wallsCount; i++) {
            var prevIndex = (i - 1 + wallsCount) % wallsCount;
            var nextIndex = (i + 1) % wallsCount;

            var meshes = WallMeshesGenerator.GetWallMeshes (walls[prevIndex], walls[i], walls[nextIndex]);
            var parent = new GameObject ($"Wall {i}").transform;
            for (int j = 0; j < meshes.Length; j++) {
                var mesh = meshes[j];
                MeshGenerator
                    .CreateGameObject (mesh.name, mesh)
                    .transform
                    .SetParent (parent, false);
            }
        }*/
    }

    private void RoomsFinderTest ()
    {
        var walls = new Walls ();
        walls.AddWallData (WallData.CreateStreight (1f, 2f, 0f, 3f, 0.1f, 2f));
        walls.AddWallData (WallData.CreateStreight (0f, 2f, 0f, 3f, 0.1f, 2f));
        
        walls.AddWallData (WallData.CreateStreight (0f, 0f, 0f, 2f, 0.1f, 2f));
        walls.AddWallData (WallData.CreateStreight (0f, 2f, 1f, 2f, 0.1f, 2f));
        walls.AddWallData (WallData.CreateStreight (1f, 2f, 1f, 3f, 0.1f, 2f));
        walls.AddWallData (WallData.CreateStreight (1f, 3f, 4f, 3f, 0.1f, 2f));
        walls.AddWallData (WallData.CreateStreight (4f, 3f, 2f, 2f, 0.1f, 2f));
        walls.AddWallData (WallData.CreateStreight (2f, 2f, 1f, 2f, 0.1f, 2f));
        walls.AddWallData (WallData.CreateStreight (2f, 2f, 2f, 0f, 0.1f, 2f));
        walls.AddWallData (WallData.CreateStreight (2f, 0f, 0f, 0f, 0.1f, 2f));
        walls.AddWallData (WallData.CreateStreight (1f, 2f, 1f, 1f, 0.1f, 2f));
        walls.AddWallData (WallData.CreateStreight (1f, 1f, 0.5f, 0.5f, 0.1f, 2f));
        
        walls.AddWallData (WallData.CreateStreight (10f, 10f, 10f, 15f, 0.1f, 2f));
        walls.AddWallData (WallData.CreateStreight (10f, 15f, 15f, 15f, 0.1f, 2f));
        walls.AddWallData (WallData.CreateStreight (15f, 15f, 15f, 10f, 0.1f, 2f));
        walls.AddWallData (WallData.CreateStreight (15f, 10f, 10f, 10f, 0.1f, 2f));
        
        walls.AddWallData (WallData.CreateStreight (11f, 11f, 11f, 13f, 0.1f, 2f));
        walls.AddWallData (WallData.CreateStreight (11f, 13f, 13f, 13f, 0.1f, 2f));
        walls.AddWallData (WallData.CreateStreight (13f, 13f, 13f, 11f, 0.1f, 2f));
        walls.AddWallData (WallData.CreateStreight (13f, 11f, 11f, 11f, 0.1f, 2f));
        

/*        walls.AddWallData (WallData.CreateStreight (0f, 0f, 0f, 2f, 0.1f, 2f));
        walls.AddWallData (WallData.CreateStreight (0f, 2f, 2f, 2f, 0.1f, 2f));
        walls.AddWallData (WallData.CreateStreight (2f, 2f, 2f, 0f, 0.1f, 2f));
        walls.AddWallData (WallData.CreateStreight (0f, 0f, 2f, 0f, 0.1f, 2f));*/
        
/*        walls.AddWallData (WallData.CreateStreight (2f, 2f, 3f, 2f, 0.1f, 2f));
        walls.AddWallData (WallData.CreateStreight (3f, 2f, 3f, 0f, 0.1f, 2f));
        walls.AddWallData (WallData.CreateStreight (3f, 0f, 2f, 0f, 0.1f, 2f));*/


        var rooms = RoomsFinder.FindRooms (walls);
/*        foreach (var room in rooms) {
            Debug.Log (
                $"room {string.Join (",", Array.ConvertAll (room, x => $"[wall {x.Points.First ()} {x.Points.Last ()}]"))}");
        }*/

        for (int j = 0; j < rooms.Length; j++) {
            var roomTransform = new GameObject ($"Room {j}").transform;
            var room = rooms[j];
            for (int count = room.Length, i = 0; i < count; i++) {
                var wall = room[i];
                var wallInverseVector = wall.GetInverseVector ();
                var prevWalls = walls
                    .GetPointWalls (wall.Start)
                    .Where(x => x != wall)
                    .Select (x => wall.Start == x.End ? x : x.Reverse ())
                    .Select (x => new Tuple<WallData, float> (x, wallInverseVector.GetWallAngle (x)))
                    .OrderBy (x => x.Item2)
                    .ToArray ();
                var nextWalls = walls
                    .GetPointWalls (wall.End)
                    .Where(x => x != wall)
                    .Select (x => wall.End == x.Start ? x : x.Reverse ())
                    .Select (x => new Tuple<WallData, float> (x, wallInverseVector.GetWallAngle (x)))
                    .OrderBy (x => x.Item2)
                    .ToArray ();

                var prevInnerWall = prevWalls.LastOrDefault ()?.Item1;
                var prevOuterWall = prevWalls.FirstOrDefault ()?.Item1;
                var nextInnerWall = nextWalls.FirstOrDefault ()?.Item1;
                var nextOuterWall = nextWalls.LastOrDefault ()?.Item1;

                CreateWallMeshes (wall, nextInnerWall, nextOuterWall, prevInnerWall, prevOuterWall, roomTransform);
            }
        }
    }

    private void Start ()
    {
//        PlaneGenerationTest ();
//        WallsTest ();
//        StreightAndCurvedWallsTest ();

        RoomsFinderTest ();
    }

    private IEnumerator LateRebuild ()
    {
        yield return null;
//        CreateRoom ();
//        BuildMesh ();
    }

    private void OnValidate ()
    {
/*        if (!Application.isPlaying)
            return;

        StartCoroutine (LateRebuild ());*/
    }
}