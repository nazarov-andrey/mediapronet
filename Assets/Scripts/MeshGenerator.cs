using System.Collections.Generic;
using System.Linq;
using Geometry;
using UnityEngine;
using UnityEngine.Assertions;

public static class MeshGenerator
{
    public static bool Triangulate (
        List<Vector2> contour,
        List<List<Vector2>> holes,
        bool flipFaces,
        out List<Vector3> vertices,
        out List<int> triangles)
    {
        var result = Triangulation.Triangulate (
            contour,
            holes,
            out triangles,
            out vertices);

        if (flipFaces)
            triangles.Reverse ();

        return result;
    }

    public static Mesh CreateMesh (Vector3[] vertices, int[] triangles, bool flipFaces = false, string name = null)
    {
        var mesh = new Mesh ();

        mesh.name = name ?? "";
        mesh.Clear ();
        mesh.vertices = vertices;
        mesh.triangles = flipFaces
            ? triangles.Reverse ().ToArray ()
            : triangles;
        mesh.RecalculateNormals ();

        return mesh;
    }

    public static Mesh CreateMesh (
        List<Vector2> contour,
        List<List<Vector2>> holes,
        bool flipFaces,
        string name = null)
    {
        List<int> triangles;
        List<Vector3> vertices;

        Assert.IsTrue (Triangulate (contour, holes, flipFaces, out vertices, out triangles));

        return CreateMesh (vertices.ToArray (), triangles.ToArray (), false, name);
    }


    public static GameObject CreateGameObject (string name, Mesh mesh)
    {
        var gameObj = new GameObject (name, typeof (MeshRenderer), typeof (MeshFilter));
        gameObj
            .GetComponent<MeshFilter> ()
            .mesh = mesh;

        gameObj
            .GetComponent<MeshRenderer> ()
            .material = new Material (Shader.Find ("Standard"));

        return gameObj;
    }

    public static GameObject CreateGameObject (string name, Vector3[] vertices, int[] triangles, bool flipFaces)
    {
        var mesh = CreateMesh (vertices, triangles, flipFaces);
        return CreateGameObject (name, mesh);
    }

    public static GameObject CreateGameObject (
        string name,
        List<Vector2> contour,
        List<List<Vector2>> holes,
        bool flipFaces,
        bool flipYZ,
        Matrix4x4 matrix)
    {
        var mesh = CreateMesh (contour, holes, flipFaces);

        mesh.vertices = mesh
            .vertices
            .Select (matrix.MultiplyPoint)
            .Select (x => flipYZ ? x.FlipYZ () : x)
            .ToArray ();

        return CreateGameObject (name, mesh);
    }
}