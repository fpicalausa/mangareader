using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MangaParser.Geometry;

namespace MangaParser.Graphics
{
    public class TransformedPolygon: IPolygon
    {
        IPolygon basePolygon;
        Matrix transform;
        Rectangle? bounds = null;
        Point? centerOfGravity = null;
        Point[] points = null;

        public IPolygon BasePolygon { get { return basePolygon; } }

        public long Area
        {
            get { return basePolygon.Area;  }
        }

        public Rectangle BoundingBox
        {
            get
            {
                if (!bounds.HasValue)
                {
                    //Note: The following is only valid as long as 
                    //      the transformation involves rotations at angles of
                    //      multiples of 90 degrees. Arbitrary rotations need
                    //      to be handled based on the transformation of every 
                    //      point of the base polygon.
                    bounds = basePolygon.BoundingBox.Transform(transform);
                }

                return bounds.Value;
            }
        }

        public Point CenterOfGravity
        {
            get
            {
                if (!centerOfGravity.HasValue) {
                    Point[] basecog = new Point[] { basePolygon.CenterOfGravity };
                    transform.TransformPoints(basecog);
                    centerOfGravity = basecog[0];
                }

                return centerOfGravity.Value;
            }
        }

        public bool Contains(IPolygon included)
        {
            //Untransform both polygons
            TransformedPolygon tsf = included as TransformedPolygon;
            if (tsf != null) included = tsf.basePolygon;
            return basePolygon.Contains(included);
        }

        public bool Contains(Point point)
        {
            return basePolygon.Contains(point);
        }

        public IEnumerable<Point> Points
        {
            get
            {
                if (points == null)
                {
                    List<Point> basePoints = basePolygon.Points.ToList();
                    points = new Point[basePoints.Count];

                    for (int i = 0; i < basePoints.Count; i++)
                    {
                        points[i] = basePoints[i];
                    }

                    transform.TransformPoints(points);
                }

                return points;
            }
        }

        /// <summary>
        /// Transform a given polygon with a given transformation matrix.
        /// </summary>
        /// <param name="basePolygon">The original polygon to be transformed.</param>
        /// <param name="transform">The transformation matrix to be applied to each point of the polygon.</param>
        public TransformedPolygon(IPolygon basePolygon, Matrix transform)
        {
            this.basePolygon = basePolygon;
            this.transform = transform;
        }
    }
}
