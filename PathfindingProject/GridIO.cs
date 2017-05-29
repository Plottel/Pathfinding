using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;

namespace PathfindingProject
{
    public static class GridIO
    {
        private static int ReadInteger(StreamReader reader)
        {
            return Convert.ToInt32(reader.ReadLine());
        }

        public static void SaveGridTo(StreamWriter writer, Grid grid)
        {
            writer.WriteLine(grid.Pos.X);
            writer.WriteLine(grid.Pos.Y);
            writer.WriteLine(grid.Cols);
            writer.WriteLine(grid.Rows);
            writer.WriteLine(grid.CellSize);
            writer.WriteLine(grid.IncludeDiagonals);

            for (int col = 0; col < grid.Cols; col++)
            {
                for (int row = 0; row < grid.Rows; row++)
                {
                    writer.WriteLine(grid[col, row].Passable);
                }
            }
        }

        public static Grid LoadGridFrom(StreamReader reader)
        {
            float x = ReadInteger(reader);
            float y = ReadInteger(reader);
            int cols = ReadInteger(reader);
            int rows = ReadInteger(reader);
            int cellSize = ReadInteger(reader);
            bool includeDiagonals = Convert.ToBoolean(reader.ReadLine());

            Grid grid = new Grid(new Vector2(x, y), cols, rows, cellSize, includeDiagonals);

            for (int col = 0; col < grid.Cols; col++)
            {
                for (int row = 0; row < grid.Rows; row++)
                {
                    Cell c = grid[col, row];

                    c.Passable = Convert.ToBoolean(reader.ReadLine());

                    if (c.Passable)
                        c.Color = Color.ForestGreen;
                    else
                        c.Color = Color.Black;
                }
            }

            return grid;
        }
    }
}
