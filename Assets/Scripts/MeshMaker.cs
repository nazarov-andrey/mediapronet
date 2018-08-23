using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Geometry;
using Model;
using Model.RoomGeometry;
using UnityEngine;
using SquareOpening = Model.SquareOpening;
using Wall = Model.Wall;

public class MeshMaker : MonoBehaviour
{
    [SerializeField]
    private Material material;

    [SerializeField]
    private float wallHeight = 2.2f;

    [SerializeField]
    private float wallWidth = 0.1f;

    private List<List<Vector2>> dummyHoles = new List<List<Vector2>> ();

    private GameObject CreateMesh (string name, Vector3[] vertices, int[] triangles, bool flipFaces, Matrix4x4 matrix)
    {
        var mesh = new Mesh ();

        mesh.Clear ();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        if (flipFaces)
            mesh.triangles = mesh.triangles.Reverse ().ToArray ();
        //mesh.Optimize();
        mesh.RecalculateNormals ();

        var gameObj = new GameObject (name, typeof (MeshRenderer), typeof (MeshFilter));
        gameObj
            .GetComponent<MeshFilter> ()
            .mesh = mesh;

        gameObj
            .GetComponent<MeshRenderer> ()
            .material = material;

        return gameObj;
    }

    private GameObject CreateMesh (
        string name,
        List<Vector2> contour,
        List<List<Vector2>> holes,
        bool flipFaces,
        bool flipYZ,
        Matrix4x4 matrix)
    {
        List<int> indices;
        List<Vector3> vertices;

        Triangulation.Triangulate (
            contour,
            holes,
            out indices,
            out vertices);

        return CreateMesh (
            name,
            vertices
                .ConvertAll (matrix.MultiplyPoint)
                .ConvertAll (x => flipYZ ? FlipYZ (x) : x)
                .ToArray (),
            indices.ToArray (),
            flipFaces,
            matrix);
    }

    private Vector2 AbsoluteOpeningVertexToRelative (Vector2 origin, Vector3 vertex)
    {
        return new Vector2 ((new Vector2 (vertex.x, vertex.y) - origin).magnitude, vertex.z);
    }

    private Vector3 FlipYZ (Vector3 v)
    {
        return new Vector3 (v.x, v.z, v.y);
    }

    private void ProcessContour (string name, Contour contour, bool flipFaces)
    {
        foreach (var wall in contour) {
            var wallSize = wall.Size;
            var angle = Mathf.Atan2 (wallSize.y, wallSize.x) * Mathf.Rad2Deg;
            var matrix = Matrix4x4.identity;
            matrix.SetTRS (wall.Start, Quaternion.Euler (0f, 0f, angle), Vector3.one);
            var vertices = new List<Vector2>
            {
                Vector2.zero,
                new Vector2 (0f, wallHeight),
                new Vector2 (wallSize.magnitude, wallHeight),
                new Vector2 (wallSize.magnitude, 0f)
            };

            var holes = new List<List<Vector2>> ();
            foreach (var opening in wall.Openings) {
                var hole = opening
                    .FrontContour
                    .ConvertAll (x => AbsoluteOpeningVertexToRelative (wall.Start, x));
                holes.Add (hole);

                if (opening.BackContour == null)
                    continue;

                if (opening.HasBackWall) {
                    var backConoutour = opening.BackContour;
                    var plane = new Plane (
                        FlipYZ (backConoutour[0]),
                        FlipYZ (backConoutour[1]),
                        FlipYZ (backConoutour[2]));
                    var planeNormal = plane.normal;
                    var backWallAngle = Mathf.Atan2 (planeNormal.z, planeNormal.x) * Mathf.Rad2Deg + 90f;

                    var origin = new Vector2 (backConoutour[0].x, backConoutour[0].y);
                    var backContourVertexes = opening
                        .BackContour
                        .ConvertAll (x => AbsoluteOpeningVertexToRelative (origin, x));
                    var openingMatrix = Matrix4x4.TRS (origin, Quaternion.Euler (0f, 0f, backWallAngle), Vector3.one);
                    CreateMesh ("jamb back wall", backContourVertexes, dummyHoles, flipFaces, true, openingMatrix);
                }

                var openingJambVertices = opening
                    .FrontContour
                    .ConvertAll (x => new Vector3 (x.x, x.z, x.y));
                openingJambVertices.AddRange (
                    opening
                        .BackContour
                        .ConvertAll (x => new Vector3 (x.x, x.z, x.y)));

                var openingJambTriangles = new List<int> ();
                for (int openingVertexCount = opening.FrontContour.Count, k = 0; k < openingVertexCount; k++) {
                    openingJambTriangles.Add (k);
                    openingJambTriangles.Add ((k + 1) % openingVertexCount);
                    openingJambTriangles.Add (openingVertexCount + k);

                    openingJambTriangles.Add ((k + 1) % openingVertexCount);
                    openingJambTriangles.Add (openingVertexCount + (k + 1) % openingVertexCount);
                    openingJambTriangles.Add (openingVertexCount + k);
                }

                gos.Add (
                    CreateMesh (
                        "opening jamb",
                        openingJambVertices.ToArray (),
                        openingJambTriangles.ToArray (),
                        flipFaces,
                        Matrix4x4.identity));
            }

            var go = CreateMesh (name, vertices, holes, flipFaces, true, matrix);
            gos.Add (go);
        }
    }

    private Room room;
    private List<GameObject> gos = new List<GameObject> ();

    private void CreateRoom ()
    {
        room = new Room (
            new Wall (
                new Vector2 (0f, 2f),
                new Vector2 (0f, 5f),
                wallWidth,
                new Wall.Openings
                {
                    new SegmentsOpening (
                        OpeningType.Through,
                        float.MaxValue,
                        new StreightLineSegment (new Vector2 (1f, 0f), new Vector2 (1f, 1f)),
                        new QuadraticBezierSegment (
                            new Vector2 (1f, 1f),
                            new Vector2 (1f, 1.5f),
                            new Vector2 (1.5f, 1.5f),
                            10),
                        new QuadraticBezierSegment (
                            new Vector2 (1.5f, 1.5f),
                            new Vector2 (2f, 1.5f),
                            new Vector2 (2f, 1f),
                            10),
                        new StreightLineSegment (
                            new Vector2 (2f, 1f),
                            new Vector2 (2f, 0f)),
                        new StreightLineSegment (new Vector2 (2f, 0f), new Vector2 (1f, 0f))),

                    new SegmentsOpening (
                        OpeningType.Through,
                        float.MaxValue,
                        new QuadraticBezierSegment (
                            new Vector2 (1.5f, 1.6f),
                            new Vector2 (1.25f, 1.6f),
                            new Vector2 (1.25f, 1.85f),
                            10),
                        new QuadraticBezierSegment (
                            new Vector2 (1.25f, 1.85f),
                            new Vector2 (1.25f, 2.1f),
                            new Vector2 (1.5f, 2.1f),
                            10),
                        new QuadraticBezierSegment (
                            new Vector2 (1.5f, 2.1f),
                            new Vector2 (1.75f, 2.1f),
                            new Vector2 (1.75f, 1.85f),
                            10),
                        new QuadraticBezierSegment (
                            new Vector2 (1.75f, 1.85f),
                            new Vector2 (1.75f, 1.6f),
                            new Vector2 (1.5f, 1.6f),
                            10))
                }),
            new Wall (
                new Vector2 (0f, 5f),
                new Vector2 (0f, 7f),
                wallWidth,
                new Wall.Openings
                {
//                    new SquareOpening (new Vector2 (1f, 0.75f), new Vector2 (1f, 1.25f)),
//                    new SquareOpening (new Vector2 (3f, 0.75f), new Vector2 (1f, 1.25f))
                }),
            new Wall (
                new Vector2 (0f, 7f),
                new Vector2 (6f, 7f),
                wallWidth * 7f,
                new Wall.Openings
                {
                    new SquareOpening (
                        OpeningType.Inner,
                        0.2f,
                        new Vector2 (1f, 0f),
                        new Vector2 (1f, 1.75f)),
                    new SquareOpening (
                        OpeningType.Outer,
                        0.2f,
                        new Vector2 (1f, 0f),
                        new Vector2 (1f, 1.75f))
                },
                Wall.WidthChangeType.Type2),
            new Wall (
                new Vector2 (6f, 7f),
                new Vector2 (8f, 7f),
                wallWidth,
                new Wall.Openings ()),
            new Wall (
                new Vector2 (8f, 7f),
                new Vector2 (12f, 5f),
                wallWidth,
                new Wall.Openings
                {
                    new SquareOpening (
                        OpeningType.Through,
                        float.MaxValue,
                        new Vector2 (1f, 0.75f),
                        new Vector2 (1f, 1.25f)),
                    new SquareOpening (
                        OpeningType.Through,
                        float.MaxValue,
                        new Vector2 (3f, 0.75f),
                        new Vector2 (1f, 1.25f))
                }),
            new Wall (
                new Vector2 (12f, 5f),
                new Vector2 (12f, 2f),
                wallWidth,
                new Wall.Openings
                {
                    new SquareOpening (
                        OpeningType.Through,
                        float.MaxValue,
                        new Vector2 (0.5f, 0.75f),
                        new Vector2 (1f, 1.25f))
                }),
            new Wall (
                new Vector2 (12f, 2f),
                new Vector2 (10f, 0f),
                wallWidth,
                new Wall.Openings
                {
                    new SquareOpening (
                        OpeningType.Through,
                        float.MaxValue,
                        new Vector2 (1f, 0f),
                        new Vector2 (1f, 1.75f))
                }),
            new Wall (
                new Vector2 (10f, 0f),
                new Vector2 (2f, 0f),
                wallWidth * 2,
                new Wall.Openings
                {
                    new SquareOpening (
                        OpeningType.Through,
                        float.MaxValue,
                        new Vector2 (1f, 0f),
                        new Vector2 (1f, 1.75f)),
                    new SquareOpening (
                        OpeningType.Through,
                        float.MaxValue,
                        new Vector2 (2.25f, 0.75f),
                        new Vector2 (1f, 1.25f)),
                    new SquareOpening (
                        OpeningType.Through,
                        float.MaxValue,
                        new Vector2 (3.5f, 0f),
                        new Vector2 (1f, 1.75f)),
                    new SquareOpening (
                        OpeningType.Outer,
                        0.1f,
                        new Vector2 (4.75f, 0.75f),
                        new Vector2 (1f, 1.25f))
                }),
            new Wall (
                new Vector2 (2f, 0f),
                new Vector2 (0f, 2f),
                wallWidth,
                new Wall.Openings
                {
                    new SquareOpening (
                        OpeningType.Through,
                        float.MaxValue,
                        new Vector2 (1f, 0f),
                        new Vector2 (1f, 1.75f))
                }));
    }

    private void Start ()
    {
        CreateRoom ();
        BuildMesh ();
    }

    private void CreateWallJamb (
        Model.RoomGeometry.Wall innerWall,
        Model.RoomGeometry.Wall outerWall,
        Func<Model.RoomGeometry.Wall, Vector2> endGetter,
        bool flipFaces)
    {
        var innerEnd = endGetter (innerWall);
        var outerEnd = endGetter (outerWall);

        var vertices = new[]
        {
            new Vector3 (innerEnd.x, 0f, innerEnd.y),
            new Vector3 (outerEnd.x, 0f, outerEnd.y),
            new Vector3 (outerEnd.x, wallHeight, outerEnd.y),
            new Vector3 (innerEnd.x, wallHeight, innerEnd.y)
        };

        var triangles = new[] {0, 3, 1, 3, 2, 1};
        var go = CreateMesh ("wall jamb", vertices, triangles, flipFaces, Matrix4x4.identity);

        gos.Add (go);
    }

    private void BuildMesh ()
    {
        if (room == null)
            return;

        foreach (var go in gos) {
            Destroy (go);
        }

        gos.Clear ();


        var roomGeometry = room.GenerateGeometry ();
        var innerContour = roomGeometry.InnerContour;
        var outerContour = roomGeometry.OuterContour;

        ProcessContour ("inner wall", innerContour, false);
        ProcessContour ("outer wall", outerContour, true);

        var innerPoints = new List<Vector2> ();
        var outerPoints = new List<Vector2> ();

        for (int count = innerContour.Count, i = 0; i < count; i++) {
            var innerWall = innerContour[i];
            var outerWall = outerContour[i];

            if (i > 0 && innerPoints[innerPoints.Count - 1] != innerContour[i].Start) {
                innerPoints.Add (innerContour[i].Start);
                outerPoints.Add (outerContour[i].Start);
            }

            innerPoints.Add (innerContour[i].End);
            outerPoints.Add (outerContour[i].End);

            CreateWallJamb (innerWall, outerWall, x => x.Start, true);
            CreateWallJamb (innerWall, outerWall, x => x.End, false);
        }

        var topMatrix = Matrix4x4.TRS (new Vector3 (0f, wallHeight, 0f), Quaternion.identity, Vector3.one);
        var topMesh = CreateMesh (
            "top",
            outerPoints,
            new List<List<Vector2>> {innerPoints},
            false,
            false,
            topMatrix);

        var floorMesh = CreateMesh (
            "floor",
            innerPoints,
            dummyHoles,
            false,
            false,
            Matrix4x4.identity);

        gos.Add (topMesh);
        gos.Add (floorMesh);
    }

    private IEnumerator LateRebuild ()
    {
        yield return null;
        CreateRoom ();
        BuildMesh ();
    }

    private void OnValidate ()
    {
        if (!Application.isPlaying)
            return;

        StartCoroutine (LateRebuild ());
    }
}