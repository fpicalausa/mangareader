using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaReader
{
    /// <summary>
    /// A null-object implementation of the view handler. This implementation
    /// is used when no manga is loaded.
    /// </summary>
    class NullViewHandler: IViewHandler
    {
        public event EventHandler<DisplayEventArgs> Display { add { } remove { } }
        public event EventHandler<DisplayEventArgs> NextPageDisplay { add { } remove { } }
        public event EventHandler<DisplayEventArgs> PreviousPageDisplay { add { } remove { } }

        public void Next()
        { }

        public void Previous()
        { }

        public void Refresh(System.Drawing.Rectangle? oldView)
        { }
    }
}
