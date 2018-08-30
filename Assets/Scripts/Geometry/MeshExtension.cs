using System.Linq;
using UnityEngine;

namespace Geometry
{
    public static class MeshExtension
    {
        public static void FlipFaces (this Mesh mesh)
        {
            mesh.triangles = mesh
                .triangles
                .Reverse ()
                .ToArray ();
        }
    }
}