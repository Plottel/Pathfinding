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
    public class FlowFieldWorld : IWorld
    {
        public int Width { get; set; }
        public int Height { get; set; }

        public Cell target;

        private Grid _grid;

        public Grid Grid
        {
            get { return _grid; }
            set
            {
                _grid = value;
                _grid.SetupCellNeighbours(true);
            }
        }

        public FlowFieldWorld(Grid grid)
        {
            Grid = grid;
        }

        public void HandleInput()
        {
            if (Input.KeyTyped(Keys.Space))
            {
                FlowFieldGenerator.GenerateFlowFieldTo(Grid, Input.MousePos);
                target = Grid.CellAt(Input.MousePos);
            }
                

            if (Input.LeftMouseDown())
            {
                Cell c = Grid.CellAt(Input.MousePos);

                if (c != null)
                {
                    c.Passable = false;
                    c.Color = Color.Black;

                    Grid.UpdateNeighboursAroundIndex(Grid.IndexAt(c.Mid));
                }
            }

            if (Input.RightMouseDown())
            {
                Cell c = Grid.CellAt(Input.MousePos);

                if (c != null)
                {
                    c.Passable = true;
                    c.Color = Color.ForestGreen;

                    Grid.UpdateNeighboursAroundIndex(Grid.IndexAt(c.Mid));
                }
            }
        }

        public void Update()
        {

        }

        public void Render(SpriteBatch spriteBatch)
        {
            Grid.Render(spriteBatch);

            for (int col = 0; col < Grid.Cols; col++)
            {
                for (int row = 0; row < Grid.Rows; row++)
                {
                    Cell c = Grid[col, row];

                    if (c.Passable && c.flowVector != Vector2.Zero && Debug.IsOn(DebugOp.ShowUniqueGrid))
                    {
                        spriteBatch.DrawLine(c.RenderMid - (c.flowVector * 10), c.RenderMid + (c.flowVector * 10), Color.White, 3);
                        spriteBatch.DrawPoint(c.RenderMid - (c.flowVector * 10), Color.Red, 5);
                    }                    
                }
            }

            if (target != null)
                spriteBatch.DrawPoint(target.RenderMid, Color.LawnGreen, 20);
        }
    }
}
