using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using MangaParser.Graphics;

namespace MangaParserTest
{
    class RectanglePoly: IPolygon
    {
        Rectangle r;

        public static implicit operator Rectangle(RectanglePoly poly) {
            return poly.BoundingBox;
        }

        public RectanglePoly(int X1, int Y1, int X2, int Y2)
        {
            r = new Rectangle(X1, Y1, X2 - X1, Y2 - Y1);
        }

        public long Area
        {
            get { return r.Width * r.Width; }
        }

        public System.Drawing.Rectangle BoundingBox
        {
            get { return r; }
        }

        public System.Drawing.Point CenterOfGravity
        {
            get { return new Point(r.X + r.Width / 2, r.Y + r.Height / 2); }
        }

        public bool Contains(IPolygon included)
        { throw new NotImplementedException(); }

        public bool Contains(System.Drawing.Point p)
        { throw new NotImplementedException(); }

        public IEnumerable<System.Drawing.Point> Points
        {
            get
            {
                yield return r.Location;
                yield return new Point(r.Left, r.Bottom);
                yield return new Point(r.Right, r.Top);
                yield return new Point(r.Right, r.Bottom);
            }
        }
    }
}
