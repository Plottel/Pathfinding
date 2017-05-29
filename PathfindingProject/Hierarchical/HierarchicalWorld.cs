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
    public class HierarchicalWorld : IWorld
    {
        public const int OUTER_CELL_SIZE = 128;
        public const int GRID_SCALE = 4;

        public List<Cell> path = new List<Cell>();
        public Vector2 start = Vector2.Zero;
        public Vector2 finish = Vector2.Zero;

        public int Width
        {
            get; set;
        }

        public int Height
        {
            get; set;
        }

        public HierarchicalGrid HGrid;

        private Grid _grid;

        public Grid Grid
        {
            get { return _grid; }

            set
            {
                _grid = value;
                HGrid = new HierarchicalGrid(_grid.Pos, OUTER_CELL_SIZE, GRID_SCALE, _grid.Cols / GRID_SCALE, _grid.Rows / GRID_SCALE);

                // Derive HierarchicalGrid from grid value.
                for (int col = 0; col < _grid.Cols; col++)
                {
                    for (int row = 0; row < _grid.Rows; row++)
                    {
                        int outerCol = col / GRID_SCALE;
                        int outerRow = row / GRID_SCALE;

                        int innerCol = col % GRID_SCALE;
                        int innerRow = row % GRID_SCALE;

                        Cell c = HGrid[outerCol, outerRow][innerCol, innerRow];
                        c.Passable = _grid[col, row].Passable;

                        if (c.Passable)
                            c.Color = Color.ForestGreen;
                        else
                            c.Color = Color.Black;
                    }
                }

                // Setup cell neighbours to restrict neighbouring to each OuterGridCell.
                for (int col = 0; col < HGrid.Cols; col++)
                {
                    for (int row = 0; row < HGrid.Rows; row++)
                    {
                        HGrid[col, row].SetupCellNeighbours(false);
                    }
                }

                HGrid.SetupNeighbours();
                HGrid.CalculateWholeGridConnections();
            }
        }



        public HierarchicalWorld(Grid grid)
        {
            Grid = grid;
            //HGrid = new HierarchicalGrid(new Vector2(0, 0), 128 , 4, 11, 6);
            HGrid.ShowGrid = true;

            Width = HGrid.Width;
            Height = HGrid.Height;
        }

        public void HandleInput()
        {

            if (Input.LeftMouseDown())
            {
                Cell cell = HGrid.InnerCellAt(Camera.VecToWorld(Input.MousePos));
                cell.Passable = false;
                cell.Color = Color.Black;
                HGrid.UpdateGridConnectionsForNeighbours(HGrid.OuterCellAt(Input.MousePos));
            }

            if (Input.RightMouseDown())
            {
                Cell cell = HGrid.InnerCellAt(Camera.VecToWorld(Input.MousePos));
                cell.Passable = true;
                cell.Color = Color.ForestGreen;
                HGrid.UpdateGridConnectionsForNeighbours(HGrid.OuterCellAt(Input.MousePos));
            }

            if (Input.KeyTyped(Keys.S))
                start = Camera.VecToWorld(Input.MousePos);

            if (Input.KeyTyped(Keys.F))
                finish = Camera.VecToWorld(Input.MousePos);

            if (Input.KeyTyped(Keys.G))
            {
                path = new List<Cell>(); // Clear so it doesn't render the path while calculating.
                path = HGrid.GetPathFromTo(start, finish);
            }
        }

        public void Update()
        {}

        public void Render(SpriteBatch spriteBatch)
        {
            HGrid.Render(spriteBatch);
            RenderPath(spriteBatch);

            if (start != Vector2.Zero)
                spriteBatch.DrawPoint(Camera.VecToScreen(start), Color.Green, 20);
            if (finish != Vector2.Zero)
                spriteBatch.DrawPoint(Camera.VecToScreen(finish), Color.Red, 20);
        }

        public void RenderPath(SpriteBatch spriteBatch)
        {
            // Render path
            if (path.Count > 0)
            {
                for (int i = 0; i < path.Count - 1; i++)
                {
                    spriteBatch.DrawPoint(path[i].RenderMid, Color.Orange, 10);
                    spriteBatch.DrawLine(path[i].RenderMid, path[i + 1].RenderMid, Color.MonoGameOrange, 4);
                }

                spriteBatch.DrawPoint(path[path.Count - 1].RenderMid, Color.Orange, 10);
            }
        }
    }
}
