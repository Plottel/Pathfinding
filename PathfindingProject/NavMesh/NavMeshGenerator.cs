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

        public static void AddConnection(Cell traceCur, Cell traceStart, NavMeshCell navCell)
        {
            if (traceCur == null || traceStart == null)
                return;

            // Determine which side the connected cell is on.
            // Generate edge at opposite side.
            Vector2 center;

            if (traceCur.Mid.X > navCell.CollisionRect.Right) // East
                center = (traceStart.CollisionRect.CenterLeft() + traceCur.CollisionRect.CenterLeft()) / 2;
            else if (traceCur.Mid.X < navCell.CollisionRect.Left) // West
                center = (traceStart.CollisionRect.CenterRight() + traceCur.CollisionRect.CenterRight()) / 2;
            else if (traceCur.Mid.Y < navCell.CollisionRect.Top) // North
                center = (traceStart.CollisionRect.CenterBottom() + traceCur.CollisionRect.CenterBottom()) / 2;
            else // South
                center = (traceStart.CollisionRect.CenterTop() + traceCur.CollisionRect.CenterTop()) / 2;

            NavMeshCell other = navCells[(int)traceStart.MeshID - 1];

            navCell.Neighbours.Add(other);
            navCell.Waypoints.Add(other, center);
        }

        public static void CalculateNeighbours()
        {
            foreach (NavMeshCell navCell in navCells)
            {
                Cell traceStart = null;
                Cell traceCur = null;

                foreach (Cell c in GetNextBorder(navCell))
                {
                    RenderNavMeshCalculation(startAt, curCell, false, false, new List<Cell>(), frontier, traceStart, traceCur);

                    // Add surrounding NavMeshCell to curCell neighbours
                    if (c.MeshID != Cell.NOT_PART_OF_MESH && !navCell.HasNeighbour(c.MeshID))
                    {
                        // Start tracing along a new nav cell.
                        if (traceStart == null)
                        {
                            traceStart = c;
                            traceCur = c;
                        }
                        else
                        {
                            // If we're still tracing along same nav cell, extend the trace.
                            if (c.MeshID == traceStart.MeshID)
                                traceCur = c;
                            else // Finished trace, generate edge.
                            {
                                AddConnection(traceCur, traceStart, navCell);

                                // Start new trace.
                                traceStart = c;
                                traceCur = c;
                            }
                        }
                    }
                }

                // Check to see if last cell should have generated a connection.
                AddConnection(traceCur, traceStart, navCell);
            }
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
                    blockedEast = DoNewCellsBlock(GetNextEastCol(curCell));

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
                    blockedSouth = DoNewCellsBlock(GetNextSouthRow(curCell));
                    
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
                    blockedWest = DoNewCellsBlock(GetNextWestCol(curCell));

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
                    blockedNorth = DoNewCellsBlock(GetNextNorthRow(curCell));

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
                    var newCells = GetNextBorder(curCell);

                    foreach (Cell c in newCells)
                    {
                        // Add to frontier if valid
                        if (c.Passable && c.MeshID == Cell.NOT_PART_OF_MESH)
                            frontier.Add(c);                                                                         
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

            CalculateNeighbours();
            return navCells;
        }

        public static void RenderNavMeshCalculation(Point startCellAtIndex,
                                                NavMeshCell curNavCell,
                                                bool blockedOnCols,
                                                bool blockedOnRows,
                                                List<Cell> toBeConsidered,
                                                List<Cell> frontier, 
                                                Cell traceStart, 
                                                Cell traceCur)
        {
            if (!Debug.IsOn(DebugOp.CalcNavMesh))
                return;

            SpriteBatch spriteBatch = Game1.Instance.spriteBatch;


            Input.UpdateStates();
            Game1.Instance.GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            Game1.Instance.world.Render(spriteBatch);

            // Render already constructed nav cells dark blue.
            foreach (NavMeshCell cell in navCells)
                spriteBatch.DrawRectangle(cell.RenderRect, Color.DarkBlue, 2);

            // Render currently being constructed nav cell pink
            spriteBatch.FillRectangle(curNavCell.RenderRect, Color.Pink);

            // Render current location to start new nav cell
            spriteBatch.FillRectangle(Grid[startCellAtIndex].RenderRect, Color.Teal);

            // Render frontier brown
            foreach (Cell c in frontier)
                spriteBatch.FillRectangle(c.RenderRect, Color.Brown);

            // Render new cells to be considered in yellow
            foreach (Cell c in toBeConsidered)
                spriteBatch.FillRectangle(c.RenderRect, Color.Yellow);

            // Render neighbours white
            foreach (NavMeshCell navCell in navCells)
            {
                foreach (Vector2 pos in navCell.Waypoints.Values)
                    spriteBatch.DrawLine(navCell.RenderMid, pos, Color.White, 1);

                spriteBatch.DrawPoint(navCell.RenderMid, Color.Orange, 6);
            }

            // Render trace.
            if (traceStart != null)
                spriteBatch.FillRectangle(traceStart.RenderRect, Color.DarkViolet);

            if (traceCur != null)
                spriteBatch.FillRectangle(traceCur.RenderRect, Color.DarkViolet);

            if (traceStart != null && traceCur != null)
                spriteBatch.DrawLine(traceStart.RenderMid, traceCur.RenderMid, Color.Red, 3);

            foreach (NavMeshCell cell in navCells)
                Game1.Instance.spriteBatch.DrawString(Game1.Instance.smallFont, cell.NavMeshID.ToString(), cell.RenderMid, Color.Black);
           
            Game1.Instance.spriteBatch.End();
            Game1.Instance.GraphicsDevice.Present();

            //System.Threading.Thread.Sleep(400);
        }

        public static bool DoNewCellsBlock(List<Cell> cells)
        {
            if (debug)
                RenderNavMeshCalculation(startAt, curCell, blockedEast, blockedSouth, cells, frontier, null, null);

            if (cells.Count == 0)
                return true;
            else
                return cells.Find(cell => !cell.Passable || cell.MeshID != Cell.NOT_PART_OF_MESH) != null;
        }
        
        private static List<Cell> GetNextBorder(NavMeshCell curCell)
        {
            return GetNextEastCol(curCell).Concat(GetNextSouthRow(curCell)).Concat(GetNextWestCol(curCell)).Concat(GetNextNorthRow(curCell)).ToList();
        }

        private static List<Cell> GetNextEastCol(NavMeshCell curCell)
        {
            var result = new List<Cell>();

            Point topRight = Grid.IndexAt(new Vector2(curCell.Pos.X + curCell.Width + 1, curCell.Pos.Y));
            int numRows = curCell.Height / Grid.CellSize;

            for (int i = 0; i < numRows; i++)
                Grid.AddCell(Grid[topRight.Col(), topRight.Row() + i], result);

            return result;
        }

        private static List<Cell> GetNextWestCol(NavMeshCell curCell)
        {
            var result = new List<Cell>();

            Point topLeft = Grid.IndexAt(new Vector2(curCell.Pos.X - 1, curCell.Pos.Y));
            int numRows = curCell.Height / Grid.CellSize;

            for (int i = 0; i < numRows; i++)
                Grid.AddCell(Grid[topLeft.Col(), topLeft.Row() + i], result);

            return result;
        }

        private static List<Cell> GetNextSouthRow(NavMeshCell curCell)
        {
            var result = new List<Cell>();

            Point botLeft = Grid.IndexAt(new Vector2(curCell.Pos.X, curCell.Pos.Y + curCell.Height + 1));
            int numCols = curCell.Width / Grid.CellSize;

            for (int i = 0; i < numCols; i++)
                Grid.AddCell(Grid[botLeft.Col() + i, botLeft.Row()], result);

            return result;
        }

        private static List<Cell> GetNextNorthRow(NavMeshCell curCell)
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
