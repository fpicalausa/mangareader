using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace MangaReader
{
    /// <summary>
    /// Describes how the page is to be displayed.
    /// </summary>
    public class DisplayEventArgs: EventArgs 
    {
        /// <summary>
        /// The rectangle of the page that must be displayed.
        /// </summary>
        public Rectangle Rectangle { get; set; }

        /// <summary>
        /// The page to be displayed.
        /// </summary>
        public MangaPage Page { get; set; }

        public DisplayEventArgs (Rectangle rectangle, MangaPage page)
        {
            this.Rectangle = rectangle;
            this.Page = page;
        }
    }

    /// <summary>
    /// Defines how to handle events in a manga view.
    /// </summary>
    public interface IViewHandler
    {
        /// <summary>
        /// Raised to indicate that the manga view must display a new area of 
        /// the current page.
        /// </summary>
        event EventHandler<DisplayEventArgs> Display;

        /// <summary>
        /// Raised to indicate that the manga view must display some area of the
        /// the next page.
        /// </summary>
        event EventHandler<DisplayEventArgs> NextPageDisplay;

        /// <summary>
        /// Raised to indicate that the manga view must display some area of the
        /// the previous page.
        /// </summary>
        event EventHandler<DisplayEventArgs> PreviousPageDisplay;

        /// <summary>
        /// Called when the next view is requested. The implementation should 
        /// respond by raising one of the Display or NextPageDisplay events.
        /// </summary>
        void Next();

        /// <summary>
        /// Called when the next view is requested. The implementation should 
        /// respond by raising one of the Display or PreviousPageDisplay events.
        /// </summary>
        void Previous();

        /// <summary>
        /// Called when the options are updated. The implementation must respond
        /// by refreshing the information it has on the current manga page, and 
        /// then raising Display with the display rectangle that best matches the 
        /// old view. If oldView is null, the currently displayed item should be kept.
        /// </summary>
        /// <param name="oldView">The previous display of the page, before the information were refreshed</param>
        void Refresh(Rectangle? oldView);
    }
}
