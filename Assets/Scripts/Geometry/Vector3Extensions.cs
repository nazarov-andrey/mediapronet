using UnityEngine;

namespace Geometry
{
    public static class Vector3Extensions
    {
        public static Vector3 TransposePoint (this Vector3 point, Vector3 direction, float distance)
        {
            return point + direction.normalized * distance;
        }

        public static Vector3 FlipYZ (this Vector3 vector)
        {
            return new Vector3 (vector.x, vector.z, vector.y);
        }
    }
}