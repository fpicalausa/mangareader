using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaReader
{
    /// <summary>
    /// A view that displays each page in full.
    /// </summary>
    class FullPageViewHandler: IViewHandler
    {
        public event EventHandler<DisplayEventArgs> Display;
        public event EventHandler<DisplayEventArgs> NextPageDisplay;
        public event EventHandler<DisplayEventArgs> PreviousPageDisplay;

        private MangaPage CurrentPage { get; set; }

        public FullPageViewHandler(MangaPage CurrentPage)
        {
            this.CurrentPage = CurrentPage;
        }

        private void Raise(EventHandler<DisplayEventArgs> evt)
        {
            if (evt == null) return;
            evt(this, new DisplayEventArgs(new Rectangle(0,0, CurrentPage.Bitmap.Width, CurrentPage.Bitmap.Height), CurrentPage));
        }


        public void Next()
        {
            if (CurrentPage.HasNext)
            {
                CurrentPage = CurrentPage.Next;
                Raise(NextPageDisplay);
            }
        }

        public void Previous()
        {
            if (CurrentPage.HasPrevious)
            {
                CurrentPage = CurrentPage.Previous;
                Raise(PreviousPageDisplay);
            }
        }

        public void Refresh(System.Drawing.Rectangle? oldView)
        {
            Raise(Display);
        }
    }
}
