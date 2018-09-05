using System.Collections.Generic;
using UnityEngine;

namespace Geometry
{
    public class Vector2Comparer : IComparer<Vector2>
    {
        public int Compare (Vector2 x, Vector2 y)
        {
            var result = Comparer<float>.Default.Compare (x.x, y.x);
            if (result == 0)
                result = Comparer<float>.Default.Compare (x.y, y.y);

            return result;
        }

        public Vector2 GetMin (Vector2 x, Vector2 y)
        {
            if (x.x < y.x || Mathf.Approximately (x.x, y.x) && x.y < y.y)
                return x;

            return y;
        }
    }
}