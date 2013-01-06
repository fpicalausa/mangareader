using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MangaParser.Graphics;

namespace MangaParser.Reader
{
    public interface IPageViewer
    {
        IEnumerable<Rectangle> ComputeView(IEnumerable<IPolygon> polygons);
    }
}
