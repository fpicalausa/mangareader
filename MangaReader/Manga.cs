using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using MangaParser.Reader;

namespace MangaReader
{
    public class Manga
    {
        MangaConfiguration configuration = new MangaConfiguration();
        MangaDirectoryManager directory;
        Dictionary<int, WeakReference<MangaPage>> pagesCache = new Dictionary<int, WeakReference<MangaPage>>();
        MangaPageViewer viewer;

        /// <summary>
        /// The current suggested width of views on the manga
        /// </summary>
        public int ViewWidth { get { return viewer.Width; } set { viewer.Width = value; } }

        /// <summary>
        /// The current suggested height of the views on the manga
        /// </summary>
        public int ViewHeight { get { return viewer.Height; } set { viewer.Height = value; } }

        /// <summary>
        /// Construct a manga information extraction from a given image file.
        /// </summary>
        /// <param name="initialPath">An initial image file path</param>
        public Manga(string initialPath)
        {
            directory = new MangaDirectoryManager(initialPath);
            configuration.PropertyChanged += configuration_PropertyChanged;
            viewer = new MangaPageViewer(configuration);
        }

        private void configuration_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            pagesCache.Clear();
        }

        /// <summary>
        /// The configuration indicating how the manga is to be read.
        /// </summary>
        public MangaConfiguration Configuration { get { return configuration; } }

        /// <summary>
        /// Retrieves the manga page corresponding to the specified 0-based index.
        /// </summary>
        /// <param name="pageNumber">The index of the page to be retrieved.</param>
        /// <returns>The manga page at the specified index.</returns>
        public MangaPage GetPage(int pageNumber)
        {
            MangaPage result = null;
            WeakReference<MangaPage> cached;
            if (pagesCache.TryGetValue(pageNumber, out cached))
            {
                cached.TryGetTarget(out result);
            }

            if (result == null)
            {
                result = new MangaPage(this, viewer, directory.GetPagePath(pageNumber), pageNumber);
                cached = new WeakReference<MangaPage>(result);
                pagesCache[pageNumber] = cached;
            }

            return result;
        }

        /// <summary>
        /// Retrieves the manga page that corresponds to the speicified image file.
        /// </summary>
        /// <param name="path">The image file that represent the manga page to be retrieved.</param>
        /// <returns>The manga page corresponding to the specified image file or null if
        /// no page corresponds to the path</returns>
        public MangaPage GetPage(string path)
        {
            int index = directory.GetPageIndex(path);

            if (index == -1) return null;
            return GetPage(index);
        }

        /// <summary>
        /// Whether a translation of this manga has been specified
        /// </summary>
        public bool HasTranslation
        {
            get
            {
                return !String.IsNullOrWhiteSpace(configuration.Translation) &&
                    Directory.Exists(configuration.Translation);
            }
        }

        /// <summary>
        /// The number of pages in this manga
        /// </summary>
        public int PageCount
        {
            get
            {
                return directory.PageCount;
            }
        }
    }
}
