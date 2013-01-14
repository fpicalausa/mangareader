using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using MangaReader;

namespace MangaReader
{
    /// <summary>
    /// A view handler that conforms to the parsing of the manga page.
    /// </summary>
    class ParsedViewHandler : IViewHandler
    {
        public MangaPage CurrentPage { get; set; }
        private int targetIndex = -1;    // The current rectangle being displayed
        private IReadOnlyList<Rectangle> targets; // The rectangles to be displayed

        public event EventHandler<DisplayEventArgs> Display;
        public event EventHandler<DisplayEventArgs> NextPageDisplay;
        public event EventHandler<DisplayEventArgs> PreviousPageDisplay;

        public ParsedViewHandler(MangaPage initialPage)
        {
            CurrentPage = initialPage;
        }

        private void Raise(EventHandler<DisplayEventArgs> evt)
        {
            if (evt == null) return;

            evt(this, new DisplayEventArgs(targets[targetIndex], CurrentPage));
        }

        private void updateTargets()
        {
            targets = CurrentPage.PageReading;
        }

        public void Next()
        {
            if (targetIndex == targets.Count - 1)
            {
                if (!CurrentPage.HasNext) return;

                CurrentPage = CurrentPage.Next;
                updateTargets();
                targetIndex = 0;

                Raise(NextPageDisplay);
            }
            else
            {
                targetIndex = targetIndex + 1;
                Raise(Display);
            }

            if (CurrentPage.HasNext) CurrentPage.Next.ComputeReadingAsync();
        }

        public void Previous()
        {
            if (targetIndex == 0)
            {
                if (!CurrentPage.HasPrevious) return;

                CurrentPage = CurrentPage.Previous;
                updateTargets();
                targetIndex = targets.Count - 1;

                Raise(PreviousPageDisplay);
            }
            else
            {
                targetIndex = targetIndex - 1;
                Raise(Display);
            }

            if (CurrentPage.HasPrevious) CurrentPage.Previous.ComputeReadingAsync();
        }

        public void Refresh(Rectangle? oldView)
        {
            updateTargets();

            if (oldView != null)
            {
                var oldCenter = oldView.Value.center();

                var contained = (from t in targets.Select((x, i) => new { target = x, index = i })
                                 where oldView.Value.Contains(t.target)
                                 select t.index);

                if (contained.Any())
                {
                    targetIndex = contained.First();
                }
                else
                {
                    targetIndex = (from t in targets.Select((x, i) => new { target = x, index = i })
                                   let c = t.target.center()
                                   orderby c.SqDist(oldCenter) ascending
                                   select t.index).First();
                }
            }
            else if (targetIndex == -1)
            {
                targetIndex = 0;
            }
            else if (targetIndex > targets.Count)
            {
                targetIndex = targets.Count - 1;
            }

            Raise(Display);
        }
    }
}
