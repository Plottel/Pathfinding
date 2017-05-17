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
            Grid = new HierarchicalGrid(new Vector2(0, 0), 128, 4, 5, 5);
            Grid.ShowGrid = true;

            Width = Grid.Width;
            Height = Grid.Height;
        }

        public void Update()
        {}

        public void Render(SpriteBatch spriteBatch)
        {
            Grid.Render(spriteBatch);
        }
    }
}
