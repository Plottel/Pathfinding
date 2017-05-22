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
    public class World
    {
        public static int Width;
        public static int Height;

        public HierarchicalGrid Grid;

        public List<Cell> path = new List<Cell>();
        public Vector2 start = Vector2.Zero;
        public Vector2 finish = Vector2.Zero;

        public World()
        {
            Grid = new HierarchicalGrid(new Vector2(0, 0), 128, 4, 11, 6);
            Grid.ShowGrid = true;

            Width = Grid.Width;
            Height = Grid.Height;
        }

        public void HandleInput()
        {

            if (Input.LeftMouseDown())
            {
                Cell cell = Grid.InnerCellAt(Camera.VecToWorld(Input.MousePos));
                cell.Passable = false;
                cell.Color = Color.Black;
                Grid.UpdateGridConnectionsForNeighbours(Grid.OuterCellAt(Input.MousePos));
            }

            if (Input.RightMouseDown())
            {
                Cell cell = Grid.InnerCellAt(Camera.VecToWorld(Input.MousePos));
                cell.Passable = true;
                cell.Color = Color.ForestGreen;
                Grid.UpdateGridConnectionsForNeighbours(Grid.OuterCellAt(Input.MousePos));
            }

            if (Input.KeyTyped(Keys.S))
                start = Input.MousePos;

            if (Input.KeyTyped(Keys.F))
                finish = Input.MousePos;

            if (Input.KeyTyped(Keys.Space))
            {
                Connection from = Grid[0, 0].Connections[0];
                Connection to = Grid[9, 4].Connections.Last();
                path = new List<Cell>(); // Clear so it doesn't render the path while calculating.
                path = Grid.GetPathFromTo(start, finish);
            }
        }

        public void Update()
        {}

        public void Render(SpriteBatch spriteBatch)
        {
            Grid.Render(spriteBatch);

            RenderPath(spriteBatch);
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
