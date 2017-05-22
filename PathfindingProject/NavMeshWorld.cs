using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;

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
            Grid = new Grid(new Vector2(0, 0), 44, 24, 32);
            Grid.ShowGrid = false;
            Grid.MakeBorder();
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
                result.Add(Grid[col, i]);

            return result;
        }

        private List<Cell> GetRowRange(int row, int start, int end)
        {
            var result = new List<Cell>();

            for (int i = start; i <= end; i++)
                result.Add(Grid[i, row]);

            return result;
        }

        public void CalculateNavMeshCells()
        {
            Point startCellAtIndex = Point.Zero;
            List<List<Cell>> currentCells = new List<List<Cell>>();
            int curCellCols = 0;
            int curCellRows = 0;
            bool finishedCalculating = false;
            bool blockedOnCols = false;
            bool blockedOnRows = false;

            _navCells = new List<NavMeshCell>();

            while (!finishedCalculating)
            {
                #region Render Nav Mesh Calculation Debug
                Input.UpdateStates();
                Game1.Instance.GraphicsDevice.Clear(Color.CornflowerBlue);

                Game1.Instance.spriteBatch.Begin();
                Game1.Instance.world.Render(Game1.Instance.spriteBatch);

                // Render already constructed nav cells dark blue.
                foreach (NavMeshCell cell in _navCells)
                    Game1.Instance.spriteBatch.DrawRectangle(cell.RenderRect, Color.DarkBlue, 5);

                Rectangle currentRect = new Rectangle(startCellAtIndex.Col() * Grid.CellSize,
                    startCellAtIndex.Row() * Grid.CellSize,
                    curCellCols * Grid.CellSize,
                    curCellRows * Grid.CellSize);

                // Render currently being constructed nav cell pink
                Game1.Instance.spriteBatch.FillRectangle(currentRect, Color.Pink);

                Game1.Instance.spriteBatch.End();
                Game1.Instance.GraphicsDevice.Present();

                System.Threading.Thread.Sleep(100);
                #endregion Render Nav Mesh Calculation Debug

                if (!blockedOnCols)
                {
                    // Add col
                    var newCol = GetColRange(startCellAtIndex.Col() + curCellCols, startCellAtIndex.Row(), startCellAtIndex.Row() + curCellRows);

                    blockedOnCols = newCol.Find(cell => !cell.Passable && cell.MeshID != Cell.NOT_PART_OF_MESH) != null;

                    if (!blockedOnCols)
                    {
                        AddColumn(currentCells, newCol);
                        ++curCellCols;
                    }
                }

                if (!blockedOnRows)
                {
                    // Add row
                    var newRow = GetRowRange(startCellAtIndex.Row() + curCellRows, startCellAtIndex.Col(), startCellAtIndex.Col() + curCellCols);

                    blockedOnRows = newRow.Find(cell => !cell.Passable && cell.MeshID != Cell.NOT_PART_OF_MESH) != null;

                    if (!blockedOnRows)
                    {
                        AddRow(currentCells, newRow);
                        ++curCellRows;
                    }
                }

                if (blockedOnCols && blockedOnRows)
                {
                    // Finish making this cell and start making a new one.
                    // Probably calculate neighbours here too.
                    var newNavMeshCell = new NavMeshCell(Grid.IndexToVec(startCellAtIndex), curCellCols * Grid.CellSize, curCellRows * Grid.CellSize);

                    uint nextMeshID = NavMeshCell.NextMeshID;
                    newNavMeshCell.NavMeshID = nextMeshID;
                    _navCells.Add(newNavMeshCell);

                    // Assign each cell in the area as part of the new Nav Mesh Cell.
                    for (int col = startCellAtIndex.Col(); col < curCellCols; col++)
                    {
                        for (int row = startCellAtIndex.Row(); row < curCellRows; row++)
                            currentCells[col][row].MeshID = nextMeshID;
                    }

                    // Scan grid until we find a new suitable spot to start a nav mesh cell
                    bool validCell = false;
                    curCellCols = 0;
                    curCellRows = 0;
                    currentCells = new List<List<Cell>>();

                    while (!validCell)
                    {
                        // Move to next col to check if we can start a new nav cell
                        startCellAtIndex.IncCol();

                        // Do we need to wrap to next row?
                        if (startCellAtIndex.Col() == Grid.Cols - 1)
                        {
                            startCellAtIndex.X = 0; // X == col
                            startCellAtIndex.IncRow();
                        }                        

                        // Check if new index is a valid cell
                        validCell = Grid[startCellAtIndex].Passable && Grid[startCellAtIndex].MeshID == Cell.NOT_PART_OF_MESH;
                    }
                }

                // Reached end of the grid, stop making nav graph cells.
                if (startCellAtIndex.Col() + curCellCols  > Grid.Cols - 1 && startCellAtIndex.Row() + curCellRows> Grid.Rows - 1)
                {
                    finishedCalculating = true;
                    break;
                }
            }            
        }
    }
}
