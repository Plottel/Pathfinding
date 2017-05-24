﻿using System;
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
        private static bool finishedCalculating = false;
        private static bool blockedEast = false;
        private static bool blockedSouth = false;
        private static bool blockedWest = false;
        private static bool blockedNorth = false;
        private static bool debug = true;
        private static List<NavMeshCell> navCells;
        private static List<Cell> frontier;
        private static NavMeshCell curCell;
        private static Grid Grid;

        private static void AddNavCell(NavMeshCell cell)
        {
            navCells.Add(cell);
            navCells = navCells.OrderBy(c => c.NavMeshID).ToList();
        }
        
        private static bool BlockedOnAllSides
        {
            get { return blockedEast && blockedSouth && blockedWest && blockedNorth; }
        }

        public static List<NavMeshCell> CalculateNavMeshCells(Grid grid)
        {
            NavMeshCell.ResetMeshIDs();
            Grid = grid;

            for (int col = 0; col < Grid.Cols; col++)
            {
                for (int row = 0; row < Grid.Rows; row++)
                    Grid[col, row].MeshID = Cell.NOT_PART_OF_MESH;
            }

            startAt = new Point(Grid.Cols / 2, Grid.Rows / 2);
            Grid[startAt].Passable = true;
            //startAt = new Point(0, 0);
            finishedCalculating = false;
            blockedEast = false;
            blockedSouth = false;
            blockedWest = false;
            blockedNorth = false;
            debug = false;

            navCells = new List<NavMeshCell>();
            curCell = new NavMeshCell(Grid[startAt].Pos, Grid.CellSize, Grid.CellSize);
            frontier = new List<Cell>();

            while (!finishedCalculating)
            {
                // If cell is becoming long and skinny, restrict expansion on that axis.

                // Restrict East / West expansion
                if (curCell.Width >= curCell.Height * 3)
                {
                    blockedEast = true;
                    blockedWest = true;
                }

                // Restrict North / South expansion
                if (curCell.Height >= curCell.Width * 3)
                {
                    blockedNorth = true;
                    blockedSouth = true;
                }

                // Try to expand east.
                if (!blockedEast)
                {
                    blockedEast = DoNewCellsBlock(GetNextEastCol());

                    // Expansion is valid.
                    if (!blockedEast)
                    {
                        // Increase size of current cell.
                        curCell.Width += Grid.CellSize;
                    }
                        
                }

                // Try to expand south.
                if (!blockedSouth)
                {
                    blockedSouth = DoNewCellsBlock(GetNextSouthRow());
                    
                    // Expansion is valid
                    if (!blockedSouth)
                    {
                        // Increase size of current cell.
                        curCell.Height += Grid.CellSize;
                    }
                        
                }

                // Try to expand west.
                if (!blockedWest)
                {
                    blockedWest = DoNewCellsBlock(GetNextWestCol());

                    // Expansion is valid
                    if (!blockedWest)
                    {
                        // Increase size of current cell.
                        curCell.Width += Grid.CellSize;
                        curCell.Pos.X -= Grid.CellSize;
                    }
                }

                // Try to expand north.
                if (!blockedNorth)
                {
                    blockedNorth = DoNewCellsBlock(GetNextNorthRow());

                    // Expansion is valid.
                    if (!blockedNorth)
                    {
                        // Increase size of current cell.
                        curCell.Height += Grid.CellSize;
                        curCell.Pos.Y -= Grid.CellSize;
                    }                    
                }

                // Blocked on both dimensinos, finalise this nav mesh cell.
                if (BlockedOnAllSides)
                {
                    // Finish making this cell and start making a new one.
                    // Probably calculate neighbours here too.
                    uint nextMeshID = NavMeshCell.NextMeshID;
                    curCell.NavMeshID = nextMeshID;
                    AddNavCell(curCell);

                    // Assign each cell in the area as part of the new Nav Mesh Cell.
                    foreach (Cell c in Grid.CellsInRect(curCell.CollisionRect.GetInflated(-1, -1)))
                        c.MeshID = nextMeshID;

                    // Update frontier to remove any cells now part of the mesh
                    for (int i = frontier.Count - 1; i >= 0; i--)
                    {
                        if (frontier[i].MeshID != Cell.NOT_PART_OF_MESH)
                            frontier.RemoveAt(i);
                    }

                    // Get surrounding cells. 
                    var newCells = GetNextEastCol().Concat(GetNextWestCol()).Concat(GetNextSouthRow()).Concat(GetNextNorthRow());

                    foreach (Cell c in newCells)
                    {
                        // Add to frontier if valid
                        if (c.Passable && c.MeshID == Cell.NOT_PART_OF_MESH)
                            frontier.Add(c);

                        // Add surrounding NavMeshCell to curCell neighbours
                        if (c.MeshID != Cell.NOT_PART_OF_MESH && !curCell.HasNeighbour(c.MeshID))
                            curCell.Neighbours.Add(navCells[(int)c.MeshID - 1]);
                    }

                    if (frontier.Count == 0)
                        finishedCalculating = true;             

                    if (!finishedCalculating)
                    {
                        startAt = Grid.IndexAt(frontier[0].Mid);
                        curCell = new NavMeshCell(Grid[startAt].Pos, Grid.CellSize, Grid.CellSize);
                        blockedEast = false;
                        blockedSouth = false;
                        blockedNorth = false;
                        blockedWest = false;
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
                Game1.Instance.spriteBatch.DrawRectangle(cell.RenderRect, Color.DarkBlue, 2);

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

            // Render neighbours white
            foreach (NavMeshCell navCell in navCells)
            {
                foreach (NavMeshCell neighbour in navCell.Neighbours)
                    Game1.Instance.spriteBatch.DrawLine(navCell.RenderMid, neighbour.RenderMid, Color.White, 1);
            }

            // Render Mesh IDs for each cell.
            //for (int col = 0; col < Grid.Cols; col++)
            //{
            //    for (int row = 0; row < Grid.Rows; row++)
            //    {
            //        Cell c = Grid[col, row];

            //        Game1.Instance.spriteBatch.DrawString(Game1.Instance.smallFont, c.MeshID.ToString(), c.RenderMid, Color.Black);
            //    }
            //}

            foreach (NavMeshCell cell in navCells)
                Game1.Instance.spriteBatch.DrawString(Game1.Instance.smallFont, cell.NavMeshID.ToString(), cell.RenderMid, Color.Black);

            Game1.Instance.spriteBatch.DrawString(Game1.Instance.smallFont, "BLOCKED ON COLS: " + blockedOnCols.ToString(), new Vector2(300, 300), Color.White);
            Game1.Instance.spriteBatch.DrawString(Game1.Instance.smallFont, "BLOCKED ON ROWS: " + blockedOnRows.ToString(), new Vector2(300, 320), Color.White);

            Game1.Instance.spriteBatch.End();
            Game1.Instance.GraphicsDevice.Present();

            //System.Threading.Thread.Sleep(400);
        }

        public static bool DoNewCellsBlock(List<Cell> cells)
        {
            if (debug)
                RenderNavMeshCalculation(startAt, curCell, blockedEast, blockedSouth, cells, frontier);

            if (cells.Count == 0)
                return true;
            else
                return cells.Find(cell => !cell.Passable || cell.MeshID != Cell.NOT_PART_OF_MESH) != null;
        }       

        private static List<Cell> GetNextEastCol()
        {
            var result = new List<Cell>();

            Point topRight = Grid.IndexAt(new Vector2(curCell.Pos.X + curCell.Width + 1, curCell.Pos.Y));
            int numRows = curCell.Height / Grid.CellSize;

            for (int i = 0; i < numRows; i++)
                Grid.AddCell(Grid[topRight.Col(), topRight.Row() + i], result);

            return result;
        }

        private static List<Cell> GetNextWestCol()
        {
            var result = new List<Cell>();

            Point topLeft = Grid.IndexAt(new Vector2(curCell.Pos.X - 1, curCell.Pos.Y));
            int numRows = curCell.Height / Grid.CellSize;

            for (int i = 0; i < numRows; i++)
                Grid.AddCell(Grid[topLeft.Col(), topLeft.Row() + i], result);

            return result;
        }

        private static List<Cell> GetNextSouthRow()
        {
            var result = new List<Cell>();

            Point botLeft = Grid.IndexAt(new Vector2(curCell.Pos.X, curCell.Pos.Y + curCell.Height + 1));
            int numCols = curCell.Width / Grid.CellSize;

            for (int i = 0; i < numCols; i++)
                Grid.AddCell(Grid[botLeft.Col() + i, botLeft.Row()], result);

            return result;
        }

        private static List<Cell> GetNextNorthRow()
        {
            var result = new List<Cell>();

            Point topLeft = Grid.IndexAt(new Vector2(curCell.Pos.X, curCell.Pos.Y - 1));
            int numCols = curCell.Width / Grid.CellSize;

            for (int i = 0; i < numCols; i++)
                Grid.AddCell(Grid[topLeft.Col() + i, topLeft.Row()], result);

            return result;
        }        
    }
}
