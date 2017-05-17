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
    using EdgeConnection = Dictionary<OuterGridCell, Dictionary<Cell, Cell>>;

    public class HierarchicalGrid
    {
        private List<List<OuterGridCell>> _cells = new List<List<OuterGridCell>>();

        //_connections[col][row][connectingToOuterCell][connectingFromInnerCell] = connectingToInnercell
        private List<List<EdgeConnection>> _edgeConnections = new List<List<EdgeConnection>>();

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
                _edgeConnections.Add(new List<EdgeConnection>());

                for (int row = 0; row < Rows; row++)
                {
                    int x = (int)Pos.X + col * _outerCellSize;
                    int y = (int)Pos.Y + row * _outerCellSize;

                    _cells[col].Add(new OuterGridCell(new Vector2(x, y), scale, scale, _innerCellSize));
                    _edgeConnections[col].Add(new EdgeConnection());
                }
            }

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

        public Dictionary<Cell, Cell> CalculateEdgeConnections(List<Cell> fromCells, List<Cell> toCells)
        {
            var connections = new Dictionary<Cell, Cell>();

            for (int i = 0; i < scale; i++)
            {
                if (fromCells[i].Passable && toCells[i].Passable)
                    connections.Add(fromCells[i], toCells[i]);
            }

            return connections;
        }

        public void CalculateWholeGridEdgeConnections()
        {
            for (int col = 0; col < Cols; col++)
            {
                for (int row = 0; row < Rows; row++)
                {
                    _edgeConnections[col][row] = new EdgeConnection();
                    EdgeConnection current = _edgeConnections[col][row];
                    OuterGridCell north = this[col, row - 1];
                    OuterGridCell south = this[col, row + 1];
                    OuterGridCell east = this[col + 1, row];
                    OuterGridCell west = this[col - 1, row];

                    // North -> From[Top] To[Bot]
                    // South -> From[Bot] To[Top]
                    // East -> From[Right] To[Left]
                    // West -> From[Left] To[Right]

                    if (north != null)
                        current.Add(north, CalculateEdgeConnections(this[col, row].GetTopRow(), north.GetBottomRow()));
                    if (south != null)
                        current.Add(south, CalculateEdgeConnections(this[col, row].GetBottomRow(), south.GetTopRow()));
                    if (east != null)
                        current.Add(east, CalculateEdgeConnections(this[col, row].GetRightCol(), east.GetLeftCol()));
                    if (west != null)
                        current.Add(west, CalculateEdgeConnections(this[col, row].GetLeftCol(), west.GetRightCol()));
                }
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
                    // Render the connections.
                    foreach (Dictionary<Cell, Cell> connections in _edgeConnections[col][row].Values)
                    {
                        foreach (KeyValuePair<Cell, Cell> connection in connections)
                            spriteBatch.DrawLine(connection.Key.RenderMid, connection.Value.RenderMid, Color.Blue, 3);
                    }
                }
            }
        }
    }
}
