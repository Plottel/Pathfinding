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
    public class OuterGridCell : Grid
    {
        private Dictionary<Dir, List<Cell>> _edges;
        private Dictionary<Dir, Cell> _corners;
        public List<Connection> Connections;
        public List<OuterGridCell> Neighbours = new List<OuterGridCell>();

        public OuterGridCell(Vector2 pos, int cols, int rows, int cellSize) : base(pos, cols, rows, cellSize, false)
        {
            _edges = new Dictionary<Dir, List<Cell>>();
            _corners = new Dictionary<Dir, Cell>();
            Connections = new List<Connection>();

            // Get bottom row -> South edge
            var bottomRow = new List<Cell>();

            foreach (List<Cell> col in _cells)
                bottomRow.Add(col.Last());

            // Get top row -> North edge
            var topRow = new List<Cell>();

            foreach (List<Cell> col in _cells)
                topRow.Add(col[0]);

            // Add N S E W edges to dictionary
            _edges.Add(Dir.N, topRow);
            _edges.Add(Dir.S, bottomRow);
            _edges.Add(Dir.W, _cells[0]);
            _edges.Add(Dir.E, _cells[Cols - 1]);

            // Add corners to dictionary
            _corners.Add(Dir.NW, _cells[0][0]);
            _corners.Add(Dir.NE, _cells[Cols - 1][0]);
            _corners.Add(Dir.SW, _cells[0][Rows - 1]);
            _corners.Add(Dir.SE, _cells[Cols - 1][Rows - 1]);
        }

        public Connection GetMatchingConnection(Connection connection)
        {
            return Connections.Find(c => c.InnerFrom == connection.InnerTo && c.InnerTo == connection.InnerFrom);
        }

        public List<Cell> GetEdge(Dir dir)
        {
            return _edges[dir];
        }

        public Cell GetCorner(Dir dir)
        {
            return _corners[dir];
        }

        private List<Cell> GetReachableEdgeCells(Cell startingCell)
        {
            var reachableEdgeCells = new List<Cell>();
            var open = new List<Cell>();
            var closed = new List<Cell>();
            Cell current = null; 
            var source = startingCell;

            open.Add(source);
            reachableEdgeCells.Add(source);

            bool searchComplete = false;

            while (!searchComplete)
            {
                current = open[0];

                foreach (Cell cell in current.Neighbours)
                {
                    if (cell.Passable && !closed.Contains(cell) && !open.Contains(cell))
                    {
                        open.Add(cell);

                        if (CellIsEdge(cell))
                            reachableEdgeCells.Add(cell);
                    }                        
                }

                open.Remove(current);
                closed.Add(current);

                searchComplete = open.Count == 0;
            }


            return reachableEdgeCells;
        }

        public List<Connection> GetReachableConnections(Cell startingCell)
        {
            var reachableConnections = new List<Connection>();

            // foreach Cell in the FloodFill's result
            foreach (Cell cell in GetReachableEdgeCells(startingCell))
                // Find corresponding connection(s) to cell.
                reachableConnections.AddRange(Connections.FindAll(c => c.InnerFrom == cell));

            return reachableConnections;
        }

        public List<Connection> GetReachableConnections(Vector2 pos)
        {
            return GetReachableConnections(CellAt(pos));
        }

        public void CalculateInternalConnections()
        {
            // foreach Connection C in connections
            foreach (Connection connection in Connections)
            {
                // Reset connections to prevent duplicates.
                connection.Connections = new HashSet<Connection>();

                foreach (Connection connectionAtCell in GetReachableConnections(connection.InnerFrom))
                {
                    if (connectionAtCell != null)
                    {
                        if (connection.OuterTo != connectionAtCell.OuterTo)
                            connection.Connections.Add(connectionAtCell);
                    }
                }                                      
            }                    
        }

        public void RenderConnections(SpriteBatch spriteBatch)
        {
            // Render the connections.
            foreach (Connection c in Connections)
            {
                // Render actual bridge connections.
                spriteBatch.DrawLine(c.InnerFrom.RenderMid, c.InnerTo.RenderMid, Color.Blue, 3);

                // Render high-level OuterGridCell connections.
                spriteBatch.DrawLine(c.OuterFrom.RenderMid, c.OuterTo.RenderMid, Color.Orange, 4);

                // Render lines between all connected connections.
                c.Render(spriteBatch);
            }
        }
    }
}
