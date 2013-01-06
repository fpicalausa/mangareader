using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MangaReader
{
    /// <summary>
    /// Parse a directory containing images, into an ordered sequence of path corresponding
    /// to manga pages.
    /// </summary>
    class MangaDirectoryManager
    {
        List<string> pages;

        private static int commonPrefix(string s1, string s2)
        {
            int result = 0;
            int max = Math.Min(s1.Length, s2.Length);

            while (result != max && s1[result] == s2[result]) result++;

            return result;
        }

        private static int numericLength(string s)
        {
            int result = 0;

            while (Char.IsDigit(s, result)) result++;

            return result;
        }

        private static int CompareFiles(FileInfo f1, FileInfo f2)
        {

            string f1name = f1.Name;
            string f2name = f2.Name;

            if (f1name == f2name) return 0;

            int prefix = commonPrefix(f1name, f2name);

            string remainder1 = f1name.Substring(prefix);
            string remainder2 = f2name.Substring(prefix);

            int numericLength1 = numericLength(remainder1);
            int numericLength2 = numericLength(remainder2);

            if (numericLength1 > 0 && numericLength2 > 0)
            {
                return int.Parse(remainder1.Substring(0, numericLength1), CultureInfo.CurrentCulture) -
                       int.Parse(remainder2.Substring(0, numericLength2), CultureInfo.CurrentCulture);
            }

            return StringComparer.CurrentCulture.Compare(remainder1, remainder2);
        }

        public MangaDirectoryManager(string Path)
        {
            FileInfo baseFile = new FileInfo(Path);
            DirectoryInfo container = baseFile.Directory;
            FileInfo[] files = container.GetFiles("*" + baseFile.Extension);

            Array.Sort(files, CompareFiles);

            pages = (from file in files select file.FullName).ToList();
        }

        /// <summary>
        /// Retrieves the manga page corresponding to the specified file path
        /// </summary>
        public int GetPageIndex(string path)
        {
            return pages.FindIndex((x) => x == path);
        }

        /// <summary>
        /// Retrieves the file path corresponding to the specified manga page
        /// </summary>
        public string GetPagePath(int pageNumber)
        {
            return pages[pageNumber];
        }

        /// <summary>
        /// The number of manga pages in the directory
        /// </summary>
        public int PageCount { get { return pages.Count; } }
    }
}
