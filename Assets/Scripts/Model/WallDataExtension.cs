using UnityEngine;

namespace Model
{
    public static class WallDataExtension
    {
        public static Vector2 GetVector (this WallData wall)
        {
            return wall.End - wall.Start;
        }

        public static Vector2 GetInverseVector (this WallData wall)
        {
            return -wall.GetVector ();
        }
    }
}