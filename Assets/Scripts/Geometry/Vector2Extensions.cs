using UnityEngine;
using SystemVector = System.Numerics.Vector2;

namespace Geometry
{
    public static class Vector2Extensions
    {
        public static Vector2 GetNormalVector (this Vector2 vector)
        {
            return new Vector2 (vector.y, -vector.x);
        }

        public static Vector2 TransposePoint (this Vector2 point, Vector2 direction, float distance)
        {
            return point + direction.normalized * distance;
        }

        public static SystemVector ToSystemVector (this Vector2 vector)
        {
            return new SystemVector (vector.x, vector.y);
        }

        public static Vector3 ToVector3 (this Vector2 vector, float z = 0f)
        {
            return new Vector3 (vector.x, vector.y, z);
        }
    }
}