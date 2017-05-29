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
        public Grid Grid { get; set; }
        public static Random bogo = new Random();
        public Vector2 start;
        public Vector2 finish;
        public List<Vector2> path = new List<Vector2>();

        public int Width { get; set; }
        public int Height { get; set; }

        public NavMeshWorld(Grid grid)
        {
            Grid = grid;
        }

        public void HandleInput()
        {
            Cell cell = Grid.CellAt(Input.MousePos);

            if (Input.KeyTyped(Keys.S))
                start = Camera.VecToWorld(Input.MousePos);

            if (Input.KeyTyped(Keys.F))
                finish = Camera.VecToWorld(Input.MousePos);

            if (Input.KeyTyped(Keys.G))
            {
                path = new List<Vector2>();
                path = GetPathFromTo(start, finish);
            }

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
                    //_navCells = new List<NavMeshCell>();
                    //_navCells = NavMeshGenerator.CalculateNavMeshCells(Grid);
                }

                if (Input.RightMouseDown())
                {
                    cell.Passable = true;
                    cell.Color = Color.ForestGreen;
                    //_navCells = new List<NavMeshCell>();
                    //_navCells = NavMeshGenerator.CalculateNavMeshCells(Grid);
                }
            }
            
        }

        public List<Vector2> GetPathFromTo(Vector2 fromPos, Vector2 toPos)
        {
            var open = new List<NavMeshCell>();
            var closed = new List<NavMeshCell>();
            var current = _navCells[(int)Grid.CellAt(fromPos).MeshID - 1];
            var source = current;
            var target = _navCells[(int)Grid.CellAt(toPos).MeshID - 1];
            var parents = new Dictionary<NavMeshCell, NavMeshCell>();
            var scores = new Dictionary<NavMeshCell, float>();

            if (source == target)
                return new List<Vector2> { toPos };

            bool debug = true;

            open.Add(current);

            parents.Add(current, null);
            scores.Add(current, 0);

            bool searchComplete = false;

            while (!searchComplete)
            {
                if (debug)
                {
                    #region Calculate Path Visual
                    Input.UpdateStates();
                    Game1.Instance.GraphicsDevice.Clear(Color.CornflowerBlue);

                    Game1.Instance.spriteBatch.Begin();
                    Game1.Instance.world.Render(Game1.Instance.spriteBatch);

                    // Render closed list pale blue
                    foreach (NavMeshCell c in closed)
                        Game1.Instance.spriteBatch.FillRectangle(c.RenderRect, Color.LightSteelBlue);

                    // Render open list cream
                    foreach (NavMeshCell c in open)
                        Game1.Instance.spriteBatch.FillRectangle(c.RenderRect, Color.BlanchedAlmond);

                    // Render source red
                    Game1.Instance.spriteBatch.FillRectangle(source.RenderRect, Color.LightSteelBlue);

                    // Render current purple
                    Game1.Instance.spriteBatch.FillRectangle(current.RenderRect, Color.LightSteelBlue);
                    Game1.Instance.spriteBatch.FillRectangle(current.RenderRect, Color.LightSteelBlue);

                    // Render target green
                    Game1.Instance.spriteBatch.DrawPoint(toPos, Color.GreenYellow, 20);

                    var tempRenderPath = new List<Vector2>();
                    var tempCurrent = current;                   

                    while (parents[tempCurrent] != null)
                    {
                        tempRenderPath.Insert(0, tempCurrent.Waypoints[parents[tempCurrent]]);
                        tempCurrent = parents[tempCurrent];
                    }

                    tempRenderPath.Insert(0, fromPos);
                    RenderPath(Game1.Instance.spriteBatch, tempRenderPath);

                    foreach (NavMeshCell c in _navCells)
                        Game1.Instance.spriteBatch.DrawRectangle(c.RenderRect, Color.DarkBlue, 2);

                    // Render scores
                    foreach (NavMeshCell c in open)
                        Game1.Instance.spriteBatch.DrawString(Game1.Instance.smallFont, Math.Floor(scores[c]).ToString(), c.RenderMid, Color.Black);


                    Game1.Instance.spriteBatch.End();
                    Game1.Instance.GraphicsDevice.Present();

                    System.Threading.Thread.Sleep(200);
                    #endregion Calculate Path Visual
                }

                foreach (NavMeshCell cell in current.Neighbours)
                {
                    if (!closed.Contains(cell) && !open.Contains(cell))
                    {
                        open.Add(cell);
                        parents.Add(cell, current);
                        scores.Add(cell, Vector2.Distance(current.Waypoints[cell], toPos));
                        
                        if (cell == target)
                        {
                            searchComplete = true;
                            current = cell;
                            break;
                        }
                    }
                }

                if (searchComplete)
                    break;

                open.Remove(current);
                closed.Add(current);

                if (open.Count == 0)
                    return new List<Vector2>();

                open = open.OrderBy(c => scores[c]).ToList();
                current = open[0];
            }

            var path = new List<Vector2>();
            path.Insert(0, toPos);
            path.Insert(0, current.Waypoints[parents[current]]);

            while (parents[current] != null)
            {
                path.Insert(0, current.Waypoints[parents[current]]);
                current = parents[current];
            }

            path.Insert(0, fromPos);

            return path;
        }

        public void Update()
        { }


        public void Render(SpriteBatch spriteBatch)
        {
            Grid.Render(spriteBatch);

            foreach (NavMeshCell c in _navCells)
                spriteBatch.DrawRectangle(c.RenderRect, Color.DarkBlue, 2);

            // Render neighbours white
            //foreach (NavMeshCell navCell in _navCells)
            //{
            //    foreach (Vector2 pos in navCell.Waypoints.Values)
            //        Game1.Instance.spriteBatch.DrawLine(navCell.RenderMid, pos, Color.White, 1);

            //    Game1.Instance.spriteBatch.DrawPoint(navCell.RenderMid, Color.Orange, 6);
            //}

            //foreach (NavMeshCell cell in _navCells)
            //    Game1.Instance.spriteBatch.DrawString(Game1.Instance.smallFont, cell.NavMeshID.ToString(), cell.RenderMid, Color.Black);

            RenderPath(spriteBatch, path);

            if (start != null)
                spriteBatch.DrawPoint(start, Color.Red, 20);

            if (finish != null)
                spriteBatch.DrawPoint(finish, Color.GreenYellow, 20);
        }

        public void RenderPath(SpriteBatch spriteBatch, List<Vector2> path)
        {
            // Render path
            if (path.Count > 0)
            {
                for (int i = 0; i < path.Count - 1; i++)
                {
                    spriteBatch.DrawPoint(path[i], Color.Orange, 10);
                    spriteBatch.DrawLine(path[i], path[i + 1], Color.MonoGameOrange, 4);
                }

                spriteBatch.DrawPoint(path[path.Count - 1], Color.Orange, 10);
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
