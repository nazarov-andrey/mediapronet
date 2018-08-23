using UnityEngine;

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
    }
}