using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MangaParser.Graphics;

namespace MangaParser.Graphics
{
    public interface ICellsSegmentation
    {
        IExtractionResult Extract(Bitmap page);
    }

    public interface IExtractionResult
    {
        Bitmap Page { get; }
        IEnumerable<IPolygon> Polygons { get; }
        bool FullPage { get; }
    }
}
