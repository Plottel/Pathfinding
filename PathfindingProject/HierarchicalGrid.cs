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
            CalculateWholeGridEdgeConnections();
        }

        public OuterGridCell OuterCellAt(Vector2 pos)
        {
            int col = (int)Math.Floor((pos.X - Pos.X) / _outerCellSize);
            int row = (int)Math.Floor((pos.Y - Pos.Y) / _outerCellSize);

            return this[col, row];
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

        public void CalculateWholeGridEdgeConnections()
        {
            for (int col = 0; col < Cols; col++)
            {
                for (int row = 0; row < Rows; row++)
                {
                    OuterGridCell current = _cells[col][row];

                    // Clear connections to prevent duplicates.
                    current.Connections = new List<Connection>();

                    OuterGridCell north = this[col, row - 1];
                    OuterGridCell south = this[col, row + 1];
                    OuterGridCell east = this[col + 1, row];
                    OuterGridCell west = this[col - 1, row];

                    // North -> From[Top] To[Bot]
                    // South -> From[Bot] To[Top]
                    // East -> From[Right] To[Left]
                    // West -> From[Left] To[Right]

                    if (north != null)
                        current.Connections.AddRange(CalculateEdgeConnections(this[col, row].GetEdge(Dir.N), north.GetEdge(Dir.S), current, north));
                    if (south != null)
                        current.Connections.AddRange(CalculateEdgeConnections(this[col, row].GetEdge(Dir.S), south.GetEdge(Dir.N), current, south));
                    if (east != null)
                        current.Connections.AddRange(CalculateEdgeConnections(this[col, row].GetEdge(Dir.E), east.GetEdge(Dir.W), current, east));
                    if (west != null)
                        current.Connections.AddRange(CalculateEdgeConnections(this[col, row].GetEdge(Dir.W), west.GetEdge(Dir.E), current, west));

                    current.CalculateInternalConnections();
                }
            }
        }

        public struct ConnectionMap
        {
            public OuterGridCell outer;
            public List<Connection> connections;

            public ConnectionMap(OuterGridCell o)
            {
                outer = o;
                connections = o.Connections;
            }
        }

        public void GetPathFromTo(Connection getPathFromConnection, Connection getPathToConnection)
        { 
            var open = new Stack<ConnectionMap>();
            var closed = new Dictionary<OuterGridCell, List<Connection>();
            Connection current = null;

            var parents = new Dictionary<Connection, Connection>();
            var scores = new Dictionary<Connection, float>();

            open.Push(new ConnectionMap(getPathFromConnection.OuterFrom));
            current = open.Peek().connections[0];

            closed.Add(open.Peek().outer, new List<Connection>());

            parents.Add(current, null);
            scores.Add(current, 0);

            bool searchComplete = false;

            while (!searchComplete)
            {
                var curOGC = open.Peek().outer;

                foreach (Connection connection in current.Connections)
                {
                    if (!closed[curOGC].Contains(connection))
                    {
                        parents.Add(connection, current);
                        // Score = Euclidian between connections + Euclidian from new to Target
                        scores.Add(connection, scores[current] +
                            Vector2.Distance(connection.InnerTo.Mid, getPathToConnection.InnerTo.Mid));

                        closed[curOGC].Add(connection);
                    }                    
                }

                Connection lowestConnection = null;
                float lowestScore = float.MaxValue;

                foreach (Connection c in open.Peek().connections)
                {
                    float score = scores[c];
                    if (score < lowestScore)
                    {
                        lowestConnection = c;
                        lowestScore = score;
                    }
                }

                current = lowestConnection;
                open.Peek().connections.Remove(current);

                // If current OGC has no unexplored connections left - pop from Open and add to Closed
                if (open.Peek().connections.Count == 0)
                    closed.Add(open.Pop().outer);


            }

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
