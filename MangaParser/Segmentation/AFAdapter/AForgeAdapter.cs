using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaParser.Graphics
{
    public static class AForgeAdapter
    {
        public static System.Drawing.Point ToPoint(this AForge.Point point)
        {
            return new System.Drawing.Point((int)(point.X), (int)(point.Y));
        }

        public static System.Drawing.Point ToPoint(this AForge.IntPoint point)
        {
            return new System.Drawing.Point(point.X, point.Y);
        }

        public static Polygon ToPolygon(this AForge.Imaging.Blob blob, AForge.Imaging.BlobCounterBase counter) {
            return blob.ToPolygon(counter, new AForge.Math.Geometry.GrahamConvexHull());
        }

        public static Polygon ToPolygon(this AForge.Imaging.Blob blob, AForge.Imaging.BlobCounterBase counter, AForge.Math.Geometry.IConvexHullAlgorithm hullAlgorithm)
        {
            List<AForge.IntPoint> leftPoints, rightPoints, edgePoints;

            counter.GetBlobsLeftAndRightEdges(blob, out leftPoints, out rightPoints);
            edgePoints = leftPoints.Union(rightPoints).ToList();

            var hull = hullAlgorithm.FindHull(edgePoints); 

            // blob's convex hull
            return new Polygon(blob.Rectangle, from point in hull select point.ToPoint(),
                new System.Drawing.Point((int)(blob.CenterOfGravity.X), 
                                         (int)(blob.CenterOfGravity.Y)));
        }
    }
}
