using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PathfindingProject
{
    public class OuterGridCell : Grid
    {
        public OuterGridCell(Vector2 pos, int cols, int rows, int cellSize) : base(pos, cols, rows, cellSize)
        {
        }
    }
}
