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
            Grid = new Grid(new Vector2(0, 0), 42, 22, 32);
            Grid.ShowGrid = false;

            Random rnd = new Random();
            for (int col = 0; col < Grid.Cols; col++)
            {
                for (int row = 0; row < Grid.Rows; row++)
                {
                    if (rnd.NextSingle(0, 1) < 0.012)
                    {
                        Grid[col, row].Passable = false;
                        Grid[col, row].Color = Color.Black;
                    }
                }
            }
        }

        public void HandleInput()
        {
            Cell cell = Grid.CellAt(Input.MousePos);

            if (Input.KeyTyped(Keys.Space))
            {
                _navCells = new List<NavMeshCell>();
                _navCells = NavMeshGenerator.CalculateNavMeshCells(Grid);
            }
                
            if (cell != null)
            {
                if (Input.LeftMouseDown())
                {
                    cell.Passable = false;
                    cell.Color = Color.Black;
                    _navCells = new List<NavMeshCell>();
                    _navCells = NavMeshGenerator.CalculateNavMeshCells(Grid);
                }

                if (Input.RightMouseDown())
                {
                    cell.Passable = true;
                    cell.Color = Color.ForestGreen;
                    _navCells = new List<NavMeshCell>();
                    _navCells = NavMeshGenerator.CalculateNavMeshCells(Grid);
                }
            }
            
        }

        public void Update()
        { }


        public void Render(SpriteBatch spriteBatch)
        {
            Grid.Render(spriteBatch);

            foreach (NavMeshCell c in _navCells)
                spriteBatch.DrawRectangle(c.RenderRect, Color.DarkBlue, 2);

            //for (int col = 0; col < Grid.Cols; col++)
            //{
            //    for (int row = 0; row < Grid.Rows; row++)
            //        spriteBatch.DrawString(Game1.Instance.smallFont, Grid[col, row].MeshID.ToString(), Grid[col, row].RenderMid, Color.Black);
            //}

            // Render neighbours white
            foreach (NavMeshCell navCell in _navCells)
            {
                foreach (NavMeshCell neighbour in navCell.Neighbours)
                    Game1.Instance.spriteBatch.DrawLine(navCell.RenderMid, neighbour.RenderMid, Color.White, 1);
            }

            foreach (NavMeshCell cell in _navCells)
                Game1.Instance.spriteBatch.DrawString(Game1.Instance.smallFont, cell.NavMeshID.ToString(), cell.RenderMid, Color.Black);
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
    }
}
