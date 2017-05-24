using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PathfindingProject
{
    public static class FlowFieldGenerator
    {
        private static Grid Grid;
        private static List<Cell> open = new List<Cell>();
        private static List<Cell> closed = new List<Cell>();
        private static Cell current;
        private static Cell source;
        private static Dictionary<Cell, Cell> parents = new Dictionary<Cell, Cell>();
        private static Dictionary<Cell, float> scores = new Dictionary<Cell, float>();

        private static bool IsDiagonal(Cell one, Cell two)
        {
            Point idxOne = Grid.IndexAt(one.Mid);
            Point idxTwo = Grid.IndexAt(two.Mid);

            return Math.Abs(idxOne.Col() - idxTwo.Col()) == Math.Abs(idxOne.Row() - idxTwo.Row());
        }

        public static void GenerateFlowFieldTo(Grid grid, Vector2 toPos)
        {
            Grid = grid;
            open = new List<Cell>();
            closed = new List<Cell>();
            source = Grid.CellAt(toPos);
            parents = new Dictionary<Cell, Cell>();
            scores = new Dictionary<Cell, float>();

            source.flowVector = Vector2.Zero;

            open.Add(source);
            scores.Add(source, 0);

            while (open.Count > 0)
            {
                open = open.OrderBy(c => scores[c]).ToList();
                current = open[0];

                foreach (Cell cell in current.Neighbours)
                {
                    if (!open.Contains(cell) && !closed.Contains(cell) && cell.Passable)
                    {
                        float score = scores[current] + 1;
                        if (IsDiagonal(current, cell))
                            score += 0.41f; // Extra diagonal cost

                        scores.Add(cell, score);
                        open.Add(cell);

                        cell.flowVector = Vector2.Normalize(current.Mid - cell.Mid);
                    }
                }

                open.Remove(current);
                closed.Add(current);
            }
        }
    }
}
