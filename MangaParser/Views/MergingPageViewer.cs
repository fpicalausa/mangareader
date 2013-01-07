using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MangaParser.Graphics;

namespace MangaParser.Reader
{
    using System.Drawing.Drawing2D;
    using Merges = Tuple<Rectangle, List<Rectangle>>;

    public abstract class MergingPageViewer: IPageViewer
    {
        private int maxWidth;
        private int maxHeight;
        private Size transformedSize;
        public Matrix viewTransformation;

        public MergingPageViewer(int maxWidth, int maxHeight)
        {
            this.maxWidth = maxWidth;
            this.maxHeight = maxHeight;
            this.transformedSize = new Size(maxWidth, maxHeight);
            this.viewTransformation = new Matrix();
        }
        /// <summary>
        /// Indicates whether two rectangles must be merged together. This relation must be symmetric in r1,r2.
        /// </summary>
        /// <param name="r1">The first rectangle</param>
        /// <param name="r2">The second rectangle</param>
        /// <returns>Returns true if r1 and r2 must be merged</returns>
        protected abstract bool Overlap(Rectangle rectangle1, Rectangle rectangle2);

        /// <summary>
        /// Merge a reference cell with all the other cells that follows it in the cells list
        /// </summary>
        /// <param name="cells">The list of all current cells</param>
        /// <param name="refindex">In index in the cells list indicating the cell that needs to be merged</param>
        /// <param name="merged">A boolean array that indicates which cells have already been merged. This array
        ///                      will be updated with all the newly merged cells</param>
        /// <returns>A Rectangle that encompasses the reference cell and all merged cells</returns>
        private Merges MergeCells(IList<Merges> cells, int refindex, bool[] merged) 
        {
            Rectangle result = cells[refindex].Item1;
            List<Rectangle> mergedCells = new List<Rectangle>();
            mergedCells.AddRange(cells[refindex].Item2);

            for (int j = refindex + 1; j < cells.Count; j++)
            {
                if (!merged[j] && Overlap(cells[refindex].Item1, cells[j].Item1))
                {
                    mergedCells.AddRange(cells[j].Item2);

                    Rectangle mergedRectangles = Rectangle.Union(result, cells[j].Item1);
                    if ((maxWidth == -1 || mergedRectangles.Width <= transformedSize.Width) &&
                        (maxHeight == - 1 || mergedRectangles.Height <= transformedSize.Height)) {
                        result = mergedRectangles;
                        merged[j] = true;
                    }
                }
            }
            return new Merges(result, mergedCells);
        }

        /// <summary>
        /// Compare each cell of the cells list with its follower, and merge the cells as necessary. The order of the 
        /// result respect the order of the original list for the first cell merged. That is, if 
        /// x' = merge(x1, ..., xN) and y' = merge(y1, ... yN) are two cells that occur in the 
        /// result in that order, then x1 occured before y1 in the original list.
        /// </summary>
        /// <param name="cells">The list of all cells</param>
        /// <returns>An enumeration of all the cells after merging, respecting the order of the original cells list as possible. 
        /// </returns>
        private IEnumerable<Merges> MergeCellsSinglePass(IList<Merges> cells)
        {
            bool[] merged = new bool[cells.Count];
            for (int i = 0; i < cells.Count; i++)
            {
                Merges result = cells[i];
                if (!merged[i])
                {
                    yield return MergeCells(cells, i, merged);
                }
            }
        }

        /// <summary>
        /// Apply the MergeCellsSinglePass to the boundaries of each cell in the polygons list iteratively,
        /// until a fixpoint is reached.
        /// </summary>
        /// <param name="polygons">The list of all cells</param>
        /// <returns>An enumeration of all the cells after merging, respecting the order of the original cells list as possible. 
        /// </returns>
        public IList<Merges> MergeCellsIterative(IEnumerable<Rectangle> cells)
        {
            bool change = true;
            IList<Merges> result = new List<Merges>();

            foreach (var cell in cells)
            {
                List<Rectangle> mergers = new List<Rectangle>();
                mergers.Add(cell);
                result.Add(Tuple.Create(cell, mergers));
            }

            while (change)
            {
                List<Merges> newResult = MergeCellsSinglePass(result).ToList();
                change = (newResult.Count != result.Count);
                result = newResult;
            }

            return result;
        }

        public virtual IEnumerable<Rectangle> ComputeView(IEnumerable<IPolygon> polygons)
        {
            var merged = MergeCellsIterative((from p in polygons select p.BoundingBox).AsParallel().ToList());

            return from cell in merged select cell.Item1;
        }

        protected Size transformSize(int width, int height)
        {
            Point[] result = new Point[] { new Point(width, height) };
            viewTransformation.TransformVectors(result);
            result[0].X = Math.Abs(result[0].X);
            result[0].Y = Math.Abs(result[0].Y);
            return new Size(result[0]);
        }

        public Matrix ViewTransformation { 
            get { return viewTransformation; }
            set {
                viewTransformation = value;
                transformedSize = transformSize(maxWidth, maxHeight);
            }
        }
    }
}
