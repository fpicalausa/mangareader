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
        private List<Rectangle> reading;
        private Task<List<Rectangle>> readingComputation;
        private WeakReference<MangaPage> next;
        private WeakReference<MangaPage> previous;
        private WeakReference<BitmapSource> pageSource;
        private WeakReference<BitmapSource> translationSource;
        private MangaPageViewer viewer;
        private WeakReference<Bitmap> bitmap;

        /// <summary>
        /// The Path of the image of this page of the Manga
        /// </summary>
        public string Path { get; private set; }

        /// <summary>
        /// The Index of this page in the Manga
        /// </summary>
        public int Index { get; private set; }

        /// <summary>
        /// The Manga that contains this page
        /// </summary>
        public Manga Manga { get; private set; }


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
                    (new Page(this.Bitmap, Manga.Configuration.ReadingDirection).ComputeView(viewer)).ToList()
                );
            }

            return readingComputation;
        }

        /// <summary>
        /// Construct a page of the given Manga using the given view.
        /// </summary>
        /// <param name="Manga">The manga containing the page.</param>
        /// <param name="viewer">A viewer describing how the page should be read.</param>
        /// <param name="Path">The path of the underlying image file.</param>
        /// <param name="Index">The page number of this page in the Manga.</param>
        public MangaPage(Manga Manga, MangaPageViewer viewer, string Path, int Index)
        {
            this.next = new WeakReference<MangaPage>(null);
            this.previous = new WeakReference<MangaPage>(null);
            this.bitmap = new WeakReference<Bitmap>(null);
            this.pageSource = new WeakReference<BitmapSource>(null);
            this.translationSource = new WeakReference<BitmapSource>(null);

            this.viewer = viewer;

            this.Manga = Manga;
            this.Path = Path;
            this.Index = Index;

            Manga.Configuration.PropertyChanged += viewer_PropertyChanged;
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
        /// A bitmap constructed from the image Path.
        /// </summary>
        public Bitmap Bitmap
        {
            get
            {
                return readCached(this.bitmap, () => new Bitmap(Path));
            }
        }

        /// <summary>
        /// A bitmap source constructed from the image Path of the Manga page.
        /// </summary>
        public BitmapSource Source
        {
            get { 
                return readCached(this.pageSource, () => new BitmapImage(new Uri(Path)));
            }
        }

        /// <summary>
        /// A bitmap source constructed from the image Path corresponding to the translation
        /// of the Manga page.
        /// </summary>
        public BitmapSource TranslationSource
        {
            get { 
                return readCached(this.translationSource, () => new BitmapImage(new Uri(TranslationPath)));
            }
        }

        /// <summary>
        /// Whether this Manga page has a translation.
        /// </summary>
        public bool HasTranslation
        {
            get
            {
                return Manga.HasTranslation && File.Exists(TranslationPath);
            }

        }

        /// <summary>
        /// The Path of the Manga translation.
        /// </summary>
        public string TranslationPath
        {
            get
            {
                return System.IO.Path.Combine(Manga.Configuration.Translation, System.IO.Path.GetFileName(Path));
            }
        }
        
        /// <summary>
        /// The Manga page that follows this one.
        /// </summary>
        public MangaPage Next {
            get {
                MangaPage page;

                if (!HasNext) return null;

                if (!next.TryGetTarget(out page))
                {
                    page = Manga.GetPage(Index + 1);
                    next.SetTarget(page);
                    page.linkUpPrevious(this);
                }

                return page;
            }
        }

        /// <summary>
        /// Whether a Manga page follows this one.
        /// </summary>
        public bool HasNext
        {
            get
            {
                return Manga.PageCount > (Index + 1);
            }
        }

        /// <summary>
        /// The Manga page that precedes this one.
        /// </summary>
        public MangaPage Previous {
            get {
                MangaPage page;

                if (!HasPrevious) return null;

                if (!previous.TryGetTarget(out page))
                {
                    page = Manga.GetPage(Index - 1);
                    previous.SetTarget(page);
                    page.linkUpNext(this);
                }

                return page;
            }
        }

        /// <summary>
        /// Whether a Manga page precedes this one.
        /// </summary>
        public bool HasPrevious
        {
            get
            {
                return Index > 0;
            }
        }

        /// <summary>
        /// Parse the Manga page, if the reading has not been computed before.
        /// After calling this method, the reading field is filled with the
        /// reading of the Manga page.
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
