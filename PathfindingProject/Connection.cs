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
    public class Connection
    {
        public Cell InnerFrom;
        public Cell InnerTo;
        public OuterGridCell OuterFrom;
        public OuterGridCell OuterTo;

        /// <summary>
        /// Represents the other Connections this Connection is connected to.
        /// This is done internally (i.e. a Connection will only ever be directly connected to
        /// other connections within the same OuterGridCell and NOT along the same edge).
        /// </summary>
        public HashSet<Connection> Connections = new HashSet<Connection>();

        public Connection Matching
        {
            get { return OuterTo.GetMatchingConnection(this); }
        }

        public Connection(Cell innerFrom, Cell innerTo, OuterGridCell outerFrom, OuterGridCell outerTo)
        {
            InnerFrom = innerFrom;
            InnerTo = innerTo;
            OuterFrom = outerFrom;
            OuterTo = outerTo;
        }

        public void Render(SpriteBatch spriteBatch)
        {
            foreach (Connection c in Connections)
            {
                spriteBatch.DrawLine(InnerFrom.RenderMid, c.InnerFrom.RenderMid, Color.White, 1);
            }
        }
    }
}
