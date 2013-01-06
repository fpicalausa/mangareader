using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MangaParser.Graphics;
using MangaParser.Reader;

namespace MangaReader
{
    public class FullPageViewer: IPageViewer
    {
        public IEnumerable<System.Drawing.Rectangle> ComputeView(IEnumerable<MangaParser.Graphics.IPolygon> polygons)
        {
            // Union over all bits
            return new Rectangle[] { polygons.Aggregate(
                polygons.First().BoundingBox,
                (Rectangle res, IPolygon p2) => Rectangle.Union(res, p2.BoundingBox)) };
        }
    }
}
