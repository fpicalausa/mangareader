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
    public class CellsPageViewer: MergingPageViewer
    {
        /// <summary>
        /// Two cells overlap if their bounding rectangle overlap by more than 50% of their area.
        /// </summary>
        /// <param name="rectangle1">The bounding rectangle of the first cell</param>
        /// <param name="rectangle2">The bounding rectangle of the second cell</param>
        /// <returns>Returns true if either rectangle overlaps the other by more than 50% of its surface.</returns>
        protected override bool Overlap(Rectangle rectangle1, Rectangle rectangle2)
        {
            // The rationale is that we want to provide a rectangular view over 
            // cells of various shape, that might be embedded in a same square.
            Decimal overlapArea = rectangle1.Overlap(rectangle2);
            return Math.Max((overlapArea / rectangle1.Area()), (overlapArea / rectangle2.Area())) >= (decimal)0.5;
        }


        public CellsPageViewer(int maxWidth, int maxHeight) : base(maxWidth, maxHeight)
        { }
    }
}
