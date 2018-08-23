using System.Collections.Generic;
using JetBrains.Annotations;

namespace Model
{
    public class Rooms : List<Room>
    {
        public Rooms ()
        {
        }

        public Rooms ([NotNull] IEnumerable<Room> collection) : base (collection)
        {
        }

        public Rooms (int capacity) : base (capacity)
        {
        }
    }
}