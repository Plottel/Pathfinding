using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System.Threading;

namespace PathfindingProject
{
    public class NavMeshWorld : IWorld
    {
        private List<NavMeshCell> _navCells = new List<NavMeshCell>();
        public Grid Grid;

        public int Width { get; set; }
        public int Height { get; set; }

        public NavMeshWorld()
        {
            //Grid = new Grid(new Vector2(0, 0), 176, 96, 8);
            Grid = new Grid(new Vector2(0, 0), 44, 24, 32);
            Grid.ShowGrid = false;
            Grid.MakeBorder();

            Random rnd = new Random();
            for (int col = 0; col < Grid.Cols; col++)
            {
                for (int row = 0; row < Grid.Rows; row++)
                {
                    if (rnd.NextSingle(0, 1) < 0.005)
                    {
                        Grid[col, row].Passable = false;
                        Grid[col, row].Color = Color.Black;
                    }
                }
            }
        }

        public void HandleInput()
        {
            if (Input.KeyTyped(Keys.Space))
                CalculateNavMeshCells();

            if (Input.LeftMouseDown())
            {
                Grid.CellAt(Input.MousePos).Passable = false;
                Grid.CellAt(Input.MousePos).Color = Color.Black;
            }

            if (Input.RightMouseDown())
            {
                Grid.CellAt(Input.MousePos).Passable = true;
                Grid.CellAt(Input.MousePos).Color = Color.ForestGreen;
            }
        }

        public void Update()
        { }


        public void Render(SpriteBatch spriteBatch)
        {
            Grid.Render(spriteBatch);

            foreach (NavMeshCell c in _navCells)
                spriteBatch.DrawRectangle(c.RenderRect, Color.DarkBlue, 5);

            for (int col = 0; col < Grid.Cols; col++)
            {
                for (int row = 0; row < Grid.Rows; row++)
                    spriteBatch.DrawString(Game1.Instance.smallFont, Grid[col, row].MeshID.ToString(), Grid[col, row].RenderMid, Color.Black);
            }
        }

        private void AddColumn(List<List<Cell>> cellList, List<Cell> newCol)
        {
            cellList.Add(newCol);
        }

        private void AddRow(List<List<Cell>> cellList, List<Cell> newRow)
        {
            for (int i = 0; i < newRow.Count - 1; i++)
                cellList[i].Add(newRow[i]);
            
        }
        
        private List<Cell> GetColRange(int col, int start, int end)
        {
            var result = new List<Cell>();           

            for (int i = start; i <= end; i++)
                Grid.AddCell(Grid[col, i], result);                

            return result;
        }

        private List<Cell> GetRowRange(int row, int start, int end)
        {
            var result = new List<Cell>();

            for (int i = start; i <= end; i++)
                Grid.AddCell(Grid[i, row], result);

            return result;
        }

        public void RenderNavMeshCalculation(Point startCellAtIndex, 
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
            foreach (NavMeshCell cell in _navCells)
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

        public void CalculateNavMeshCells()
        {
            for (int col = 0; col < Grid.Cols; col++)
            {
                for (int row = 0; row < Grid.Rows; row++)
                    Grid[col, row].MeshID = Cell.NOT_PART_OF_MESH;
            }

            Point startAt = new Point(1, 1);
            int curCellCols = 1;
            int curCellRows = 1;
            bool finishedCalculating = false;
            bool blockedOnCols = false;
            bool blockedOnRows = false;
            bool debug = true;

            _navCells = new List<NavMeshCell>();

            var curCell = new NavMeshCell(Grid[startAt].Pos, Grid.CellSize, Grid.CellSize);

            var frontier = new List<Cell>();

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
                        _navCells.Add(curCell);

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
        }
    }
}
