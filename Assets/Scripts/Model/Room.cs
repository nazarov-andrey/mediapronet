using System.Collections.Generic;

namespace Model
{
    public class Room
    {
        public List<Wall> Walls;

        public Room (params Wall[] walls)
        {
            Walls = new List<Wall> (walls);
        }
    }
}