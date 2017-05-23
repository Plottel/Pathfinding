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
            {
                _navCells = new List<NavMeshCell>();
                _navCells = NavMeshGenerator.CalculateNavMeshCells(Grid);
            }
                

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
    }
}
