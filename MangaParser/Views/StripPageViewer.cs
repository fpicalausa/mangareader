using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MangaParser.Graphics;
using MangaParser.Geometry;

namespace MangaParser.Reader
{
    /// <summary>
    /// A view that encompasses horizontal strips of cells of the manga
    /// </summary>
    public class StripPageViewer: MergingPageViewer
    {
        private int maxWidth = 0;
        private int maxHeight = 0;

        /// <summary>
        /// Cells are to be merged if they are adjacent, that is, if they share a "significant" number of
        /// pixels when proejected on the Y axis.
        /// </summary>
        protected override bool Overlap(Rectangle rectangle1, Rectangle rectangle2)
        {
            int overlap = rectangle1.YOverlap(rectangle2);
            return Math.Max((overlap * 1.0 / rectangle1.Height), (overlap * 1.0 / rectangle2.Height)) > 0.5;
        }

        /// <summary>
        /// Construct a viewer that considers the manga page one row of cells at a time. 
        /// The row is built to fit, as best as it can, in the specified size limits.
        /// </summary>
        /// <param name="maxWidth">The maximum expected width of a strip (in pixels)</param>
        /// <param name="maxHeight">The maximum expected height of a strip (in pixels)</param>
        public StripPageViewer(int maxWidth, int maxHeight) : base(-1, -1)
        {
            this.maxWidth = maxWidth;
            this.maxHeight = maxHeight;
        }

        /// <summary>
        /// Greedily construct a sequences of cells (sub strip) that satisfy the width limit.
        /// </summary>
        /// <param name="mergedCells">A list of all cells of the strip as if appears on the page,
        /// without size limitation</param>
        /// <returns>Indices indicating where each sequence begins in the input list</returns>
        private IEnumerable<int> maxViews(List<Rectangle> mergedCells)
        {
            Rectangle current = mergedCells.First();

            for (int i = 1; i < mergedCells.Count; i++)
            {
                Rectangle merge = Rectangle.Union(current, mergedCells[i]);

                if (merge.Width < maxWidth)
                {
                    current = merge;
                }
                else
                {
                    yield return i;
                    current = mergedCells[i];
                }
            }

            yield return mergedCells.Count;
        }

        /// <summary>
        /// Determines whether a cell can be moved from one sub strip to the following one,
        /// that is, if moving the cell improve the uniformity of size of the sub strips.
        /// </summary>
        /// <param name="cells">The list of all cells in the full strip</param>
        /// <param name="first">The index of the first cell (inclusive) of the first sub strip</param>
        /// <param name="middle">The index of the last (exclusive) cell of the first sub strip</param>
        /// <param name="last">The index of the last (exclusive) cell of the second sub strip</param>
        /// <param name="currentBalance">How balanced the two sub strips are, if the cell can be forwarded</param>
        /// <returns>Returns true if moving the cell from one substrip to the following one improves the balance.</returns>
        private bool canForward(List<Rectangle> cells, int first, int middle, int last, ref double currentBalance)
        {
            // The view contains more that one cell
            if ((middle - first) <= 1) return false;

            int w1_before = getWidth(cells, first, middle - first);
            int w2_before = getWidth(cells, middle, last - middle);

            // The view from which we transfer is larger than the one to which we 
            // transfer.
            if (w1_before <= w2_before) return false;

            int w2_after = getWidth(cells, middle - 1, last - middle + 1);

            // Transfering does not make view 2 larger than the limit
            if (w2_after >= maxWidth) return false;

            int w1_after = getWidth(cells, first, middle - first - 1);

            // Transfering improves the balance
            currentBalance =  balance(w1_before, w2_before);
            if (balance(w1_after, w2_after) > currentBalance) return false;

            return true;
        }

        /// <summary>
        /// Get the width of a sub strip
        /// </summary>
        private static int getWidth(List<Rectangle> cells, int index, int length)
        {
            int left = (from cell in cells.Skip(index).Take(length) select cell.Left).Min();
            int right = (from cell in cells.Skip(index).Take(length) select cell.Right).Max();

            return right - left;
        }

        /// <summary>
        /// Determines the width balance of two sub strips. This function is symmetric in width1, width2. 
        /// The minimum balance is 0; larger values correspond to larger imbalance of the two widths.
        /// </summary>
        private static double balance(int width1, int width2)
        {
            return Math.Abs(1.0 - Math.Min(width1, width2) * 1.0 / Math.Max(width2, width1));
        }

        /// <summary>
        /// Try to obtain similarly sized views (of many cells), by moving cells forward between
        /// consecutive views.
        /// </summary>
        /// <param name="mergedCells"></param>
        private IEnumerable<int> balance(List<int> views, List<Rectangle> cells)
        {
            double worseBalance = 0.0;
            int index = -1;
            bool changed = true;

            while ( changed ) {
                changed = false;

                // Find the most unbalanced pair of views.
                index = -1;
                for (int j = 0, i = 0; i < views.Count - 1; j = views[i], i++)
                {
                    double oldbalance = 0.0;
                    if (canForward(cells, j, views[i], views[i+1], ref oldbalance)) {
                        if (oldbalance > worseBalance)
                        {
                            index = i;
                            worseBalance = oldbalance;
                        }
                    }
                }

                if (worseBalance < 0.2 || index == -1) break;

                // If they are indeed unbalanced, try to improve the situation
                views[index] -= 1;
                changed = true;
            }

            return views;
        }

        public override IEnumerable<Rectangle> ComputeView(IEnumerable<IPolygon> polygons)
        {
            var merged = MergeCellsIterative((from p in polygons select p.BoundingBox).AsParallel().ToList());
            CellsPageViewer cellsmerger = new CellsPageViewer(maxWidth, maxHeight);

            foreach (var merge in merged)
            {
                if (merge.Item1.Width < maxWidth)
                {
                    yield return merge.Item1;
                }
                else
                {
                    // If the strip does not fit in the current view, we want to
                    // split it at cells boundaries in such a way that each chunk 
                    // of the strip fits in the view. 

                    // As a first approximation, we use a greedy algorithm that
                    // stuffs as many cells of the strip as the view can hold, 
                    // repeatedly. This can cause the views to be unbalanced:
                    // a strip of 4 cells of the same size could be split into
                    // a large view of 3 cells and a small view of 1 cell.

                    // We use a naive algorithm to balance the cells between each 
                    // views to avoid this.

                    var mergedCells = (
                        from cell in cellsmerger.MergeCellsIterative(merge.Item2)
                        select cell.Item1).ToList();
                    var views = balance(maxViews(mergedCells).ToList(), mergedCells);

                    int i = 0;
                    foreach (var view in views)
                    {
                        Rectangle viewRect = mergedCells[i++];
                        while (i < view)
                        {
                            viewRect = Rectangle.Union(viewRect, mergedCells[i]);
                            i++;
                        }
                        yield return viewRect;
                    }
                }
            }
        }

    }
}
