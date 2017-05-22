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

        public static uint NextMeshID
        {
            get { return _nextMeshID++; }
        }


        public NavMeshCell(Vector2 pos, int width, int height) : base(pos, width, height)
        { 
        }
    }
}
