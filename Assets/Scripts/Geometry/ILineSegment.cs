using System.Collections.Generic;
using UnityEngine;

namespace Geometry
{
    public interface ILineSegment
    {
        List<Vector2> Points { get; }
    }
}