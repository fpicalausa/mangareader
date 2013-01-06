using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MangaParser.Graphics;

namespace MangaParser.Reader
{
    public interface ICellReadingOrder
    {
        IEnumerable<IPolygon> GetReadingOrder(IEnumerable<IPolygon> cells);
    }
}
