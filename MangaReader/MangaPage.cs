using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using MangaParser.Reader;

namespace MangaReader
{
    public class MangaPage
    {
        private string path;
        private int index;
        private List<Rectangle> reading;
        private Task<List<Rectangle>> readingComputation;
        private WeakReference<MangaPage> next;
        private WeakReference<MangaPage> previous;
        private WeakReference<BitmapSource> pageSource;
        private WeakReference<BitmapSource> translationSource;
        private Manga manga;
        private MangaPageViewer viewer;
        private WeakReference<Bitmap> bitmap;

        /// <summary>
        /// Precomputes the reading on this page.
        /// </summary>
        /// <returns>A task that, when completed, returns a list of rectangles of the page in their
        /// reading order</returns>
        public Task<List<Rectangle>> ComputeReadingAsync()
        {
            if (readingComputation == null)
            {
                readingComputation = Task.Run(() =>
                    (new Page(this.Bitmap, manga.Configuration.ReadingDirection).ComputeView(viewer)).ToList()
                );
            }

            return readingComputation;
        }

        /// <summary>
        /// Construct a page of the given manga using the given view.
        /// </summary>
        /// <param name="manga">The manga containing the page.</param>
        /// <param name="viewer">A viewer describing how the page should be read.</param>
        /// <param name="path">The path of the underlying image file.</param>
        /// <param name="index">The page number of this page in the manga.</param>
        public MangaPage(Manga manga, MangaPageViewer viewer, string path, int index)
        {
            this.manga = manga;
            this.next = new WeakReference<MangaPage>(null);
            this.previous = new WeakReference<MangaPage>(null);
            this.bitmap = new WeakReference<Bitmap>(null);
            this.pageSource = new WeakReference<BitmapSource>(null);
            this.translationSource = new WeakReference<BitmapSource>(null);
            this.viewer = viewer;
            this.path = path;
            this.index = index;

            manga.Configuration.PropertyChanged += viewer_PropertyChanged;
            viewer.PropertyChanged += viewer_PropertyChanged;
        }

        private void viewer_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            // When any property that update changes the reading is updated, 
            // clear the cached reading
            this.readingComputation = null;
            this.reading = null;
            this.translationSource.SetTarget(null);
        }

        /// <summary>
        /// Set a reference to the previous page.
        /// </summary>
        /// <param name="newPrevious">The page that precedes this page</param>
        private void linkUpPrevious(MangaPage newPrevious)
        {
            this.previous = new WeakReference<MangaPage>(newPrevious);
        }

        /// <summary>
        /// Set a reference to the next page.
        /// </summary>
        /// <param name="newPrevious">The page that follows this page</param>
        private void linkUpNext(MangaPage newNext)
        {
            this.next = new WeakReference<MangaPage>(newNext);
        }

        /// <summary>
        /// Extracts a value from the given weak reference if it exists, or
        /// create it otherwise.
        /// </summary>
        /// <param name="cache">The weak reference that possibly contains a value. This
        /// reference is updated if a new value is created.</param>
        /// <param name="create">A function that can instanciate a new value to
        /// be stored in the reference</param>
        /// <returns>Returns the value previously stored by the weak reference,
        /// or the new value that was created.</returns>
        private static T readCached<T>(WeakReference<T> cache, Func<T> create) where T:class
        {
            T result;

            if (!cache.TryGetTarget(out result))
            {
                result = create();
                cache.SetTarget(result);
            }

            return result;
        }

        /// <summary>
        /// A bitmap constructed from the image path.
        /// </summary>
        public Bitmap Bitmap
        {
            get
            {
                return readCached(this.bitmap, () => new Bitmap(path));
            }
        }

        /// <summary>
        /// A bitmap source constructed from the image path of the manga page.
        /// </summary>
        public BitmapSource Source
        {
            get { 
                return readCached(this.pageSource, () => new BitmapImage(new Uri(path)));
            }
        }

        /// <summary>
        /// A bitmap source constructed from the image path corresponding to the translation
        /// of the manga page.
        /// </summary>
        public BitmapSource TranslationSource
        {
            get { 
                return readCached(this.translationSource, () => new BitmapImage(new Uri(TranslationPath)));
            }
        }

        /// <summary>
        /// Whether this manga page has a translation.
        /// </summary>
        public bool HasTranslation
        {
            get
            {
                return manga.HasTranslation && File.Exists(TranslationPath);
            }

        }

        /// <summary>
        /// The path of the manga translation.
        /// </summary>
        public string TranslationPath
        {
            get
            {
                return Path.Combine(manga.Configuration.Translation, Path.GetFileName(path));
            }
        }
        
        /// <summary>
        /// The manga page that follows this one.
        /// </summary>
        public MangaPage Next {
            get {
                MangaPage page;

                if (!HasNext) return null;

                if (!next.TryGetTarget(out page))
                {
                    page = manga.GetPage(index + 1);
                    next.SetTarget(page);
                    page.linkUpPrevious(this);
                }

                return page;
            }
        }

        /// <summary>
        /// Whether a manga page follows this one.
        /// </summary>
        public bool HasNext
        {
            get
            {
                return manga.PageCount > (index + 1);
            }
        }

        /// <summary>
        /// The manga page that precedes this one.
        /// </summary>
        public MangaPage Previous {
            get {
                MangaPage page;

                if (!HasPrevious) return null;

                if (!previous.TryGetTarget(out page))
                {
                    page = manga.GetPage(index - 1);
                    previous.SetTarget(page);
                    page.linkUpNext(this);
                }

                return page;
            }
        }

        /// <summary>
        /// Whether a manga page precedes this one.
        /// </summary>
        public bool HasPrevious
        {
            get
            {
                return index > 0;
            }
        }

        /// <summary>
        /// Parse the manga page, if the reading has not been computed before.
        /// After calling this method, the reading field is filled with the
        /// reading of the manga page.
        /// </summary>
        private void ensuresReading()
        {
            if (this.readingComputation == null)
                this.readingComputation = ComputeReadingAsync();
            readingComputation.Wait();
            this.reading = readingComputation.Result;
        }

        /// <summary>
        /// Get a sequence of views on this page, in reading order.
        /// </summary>
        public IReadOnlyList<Rectangle> PageReading
        {
            get
            {
                ensuresReading();
                return this.reading;
            }
        }
    }
}
