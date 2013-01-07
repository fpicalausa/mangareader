using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MangaParser.Graphics;

namespace MangaParser.Reader
{
    public interface IPageViewer
    {
        /**
         * The tranformation of the cells under which the view is computed
         */
        Matrix ViewTransformation { get; set; }

        /**
         * Compute a view of the manga cells. 
         */
        IEnumerable<Rectangle> ComputeView(IEnumerable<IPolygon> polygons);
    }
}
