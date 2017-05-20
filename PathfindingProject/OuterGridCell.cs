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
        public List<Connection> Connections;
        public List<OuterGridCell> Neighbours = new List<OuterGridCell>();

        public OuterGridCell(Vector2 pos, int cols, int rows, int cellSize) : base(pos, cols, rows, cellSize)
        {
            _edges = new Dictionary<Dir, List<Cell>>();
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
        }

        public Connection GetMatchingConnection(Connection connection)
        {
            return Connections.Find(c => c.InnerFrom == connection.InnerTo && c.InnerTo == connection.InnerFrom);
        }

        public List<Cell> GetEdge(Dir dir)
        {
            return _edges[dir];
        }

        private List<Cell> GetReachableEdgeCells(Connection calculatingFor)
        {
            var reachableEdgeCells = new List<Cell>();
            var open = new List<Cell>();
            var closed = new List<Cell>();
            Cell current = null; 
            var source = calculatingFor.InnerFrom;

            open.Add(source);

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

        public void CalculateInternalConnections()
        {
            // foreach Connection C in connections
            foreach (Connection connection in Connections)
            {
                // Reset connections to prevent duplicates.
                connection.Connections = new HashSet<Connection>();

                // Run Floodfill's and see which cells it reaches
                var reachableCells = GetReachableEdgeCells(connection);

                // foreach Cell in the FloodFill's result
                foreach (Cell cell in reachableCells)
                {
                    // Find corresponding connection to cell.
                    Connection connectionAtCell = Connections.Find(c => c.InnerFrom == cell);

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
                //spriteBatch.DrawLine(c.InnerFrom.RenderMid, c.InnerTo.RenderMid, Color.Blue, 3);

               // spriteBatch.DrawLine(c.OuterFrom.RenderMid, c.OuterTo.RenderMid, Color.Orange, 4);
                //spriteBatch.DrawPoint(c.InnerFrom.RenderMid, Color.Blue, 5);
               // c.Render(spriteBatch);
            }
        }
    }
}
