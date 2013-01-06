using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaParser.Geometry
{
    static class ReadingUtilities
    {
        /// <summary>
        /// Determines whether two 1D segments intersect.
        /// </summary>
        /// <param name="a1">The beginning of the first segment</param>
        /// <param name="b1">The end of the first segment</param>
        /// <param name="a2">The begining of the second segment</param>
        /// <param name="b2">The end of the second segment</param>
        /// <returns>Returns whether the two segments intersect.</returns>
        public static bool intersect(int a1, int b1, int a2, int b2)
        {
            return (a1 < b2 && a2 < b1);
        }

        /// <summary>
        /// Determines whether two rectangles share similar Y coordinates.
        /// </summary>
        /// <param name="r1">The reference rectangle</param>
        /// <param name="r2">The test rectangle</param>
        /// <returns>Returns true if the height of both rectangles share some common Y coordinates</returns>
        public static bool HeightIntersect(this System.Drawing.Rectangle r1, System.Drawing.Rectangle r2)
        {
            return intersect(r1.Top, r1.Bottom, r2.Top, r2.Bottom);
        }

        /// <summary>
        /// Compute the area of the rectangle. This is the same as multiplying the height of the rectangle by its width.
        /// </summary>
        /// <param name="r1">The rectangle whose area must be computed.</param>
        /// <returns>The area of the given rectangle</returns>
        public static Decimal Area(this System.Drawing.Rectangle r1)
        {
            return r1.Width * r1.Height;
        }

        /// <summary>
        /// Computes the overlapping area of two rectangles.
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns>The total area common to the two rectangles</returns>
        public static Decimal Overlap(this System.Drawing.Rectangle r1, System.Drawing.Rectangle r2)
        {
            return System.Drawing.Rectangle.Intersect(r1, r2).Area();
        }


        /// <summary>
        /// Computes the overlapping area of two rectangles, when projected on the Y axis.
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns>The length common to the two rectangles on the Y axis</returns>
        public static int YOverlap(this System.Drawing.Rectangle r1, System.Drawing.Rectangle r2)
        {
            return Math.Max(0, (Math.Min(r1.Bottom, r2.Bottom) - Math.Max(r1.Top, r2.Top)));
        }

        /// <summary>
        /// Gets a rectangular bounding box that encompasses the specified rectangle after the specified transformation has
        /// been applied.
        /// </summary>
        /// <param name="rect">The rectangle whose transformation is considered</param>
        /// <param name="transform">The transformation considered</param>
        /// <returns>A bounding box that encompasses the specified the specified rectangle after the specified transformation
        /// has been applied</returns>
        public static Rectangle Transform(this Rectangle rect, Matrix transform)
        {
            //Transform each corner of the rectangle
            Point[] corners = {
                new Point(rect.Left, rect.Top),
                new Point(rect.Right, rect.Top),
                new Point(rect.Left, rect.Bottom),
                new Point(rect.Right, rect.Bottom) };

            transform.TransformPoints(corners);

            // Fit a bounding box on the transformed rectangle
            int x1 = (from c in corners select c.X).Min();
            int x2 = (from c in corners select c.X).Max();
            int y1 = (from c in corners select c.Y).Min();
            int y2 = (from c in corners select c.Y).Max();

            return new Rectangle(x1, y1, x2 - x1, y2 - y1);
        }
    }
}
