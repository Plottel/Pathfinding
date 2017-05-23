using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace PathfindingProject
{
    public static class ExtensionMethods
    {
        public static Rectangle GetInflated(this Rectangle rectangle, int horizontalAmount, int verticalAmount)
        {
            Rectangle inflated = new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
            inflated.Inflate(horizontalAmount, verticalAmount);

            return inflated;
        }

        public static Rectangle GetInflated(this Rectangle rectangle, float horizontalAmount, float verticalAmount)
        {
            Rectangle inflated = new Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
            inflated.Inflate(horizontalAmount, verticalAmount);

            return inflated;
        }

        public static Vector2 TopLeft(this Rectangle rectangle)
        {
            return new Vector2(rectangle.Left, rectangle.Top);
        }

        public static Vector2 BottomRight(this Rectangle rectangle)
        {
            return new Vector2(rectangle.Right, rectangle.Bottom);
        }

        public static Vector2 BottomLeft(this Rectangle rectangle)
        {
            return new Vector2(rectangle.Left, rectangle.Bottom);
        }

        public static Vector2 TopRight(this Rectangle rectangle)
        {
            return new Vector2(rectangle.Right, rectangle.Top);
        }

        public static Point IncCol(this Point pt)
        {
            ++pt.X;
            return pt;
        }

        public static Point IncRow(this Point pt)
        {
            ++pt.Y;
            return pt;
        }

        public static int Col (this Point pt)
        {
            return pt.X;
        }

        public static int Row (this Point pt)
        {
            return pt.Y;
        }
    }
}
