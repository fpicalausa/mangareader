using System;

namespace MangaReader
{
    /// <summary>
    /// A view that does not take into account manga cells to display the page, 
    /// but displays page as full width.
    /// </summary>
    class FreeviewHandler: IViewHandler
    {
        private int PageHeight;
        private int PageWidth;
        private int ViewTop = 0;
        private int ViewHeight;
        private Manga Manga;
        private MangaPage CurrentPage;

        public event EventHandler<DisplayEventArgs> Display;

        public event EventHandler<DisplayEventArgs> NextPageDisplay;

        public event EventHandler<DisplayEventArgs> PreviousPageDisplay;

        private void Raise(EventHandler<DisplayEventArgs> evt)
        {
            if (evt == null) return;
            evt(this, new DisplayEventArgs(new System.Drawing.Rectangle(0,ViewTop, PageWidth, ViewHeight), CurrentPage));
        }

        public FreeviewHandler(Manga manga, MangaPage currentPage)
        {
            this.CurrentPage = currentPage;
            this.Manga = manga;

            Refresh(null);
        }

        public void Next()
        {
            if (ViewTop + ViewHeight >= PageHeight)
            {
                if (CurrentPage.HasNext)
                {
                    CurrentPage = CurrentPage.Next;
                    updateSettings();
                    ViewTop = 0;
                    Raise(NextPageDisplay);
                }
            }
            else
            {
                ViewTop = Math.Min(ViewTop + ViewHeight / 4, PageHeight - ViewHeight);
                Raise(Display);
            }
        }

        public void Previous()
        {
            if (ViewTop <= 0)
            {
                if (CurrentPage.HasPrevious)
                {
                    CurrentPage = CurrentPage.Previous;
                    updateSettings();
                    ViewTop = PageHeight - ViewHeight;
                    Raise(PreviousPageDisplay);
                }
            }
            else
            {
                ViewTop = Math.Max(ViewTop - ViewHeight / 4, 0);
                Raise(Display);
            }
        }

        private void updateSettings()
        {
            PageHeight = CurrentPage.Bitmap.Height;
            PageWidth = CurrentPage.Bitmap.Width;
            ViewHeight = Manga.ViewHeight;
        }

        public void Refresh(System.Drawing.Rectangle? oldView)
        {
            updateSettings();

            if (oldView != null)
            {
                int oldheight = Math.Min(oldView.Value.Height, ViewHeight);
                ViewTop = (int)((oldView.Value.center().Y - oldheight) / 2);
            }

            Raise(Display);
        }
    }
}
