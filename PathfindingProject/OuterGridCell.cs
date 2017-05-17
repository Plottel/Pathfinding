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

        public List<Cell> GetBottomRow()
        {
            var result = new List<Cell>();

            foreach (List<Cell> col in _cells)
                result.Add(col.Last());

            return result;
        }

        public List<Cell> GetTopRow()
        {
            var result = new List<Cell>();

            foreach (List<Cell> col in _cells)
                result.Add(col[0]);

            return result;
        }

        public List<Cell> GetLeftCol()
        {
            return _cells[0];
        }

        public List<Cell> GetRightCol()
        {
            return _cells[Cols - 1];
        }
    }
}
