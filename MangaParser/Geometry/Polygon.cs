using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace MangaParser.Graphics
{
    public class Polygon : IPolygon
    {
        private WeakReference<List<Point>> PointsList = new WeakReference<List<Point>>(null);
        private long? area = null;

        public Rectangle BoundingBox { get; private set; }
        public IEnumerable<Point> Points { get; private set; }
        public Point CenterOfGravity { get; private set; }

        private static long crossProduct(int x1, int y1, int x2, int y2)
        {
            return x1 * y2 - x2 * y1;
        }

        private List<Point> getPointsList()
        {
            List<Point> result;
            if (!PointsList.TryGetTarget(out result))
            {
                result = Points.AsParallel().ToList();
                PointsList = new WeakReference<List<Point>>(result);
            }
            return result;
        }

        public long Area
        {
            get
            {
                if (area == null)
                {
                    List<Point> Points = getPointsList();

                    area = 0;
                    for (int i = 0, j = Points.Count - 1; i != Points.Count; j = i++)
                    {
                        area += crossProduct(Points[j].X, Points[j].Y, Points[i].X, Points[i].Y);
                    }

                    area /= 2;
                }

                return area.Value;
            }
        }

        public bool Contains(System.Drawing.Point point)
        {
            bool result = false;
            List<Point> Points = getPointsList();

            //For each segment of the polygon
            for (int i = 0, j = Points.Count - 1; i != Points.Count; j = i++)
            {
                if (((Points[i].Y > point.Y) != (Points[j].Y > point.Y)) &&
                     ((point.X - Points[i].X) <
                        ((Points[j].X - Points[i].X) * (point.Y - Points[i].Y)) /
                        (Points[j].Y - Points[i].Y)))
                    result = !result;
            }
            return result;
        }

        public bool Contains(IPolygon included)
        {
            if (included.BoundingBox.IntersectsWith(BoundingBox))
            {
                return included.Points.All((System.Drawing.Point p) => Contains(p));
            }
            return false;
        }

        public Polygon(Rectangle boundingBox, IEnumerable<Point> points, Point centerOfGravity)
        {
            this.BoundingBox = boundingBox;
            this.Points = points;
            this.CenterOfGravity = centerOfGravity;
        }

        internal static Polygon Rectangle(int p1, int p2, int p3, int p4)
        {
            return new Polygon(new Rectangle(p1, p2, p3, p4),
                new Point[] { new Point(p1, p2),
                              new Point(p1 + p3, p2),
                              new Point(p1 + p3, p2 + p4),
                              new Point(p1, p2 + p4)},
                new Point(p1 + p3 / 2, p3 + p4 / 2)
                );

        }
    }
}
