using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace PathfindingProject
{
    public class NavMeshCell : Cell
    {
        private static uint _nextMeshID = 1;

        public uint NavMeshID { get; set; }

        public Dictionary<NavMeshCell, Vector2> Waypoints = new Dictionary<NavMeshCell, Vector2>();

        public static uint NextMeshID
        {
            get { return _nextMeshID++; }
        }

        public static void ResetMeshIDs()
        {
            _nextMeshID = 1;
        }

        public bool HasNeighbour(uint id)
        {
            foreach (NavMeshCell cell in Neighbours)
            {
                if (cell.NavMeshID == id)
                    return true;
            }
            return false;
        }

        public NavMeshCell(Vector2 pos, int width, int height) : base(pos, width, height)
        {
            Color = Color.Transparent;
        }
    }
}
