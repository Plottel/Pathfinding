using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended;


namespace PathfindingProject
{
    public class HierarchicalGrid
    {
        private List<List<OuterGridCell>> _cells = new List<List<OuterGridCell>>();

        public bool ShowGrid = true;
        public Vector2 Pos;

        private int _outerCellSize;
        private int _innerCellSize;
        private int scale;
        private int _cols;
        private int _rows;

        public int Cols
        {
            get { return _cols; }
        }

        public int Rows
        {
            get { return _rows; }
        }

        public int Width
        {
            get { return Cols * _outerCellSize - 1; }
        }

        /// <summary>
        /// The height of the grid.
        /// -1 so the absolute edge doesn't register next cell and out of bounds.
        /// </summary>
        public int Height
        {
            get { return Rows * _outerCellSize - 1; }
        }

        public OuterGridCell this[int col, int row]
        {
            get
            {
                if (col < 0 || col > Cols - 1 || row < 0 || row > Rows - 1)
                    return null;
                else
                    return _cells[col][row];
            }

            set { _cells[col][row] = value; }
        }

        public void AddOuterCell(OuterGridCell cell, ICollection<OuterGridCell> list)
        {
            if (cell != null)
                list.Add(cell);
        }

        public OuterGridCell this[Point cellIndex]
        {
            get { return this[cellIndex.Col(), cellIndex.Row()]; }
            set { this[cellIndex.Col(), cellIndex.Row()] = value; }
        }

        public HierarchicalGrid(Vector2 pos, int outerSize, int scale, int cols, int rows)
        {
            Pos = pos;
            _outerCellSize = outerSize;
            _innerCellSize = outerSize / scale;
            this.scale = scale;
            _rows = rows;
            _cols = cols;

            for (int col = 0; col < Cols; col++)
            {
                _cells.Add(new List<OuterGridCell>());

                for (int row = 0; row < Rows; row++)
                {
                    int x = (int)Pos.X + col * _outerCellSize;
                    int y = (int)Pos.Y + row * _outerCellSize;

                    _cells[col].Add(new OuterGridCell(new Vector2(x, y), scale, scale, _innerCellSize));
                }
            }

            SetupNeighbours();
            CalculateWholeGridConnections();
        }

        public OuterGridCell OuterCellAt(Vector2 pos)
        {
            int col = (int)Math.Floor((pos.X - Pos.X) / _outerCellSize);
            int row = (int)Math.Floor((pos.Y - Pos.Y) / _outerCellSize);

            return this[col, row];
        }

        public Point OuterCellIndexAt(Vector2 pos)
        {
            int col = (int)Math.Floor((pos.X - Pos.X) / _outerCellSize);
            int row = (int)Math.Floor((pos.Y - Pos.Y) / _outerCellSize);

            return new Point(col, row);
        }

        public Cell InnerCellAt(Vector2 pos)
        {
            return OuterCellAt(pos).CellAt(pos);
        }

        private void SetupNeighbours()
        {
            for (int col = 0; col < Cols; col++)
            {
                for (int row = 0; row < Rows; row++)
                {
                    var neighbours = new List<OuterGridCell>();
                    AddOuterCell(this[col - 1, row], neighbours);
                    AddOuterCell(this[col + 1, row], neighbours);
                    AddOuterCell(this[col, row - 1], neighbours);
                    AddOuterCell(this[col, row + 1], neighbours);

                    this[col, row].Neighbours = neighbours;
                }
            }
        }

        public List<Connection> CalculateEdgeConnections(List<Cell> fromCells, List<Cell> toCells, OuterGridCell outerFrom, OuterGridCell outerTo)
        {
            var connections = new List<Connection>();

            for (int i = 0; i < scale; i++)
            {
                if (fromCells[i].Passable && toCells[i].Passable)
                    connections.Add(new Connection(fromCells[i], toCells[i], outerFrom, outerTo));
            }

            return connections;
        }

        public void UpdateGridConnectionsForNeighbours(OuterGridCell current)
        {
            Point cellIndex = OuterCellIndexAt(current.Mid);

            for (int col = cellIndex.Col() -1; col <= cellIndex.Col() + 1; col++)
            {
                for (int row = cellIndex.Row() - 1; row <= cellIndex.Row() + 1; row++)
                {
                    CalculateGridConnectionsForCell(this[col, row], col, row);
                }
            }
        }

        public void CalculateGridConnectionsForCell(OuterGridCell current, int col, int row)
        {
            if (current == null)
                return;

            // Clear connections to prevent duplicates.
            current.Connections = new List<Connection>();

            // Orthogonal neighbours.
            OuterGridCell north = this[col, row - 1];
            OuterGridCell south = this[col, row + 1];
            OuterGridCell east = this[col + 1, row];
            OuterGridCell west = this[col - 1, row];

            // Diagonal neighbours.
            OuterGridCell northWest = this[col - 1, row - 1];
            OuterGridCell northEast = this[col + 1, row - 1];
            OuterGridCell southWest = this[col - 1, row + 1];
            OuterGridCell southEast = this[col + 1, row + 1];

            // North -> From[Top] To[Bot]
            // South -> From[Bot] To[Top]
            // East -> From[Right] To[Left]
            // West -> From[Left] To[Right]

            // Calculate orthogonal edge connections
            if (north != null)
                current.Connections.AddRange(CalculateEdgeConnections(this[col, row].GetEdge(Dir.N), north.GetEdge(Dir.S), current, north));
            if (south != null)
                current.Connections.AddRange(CalculateEdgeConnections(this[col, row].GetEdge(Dir.S), south.GetEdge(Dir.N), current, south));
            if (east != null)
                current.Connections.AddRange(CalculateEdgeConnections(this[col, row].GetEdge(Dir.E), east.GetEdge(Dir.W), current, east));
            if (west != null)
                current.Connections.AddRange(CalculateEdgeConnections(this[col, row].GetEdge(Dir.W), west.GetEdge(Dir.E), current, west));

            // Calculate diagonal edge connections
            if (CanConnectDiagonally(current, west, north, northWest)) // North West
                current.Connections.Add(new Connection(current.GetCorner(Dir.NW), northWest.GetCorner(Dir.SE), current, northWest));
            if (CanConnectDiagonally(east, current, northEast, north)) // North East
                current.Connections.Add(new Connection(current.GetCorner(Dir.NE), northEast.GetCorner(Dir.SW), current, northEast));
            if (CanConnectDiagonally(south, southWest, current, west)) // South West
                current.Connections.Add(new Connection(current.GetCorner(Dir.SW), southWest.GetCorner(Dir.NE), current, southWest));
            if (CanConnectDiagonally(southEast, south, east, current)) // South East
                current.Connections.Add(new Connection(current.GetCorner(Dir.SE), southEast.GetCorner(Dir.NW), current, southEast));

            // Calculate internal edge connections
            current.CalculateInternalConnections();
        }

        public void CalculateWholeGridConnections()
        {
            for (int col = 0; col < Cols; col++)
            {
                for (int row = 0; row < Rows; row++)
                {
                    CalculateGridConnectionsForCell(_cells[col][row], col, row);                    
                }
            }
        }

        private bool CanConnectDiagonally(OuterGridCell nw, OuterGridCell ne, OuterGridCell sw, OuterGridCell se)
        {
            if (nw == null || ne == null || sw == null || se == null)
                return false;

            return nw.GetCorner(Dir.NW).Passable && 
                ne.GetCorner(Dir.NE).Passable && 
                sw.GetCorner(Dir.SW).Passable && 
                se.GetCorner(Dir.SE).Passable;
        }

        public List<Cell> GetPathFromTo(Vector2 fromPos, Vector2 toPos)
        {
            var reachableStartingConnections = OuterCellAt(fromPos).GetReachableConnections(fromPos);
            var reachableEndingConnections = OuterCellAt(toPos).GetReachableConnections(toPos);

            // No connections to exit current cell, therefore no path.
            if (reachableStartingConnections.Count == 0)
                return new List<Cell>();

            // No connections to exit finishing cell, therefore no path.
            if (reachableEndingConnections.Count == 0)
                return new List<Cell>();

            // Start with connection closest to finish position.
            reachableStartingConnections = reachableStartingConnections.OrderBy(c => Vector2.Distance(toPos, c.InnerFrom.Mid)).ToList();

            var getPathFromConnection = reachableStartingConnections[0];
            var getPathToConnection = reachableEndingConnections[0];

            var open = new List<Connection>();
            var closed = new List<Connection>();
            Connection current = getPathFromConnection.Matching;
            var source = current;
            var parents = new Dictionary<Connection, Connection>();
            var scores = new Dictionary<Connection, float>();

            open.Add(current);

            parents.Add(getPathFromConnection, null);
            parents.Add(current, getPathFromConnection);
            scores.Add(current, 0);
            scores.Add(getPathFromConnection, 0);

            bool searchComplete = false;

            bool debug = true;

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
                    foreach (Connection c in closed)
                    {
                        Game1.Instance.spriteBatch.FillRectangle(c.InnerFrom.RenderRect, Color.LightSteelBlue);
                        Game1.Instance.spriteBatch.FillRectangle(c.InnerTo.RenderRect, Color.LightSteelBlue);
                    }

                    // Render open list cream
                    foreach (Connection c in open)
                    {
                        Game1.Instance.spriteBatch.FillRectangle(c.InnerFrom.RenderRect, Color.BlanchedAlmond);
                        Game1.Instance.spriteBatch.FillRectangle(c.InnerTo.RenderRect, Color.BlanchedAlmond);
                    }

                    // Render source red
                    Game1.Instance.spriteBatch.FillRectangle(source.InnerFrom.RenderRect, Color.LightSteelBlue);
                    Game1.Instance.spriteBatch.FillRectangle(source.InnerTo.RenderRect, Color.LightSteelBlue);

                    // Render current purple
                    Game1.Instance.spriteBatch.FillRectangle(current.InnerFrom.RenderRect, Color.LightSteelBlue);
                    Game1.Instance.spriteBatch.FillRectangle(current.InnerTo.RenderRect, Color.LightSteelBlue);

                    // Render target green
                    Game1.Instance.spriteBatch.FillRectangle(getPathToConnection.InnerFrom.RenderRect, Color.YellowGreen);
                    Game1.Instance.spriteBatch.FillRectangle(getPathToConnection.InnerTo.RenderRect, Color.YellowGreen);

                    // Render path to current orange.
                    Connection curRender = current;

                    while (parents[curRender] != null)
                    {
                        Game1.Instance.spriteBatch.DrawLine(curRender.InnerTo.RenderMid, parents[curRender].InnerFrom.RenderMid, Color.Orange, 6);
                        curRender = parents[curRender];
                    }

                    // Render scores
                    foreach (Connection c in open)
                        Game1.Instance.spriteBatch.DrawString(Game1.Instance.smallFont, Math.Floor(scores[c]).ToString(), c.InnerFrom.RenderMid, Color.Black);

                    Game1.Instance.spriteBatch.End();
                    Game1.Instance.GraphicsDevice.Present();

                    System.Threading.Thread.Sleep(100);


                    #endregion Calculate Path Visual
                }

                foreach (Connection connection in current.Connections)
                {
                    if (!closed.Contains(connection) && !open.Contains(connection))
                    {
                        parents.Add(connection, current); // Parent internal.
                        parents.Add(connection.Matching, connection); // Parent external (matching)

                        scores.Add(connection, Vector2.Distance(connection.InnerTo.Mid, toPos));
                        scores.Add(connection.Matching, scores[connection]);

                        open.Add(connection); // Add internal
                        open.Add(connection.Matching); // Add external (matching)

                        if (reachableEndingConnections.Contains(connection.Matching))
                        {
                            searchComplete = true;
                            current = connection.Matching;
                            break;
                        }
                    }                
                }

                if (searchComplete)
                    break;

                open.Remove(current);
                open.Remove(current.Matching);
                closed.Add(current.Matching);
                closed.Add(current);

                open = open.OrderBy(c => scores[c]).ToList();
                current = open[0].Matching;
                open.Add(current);
            }

            var path = new List<Cell>();
            path.Insert(0, InnerCellAt(toPos));
            path.Insert(0, current.InnerFrom);

            while (parents[current] != null)
            {
                path.Insert(0, parents[current].InnerFrom);
                current = parents[current];
            }

            path.Insert(0, InnerCellAt(fromPos));

            return path;
        }

        public void Render(SpriteBatch spriteBatch)
        {
            for (int col = 0; col < Cols; col++)
            {
                for (int row = 0; row < Rows; row++)
                {
                    this[col, row].Render(spriteBatch);

                    if (ShowGrid)
                        spriteBatch.DrawRectangle(this[col, row].RenderRect, Color.DarkSlateGray, 5);
                }
            }

            // Render connections
            for (int col = 0; col < Cols; col++)
            {
                for (int row = 0; row < Rows; row++)
                {
                    _cells[col][row].RenderConnections(spriteBatch);                    
                }
            }
        }
    }
}
