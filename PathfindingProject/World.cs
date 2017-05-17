using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PathfindingProject
{
    public class World
    {
        public static int Width;
        public static int Height;

        public HierarchicalGrid Grid;

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
                Grid.CalculateWholeGridEdgeConnections();
            }

            if (Input.RightMouseDown())
            {
                Cell cell = Grid.InnerCellAt(Camera.VecToWorld(Input.MousePos));
                cell.Passable = true;
                cell.Color = Color.ForestGreen;
                Grid.CalculateWholeGridEdgeConnections();
            }
        }

        public void Update()
        {}

        public void Render(SpriteBatch spriteBatch)
        {
            Grid.Render(spriteBatch);
        }
    }
}
