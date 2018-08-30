using UnityEngine;
using SystemVector2 = System.Numerics.Vector2;

namespace Geometry
{
    public static class SystemVector2Extension
    {
        public static Vector2 ToUnityVector2 (this SystemVector2 vector)
        {
            return new Vector2 (vector.X, vector.Y);
        }

        public static Vector3 ToUnityVector3 (this SystemVector2 vector, float z = 0f, bool flipYZ = false)
        {
            var result = new Vector3 (vector.X, vector.Y, z);
            if (flipYZ)
                result = result.FlipYZ ();

            return result;
        }
    }
}