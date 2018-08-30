using System.Collections.Generic;

namespace Model
{
    public class Room
    {
        public List<IWall> Walls;

        public Room (params IWall[] walls)
        {
            Walls = new List<IWall> (walls);
        }
    }
}