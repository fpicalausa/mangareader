using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MangaParser.Reader;
using MangaParser.Geometry;

namespace MangaParser.Reader
{
    public static class MangaPageUtil
    {
        public static IEnumerable<Rectangle> ComputeView(this Page page, IPageViewer viewer)
        {
            viewer.ViewTransformation = Page.ReadingDirectionToMatrix(page.Direction);
            IEnumerable<Rectangle> view = viewer.ComputeView(page.TransformedCells);
            Matrix invert = Page.ReadingDirectionToInverseMatrix(page.Direction);
            return (from rectangle in view select rectangle.Transform(invert));
        }
    }
}
