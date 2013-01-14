using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Xml.Serialization;
using System.Collections.ObjectModel;

namespace MangaReader
{
    [Serializable]
    public class RecentMangas 
    {
        private const int limit = 10;

        public ObservableCollection<MangaState> Recent { get; set; }

        [XmlIgnore]
        public int PinnedCount { get; private set; }

        private static string ConfigurationPath()
        {
            var appdata = System.Environment.SpecialFolder.ApplicationData;
            var directory = Path.Combine(System.Environment.GetFolderPath(appdata), "MangaReader");

            Directory.CreateDirectory(directory); // Ensures that the directory exists

            return Path.Combine(directory, "recent.xml");
        }

        private MangaState buildState(MangaPage page, Rectangle view, bool pinned)
        {
            return new MangaState(page.Path, view, page.Manga.Configuration, pinned);
        }

        private int findState(MangaState state)
        {
            return Recent
                .Select( (value, index) => new { value, index })
                .Where((x) => x.value.MangaPath.FullName == state.MangaPath.FullName)
                .Select((x) => x.index)
                .DefaultIfEmpty(-1).First();
        }

        private void removeState(int index)
        {
            if (index == -1) return;

            MangaState state = Recent[index];
            Recent.RemoveAt(Recent.Count);
            unbindState(state);

            if (state.Pinned)
            {
                PinnedCount -= 1;
            }
        }

        private void trimRecents()
        {
            if (Recent.Count - PinnedCount > limit)
            {
                var state = Recent[Recent.Count - 1];
                removeState(Recent.Count - 1);
            }
        }

        private void bindState(MangaState state)
        {
            state.PropertyChanged += state_PropertyChanged;
        }

        private void unbindState(MangaState state)
        {
            state.PropertyChanged -= state_PropertyChanged;
        }

        private void state_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "Pinned")
            {
                updateRecentState(sender as MangaState);
            }
        }

        private void addPinned(MangaState state)
        {
            bindState(state);
            Recent.Insert(0, state);
            PinnedCount++;
        }

        private void addUnpinned(MangaState state)
        {
            bindState(state);
            Recent.Insert(PinnedCount, state);
            trimRecents();
        }

        private void addRecent(MangaState state)
        {
            if (state.Pinned) addPinned(state);
            else addUnpinned(state);
        }

        private void updateRecentState(MangaState state)
        {
            var ri = findState(state);

            if (ri == -1)
            {
                addRecent(state);
            }
            else if (Recent[ri].Pinned == state.Pinned)
            {
                Recent[ri] = state;
            }
            else {
                removeState(ri);
                addRecent(state);
            }
        }

        public void AddRecent(MangaPage page, Rectangle view)
        {
            updateRecentState(buildState(page, view, false));
        }

        public RecentMangas()
        {
            Recent = new ObservableCollection<MangaState>();
        }

        public void Save()
        {
            using (var stream = new FileStream(ConfigurationPath(), FileMode.Create, FileAccess.Write))
            {
                XmlSerializer xser = new XmlSerializer(typeof(RecentMangas));
                xser.Serialize(stream, this);
                stream.Close();
            }
        }

        public static RecentMangas Load()
        {
            var result = new RecentMangas();

            try
            {
                using (var stream = new FileStream(ConfigurationPath(), FileMode.Open, FileAccess.ReadWrite))
                {
                    XmlSerializer xser = new XmlSerializer(typeof(RecentMangas));
                    result = xser.Deserialize(stream) as RecentMangas;
                    stream.Close();

                    fixupDeserialized(result);
                }
            }
            catch (FileNotFoundException) { /* Do not load anything */ }
            catch (InvalidOperationException) { /* data corrupted -- too bad */ }
            return result;
        }

        private static void fixupDeserialized(RecentMangas result)
        {
            result.PinnedCount = result.Recent.Where((x) => x.Pinned).Count();

            foreach (var item in result.Recent)
            {
                result.bindState(item);
            }
        }


    }
}
