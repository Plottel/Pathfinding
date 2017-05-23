using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;

namespace PathfindingProject
{
    public static class NavMeshGenerator
    {
        private static Point startAt = new Point(1, 1);
        private static int curCellCols = 1;
        private static int curCellRows = 1;
        private static bool finishedCalculating = false;
        private static bool blockedOnCols = false;
        private static bool blockedOnRows = false;
        private static bool debug = true;
        private static List<NavMeshCell> navCells;
        private static List<Cell> frontier;
        private static NavMeshCell curCell;
        private static Grid Grid;

        public static List<NavMeshCell> CalculateNavMeshCells(Grid grid)
        {
            Grid = grid;

            for (int col = 0; col < Grid.Cols; col++)
            {
                for (int row = 0; row < Grid.Rows; row++)
                    Grid[col, row].MeshID = Cell.NOT_PART_OF_MESH;
            }

            startAt = new Point(1, 1);
            curCellCols = 1;
            curCellRows = 1;
            finishedCalculating = false;
            blockedOnCols = false;
            blockedOnRows = false;
            debug = true;

            navCells = new List<NavMeshCell>();
            curCell = new NavMeshCell(Grid[startAt].Pos, Grid.CellSize, Grid.CellSize);
            frontier = new List<Cell>();

            while (!finishedCalculating)
            {
                if (!blockedOnCols)
                {
                    // Add col
                    var newCol = GetColRange(startAt.Col() + curCellCols, startAt.Row(), startAt.Row() + curCellRows - 1);

                    if (debug)
                        RenderNavMeshCalculation(startAt, curCell, blockedOnCols, blockedOnRows, newCol, frontier);

                    // If one of the cells in the list is !Passable OR it's already part of the mesh, we are blocked on cols.
                    if (newCol.Count == 0)
                        blockedOnCols = true;
                    else
                        blockedOnCols = newCol.Find(cell => !cell.Passable || cell.MeshID != Cell.NOT_PART_OF_MESH) != null;

                    if (!blockedOnCols)
                    {
                        curCell.Width += Grid.CellSize;
                        //AddColumn(currentCells, newCol);
                        ++curCellCols;
                    }
                }

                if (!blockedOnRows)
                {
                    // Add row
                    var newRow = GetRowRange(startAt.Row() + curCellRows, startAt.Col(), startAt.Col() + curCellCols - 1);

                    if (debug)
                        RenderNavMeshCalculation(startAt, curCell, blockedOnCols, blockedOnRows, newRow, frontier);

                    if (newRow.Count == 0)
                        blockedOnRows = true;
                    else
                        blockedOnRows = newRow.Find(cell => !cell.Passable || cell.MeshID != Cell.NOT_PART_OF_MESH) != null;

                    if (!blockedOnRows)
                    {
                        curCell.Height += Grid.CellSize;
                        ++curCellRows;
                    }
                }

                if (blockedOnCols && blockedOnRows)
                {
                    // Finish making this cell and start making a new one.
                    // Probably calculate neighbours here too.
                    if (curCellCols > 0 && curCellRows > 0)
                    {
                        uint nextMeshID = NavMeshCell.NextMeshID;
                        curCell.NavMeshID = nextMeshID;
                        navCells.Add(curCell);

                        // Assign each cell in the area as part of the new Nav Mesh Cell.
                        Point topLeft = Grid.IndexAt(curCell.Pos);
                        Point bottomRight = Grid.IndexAt(curCell.CollisionRect.BottomRight());

                        for (int col = topLeft.Col(); col < bottomRight.Col(); col++)
                        {
                            for (int row = topLeft.Row(); row < bottomRight.Row(); row++)
                                Grid[col, row].MeshID = nextMeshID;
                        }

                        // Update frontier to remove any cells now part of the mesh
                        for (int i = frontier.Count - 1; i >= 0; i--)
                        {
                            if (frontier[i].MeshID != Cell.NOT_PART_OF_MESH)
                                frontier.RemoveAt(i);
                        }

                        // Add next col and row to frontier.
                        var nextCol = GetColRange(bottomRight.Col(), topLeft.Row(), bottomRight.Row());
                        var nextRow = GetRowRange(bottomRight.Row(), topLeft.Col(), bottomRight.Col());
                        var aboveRow = GetRowRange(topLeft.Row() - 1, topLeft.Col(), bottomRight.Col());
                        var leftCol = GetColRange(topLeft.Col() - 1, topLeft.Col(), bottomRight.Col());

                        var newCells = nextCol.Concat(nextCol).Concat(aboveRow).Concat(leftCol);

                        foreach (Cell c in newCells)
                        {
                            if (c.Passable && c.MeshID == Cell.NOT_PART_OF_MESH)
                                frontier.Add(c);
                        }
                    }

                    // Scan grid until we find a new suitable spot to start a nav mesh cell
                    bool validCell = false;
                    //currentCells = new List<List<Cell>>();

                    if (frontier.Count == 0)
                    {
                        finishedCalculating = true;
                        break;
                    }

                    Random rnd = new Random();

                    while (!validCell)
                    {
                        int next = rnd.Next(0, frontier.Count - 1);

                        if (frontier[next].Passable)
                        {
                            startAt = Grid.IndexAt(frontier[next].Mid);
                            validCell = true;
                            break;
                        }

                        if (debug)
                            RenderNavMeshCalculation(startAt, curCell, blockedOnCols, blockedOnRows, new List<Cell>(), frontier);
                    }

                    if (!finishedCalculating)
                    {
                        // Have we reached end of graph
                        curCell = new NavMeshCell(Grid[startAt].Pos, Grid.CellSize, Grid.CellSize);
                        curCellCols = 1;
                        curCellRows = 1;
                        blockedOnCols = false;
                        blockedOnRows = false;
                    }
                }
            }

            return navCells;
        }

        public static void RenderNavMeshCalculation(Point startCellAtIndex,
                                                NavMeshCell curNavCell,
                                                bool blockedOnCols,
                                                bool blockedOnRows,
                                                List<Cell> toBeConsidered,
                                                List<Cell> frontier)
        {
            Input.UpdateStates();
            Game1.Instance.GraphicsDevice.Clear(Color.CornflowerBlue);

            Game1.Instance.spriteBatch.Begin();
            Game1.Instance.world.Render(Game1.Instance.spriteBatch);

            // Render already constructed nav cells dark blue.
            foreach (NavMeshCell cell in navCells)
                Game1.Instance.spriteBatch.DrawRectangle(cell.RenderRect, Color.DarkBlue, 5);

            // Render currently being constructed nav cell pink
            Game1.Instance.spriteBatch.FillRectangle(curNavCell.RenderRect, Color.Pink);

            // Render current location to start new nav cell
            Game1.Instance.spriteBatch.FillRectangle(Grid[startCellAtIndex].RenderRect, Color.Teal);

            // Render frontier brown
            foreach (Cell c in frontier)
                Game1.Instance.spriteBatch.FillRectangle(c.RenderRect, Color.Brown);

            // Render new cells to be considered in yellow
            foreach (Cell c in toBeConsidered)
                Game1.Instance.spriteBatch.FillRectangle(c.RenderRect, Color.Yellow);

            // Render Mesh IDs for each cell.
            for (int col = 0; col < Grid.Cols; col++)
            {
                for (int row = 0; row < Grid.Rows; row++)
                {
                    Cell c = Grid[col, row];

                    Game1.Instance.spriteBatch.DrawString(Game1.Instance.smallFont, c.MeshID.ToString(), c.RenderMid, Color.Black);
                }
            }

            Game1.Instance.spriteBatch.DrawString(Game1.Instance.smallFont, "BLOCKED ON COLS: " + blockedOnCols.ToString(), new Vector2(300, 300), Color.White);
            Game1.Instance.spriteBatch.DrawString(Game1.Instance.smallFont, "BLOCKED ON ROWS: " + blockedOnRows.ToString(), new Vector2(300, 320), Color.White);

            Game1.Instance.spriteBatch.End();
            Game1.Instance.GraphicsDevice.Present();

            //System.Threading.Thread.Sleep(450);
        }

        private static List<Cell> GetColRange(int col, int start, int end)
        {
            var result = new List<Cell>();

            for (int i = start; i <= end; i++)
                Grid.AddCell(Grid[col, i], result);

            return result;
        }

        private static List<Cell> GetRowRange(int row, int start, int end)
        {
            var result = new List<Cell>();

            for (int i = start; i <= end; i++)
                Grid.AddCell(Grid[i, row], result);

            return result;
        }
    }
}
