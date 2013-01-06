using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaParser.Graphics.Raster
{
    public class Blob
    {
        public struct Segment: ICloneable, IComparable<Segment>
        {
            public int from;
            public int to;

            public int Width { get { return to - from; } }

            public Segment (int from, int to)
            {
                this.from = from;
                this.to = to;
            }

            public object Clone()
            {
                return new Segment(from, to);
            }

            public int CompareTo(Segment other)
            {
                if (other.from == this.from) return other.to - this.to;
                return other.from - this.from;
            }
        }

        List<Tuple<int, Segment>> content = new List<Tuple<int,Segment>>();
        public IEnumerable<Tuple<int,Segment>> Regions { get { return content; } }
        private Rectangle? boundingBox;
        bool sorted = true;
        private List<Point> convexHull = null;

        public Rectangle BoundingBox { get {
            if (boundingBox == null)
            {
                int minX = content[0].Item2.from,
                    maxX = content[0].Item2.to,
                    minY = content[0].Item1,
                    maxY = content[0].Item1 + 1;

                foreach (var segment in content)
                {
                    minX = Math.Min(minX, segment.Item2.from);
                    maxX = Math.Max(maxX, segment.Item2.to);
                    minY = Math.Min(minY, segment.Item1);
                    maxY = Math.Max(maxY, segment.Item1 + 1);
                }

                boundingBox = Rectangle.FromLTRB(minX, minY, maxX, maxY);
            }
            return boundingBox.Value; 
        } }


        private void updateDerived(Tuple<int, Segment> addedSegment)
        {
            boundingBox = null;
            convexHull = null;
            sorted = content.Count == 0 || 
                (addedSegment != null && sorted && 
                    ((content.Last().Item1 < addedSegment.Item1) || 
                     (content.Last().Item1 == addedSegment.Item1 && content.Last().Item2.to < addedSegment.Item2.from)));
        }

        public Blob()
        {
            updateDerived(null);
        }

        public Blob(int y, Segment segment)
        {
            AddRegion(y, segment);
        }

        public void AddRegion(int y, Segment segment) {
            var added = new Tuple<int,Segment>(y, segment);
            content.Add(added);
            updateDerived(added);
        }

        public void MergeWith(Blob other)
        {
            sorted = false;

            var interSeg = new HashSet<Tuple<int, Segment>>(this.content);
            foreach (var s in other.content)
            {
                if (interSeg.Contains(s)) throw new Exception("Blah");
            }

            content.AddRange(other.content);
            updateDerived(null);
        }

        private void sort()
        {
            if (sorted) return;

            content = (from Tuple<int,Segment> t in content
                      orderby t.Item1 
                      select t).AsParallel().ToList();
            sorted = true;
        }

        /// <summary>
        /// Returns the extremities of each row of this blob
        /// </summary>
        private IEnumerable<Point> toScanExtremities()
        {
            if (content.Count == 0) yield break;

            sort();

            int y = content[0].Item1 - 1, x = -1;

            foreach (var segment in content)
            {
                if (segment.Item1 != y) {
                    //Yield the end of the previous segment
                    if (x != -1)
                        yield return new Point(x, y);
                        
                    y = segment.Item1;
                    //Yield the beginning of the new segment
                    yield return new Point(segment.Item2.from, y);
                }
                x = segment.Item2.to;
            }
            //Yield the end of the last segment
            yield return new Point(x, y);
        }

        public int ComputeArea()
        {
            int result = 0;
            foreach (var segment in this.content)
            {
                result += segment.Item2.Width;
            }
            return result;
        }

        /// <summary>
        /// Compute the cross product of vector (origin -- a) with vector (origin -- b)
        /// </summary>
        private int vectorProduct(Point origin, Point a, Point b)
        {
            return (a.X - origin.X) * (b.Y - origin.Y) - (a.Y - origin.Y) * (b.X - origin.X);
        }

        public List<Point> ComputeConvexHull()
        {
            if (convexHull != null) return convexHull;

            if (content.Count == 0) return new List<Point>();

            List<Point> points = toScanExtremities().AsParallel().ToList();
            List<Point> Upper = new List<Point>();
            List<Point> Lower = new List<Point>();

            for (int i = 0; i < points.Count; i++)
            {
                Point p = points[i];
                //Invalidate points of the lower hull that are encompassed by (Lower[-1], p)
                while (Lower.Count >= 2 && vectorProduct(Lower[Lower.Count - 2], Lower[Lower.Count - 1], p) <= 0)
                    Lower.RemoveAt(Lower.Count - 1);
                //Add p to the convex hull
                Lower.Add(p);
            }

            for (int i = points.Count - 1; i >= 0; i--)
            {
                Point p = points[i];
                //Invalidate points of the upper hull that are encompassed by (Lower[-1], p)
                while (Upper.Count >= 2 && vectorProduct(Upper[Upper.Count - 2], Upper[Upper.Count - 1], p) <= 0)
                    Upper.RemoveAt(Upper.Count - 1);
                //Add p to the convex hull
                Upper.Add(p);
            }

            Lower.RemoveAt(Lower.Count - 1);
            Upper.RemoveAt(Upper.Count - 1);
            return Lower.Concat(Upper).AsParallel().ToList();
        }

        public Boolean ConvexInclude(Point point)
        {
            if (point.X < BoundingBox.Left || point.X > BoundingBox.Right ||
                point.Y < BoundingBox.Top || point.Y > BoundingBox.Bottom) return false; 

            List<Point> hull = ComputeConvexHull();

            for (int i = 0; i < hull.Count - 1; i++)
            {
                if (vectorProduct(hull[i], hull[i+1], point) <= 0) return false;
            }

            return true;
        }

        private bool convexInclude(Blob other)
        {
            bool result = false;
            if (BoundingBox.Contains(other.BoundingBox))
            {
                List<Point> otherhull = other.ComputeConvexHull();
                result =  otherhull.All((Point p) => ConvexInclude(p));
            }
            return result;

        }
    }
}
